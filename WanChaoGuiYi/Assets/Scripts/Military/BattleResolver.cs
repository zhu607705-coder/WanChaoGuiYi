using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class BattleResolver : MonoBehaviour
    {
        [SerializeField] private BattleSetupSystem setupSystem;
        [SerializeField] private bool allowLegacyOwnershipChange = false;

        public BattleResult Resolve(GameContext context, ArmyState attacker, ArmyState defender)
        {
            UnitDefinition attackerUnit = context.Data.GetUnit(attacker.unitId);
            UnitDefinition defenderUnit = context.Data.GetUnit(defender.unitId);
            FactionState attackerFaction = context.State.FindFaction(attacker.ownerFactionId);
            FactionState defenderFaction = context.State.FindFaction(defender.ownerFactionId);

            int attackerPower = CalculatePower(attacker, attackerUnit, attackerFaction, true);
            int defenderPower = CalculatePower(defender, defenderUnit, defenderFaction, false);
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
            if (attackerWon && allowLegacyOwnershipChange)
            {
                context.ChangeRegionOwner(defender.regionId, attacker.ownerFactionId);
            }
            else if (attackerWon)
            {
                context.State.AddLog("war", "旧战斗结算未改变地区归属；地图主导战争需通过 OccupationSystem 占领。");
            }

            context.Events.Publish(new GameEvent(GameEventType.BattleResolved, attacker.id, result));
            return result;
        }

        public BattleResult ResolveSemiAuto(GameContext context, BattleSetup setup, List<BattleLog> battleLog)
        {
            if (setup == null) return null;

            FactionState attackerFaction = context.State.FindFaction(setup.attackerFactionId);
            FactionState defenderFaction = context.State.FindFaction(setup.defenderFactionId);
            if (attackerFaction == null || defenderFaction == null) return null;

            int attackerPower = CalculateFactionPower(context, attackerFaction, setup.attackerDeployments, true);
            int defenderPower = CalculateFactionPower(context, defenderFaction, setup.defenderDeployments, false);

            int formationBonus = CalculateFormationBonus(setup.attackerFormationId, setup.defenderFormationId);
            attackerPower += formationBonus;

            int tacticBonus = CalculateTacticBonus(battleLog);
            attackerPower += tacticBonus;

            int terrainBonus = CalculateTerrainBonus(context, setup.terrainId);
            attackerPower += terrainBonus;

            int generalBonus = CalculateGeneralBonus(context, setup.attackerDeployments, setup.defenderDeployments);
            attackerPower += generalBonus;

            bool attackerWon = attackerPower >= defenderPower;

            BattleResult result = new BattleResult
            {
                attackerArmyId = setup.attackerFactionId,
                defenderArmyId = setup.defenderFactionId,
                attackerPower = attackerPower,
                defenderPower = defenderPower,
                attackerWon = attackerWon,
                battleLog = battleLog,
                formationBonus = formationBonus,
                tacticBonus = tacticBonus,
                terrainBonus = terrainBonus,
                generalBonus = generalBonus
            };

            ApplySemiAutoLosses(context, setup, attackerWon);
            context.Events.Publish(new GameEvent(GameEventType.BattleResolved, setup.attackerFactionId, result));
            return result;
        }

        private static ArmyState FindArmy(GameContext context, string armyId)
        {
            for (int j = 0; j < context.State.armies.Count; j++)
            {
                if (context.State.armies[j].id == armyId)
                {
                    return context.State.armies[j];
                }
            }
            return null;
        }

        private int CalculateFactionPower(GameContext context, FactionState faction, BattleDeployment[] deployments, bool attacking)
        {
            int totalPower = 0;
            for (int i = 0; i < deployments.Length; i++)
            {
                ArmyState army = FindArmy(context, deployments[i].armyId);
                if (army == null) continue;

                UnitDefinition unit = context.Data.GetUnit(army.unitId);
                EquipmentBonus equipBonus = GetEquipmentBonus(army);
                int power = NumericFormulas.CalculateBattlePower(army, unit, faction, equipBonus, attacking);
                totalPower += power;
            }
            return totalPower;
        }

        private static int CalculatePower(ArmyState army, UnitDefinition unit, FactionState faction, bool attacking)
        {
            EquipmentBonus equipBonus = GetEquipmentBonus(army);
            return NumericFormulas.CalculateBattlePower(army, unit, faction, equipBonus, attacking);
        }

        private int CalculateFormationBonus(string attackerFormationId, string defenderFormationId)
        {
            if (setupSystem == null) return 0;
            return setupSystem.CalculateFormationBonus(attackerFormationId, defenderFormationId);
        }

        private int CalculateTacticBonus(List<BattleLog> battleLog)
        {
            if (battleLog == null) return 0;
            int bonus = 0;
            for (int i = 0; i < battleLog.Count; i++)
            {
                bonus += battleLog[i].damageDealt;
            }
            return bonus;
        }

        private int CalculateTerrainBonus(GameContext context, string terrainId)
        {
            if (string.IsNullOrEmpty(terrainId)) return 0;
            RegionDefinition region = context.Data.GetRegion(terrainId);
            if (region == null) return 0;

            switch (region.terrain)
            {
                case "mountain": return NumericTuning.TerrainMountainAttackPenalty;
                case "plain": return NumericTuning.TerrainPlainAttackBonus;
                case "river": return NumericTuning.TerrainRiverAttackPenalty;
                default: return 0;
            }
        }

        private int CalculateGeneralBonus(GameContext context, BattleDeployment[] attackerDeployments, BattleDeployment[] defenderDeployments)
        {
            int attackerBonus = 0;
            int defenderBonus = 0;

            for (int i = 0; i < attackerDeployments.Length; i++)
            {
                if (string.IsNullOrEmpty(attackerDeployments[i].generalId)) continue;
                foreach (var kvp in context.Data.Generals)
                {
                    if (kvp.Key == attackerDeployments[i].generalId)
                    {
                        attackerBonus += kvp.Value.military / NumericTuning.GeneralMilitaryDivisor;
                        break;
                    }
                }
            }

            for (int i = 0; i < defenderDeployments.Length; i++)
            {
                if (string.IsNullOrEmpty(defenderDeployments[i].generalId)) continue;
                foreach (var kvp in context.Data.Generals)
                {
                    if (kvp.Key == defenderDeployments[i].generalId)
                    {
                        defenderBonus += kvp.Value.military / NumericTuning.GeneralMilitaryDivisor;
                        break;
                    }
                }
            }

            return attackerBonus - defenderBonus;
        }

        private void ApplySemiAutoLosses(GameContext context, BattleSetup setup, bool attackerWon)
        {
            for (int i = 0; i < setup.attackerDeployments.Length; i++)
            {
                ArmyState army = FindArmy(context, setup.attackerDeployments[i].armyId);
                if (army == null) continue;

                if (attackerWon)
                {
                    army.soldiers = Mathf.RoundToInt(army.soldiers * 0.85f);
                    army.morale = Mathf.Min(100, army.morale + 5);
                }
                else
                {
                    army.soldiers = Mathf.RoundToInt(army.soldiers * 0.45f);
                    army.morale = Mathf.Max(0, army.morale - 15);
                }
            }

            for (int i = 0; i < setup.defenderDeployments.Length; i++)
            {
                ArmyState army = FindArmy(context, setup.defenderDeployments[i].armyId);
                if (army == null) continue;

                if (attackerWon)
                {
                    army.soldiers = Mathf.RoundToInt(army.soldiers * 0.35f);
                    army.morale = Mathf.Max(0, army.morale - 15);
                }
                else
                {
                    army.soldiers = Mathf.RoundToInt(army.soldiers * 0.9f);
                    army.morale = Mathf.Min(100, army.morale + 5);
                }
            }
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
