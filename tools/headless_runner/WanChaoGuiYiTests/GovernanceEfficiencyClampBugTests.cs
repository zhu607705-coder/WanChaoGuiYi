using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: NumericFormulas.CalculateGovernanceEfficiency
    /// clamps its output to [0.10, 1.25] but does NOT clamp its inputs.  A
    /// region with integration above 100 (e.g. due to a buggy save load,
    /// duplicated event apply, or modder data) inflates the integration
    /// factor before the output clamp engages.  That output clamp at 1.25
    /// keeps the regional yield reasonable, but only by accident — the
    /// downstream economy already trusts integrationPercent in many places
    /// (taxContributionPercent, runtime mirrors).  The desired invariant:
    /// a region with integration > 100 or < 0 must yield identical income
    /// to integration == 100 and integration == 0 respectively, OR the
    /// system must reject such region states up-front.
    ///
    /// This test pins down a related but immediately observable property:
    /// CalculateRegionalTax must be monotone in integration.  Pumping
    /// integration to 200 must never produce LESS tax than integration =
    /// 100, and pumping it negative must never produce MORE tax than
    /// integration = 0.  If clamps are missing those guarantees break.
    /// </summary>
    public sealed class GovernanceEfficiencyClampBugTests
    {
        private readonly ITestOutputHelper output;

        public GovernanceEfficiencyClampBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void RegionalTax_Must_Be_Monotone_In_Integration()
        {
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(1, out data);
            RegionState region = state.regions[0];
            FactionState faction = state.factions[0];

            region.taxOutput = 100;
            region.foodOutput = 100;
            region.rebellionRisk = 0;
            region.localPower = 0;
            region.annexationPressure = 0;

            int taxAtZero;
            int taxAtFull;
            int taxAtNegative;
            int taxAtAboveCap;

            region.integration = 0;
            taxAtZero = NumericFormulas.CalculateRegionalTax(region, faction);

            region.integration = 100;
            taxAtFull = NumericFormulas.CalculateRegionalTax(region, faction);

            region.integration = -50;
            taxAtNegative = NumericFormulas.CalculateRegionalTax(region, faction);

            region.integration = 200;
            taxAtAboveCap = NumericFormulas.CalculateRegionalTax(region, faction);

            output.WriteLine("tax at integration=0:    " + taxAtZero);
            output.WriteLine("tax at integration=100:  " + taxAtFull);
            output.WriteLine("tax at integration=-50:  " + taxAtNegative);
            output.WriteLine("tax at integration=200:  " + taxAtAboveCap);

            // Lower bound: negative integration must not pay MORE than
            // integration=0.  If clamps are missing, the integration
            // factor goes negative and combines with rebellionPenalty in
            // ways that usually reduce yield, but at integration=0 with
            // rebellion=0 the formula still scales by IntegrationTaxFloor.
            // Negative integration should not exceed that.
            Assert.True(taxAtNegative <= taxAtZero, "integration=-50 yielded MORE tax than integration=0");

            // Upper bound: integration > 100 must not pay LESS than
            // integration=100, AND must not pay arbitrarily MORE.  The
            // 1.25 output clamp gives us an upper limit relative to
            // taxOutput, but the relevant invariant for callers is
            // monotonicity:  past 100, the yield should plateau.
            Assert.True(taxAtAboveCap >= taxAtFull, "integration=200 yielded LESS tax than integration=100");

            // And it must not exceed the documented 1.25x ceiling that
            // the formula's output clamp implies.
            int ceiling = (int)System.Math.Ceiling(region.taxOutput * 1.25);
            Assert.True(taxAtAboveCap <= ceiling,
                "integration=200 broke the 1.25x output ceiling: tax=" + taxAtAboveCap + " ceiling=" + ceiling);
        }
    }
}
