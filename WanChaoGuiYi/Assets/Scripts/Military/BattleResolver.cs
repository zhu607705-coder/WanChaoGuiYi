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
            return Mathf.RoundToInt(baseStat * Mathf.Max(1, army.soldiers) * Mathf.Clamp01(army.morale / 100f));
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
}
