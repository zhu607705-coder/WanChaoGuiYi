namespace WanChaoGuiYi
{
    public sealed class DomainEconomySystem : IGameSystem
    {
        private readonly WorldState worldState;

        public DomainEconomySystem(WorldState worldState)
        {
            this.worldState = worldState;
        }

        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            if (context == null || context.State == null) return;

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

                    tax += CalculateEffectiveRegionalTax(region, numericContext);
                    food += CalculateEffectiveRegionalFood(region, numericContext);
                }

                int moneyUpkeep = CalculateArmyMoneyUpkeep(context, faction, numericContext) + NumericFormulas.CalculateGovernanceUpkeep(numericContext, false) + NumericFormulas.CalculateTreasuryReserveDrag(faction, false);
                int foodUpkeep = CalculateArmyFoodUpkeep(context, faction, numericContext) + NumericFormulas.CalculateGovernanceUpkeep(numericContext, true) + NumericFormulas.CalculateTreasuryReserveDrag(faction, true);
                faction.money += tax - moneyUpkeep;
                faction.food += food - foodUpkeep;

                if (faction.money < 0)
                {
                    faction.legitimacy = DomainMath.Max(0, faction.legitimacy - NumericFormulas.CalculateFiscalLegitimacyPenalty(numericContext));
                    faction.courtFactionPressure = DomainMath.Min(100, faction.courtFactionPressure + NumericFormulas.CalculateFiscalCourtPressure(numericContext));
                }

                int successionPressure = NumericFormulas.CalculateExpansionSuccessionPressure(numericContext);
                if (successionPressure > 0)
                {
                    faction.successionRisk = DomainMath.Min(100, faction.successionRisk + successionPressure);
                }

                context.State.AddLog("economy", faction.name + "收入 金钱+" + tax + " 粮食+" + food +
                    " 军费-" + moneyUpkeep + " 军粮-" + foodUpkeep + "。原因：地区贡献率折算产出后扣除军队和治理成本。影响：金钱和粮食立即更新。");
            }
        }

        private int CalculateEffectiveRegionalTax(RegionState region, NumericContext numericContext)
        {
            int baseTax = NumericFormulas.CalculateRegionalTax(region, numericContext);
            RegionRuntimeState runtimeRegion = GetRuntimeRegion(region.id);
            if (runtimeRegion == null) return baseTax;
            return ApplyContributionPercent(baseTax, runtimeRegion.taxContributionPercent);
        }

        private int CalculateEffectiveRegionalFood(RegionState region, NumericContext numericContext)
        {
            int baseFood = NumericFormulas.CalculateRegionalFood(region, numericContext);
            RegionRuntimeState runtimeRegion = GetRuntimeRegion(region.id);
            if (runtimeRegion == null) return baseFood;
            return ApplyContributionPercent(baseFood, runtimeRegion.foodContributionPercent);
        }

        private RegionRuntimeState GetRuntimeRegion(string regionId)
        {
            if (worldState == null || worldState.Map == null) return null;

            RegionRuntimeState runtimeRegion;
            if (!worldState.Map.TryGetRegion(regionId, out runtimeRegion)) return null;
            return runtimeRegion;
        }

        private static int ApplyContributionPercent(int value, int contributionPercent)
        {
            return DomainMath.RoundToInt(value * DomainMath.Clamp(contributionPercent, 0, 100) / 100f);
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
