using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class StrategicAI : MonoBehaviour, IGameSystem
    {
        private PolicyAI policyAI;
        private MilitaryAI militaryAI;
        private DiplomacyAI diplomacyAI;
        private ReformSystem reformSystem;
        private GameManager gameManager;

        public void Initialize(GameContext context)
        {
            gameManager = GetComponent<GameManager>();

            policyAI = GetComponent<PolicyAI>();
            if (policyAI == null) policyAI = gameObject.AddComponent<PolicyAI>();

            militaryAI = GetComponent<MilitaryAI>();
            if (militaryAI == null) militaryAI = gameObject.AddComponent<MilitaryAI>();

            diplomacyAI = GetComponent<DiplomacyAI>();
            if (diplomacyAI == null) diplomacyAI = gameObject.AddComponent<DiplomacyAI>();

            reformSystem = GetComponent<ReformSystem>();
        }

        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            for (int i = 0; i < context.State.factions.Count; i++)
            {
                FactionState faction = context.State.factions[i];
                if (faction.id == context.State.playerFactionId) continue;

                ApplyGovernancePriority(context, faction);

                bool raidedLogistics = militaryAI != null && militaryAI.TryRaidEnemyLogistics(gameManager, faction);
                if (raidedLogistics)
                {
                    context.State.AddLog("ai", faction.name + "截粮前线后勤运输队。");
                }

                string policyId = policyAI.ChoosePolicy(context, faction);
                if (!string.IsNullOrEmpty(policyId))
                {
                    bool applied = reformSystem != null && reformSystem.ApplyPolicy(context, faction.id, policyId);
                    context.State.AddLog("ai", faction.name + (applied ? "执行局势政策：" : "倾向局势政策：") + policyId);
                }

                string targetRegionId = militaryAI.ChooseExpansionTarget(context, faction);
                if (!string.IsNullOrEmpty(targetRegionId))
                {
                    bool issued = TryIssueWarPressure(context, faction, targetRegionId);
                    context.State.AddLog("ai", faction.name + (issued ? "下令战争推进：" : "关注战争目标：") + targetRegionId);
                }

                diplomacyAI.ExecuteAIDiplomacy(context, faction);
            }
        }

        private void ApplyGovernancePriority(GameContext context, FactionState faction)
        {
            if (context == null || context.State == null || context.Data == null || faction == null) return;

            RegionState bestState = null;
            RegionDefinition bestDefinition = null;
            GovernanceActionForecast bestForecast = null;
            int bestScore = int.MinValue;

            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState state = context.State.FindRegion(faction.regionIds[i]);
                if (state == null) continue;

                RegionDefinition definition = context.Data.GetRegion(state.id);
                GovernanceActionForecast forecast = StrategyMapRulebook.BuildRecommendedGovernanceForecast(context, definition, state, faction);
                if (forecast == null || forecast.action == GovernanceActionKind.Hold || !forecast.canApply) continue;

                int score = ScoreGovernancePriority(faction, state, forecast);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestState = state;
                    bestDefinition = definition;
                    bestForecast = forecast;
                }
            }

            if (bestState == null || bestForecast == null || bestScore < 45) return;

            GovernanceActionForecast applied = StrategyMapRulebook.ApplyGovernanceAction(context, bestDefinition, bestState, faction, bestForecast.action);
            if (applied != null && applied.canApply)
            {
                context.State.AddLog("ai", faction.name + "处理治理缺口：" + bestState.id + " " + applied.actionId);
            }
        }

        private static int ScoreGovernancePriority(FactionState faction, RegionState state, GovernanceActionForecast forecast)
        {
            int score = state.rebellionRisk + state.localPower / 2 + (100 - state.localAcceptance) / 2;
            ControlStage stage = StrategyMapRulebook.ResolveControlStage(state);
            if (stage != ControlStage.Controlled) score += 40;
            if (state.annexationPressure >= 55) score += 12;

            switch (forecast.action)
            {
                case GovernanceActionKind.MilitaryGovern:
                    score += 24;
                    break;
                case GovernanceActionKind.Pacify:
                    score += 18;
                    break;
                case GovernanceActionKind.RegisterHouseholds:
                    score += 16;
                    break;
                case GovernanceActionKind.Relief:
                    score += 18;
                    break;
                case GovernanceActionKind.TaxPressure:
                case GovernanceActionKind.Conscription:
                    score += 8;
                    break;
            }

            if (forecast.moneyDelta < 0 && faction.money < 120) score -= 18;
            if (forecast.foodDelta < 0 && faction.food < 160) score -= 18;
            if (forecast.legitimacyDelta < 0 && faction.legitimacy < 45) score -= 20;
            return score;
        }

        private bool TryIssueWarPressure(GameContext context, FactionState faction, string targetRegionId)
        {
            if (gameManager == null || gameManager.World == null || gameManager.World.Map == null || gameManager.MapQueries == null) return false;
            if (!IsAtWarWithTarget(context, faction, targetRegionId)) return false;

            ArmyRuntimeState army = FindBestOffensiveArmy(context, faction, targetRegionId);
            if (army == null) return false;
            return gameManager.MoveArmy(army.id, targetRegionId);
        }

        private ArmyRuntimeState FindBestOffensiveArmy(GameContext context, FactionState faction, string targetRegionId)
        {
            ArmyRuntimeState bestArmy = null;
            int bestScore = int.MinValue;

            foreach (ArmyRuntimeState army in gameManager.World.Map.ArmiesById.Values)
            {
                if (army == null || army.ownerFactionId != faction.id || army.task != ArmyTask.Idle) continue;
                if (!string.IsNullOrEmpty(army.engagementId) || army.locationRegionId == targetRegionId) continue;

                CampaignRouteForecast forecast = StrategyMapRulebook.BuildCampaignRouteForecast(gameManager.MapQueries, context, army, targetRegionId);
                if (forecast == null || !forecast.canDispatch) continue;

                int score = army.supply + army.soldiers / 200 + forecast.supplyPowerPercent -
                            forecast.routeSteps * 5 - forecast.contactRisk / 2 - forecast.interceptionRisk / 2;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestArmy = army;
                }
            }

            return bestScore >= 25 ? bestArmy : null;
        }

        private static bool IsAtWarWithTarget(GameContext context, FactionState faction, string targetRegionId)
        {
            if (context == null || context.State == null || faction == null || string.IsNullOrEmpty(targetRegionId)) return false;

            RegionState targetState = context.State.FindRegion(targetRegionId);
            if (targetState == null || targetState.ownerFactionId == faction.id) return false;

            for (int i = 0; i < context.State.diplomaticRelations.Count; i++)
            {
                DiplomaticRelation relation = context.State.diplomaticRelations[i];
                if (relation.status != DiplomacyStatus.AtWar) continue;
                if ((relation.factionA == faction.id && relation.factionB == targetState.ownerFactionId) ||
                    (relation.factionB == faction.id && relation.factionA == targetState.ownerFactionId))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
