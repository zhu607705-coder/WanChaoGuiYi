using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class MilitaryAI : MonoBehaviour
    {
        public bool TryRaidEnemyLogistics(GameManager gameManager, FactionState faction)
        {
            if (gameManager == null || faction == null) return false;
            return gameManager.ApplyEnemyLogisticsRaid(faction.id);
        }

        public string ChooseExpansionTarget(GameContext context, FactionState faction)
        {
            if (context == null || context.State == null || context.Data == null || faction == null)
            {
                return string.Empty;
            }

            EmperorDefinition emperor = context.Data.GetEmperor(faction.emperorId);
            string bestRegionId = string.Empty;
            int bestScore = int.MinValue;

            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionDefinition owned = context.Data.GetRegion(faction.regionIds[i]);
                if (owned.neighbors == null) continue;

                for (int j = 0; j < owned.neighbors.Length; j++)
                {
                    RegionState neighbor = context.State.FindRegion(owned.neighbors[j]);
                    if (neighbor != null && neighbor.ownerFactionId != faction.id)
                    {
                        RegionDefinition targetDefinition = context.Data.GetRegion(neighbor.id);
                        RegionState ownedState = context.State.FindRegion(owned.id);
                        int score = ScoreExpansionTarget(context, faction, emperor, ownedState, targetDefinition, neighbor);
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestRegionId = neighbor.id;
                        }
                    }
                }
            }

            return bestScore >= ResolveExpansionThreshold(faction, emperor) ? bestRegionId : string.Empty;
        }

        private static int ScoreExpansionTarget(GameContext context, FactionState faction, EmperorDefinition emperor,
            RegionState stagingRegion, RegionDefinition targetDefinition, RegionState targetState)
        {
            if (targetDefinition == null || targetState == null) return int.MinValue;

            RegionSpecialization specialization = StrategyMapRulebook.ResolveSpecialization(targetDefinition, targetState, context.Data);
            int score = 20;

            score += targetState.foodOutput / 10;
            score += targetState.taxOutput / 10;
            score += targetState.manpower / 2;

            if (targetState.supplyNode) score += 18;
            if (stagingRegion != null && stagingRegion.supplyNode) score += 10;

            switch (specialization)
            {
                case RegionSpecialization.Grain:
                    score += faction.food < 160 ? 26 : 12;
                    break;
                case RegionSpecialization.Tax:
                    score += faction.money < 140 ? 22 : 12;
                    break;
                case RegionSpecialization.Military:
                    score += 18;
                    break;
                case RegionSpecialization.Border:
                    score += 16;
                    break;
                case RegionSpecialization.Capital:
                case RegionSpecialization.Legitimacy:
                    score += faction.legitimacy >= 45 ? 12 : 4;
                    break;
                case RegionSpecialization.Culture:
                    score += 8;
                    break;
            }

            score -= targetState.rebellionRisk / 3;
            score -= targetState.localPower / 4;
            score -= targetState.annexationPressure / 4;

            FactionState targetOwner = context.State.FindFaction(targetState.ownerFactionId);
            if (targetOwner != null)
            {
                if (targetOwner.legitimacy < 45) score += 10;
                if (targetOwner.regionIds.Count <= 2) score += 8;
            }

            if (faction.food < 100) score -= 24;
            if (faction.money < 80) score -= 14;
            if (faction.legitimacy < 40) score -= 22;
            if (faction.successionRisk >= 65) score -= 18;

            if (emperor != null && emperor.aiPersonality != null)
            {
                score += (emperor.aiPersonality.expansion - 50) / 2;
                score += (emperor.aiPersonality.riskTolerance - 50) / 4;
                score -= (emperor.aiPersonality.governance - 50) / 8;
            }

            return score;
        }

        private static int ResolveExpansionThreshold(FactionState faction, EmperorDefinition emperor)
        {
            int threshold = 42;
            if (emperor != null && emperor.aiPersonality != null)
            {
                threshold -= (emperor.aiPersonality.expansion - 50) / 6;
                if (emperor.aiPersonality.riskTolerance < 40) threshold += 8;
            }

            if (faction.food < 100) threshold += 12;
            if (faction.money < 80) threshold += 8;
            if (faction.legitimacy < 40) threshold += 12;
            if (faction.successionRisk >= 65) threshold += 10;
            return threshold;
        }
    }
}
