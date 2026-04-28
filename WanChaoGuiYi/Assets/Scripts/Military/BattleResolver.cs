using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class BattleResolver : MonoBehaviour
    {
        public BattleResult Resolve(GameContext context, ArmyState attacker, ArmyState defender)
        {
            UnitDefinition attackerUnit = context.Data.GetUnit(attacker.unitId);
            UnitDefinition defenderUnit = context.Data.GetUnit(defender.unitId);

            int attackerPower = CalculatePower(attacker, attackerUnit, true);
            int defenderPower = CalculatePower(defender, defenderUnit, false);
            bool attackerWon = attackerPower >= defenderPower;

            BattleResult result = new BattleResult
            {
                attackerArmyId = attacker.id,
                defenderArmyId = defender.id,
                attackerPower = attackerPower,
                defenderPower = defenderPower,
                attackerWon = attackerWon
            };

            ApplyLosses(attacker, defender, attackerWon);
            context.Events.Publish(new GameEvent(GameEventType.BattleResolved, attacker.id, result));
            return result;
        }

        private static int CalculatePower(ArmyState army, UnitDefinition unit, bool attacking)
        {
            int baseStat = attacking ? unit.stats.attack : unit.stats.defense;

            // 装备加成
            EquipmentBonus equipBonus = GetEquipmentBonus(army);
            if (attacking)
            {
                baseStat += equipBonus.attack;
            }
            else
            {
                baseStat += equipBonus.defense;
            }

            return Mathf.RoundToInt(baseStat * Mathf.Max(1, army.soldiers) * Mathf.Clamp01(army.morale / 100f));
        }

        private static EquipmentBonus GetEquipmentBonus(ArmyState army)
        {
            EquipmentBonus bonus = new EquipmentBonus();

            ApplyEquipmentSlot(army.weaponSlot, bonus);
            ApplyEquipmentSlot(army.armorSlot, bonus);
            ApplyEquipmentSlot(army.specialSlot, bonus);

            return bonus;
        }

        private static void ApplyEquipmentSlot(string slotId, EquipmentBonus bonus)
        {
            if (string.IsNullOrEmpty(slotId)) return;

            // 从内置装备库查询
            EquipmentDefinition equip = EquipmentLookup.Get(slotId);
            if (equip == null) return;

            bonus.attack += equip.attackBonus;
            bonus.defense += equip.defenseBonus;
            bonus.mobility += equip.mobilityBonus;
            bonus.siege += equip.siegeBonus;
        }

        private static void ApplyLosses(ArmyState attacker, ArmyState defender, bool attackerWon)
        {
            if (attackerWon)
            {
                attacker.soldiers = Mathf.RoundToInt(attacker.soldiers * 0.85f);
                defender.soldiers = Mathf.RoundToInt(defender.soldiers * 0.35f);
                attacker.morale = Mathf.Min(100, attacker.morale + 5);
                defender.morale = Mathf.Max(0, defender.morale - 15);
            }
            else
            {
                attacker.soldiers = Mathf.RoundToInt(attacker.soldiers * 0.45f);
                defender.soldiers = Mathf.RoundToInt(defender.soldiers * 0.9f);
                attacker.morale = Mathf.Max(0, attacker.morale - 15);
                defender.morale = Mathf.Min(100, defender.morale + 5);
            }
        }
    }

    public sealed class BattleResult
    {
        public string attackerArmyId;
        public string defenderArmyId;
        public int attackerPower;
        public int defenderPower;
        public bool attackerWon;
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
