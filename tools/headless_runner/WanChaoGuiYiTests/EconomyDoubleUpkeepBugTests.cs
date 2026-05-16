using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: NumericModifierFactory.ForFaction injects
    /// per-region governance Money/Food upkeep as additive modifiers on
    /// MoneyUpkeep/FoodUpkeep stats. When DomainEconomySystem then computes
    /// army upkeep through the same NumericContext, the per-region cost is
    /// re-applied for every army, not just once. This test pins down the
    /// expected invariant: with N regions and 0 armies, the governance
    /// upkeep should equal exactly (N-1) * GovernanceMoneyCostPerRegion,
    /// and adding armies must not change that governance cost (only the
    /// per-army cost changes).
    /// </summary>
    public sealed class EconomyDoubleUpkeepBugTests
    {
        private readonly ITestOutputHelper output;

        public EconomyDoubleUpkeepBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void GovernanceUpkeep_With_5Regions_NoArmies_Charges_Cost_Once()
        {
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(5, out data);
            FactionState faction = state.factions[0];
            int moneyBefore = faction.money;
            int foodBefore = faction.food;

            GameContext context = TestFixtures.BuildContext(state, data);
            DomainEconomySystem economy = new DomainEconomySystem(null);
            economy.ExecuteTurn(context);

            int expectedRegionMoneyCost = 4 * (int)NumericTuning.GovernanceMoneyCostPerRegion;
            int expectedRegionFoodCost = 4 * (int)NumericTuning.GovernanceFoodCostPerRegion;
            int actualMoneyDelta = moneyBefore - faction.money;
            int actualFoodDelta = foodBefore - faction.food;

            output.WriteLine("expected governance money cost: " + expectedRegionMoneyCost);
            output.WriteLine("expected governance food cost:  " + expectedRegionFoodCost);
            output.WriteLine("actual money delta:             " + actualMoneyDelta);
            output.WriteLine("actual food delta:              " + actualFoodDelta);

            // With 0 armies we expect EXACTLY one application of the
            // per-region governance cost.  Treasury reserve drag is also
            // active because money/food start above the soft cap, so we
            // tolerate a documented additional drag amount.
            int reserveDragMoney = NumericFormulas.CalculateTreasuryReserveDrag(faction, false);
            int reserveDragFood = NumericFormulas.CalculateTreasuryReserveDrag(faction, true);
            // After ExecuteTurn the values changed; recompute reserve drag from
            // the pre-turn snapshot rather than the post-turn faction.
            FactionState snapshot = new FactionState
            {
                money = moneyBefore,
                food = foodBefore
            };
            int reserveDragMoneyBefore = NumericFormulas.CalculateTreasuryReserveDrag(snapshot, false);
            int reserveDragFoodBefore = NumericFormulas.CalculateTreasuryReserveDrag(snapshot, true);

            // Tax/food output is 0 in this fixture, so the only outflow
            // should be governance cost + reserve drag.
            Assert.Equal(expectedRegionMoneyCost + reserveDragMoneyBefore, actualMoneyDelta);
            Assert.Equal(expectedRegionFoodCost + reserveDragFoodBefore, actualFoodDelta);

            // Suppress unused-variable warnings (these are kept for
            // diagnostic clarity in the xunit output).
            output.WriteLine("post-turn reserve drag (money/food): " + reserveDragMoney + "/" + reserveDragFood);
        }

        [Fact]
        public void Adding_One_Army_Should_Not_Multiply_Governance_Cost()
        {
            // Run twice: once with 0 armies, once with 1 army.  Subtract
            // the per-army upkeep contribution and check that the
            // residual (governance + reserve drag) is identical between
            // runs.  If governance cost is being re-applied per army,
            // the residuals will diverge.
            FakeDataRepository dataNoArmy;
            GameState stateNoArmy = TestFixtures.BuildSinglePlayerWorld(5, out dataNoArmy);
            FactionState factionNoArmy = stateNoArmy.factions[0];
            int moneyBeforeNoArmy = factionNoArmy.money;
            new DomainEconomySystem(null).ExecuteTurn(TestFixtures.BuildContext(stateNoArmy, dataNoArmy));
            int residualNoArmy = moneyBeforeNoArmy - factionNoArmy.money;

            FakeDataRepository dataWithArmy;
            GameState stateWithArmy = TestFixtures.BuildSinglePlayerWorld(5, out dataWithArmy);
            // Add one army with a known unit cost.
            dataWithArmy.UnitMap["infantry"] = new UnitDefinition
            {
                id = "infantry",
                name = "Infantry",
                stats = new UnitStats { attack = 10, defense = 10, mobility = 1, siege = 0 },
                upkeep = new CostSet { money = 5, food = 5 }
            };
            stateWithArmy.armies.Add(new ArmyState
            {
                id = "army_test",
                ownerFactionId = stateWithArmy.factions[0].id,
                regionId = "r0",
                unitId = "infantry",
                soldiers = 1000,
                morale = 70,
                movementProgress = 0
            });
            FactionState factionWithArmy = stateWithArmy.factions[0];
            int moneyBeforeWithArmy = factionWithArmy.money;
            new DomainEconomySystem(null).ExecuteTurn(TestFixtures.BuildContext(stateWithArmy, dataWithArmy));
            int totalCostWithArmy = moneyBeforeWithArmy - factionWithArmy.money;

            // Per-army base contribution: 1 * 5 = 5 (CeilToInt(1000/1000) * upkeep.money)
            // Subtract that from the with-army total. The residual MUST equal
            // the no-army residual (i.e. governance cost is region-bound, not army-bound).
            int residualWithArmy = totalCostWithArmy - 5;

            output.WriteLine("residual (governance+drag) no army:  " + residualNoArmy);
            output.WriteLine("residual (governance+drag) 1 army:   " + residualWithArmy);
            output.WriteLine("total cost no army:                  " + (moneyBeforeNoArmy - factionNoArmy.money));
            output.WriteLine("total cost 1 army:                   " + totalCostWithArmy);

            Assert.Equal(residualNoArmy, residualWithArmy);
        }
    }
}
