using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class VictorySystem : MonoBehaviour, IGameSystem
    {
        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void ExecuteTurn(GameContext context) { }

        public void OnTurnEnd(GameContext context)
        {
            for (int i = 0; i < context.State.factions.Count; i++)
            {
                FactionState faction = context.State.factions[i];
                VictoryCheckResult result = CheckVictory(context, faction);

                if (result.achieved)
                {
                    context.State.AddLog("victory", faction.name + "达成胜利：" + result.conditionName + "！");
                    context.Events.Publish(new GameEvent(GameEventType.VictoryChecked, faction.id, result));
                }
            }
        }

        public VictoryCheckResult CheckVictory(GameContext context, FactionState faction)
        {
            // 使用统一数值公式计算胜利进度
            NumericContext numericContext = NumericModifierFactory.ForFaction(faction);
            float victoryProgress = NumericFormulas.CalculateVictoryProgress(faction, numericContext);

            foreach (VictoryConditionDefinition vc in context.Data.VictoryConditions.Values)
            {
                if (CheckCondition(context, faction, vc.requirements))
                {
                    return new VictoryCheckResult
                    {
                        achieved = true,
                        conditionId = vc.id,
                        conditionName = vc.name,
                        factionId = faction.id,
                        progress = victoryProgress
                    };
                }
            }

            return new VictoryCheckResult { achieved = false, progress = victoryProgress };
        }

        private static bool CheckCondition(GameContext context, FactionState faction, VictoryRequirement req)
        {
            if (req == null) return false;

            // 统一九州：控制关键区域
            if (req.controlAllKeyRegions)
            {
                if (!ControlsAllRegions(context, faction)) return false;
            }

            // 最低合法性
            if (faction.legitimacy < req.minLegitimacy) return false;

            // 平稳继承次数
            if (faction.stableSuccessions < req.stableSuccessions) return false;

            // 最大兼并压力
            if (req.maxAnnexationPressure > 0)
            {
                float avgPressure = CalculateAverageAnnexationPressure(context, faction);
                if (avgPressure > req.maxAnnexationPressure) return false;
            }

            // 最小财政稳定
            if (req.minTreasuryStability > 0)
            {
                if (faction.money < req.minTreasuryStability) return false;
            }

            // 完成核心改革
            if (req.completedCoreReforms > 0)
            {
                if (faction.completedReformIds.Count < req.completedCoreReforms) return false;
            }

            return true;
        }

        private static bool ControlsAllRegions(GameContext context, FactionState faction)
        {
            // 简化：检查是否控制超过 80% 的区域
            int totalRegions = context.State.regions.Count;
            int threshold = Mathf.CeilToInt(totalRegions * 0.8f);
            return faction.regionIds.Count >= threshold;
        }

        private static float CalculateAverageAnnexationPressure(GameContext context, FactionState faction)
        {
            if (faction.regionIds.Count == 0) return 100f;

            int total = 0;
            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = context.State.FindRegion(faction.regionIds[i]);
                if (region != null)
                {
                    total += region.annexationPressure;
                }
            }

            return (float)total / faction.regionIds.Count;
        }
    }

    public sealed class VictoryCheckResult
    {
        public bool achieved;
        public string conditionId;
        public string conditionName;
        public string factionId;
        public float progress;
    }
}
