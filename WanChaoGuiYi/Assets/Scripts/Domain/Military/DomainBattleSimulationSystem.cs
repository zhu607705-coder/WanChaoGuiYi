using System.Collections.Generic;

namespace WanChaoGuiYi
{
    public sealed class DomainBattleSimulationSystem
    {
        public BattleResult ResolveEngagement(GameContext context, MapState mapState, string engagementId)
        {
            if (context == null || mapState == null || string.IsNullOrEmpty(engagementId)) return null;

            EngagementRuntimeState engagement;
            if (!mapState.EngagementsById.TryGetValue(engagementId, out engagement)) return null;
            if (engagement.phase == EngagementPhase.Resolved && engagement.result != null) return engagement.result;

            engagement.phase = EngagementPhase.Resolving;

            int attackerPower = CalculateSidePower(context, mapState, engagement.attackerArmyIds, true);
            int defenderPower = CalculateSidePower(context, mapState, engagement.defenderArmyIds, false);
            bool attackerWon = attackerPower >= defenderPower;

            BattleResult result = new BattleResult
            {
                attackerArmyId = JoinArmyIds(engagement.attackerArmyIds),
                defenderArmyId = JoinArmyIds(engagement.defenderArmyIds),
                attackerPower = attackerPower,
                defenderPower = defenderPower,
                attackerWon = attackerWon
            };

            ApplySideLosses(context, mapState, engagement.attackerArmyIds, attackerWon);
            ApplySideLosses(context, mapState, engagement.defenderArmyIds, !attackerWon);

            engagement.result = result;
            engagement.phase = EngagementPhase.Resolved;

            context.State.AddLog("war", engagement.regionId + "战斗结束：" + (attackerWon ? "进攻方获胜" : "防守方获胜") + "。原因：自动结算比较双方兵力、士气和攻防修正。影响：胜负会驱动占领、防守、撤退或溃散结果。");
            context.Events.Publish(new GameEvent(GameEventType.BattleResolved, engagement.id, result));
            return result;
        }

        public void ResolveAllReadyEngagements(GameContext context, MapState mapState)
        {
            if (mapState == null) return;

            List<string> engagementIds = new List<string>(mapState.EngagementsById.Keys);
            for (int i = 0; i < engagementIds.Count; i++)
            {
                ResolveEngagement(context, mapState, engagementIds[i]);
            }
        }

        private static int CalculateSidePower(GameContext context, MapState mapState, List<string> armyIds, bool attacking)
        {
            int totalPower = 0;
            for (int i = 0; i < armyIds.Count; i++)
            {
                ArmyRuntimeState runtimeArmy;
                if (!mapState.TryGetArmy(armyIds[i], out runtimeArmy)) continue;

                UnitDefinition unit;
                if (!context.Data.Units.TryGetValue(runtimeArmy.unitId, out unit)) continue;

                FactionState faction = context.State.FindFaction(runtimeArmy.ownerFactionId);
                ArmyState legacyArmy = FindLegacyArmy(context, runtimeArmy.id);
                if (legacyArmy == null) continue;

                EquipmentBonus equipmentBonus = GetEquipmentBonus(legacyArmy);
                totalPower += NumericFormulas.CalculateBattlePower(legacyArmy, unit, faction, equipmentBonus, attacking);
            }

            return totalPower;
        }

        private static void ApplySideLosses(GameContext context, MapState mapState, List<string> armyIds, bool won)
        {
            for (int i = 0; i < armyIds.Count; i++)
            {
                ArmyRuntimeState runtimeArmy;
                if (!mapState.TryGetArmy(armyIds[i], out runtimeArmy)) continue;

                if (won)
                {
                    runtimeArmy.soldiers = DomainMath.RoundToInt(runtimeArmy.soldiers * 0.85f);
                    runtimeArmy.morale = DomainMath.Min(100, runtimeArmy.morale + 5);
                }
                else
                {
                    runtimeArmy.soldiers = DomainMath.RoundToInt(runtimeArmy.soldiers * 0.45f);
                    runtimeArmy.morale = DomainMath.Max(0, runtimeArmy.morale - 15);
                }

                ArmyState legacyArmy = FindLegacyArmy(context, runtimeArmy.id);
                if (legacyArmy != null)
                {
                    legacyArmy.soldiers = runtimeArmy.soldiers;
                    legacyArmy.morale = runtimeArmy.morale;
                }
            }
        }

        private static ArmyState FindLegacyArmy(GameContext context, string armyId)
        {
            for (int i = 0; i < context.State.armies.Count; i++)
            {
                if (context.State.armies[i].id == armyId) return context.State.armies[i];
            }

            return null;
        }

        private static string JoinArmyIds(List<string> armyIds)
        {
            if (armyIds == null || armyIds.Count == 0) return string.Empty;
            return string.Join(",", armyIds.ToArray());
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
    }
}
