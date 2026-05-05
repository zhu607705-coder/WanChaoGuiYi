using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class EspionageSystem : MonoBehaviour, IGameSystem
    {
        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            ProcessActiveOperations(context);
        }

        public void OnTurnEnd(GameContext context) { }

        public EspionageResult StartOperation(GameContext context, string agentFactionId, string targetFactionId,
            EspionageActionType actionType, string targetEntityId = null)
        {
            FactionState agent = context.State.FindFaction(agentFactionId);
            FactionState target = context.State.FindFaction(targetFactionId);
            if (agent == null || target == null)
            {
                return new EspionageResult { success = false, reason = "faction_not_found" };
            }

            if (agent.talentIds.Count < 1)
            {
                return new EspionageResult { success = false, reason = "no_talent" };
            }

            NumericContext numericContext = NumericModifierFactory.ForFaction(agent);
            int cost = CalculateEspionageCost(actionType, numericContext);
            if (agent.money < cost)
            {
                return new EspionageResult { success = false, reason = "insufficient_funds" };
            }

            for (int i = 0; i < context.State.activeOperations.Count; i++)
            {
                EspionageOperation op = context.State.activeOperations[i];
                if (op.agentFactionId == agentFactionId && op.targetFactionId == targetFactionId && op.actionType == actionType)
                {
                    return new EspionageResult { success = false, reason = "operation_already_active" };
                }
            }

            agent.money -= cost;
            string talentId = agent.talentIds[0];
            agent.talentIds.RemoveAt(0);

            int detectionRisk = CalculateDetectionRisk(context, agent, target, actionType, numericContext);

            EspionageOperation operation = new EspionageOperation
            {
                id = "esp_" + agentFactionId + "_" + targetFactionId + "_" + context.State.turn,
                agentFactionId = agentFactionId,
                targetFactionId = targetFactionId,
                actionType = actionType,
                progress = 0,
                detectionRisk = detectionRisk,
                targetEntityId = targetEntityId
            };

            context.State.activeOperations.Add(operation);

            string actionName = GetActionName(actionType);
            context.State.AddLog("espionage", agent.name + "对" + target.name + "启动" + actionName + "行动。");
            context.Events.Publish(new GameEvent(GameEventType.EspionageOperationStarted, operation.id, operation));
            return new EspionageResult { success = true, reason = "operation_started", operationId = operation.id };
        }

        private void ProcessActiveOperations(GameContext context)
        {
            for (int i = context.State.activeOperations.Count - 1; i >= 0; i--)
            {
                EspionageOperation op = context.State.activeOperations[i];
                FactionState agent = context.State.FindFaction(op.agentFactionId);
                FactionState target = context.State.FindFaction(op.targetFactionId);

                if (agent == null || target == null)
                {
                    context.State.activeOperations.RemoveAt(i);
                    continue;
                }

                NumericContext numericContext = NumericModifierFactory.ForFaction(agent);
                int progressGain = NumericFormulas.CalculateEspionageProgress(op, numericContext);

                op.progress += progressGain;

                int adjustedDetectionRisk = Mathf.Max(0, op.detectionRisk - GetEmperorEspionageBonus(context, agent, "stealth"));
                int defenseBonus = target.espionageDefense * 2;
                adjustedDetectionRisk = Mathf.Max(0, adjustedDetectionRisk - defenseBonus);

                if (Random.Range(0, 100) < adjustedDetectionRisk)
                {
                    OnAgentCaught(context, op, agent, target);
                    context.State.activeOperations.RemoveAt(i);
                    continue;
                }

                if (op.progress >= 100)
                {
                    OnOperationCompleted(context, op, agent, target);
                    context.State.activeOperations.RemoveAt(i);
                }
            }
        }

        private void OnOperationCompleted(GameContext context, EspionageOperation op, FactionState agent, FactionState target)
        {
            string actionName = GetActionName(op.actionType);
            context.State.AddLog("espionage", agent.name + "对" + target.name + "的" + actionName + "行动成功！");

            switch (op.actionType)
            {
                case EspionageActionType.ScoutIntel:
                    ApplyScoutIntel(context, agent, target);
                    break;
                case EspionageActionType.Sabotage:
                    ApplySabotage(context, agent, target);
                    break;
                case EspionageActionType.SpreadRumors:
                    ApplySpreadRumors(context, agent, target);
                    break;
                case EspionageActionType.Assassinate:
                    ApplyAssassination(context, agent, target, op.targetEntityId);
                    break;
            }

            context.Events.Publish(new GameEvent(GameEventType.EspionageOperationCompleted, op.id, op));
        }

        private void ApplyScoutIntel(GameContext context, FactionState agent, FactionState target)
        {
            int totalSoldiers = 0;
            for (int i = 0; i < context.State.armies.Count; i++)
            {
                if (context.State.armies[i].ownerFactionId == target.id)
                {
                    totalSoldiers += context.State.armies[i].soldiers;
                }
            }

            context.State.AddLog("espionage", "[情报] " + target.name + "：金钱" + target.money +
                "，粮食" + target.food + "，兵力" + totalSoldiers +
                "，合法性" + target.legitimacy + "，领地" + target.regionIds.Count + "处。");
        }

        private void ApplySabotage(GameContext context, FactionState agent, FactionState target)
        {
            target.legitimacy = Mathf.Max(0, target.legitimacy - 8);
            target.courtFactionPressure = Mathf.Min(100, target.courtFactionPressure + 10);
            context.State.AddLog("espionage", agent.name + "对" + target.name + "实施离间计，朝堂动荡。");
        }

        private void ApplySpreadRumors(GameContext context, FactionState agent, FactionState target)
        {
            int regionsAffected = 0;
            for (int i = 0; i < target.regionIds.Count; i++)
            {
                RegionState region = context.State.FindRegion(target.regionIds[i]);
                if (region != null)
                {
                    region.rebellionRisk = Mathf.Min(100, region.rebellionRisk + 5);
                    regionsAffected++;
                }
            }

            target.legitimacy = Mathf.Max(0, target.legitimacy - 5);
            context.State.AddLog("espionage", agent.name + "在" + target.name + "境内散布谣言，" + regionsAffected + "处领地民心不稳。");
        }

        private void ApplyAssassination(GameContext context, FactionState agent, FactionState target, string targetEntityId)
        {
            if (!string.IsNullOrEmpty(targetEntityId) && target.heir != null)
            {
                target.heir = null;
                target.successionRisk = Mathf.Min(100, target.successionRisk + 20);
                context.State.AddLog("espionage", agent.name + "暗杀了" + target.name + "的继承人！继承危机加剧。");
            }
            else
            {
                target.legitimacy = Mathf.Max(0, target.legitimacy - 10);
                target.courtFactionPressure = Mathf.Min(100, target.courtFactionPressure + 15);
                context.State.AddLog("espionage", agent.name + "对" + target.name + "实施暗杀行动，朝堂人心惶惶。");
            }
        }

        private void OnAgentCaught(GameContext context, EspionageOperation op, FactionState agent, FactionState target)
        {
            string actionName = GetActionName(op.actionType);

            DiplomaticRelation relation = FindRelationFromState(context, agent.id, target.id);
            if (relation != null)
            {
                relation.opinion = Mathf.Max(NumericTuning.MinOpinion, relation.opinion - 25);
                relation.grudge += 20;
            }

            agent.legitimacy = Mathf.Max(0, agent.legitimacy - 5);

            if (op.actionType == EspionageActionType.Assassinate)
            {
                if (relation != null)
                {
                    relation.status = DiplomacyStatus.AtWar;
                    relation.turnsRemaining = -1;
                    relation.grudge += 40;
                }
                context.State.AddLog("espionage", agent.name + "暗杀" + target.name + "的间谍被抓！" + target.name + "直接宣战！");
            }
            else
            {
                context.State.AddLog("espionage", agent.name + "对" + target.name + "的" + actionName + "行动被发现！关系恶化。");
            }

            context.Events.Publish(new GameEvent(GameEventType.EspionageAgentCaught, op.id, op));
        }

        private DiplomaticRelation FindRelationFromState(GameContext context, string factionA, string factionB)
        {
            for (int i = 0; i < context.State.diplomaticRelations.Count; i++)
            {
                DiplomaticRelation r = context.State.diplomaticRelations[i];
                if ((r.factionA == factionA && r.factionB == factionB) ||
                    (r.factionA == factionB && r.factionB == factionA))
                {
                    return r;
                }
            }
            return null;
        }

        public void InvestInCounterEspionage(GameContext context, string factionId, int amount)
        {
            FactionState faction = context.State.FindFaction(factionId);
            if (faction == null || faction.money < amount) return;

            faction.money -= amount;
            int defenseIncrease = amount / 20;
            faction.espionageDefense = Mathf.Min(100, faction.espionageDefense + defenseIncrease);
            context.State.AddLog("espionage", faction.name + "投入" + amount + "金加强反间谍防御，防御等级提升至" + faction.espionageDefense + "。");
        }

        private int GetEmperorEspionageBonus(GameContext context, FactionState faction, string bonusType)
        {
            EmperorDefinition emperor = context.Data.GetEmperor(faction.emperorId);
            if (emperor == null) return 0;

            int wisdom = emperor.score.wisdom;
            int diplomacy = emperor.stats.diplomacy;

            switch (bonusType)
            {
                case "progress":
                    return wisdom / 20;
                case "stealth":
                    return diplomacy / 15;
                default:
                    return 0;
            }
        }

        private int CalculateDetectionRisk(GameContext context, FactionState agent, FactionState target, EspionageActionType actionType, NumericContext numericContext)
        {
            return NumericFormulas.CalculateEspionageDetectionRisk(agent, target, numericContext);
        }

        private int CalculateEspionageCost(EspionageActionType actionType, NumericContext numericContext)
        {
            string actionName;
            switch (actionType)
            {
                case EspionageActionType.ScoutIntel: actionName = "scout_intel"; break;
                case EspionageActionType.Sabotage: actionName = "sabotage"; break;
                case EspionageActionType.SpreadRumors: actionName = "spread_rumors"; break;
                case EspionageActionType.Assassinate: actionName = "assassinate"; break;
                default: return 0;
            }
            return NumericFormulas.CalculateEspionageCost(actionName, numericContext);
        }

        private string GetActionName(EspionageActionType actionType)
        {
            switch (actionType)
            {
                case EspionageActionType.ScoutIntel: return "刺探情报";
                case EspionageActionType.Sabotage: return "离间策反";
                case EspionageActionType.SpreadRumors: return "散布谣言";
                case EspionageActionType.Assassinate: return "暗杀";
                default: return "未知";
            }
        }
    }

    public sealed class EspionageResult
    {
        public bool success;
        public string reason;
        public string operationId;
    }
}
