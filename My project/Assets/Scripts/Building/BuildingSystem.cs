using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class BuildingSystem : MonoBehaviour, IGameSystem
    {
        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            // 建筑效果每回合结算
            for (int i = 0; i < context.State.regions.Count; i++)
            {
                RegionState region = context.State.regions[i];
                if (region.buildings == null) continue;

                for (int j = 0; j < region.buildings.Count; j++)
                {
                    ApplyBuildingEffect(context, region, region.buildings[j]);
                }
            }
        }

        public bool CanBuild(GameContext context, FactionState faction, RegionState region, string buildingId)
        {
            BuildingDefinition building;
            if (!context.Data.Buildings.TryGetValue(buildingId, out building)) return false;

            // 检查科技前置
            if (!string.IsNullOrEmpty(building.requiresTech) && !faction.completedTechIds.Contains(building.requiresTech))
            {
                return false;
            }

            // 检查金钱
            if (faction.money < building.cost) return false;

            // 检查槽位（每个区域最多 3 个建筑）
            if (region.buildings != null && region.buildings.Count >= 3) return false;

            // 检查是否已建造
            if (region.buildings != null && region.buildings.Contains(buildingId)) return false;

            return true;
        }

        public bool Build(GameContext context, FactionState faction, RegionState region, string buildingId)
        {
            if (!CanBuild(context, faction, region, buildingId)) return false;

            BuildingDefinition building;
            if (!context.Data.Buildings.TryGetValue(buildingId, out building)) return false;

            // 扣除费用
            faction.money -= building.cost;

            // 添加建筑
            if (region.buildings == null)
            {
                region.buildings = new List<string>();
            }

            region.buildings.Add(buildingId);

            context.State.AddLog("building", faction.name + "在" + region.id + "建造了" + building.name);
            return true;
        }

        private static void ApplyBuildingEffect(GameContext context, RegionState region, string buildingId)
        {
            BuildingDefinition building;
            if (!context.Data.Buildings.TryGetValue(buildingId, out building)) return;
            if (building.effects == null) return;

            // 建筑效果已通过 RegionState 的修正值体现
            // 这里只处理需要每回合结算的效果
        }
    }
}
