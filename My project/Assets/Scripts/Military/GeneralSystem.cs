using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class GeneralSystem : MonoBehaviour, IGameSystem
    {
        private static readonly Dictionary<string, GeneralDefinition> GeneralsDB = new Dictionary<string, GeneralDefinition>();

        public void Initialize(GameContext context)
        {
            if (context.Data.Generals != null)
            {
                GeneralsDB.Clear();
                foreach (var kvp in context.Data.Generals)
                {
                    GeneralsDB[kvp.Key] = kvp.Value;
                }
            }
        }

        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            // 将领系统每回合检查将领忠诚度变化
            // 将领数据存储在 FactionState.talentIds 中（复用人才系统）
        }

        public static int GetTerrainBonus(string generalId, string terrain)
        {
            GeneralDefinition general;
            if (!GeneralsDB.TryGetValue(generalId, out general)) return 0;

            if (general.terrainBonus == null) return 0;

            switch (terrain)
            {
                case "river_plain": return general.terrainBonus.river_plain;
                case "river_delta": return general.terrainBonus.river_delta;
                case "mountain": return general.terrainBonus.mountain;
                case "mountain_pass": return general.terrainBonus.mountain_pass;
                case "open_plain": return general.terrainBonus.open_plain;
                case "frontier_plain": return general.terrainBonus.frontier_plain;
                case "steppe_edge": return general.terrainBonus.steppe_edge;
                case "huai_river_plain": return general.terrainBonus.huai_river_plain;
                case "mountain_coast": return general.terrainBonus.mountain_coast;
                case "subtropical": return general.terrainBonus.subtropical;
                default: return 0;
            }
        }

        public static int GetUnitBonus(string generalId, string unitId)
        {
            GeneralDefinition general;
            if (!GeneralsDB.TryGetValue(generalId, out general)) return 0;

            if (general.unitBonus == null) return 0;

            switch (unitId)
            {
                case "infantry": return general.unitBonus.infantry;
                case "cavalry": return general.unitBonus.cavalry;
                case "crossbowmen": return general.unitBonus.crossbowmen;
                case "siege_engineer": return general.unitBonus.siege_engineer;
                case "frontier_cavalry": return general.unitBonus.frontier_cavalry;
                case "garrison": return general.unitBonus.garrison;
                case "river_navy": return general.unitBonus.river_navy;
                case "fire_lance_guard": return general.unitBonus.fire_lance_guard;
                default: return 0;
            }
        }

        public static GeneralDefinition GetGeneral(string id)
        {
            GeneralDefinition general;
            GeneralsDB.TryGetValue(id, out general);
            return general;
        }
    }
}
