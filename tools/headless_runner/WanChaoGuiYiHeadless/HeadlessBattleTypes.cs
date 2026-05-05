using System.Collections.Generic;

namespace WanChaoGuiYi
{
    public sealed class BattleResult
    {
        public string attackerArmyId;
        public string defenderArmyId;
        public string battleRegionId;
        public int attackerPower;
        public int defenderPower;
        public int attackerSupplyPowerPercent = 100;
        public int defenderSupplyPowerPercent = 100;
        public int attackerLowestSupply = -1;
        public int defenderLowestSupply = -1;
        public int attackerSoldiersBefore;
        public int attackerSoldiersAfter;
        public int defenderSoldiersBefore;
        public int defenderSoldiersAfter;
        public bool attackerWon;
        public List<BattleLog> battleLog;
        public int formationBonus;
        public int tacticBonus;
        public int terrainBonus;
        public int generalBonus;
    }

    internal static class EquipmentLookup
    {
        private static readonly Dictionary<string, EquipmentDefinition> DB = new Dictionary<string, EquipmentDefinition>
        {
            { "bronze_spear", new EquipmentDefinition { id = "bronze_spear", name = "青铜戈", slot = "weapon", era = "classical", requiresTech = "bronze_casting", cost = 20, attackBonus = 10, defenseBonus = 0, mobilityBonus = 0, siegeBonus = 0 } },
            { "bronze_armor", new EquipmentDefinition { id = "bronze_armor", name = "皮甲", slot = "armor", era = "classical", requiresTech = "bronze_casting", cost = 15, attackBonus = 0, defenseBonus = 10, mobilityBonus = 0, siegeBonus = 0 } },
            { "war_chariot", new EquipmentDefinition { id = "war_chariot", name = "战车", slot = "special", era = "classical", requiresTech = "mounted_warfare", cost = 40, attackBonus = 8, defenseBonus = 5, mobilityBonus = 15, siegeBonus = 0 } },
            { "iron_halberd", new EquipmentDefinition { id = "iron_halberd", name = "铁戟", slot = "weapon", era = "medieval", requiresTech = "iron_smelting", cost = 35, attackBonus = 15, defenseBonus = 0, mobilityBonus = 0, siegeBonus = 0 } },
            { "fish_scale_armor", new EquipmentDefinition { id = "fish_scale_armor", name = "鱼鳞甲", slot = "armor", era = "medieval", requiresTech = "iron_smelting", cost = 30, attackBonus = 0, defenseBonus = 18, mobilityBonus = -3, siegeBonus = 0 } },
            { "stirrup", new EquipmentDefinition { id = "stirrup", name = "马镫", slot = "special", era = "medieval", requiresTech = "mounted_warfare", cost = 25, attackBonus = 12, defenseBonus = 0, mobilityBonus = 20, siegeBonus = 0 } },
            { "crossbow", new EquipmentDefinition { id = "crossbow", name = "强弩", slot = "weapon", era = "medieval", requiresTech = "crossbow_standardization", cost = 30, attackBonus = 18, defenseBonus = 0, mobilityBonus = 0, siegeBonus = 5 } },
            { "fire_lance", new EquipmentDefinition { id = "fire_lance", name = "火铳", slot = "weapon", era = "early_modern", requiresTech = "gunpowder_formula", cost = 50, attackBonus = 25, defenseBonus = 0, mobilityBonus = -5, siegeBonus = 10 } },
            { "cotton_armor", new EquipmentDefinition { id = "cotton_armor", name = "棉甲", slot = "armor", era = "early_modern", requiresTech = "gunpowder_formula", cost = 35, attackBonus = 0, defenseBonus = 15, mobilityBonus = 2, siegeBonus = 0 } },
            { "cannon", new EquipmentDefinition { id = "cannon", name = "佛郎机", slot = "special", era = "early_modern", requiresTech = "gunpowder_formula", cost = 60, attackBonus = 5, defenseBonus = 5, mobilityBonus = -10, siegeBonus = 25 } }
        };

        public static EquipmentDefinition Get(string id)
        {
            EquipmentDefinition equip;
            DB.TryGetValue(id, out equip);
            return equip;
        }
    }
}
