using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class TechSystem : MonoBehaviour, IGameSystem
    {
        // 常量已迁移到 NumericTuning

        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            for (int i = 0; i < context.State.factions.Count; i++)
            {
                FactionState faction = context.State.factions[i];
                ProcessResearch(context, faction);
            }
        }

        private void ProcessResearch(GameContext context, FactionState faction)
        {
            // 如果没有选择研究目标，自动选择一个
            if (string.IsNullOrEmpty(faction.currentResearchId))
            {
                faction.currentResearchId = ChooseAutoResearch(context, faction);
                if (string.IsNullOrEmpty(faction.currentResearchId)) return;
            }

            TechnologyDefinition tech;
            if (!context.Data.Technologies.TryGetValue(faction.currentResearchId, out tech))
            {
                faction.currentResearchId = null;
                return;
            }

            // 检查前置条件
            if (!HasPrerequisites(faction, tech))
            {
                faction.currentResearchId = null;
                return;
            }

            // 计算研究点数
            int researchPoints = CalculateResearchPoints(context, faction, tech);
            faction.researchPoints += researchPoints;

            // 检查 Boost（启发）
            int boostBonus = CheckBoost(context, faction, tech);

            // 检查是否研究完成（使用统一公式计算科技成本）
            NumericContext costContext = NumericModifierFactory.ForFaction(faction);
            int baseCost = NumericFormulas.CalculateTechCost(tech, costContext);
            int effectiveCost = Mathf.Max(1, baseCost - boostBonus);
            if (faction.researchPoints >= effectiveCost)
            {
                CompleteTech(context, faction, tech);
            }
        }

        private int CalculateResearchPoints(GameContext context, FactionState faction, TechnologyDefinition tech)
        {
            EmperorDefinition emperor = context.Data.GetEmperor(faction.emperorId);
            NumericContext numericContext = NumericModifierFactory.ForFaction(faction);

            // 基础研究点数通过统一公式计算
            int points = NumericFormulas.CalculateResearchPoints(faction, emperor, numericContext);

            // 地区历史层科技亲和加成
            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                HistoricalLayerDefinition layer = FindHistoricalLayer(context, faction.regionIds[i]);
                if (layer != null && layer.techAffinities != null)
                {
                    for (int j = 0; j < layer.techAffinities.Length; j++)
                    {
                        if (layer.techAffinities[j] == tech.id)
                        {
                            points += NumericTuning.TechAffinityBonus;
                            break;
                        }
                    }
                }
            }

            return Mathf.Max(1, points);
        }

        private int CheckBoost(GameContext context, FactionState faction, TechnologyDefinition tech)
        {
            if (tech.boost == null) return 0;

            // Boost 通过游戏事件触发，这里检查是否已满足条件
            // 简化实现：如果帝皇 reform > 80，给予部分 boost
            EmperorDefinition emperor = context.Data.GetEmperor(faction.emperorId);
            if (emperor != null && emperor.stats.reform > 80)
            {
                return tech.boost.progressBonus / NumericTuning.ReformBonusDivisor;
            }

            return 0;
        }

        private void CompleteTech(GameContext context, FactionState faction, TechnologyDefinition tech)
        {
            faction.completedTechIds.Add(tech.id);
            faction.researchPoints = 0;
            faction.currentResearchId = null;

            // 应用效果
            if (tech.effects != null)
            {
                faction.food += tech.effects.food;
                faction.money += tech.effects.money;
                faction.legitimacy = ClampPercent(faction.legitimacy + tech.effects.legitimacy);
                faction.weatherResilience += tech.effects.weatherResilience;
                faction.disasterMitigation += tech.effects.disasterMitigation;
            }

            context.State.AddLog("tech", faction.name + "研发完成：" + tech.name);
            context.Events.Publish(new GameEvent(GameEventType.TechResearched, faction.id, tech));
        }

        public bool HasPrerequisites(FactionState faction, TechnologyDefinition tech)
        {
            if (tech.prerequisites == null || tech.prerequisites.Length == 0) return true;

            for (int i = 0; i < tech.prerequisites.Length; i++)
            {
                if (!faction.completedTechIds.Contains(tech.prerequisites[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasTech(FactionState faction, string techId)
        {
            return faction.completedTechIds.Contains(techId);
        }

        public void SetResearch(FactionState faction, string techId)
        {
            faction.currentResearchId = techId;
            faction.researchPoints = 0;
        }

        private string ChooseAutoResearch(GameContext context, FactionState faction)
        {
            EmperorDefinition emperor = context.Data.GetEmperor(faction.emperorId);

            // 优先选择帝皇时代对应的科技
            foreach (TechnologyDefinition tech in context.Data.Technologies.Values)
            {
                if (faction.completedTechIds.Contains(tech.id)) continue;
                if (!HasPrerequisites(faction, tech)) continue;
                if (emperor != null && tech.era == emperor.era) return tech.id;
            }

            // 回退：选择任何可用科技
            foreach (TechnologyDefinition tech in context.Data.Technologies.Values)
            {
                if (faction.completedTechIds.Contains(tech.id)) continue;
                if (!HasPrerequisites(faction, tech)) continue;
                return tech.id;
            }

            return null;
        }

        private static HistoricalLayerDefinition FindHistoricalLayer(GameContext context, string regionId)
        {
            foreach (HistoricalLayerDefinition layer in context.Data.HistoricalLayers.Values)
            {
                if (layer.regionId == regionId) return layer;
            }

            return null;
        }

        private static int ClampPercent(int value)
        {
            if (value < 0) return 0;
            if (value > 100) return 100;
            return value;
        }
    }
}
