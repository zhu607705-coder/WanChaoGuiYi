using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: RegionState.occupationStatus and
    /// RegionRuntimeState.occupationStatus are public fields with no
    /// state-machine enforcement. Code can transition Occupied →
    /// Controlled directly without going through Pacified / Registered.
    /// Save load, scripted scenarios, and modder data can therefore
    /// place a region in any status — including impossible
    /// combinations like Occupied + integration=100 (which the
    /// economy then treats as fully contributing).
    ///
    /// Pinned invariant: setting occupationStatus = Controlled while
    /// integration is below the OccupiedIntegration threshold should
    /// either be rejected or normalised. Today nothing checks.
    /// </summary>
    public sealed class OccupationStatusTransitionBugTests
    {
        private readonly ITestOutputHelper output;

        public OccupationStatusTransitionBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void Region_With_Occupied_Integration_Cannot_Be_Controlled()
        {
            // We need to prove the property holds via at least one
            // observable: economy must not pay full tax on a region
            // whose status was just smashed to Controlled but whose
            // integration is still 25 (Occupied range).
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(1, out data);
            FactionState faction = state.factions[0];
            RegionState region = state.regions[0];

            region.taxOutput = 100;
            region.integration = StrategyCausalRules.OccupiedIntegration; // 25
            region.occupationStatus = OccupationStatus.Occupied;
            region.taxContributionPercent = StrategyCausalRules.OccupiedContributionPercent; // 35
            region.foodContributionPercent = StrategyCausalRules.OccupiedContributionPercent;

            // Compute baseline expected tax under Occupied state.
            int taxAtOccupied = NumericFormulas.CalculateRegionalTax(region, faction);

            // Now bypass the state machine: set status to Controlled
            // without changing integration or contribution percent.
            region.occupationStatus = OccupationStatus.Controlled;

            int taxAfterBypass = NumericFormulas.CalculateRegionalTax(region, faction);

            output.WriteLine("tax at Occupied: " + taxAtOccupied);
            output.WriteLine("tax after status forced to Controlled (integration=25): " + taxAfterBypass);

            // Either the region's tax should remain pinned to the
            // Occupied value (status change is ignored without a
            // pacification chain), OR the formula should refuse to
            // pay full tax when integration < 100.
            //
            // Today CalculateRegionalTax doesn't know about status
            // at all — it's purely a function of integration,
            // rebellionRisk, and modifiers. So taxAtOccupied ==
            // taxAfterBypass in this isolated unit test. The bug
            // surfaces in the SYSTEM-level pipeline where
            // taxContributionPercent gets reset to 100 on a fresh
            // factory but Controlled status implies "fully
            // integrated" elsewhere. The simpler pin we can write
            // here: contribution percent and status must not drift
            // apart. We simulate that drift by setting
            // taxContributionPercent = 100 (Controlled) while
            // integration stays 25.
            region.taxContributionPercent = 100;
            // Now re-evaluate via the runtime path that DomainEconomySystem
            // uses (RegionRuntimeState contribution percent).
            // For this isolated test, just verify the inconsistency:
            // a status-change that doesn't update integration is
            // an obvious bug we want to catch.

            Assert.True(region.integration >= 100 || region.taxContributionPercent <= StrategyCausalRules.OccupiedContributionPercent,
                "Region status forced to Controlled while integration=" + region.integration +
                " and taxContributionPercent=" + region.taxContributionPercent +
                " — drift between status and quantitative governance fields.");
        }
    }
}
