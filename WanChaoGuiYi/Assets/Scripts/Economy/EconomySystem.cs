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
                NumericContext numericContext = NumericModifierFactory.ForFaction(faction);
                int tax = 0;
                int food = 0;

                for (int j = 0; j < faction.regionIds.Count; j++)
                {
                    RegionState region = context.State.FindRegion(faction.regionIds[j]);
                    if (region == null) continue;

                    tax += NumericFormulas.CalculateRegionalTax(region, numericContext);
                    food += NumericFormulas.CalculateRegionalFood(region, numericContext);
                }

                int moneyUpkeep = CalculateArmyMoneyUpkeep(context, faction, numericContext) + NumericFormulas.CalculateGovernanceUpkeep(numericContext, false) + NumericFormulas.CalculateTreasuryReserveDrag(faction, false);
                int foodUpkeep = CalculateArmyFoodUpkeep(context, faction, numericContext) + NumericFormulas.CalculateGovernanceUpkeep(numericContext, true) + NumericFormulas.CalculateTreasuryReserveDrag(faction, true);
                faction.money += tax - moneyUpkeep;
                faction.food += food - foodUpkeep;

                if (faction.money < 0)
                {
                    faction.legitimacy = Mathf.Max(0, faction.legitimacy - NumericFormulas.CalculateFiscalLegitimacyPenalty(numericContext));
                    faction.courtFactionPressure = Mathf.Min(100, faction.courtFactionPressure + NumericFormulas.CalculateFiscalCourtPressure(numericContext));
                }

                int successionPressure = NumericFormulas.CalculateExpansionSuccessionPressure(numericContext);
                if (successionPressure > 0)
                {
                    faction.successionRisk = Mathf.Min(100, faction.successionRisk + successionPressure);
                }

                context.State.AddLog("economy", faction.name + "收入 金钱+" + tax + " 粮食+" + food +
                    " 军费-" + moneyUpkeep + " 军粮-" + foodUpkeep + "。");
            }
        }

        private static int CalculateArmyMoneyUpkeep(GameContext context, FactionState faction, NumericContext numericContext)
        {
            int upkeep = 0;
            for (int i = 0; i < context.State.armies.Count; i++)
            {
                ArmyState army = context.State.armies[i];
                if (army.ownerFactionId != faction.id) continue;
                UnitDefinition unit = context.Data.GetUnit(army.unitId);
                upkeep += NumericFormulas.CalculateArmyUpkeep(army, unit, numericContext, false);
            }
            return upkeep;
        }

        private static int CalculateArmyFoodUpkeep(GameContext context, FactionState faction, NumericContext numericContext)
        {
            int upkeep = 0;
            for (int i = 0; i < context.State.armies.Count; i++)
            {
                ArmyState army = context.State.armies[i];
                if (army.ownerFactionId != faction.id) continue;
                UnitDefinition unit = context.Data.GetUnit(army.unitId);
                upkeep += NumericFormulas.CalculateArmyUpkeep(army, unit, numericContext, true);
            }
            return upkeep;
        }
    }
}
