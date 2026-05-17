using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: RegionState.taxContributionPercent and
    /// foodContributionPercent are public int fields with no clamp.
    /// A bug in policy, save load, or a malformed JSON can set them to
    /// 200, -50, etc. The economy system multiplies tax/food output
    /// by these — a 200% region delivers double yield silently.
    ///
    /// Pinned invariant: when a region's contribution percent is out
    /// of [0, 100], the economy must clamp on read OR the field's
    /// setter must validate.
    /// </summary>
    public sealed class RegionState_ContributionPercentRangeBugTests
    {
        private readonly ITestOutputHelper output;

        public RegionState_ContributionPercentRangeBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void Out_Of_Range_ContributionPercent_Must_Be_Clamped()
        {
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(1, out data);
            RegionState region = state.regions[0];
            region.taxOutput = 100;
            region.taxContributionPercent = 100;
            int taxAtFull = NumericFormulas.CalculateRegionalTax(region, state.factions[0]);

            // Force pathological 200% (e.g. a buggy buff doubling).
            region.taxContributionPercent = 200;

            // CalculateRegionalTax doesn't read taxContributionPercent
            // directly — that's used by DomainEconomySystem via runtime
            // mirror. So we measure through the runtime path:
            WorldState world = WorldStateFactory.Create(state, data);
            world.Map.RegionsById["r0"].taxContributionPercent = 200;

            int moneyBefore = state.factions[0].money;
            GameContext context = new GameContext(state, data, new EventBus());
            new DomainEconomySystem(world).ExecuteTurn(context);
            int moneyAfter = state.factions[0].money;

            output.WriteLine("baseline tax (100% contribution): " + taxAtFull);
            output.WriteLine("money before: " + moneyBefore);
            output.WriteLine("money after with 200% contribution: " + moneyAfter);

            int actualGain = moneyAfter - moneyBefore + 1000; // crude offset

            // The economy receives base tax * 200% = double. The
            // pin: with a 200% contribution percent, tax inflow
            // must NOT exceed 100% baseline — the field should clamp.
            Assert.True(moneyAfter <= moneyBefore + taxAtFull + 50,
                "Region with 200% contribution percent doubled tax revenue. " +
                "moneyAfter - moneyBefore = " + (moneyAfter - moneyBefore) +
                "; expected <= " + taxAtFull);
        }
    }
}
