using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class EquipmentSystem : MonoBehaviour, IGameSystem
    {
        private static readonly Dictionary<string, EquipmentDefinition> EquipmentDB = new Dictionary<string, EquipmentDefinition>
        {
            // 先秦
            { "bronze_spear", new EquipmentDefinition { id = "bronze_spear", name = "青铜戈", slot = "weapon", era = "classical", requiresTech = "bronze_casting", cost = 20, attackBonus = 10, defenseBonus = 0, mobilityBonus = 0, siegeBonus = 0 } },
            { "bronze_armor", new EquipmentDefinition { id = "bronze_armor", name = "皮甲", slot = "armor", era = "classical", requiresTech = "bronze_casting", cost = 15, attackBonus = 0, defenseBonus = 10, mobilityBonus = 0, siegeBonus = 0 } },
            { "war_chariot", new EquipmentDefinition { id = "war_chariot", name = "战车", slot = "special", era = "classical", requiresTech = "mounted_warfare", cost = 40, attackBonus = 8, defenseBonus = 5, mobilityBonus = 15, siegeBonus = 0 } },
            // 中古
            { "iron_halberd", new EquipmentDefinition { id = "iron_halberd", name = "铁戟", slot = "weapon", era = "medieval", requiresTech = "iron_smelting", cost = 35, attackBonus = 15, defenseBonus = 0, mobilityBonus = 0, siegeBonus = 0 } },
            { "fish_scale_armor", new EquipmentDefinition { id = "fish_scale_armor", name = "鱼鳞甲", slot = "armor", era = "medieval", requiresTech = "iron_smelting", cost = 30, attackBonus = 0, defenseBonus = 18, mobilityBonus = -3, siegeBonus = 0 } },
            { "stirrup", new EquipmentDefinition { id = "stirrup", name = "马镫", slot = "special", era = "medieval", requiresTech = "mounted_warfare", cost = 25, attackBonus = 12, defenseBonus = 0, mobilityBonus = 20, siegeBonus = 0 } },
            { "crossbow", new EquipmentDefinition { id = "crossbow", name = "强弩", slot = "weapon", era = "medieval", requiresTech = "crossbow_standardization", cost = 30, attackBonus = 18, defenseBonus = 0, mobilityBonus = 0, siegeBonus = 5 } },
            // 近世
            { "fire_lance", new EquipmentDefinition { id = "fire_lance", name = "火铳", slot = "weapon", era = "early_modern", requiresTech = "gunpowder_formula", cost = 50, attackBonus = 25, defenseBonus = 0, mobilityBonus = -5, siegeBonus = 10 } },
            { "cotton_armor", new EquipmentDefinition { id = "cotton_armor", name = "棉甲", slot = "armor", era = "early_modern", requiresTech = "gunpowder_formula", cost = 35, attackBonus = 0, defenseBonus = 15, mobilityBonus = 2, siegeBonus = 0 } },
            { "cannon", new EquipmentDefinition { id = "cannon", name = "佛郎机", slot = "special", era = "early_modern", requiresTech = "gunpowder_formula", cost = 60, attackBonus = 5, defenseBonus = 5, mobilityBonus = -10, siegeBonus = 25 } }
        };

        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            // 每回合检查是否有新装备可用（科技解锁后）
            for (int i = 0; i < context.State.factions.Count; i++)
            {
                FactionState faction = context.State.factions[i];
                CheckNewEquipment(context, faction);
            }
        }

        private void CheckNewEquipment(GameContext context, FactionState faction)
        {
            foreach (EquipmentDefinition equip in EquipmentDB.Values)
            {
                if (!string.IsNullOrEmpty(equip.requiresTech) && !faction.completedTechIds.Contains(equip.requiresTech))
                {
                    continue;
                }

                // 装备可用，发布事件
                context.Events.Publish(new GameEvent(GameEventType.EquipmentUnlocked, faction.id, equip));
            }
        }

        public bool EquipArmy(ArmyState army, string equipmentId)
        {
            EquipmentDefinition equip;
            if (!EquipmentDB.TryGetValue(equipmentId, out equip)) return false;

            switch (equip.slot)
            {
                case "weapon":
                    army.weaponSlot = equipmentId;
                    break;
                case "armor":
                    army.armorSlot = equipmentId;
                    break;
                case "special":
                    army.specialSlot = equipmentId;
                    break;
                default:
                    return false;
            }

            return true;
        }

        public EquipmentBonus GetArmyEquipmentBonus(ArmyState army)
        {
            EquipmentBonus bonus = new EquipmentBonus();

            ApplySlotBonus(army.weaponSlot, bonus);
            ApplySlotBonus(army.armorSlot, bonus);
            ApplySlotBonus(army.specialSlot, bonus);

            return bonus;
        }

        private void ApplySlotBonus(string slotId, EquipmentBonus bonus)
        {
            if (string.IsNullOrEmpty(slotId)) return;

            EquipmentDefinition equip;
            if (!EquipmentDB.TryGetValue(slotId, out equip)) return;

            bonus.attack += equip.attackBonus;
            bonus.defense += equip.defenseBonus;
            bonus.mobility += equip.mobilityBonus;
            bonus.siege += equip.siegeBonus;
        }

        public List<EquipmentDefinition> GetAvailableEquipment(FactionState faction)
        {
            List<EquipmentDefinition> available = new List<EquipmentDefinition>();

            foreach (EquipmentDefinition equip in EquipmentDB.Values)
            {
                if (string.IsNullOrEmpty(equip.requiresTech) || faction.completedTechIds.Contains(equip.requiresTech))
                {
                    available.Add(equip);
                }
            }

            return available;
        }

        public EquipmentDefinition GetEquipment(string id)
        {
            EquipmentDefinition equip;
            EquipmentDB.TryGetValue(id, out equip);
            return equip;
        }
    }

    public sealed class EquipmentDefinition
    {
        public string id;
        public string name;
        public string slot;
        public string era;
        public string requiresTech;
        public int cost;
        public int attackBonus;
        public int defenseBonus;
        public int mobilityBonus;
        public int siegeBonus;
    }

    public sealed class EquipmentBonus
    {
        public int attack;
        public int defense;
        public int mobility;
        public int siege;
    }
}
