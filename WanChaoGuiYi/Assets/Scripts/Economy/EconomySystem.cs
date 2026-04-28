using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class EconomySystem : MonoBehaviour, IGameSystem
    {
        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            for (int i = 0; i < context.State.factions.Count; i++)
            {
                FactionState faction = context.State.factions[i];
                int tax = 0;
                int food = 0;

                for (int j = 0; j < faction.regionIds.Count; j++)
                {
                    RegionState region = context.State.FindRegion(faction.regionIds[j]);
                    if (region == null) continue;

                    tax += TaxSystem.CalculateRegionalTax(region);
                    food += region.foodOutput;
                }

                int upkeep = CalculateArmyUpkeep(context, faction.id);
                faction.money += tax - upkeep;
                faction.food += food - upkeep;

                if (faction.money < 0)
                {
                    faction.legitimacy -= 2;
                    faction.courtFactionPressure += 3;
                }

                context.State.AddLog("economy", faction.name + "收入 金钱+" + tax + " 粮食+" + food + " 维护-" + upkeep);
            }
        }

        private static int CalculateArmyUpkeep(GameContext context, string factionId)
        {
            int upkeep = 0;
            for (int i = 0; i < context.State.armies.Count; i++)
            {
                ArmyState army = context.State.armies[i];
                if (army.ownerFactionId != factionId) continue;
                UnitDefinition unit = context.Data.GetUnit(army.unitId);
                if (unit.upkeep != null)
                {
                    upkeep += Mathf.CeilToInt(army.soldiers / 1000f) * unit.upkeep.money;
                }
            }

            return upkeep;
        }
    }
}
