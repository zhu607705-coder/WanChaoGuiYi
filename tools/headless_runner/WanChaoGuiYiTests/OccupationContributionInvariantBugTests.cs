using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: even after the OccupationStatus and
    /// contribution percent fields became properties with normalisers,
    /// nothing prevents an Occupied region from carrying a 100%
    /// contribution percent on read. The setter clamps low values
    /// when status changes, but if a save load sets `integration` and
    /// `taxContributionPercent` independently with no status awareness,
    /// the read path delivers Occupied + 100% silently.
    ///
    /// Pinned invariant: when occupationStatus == Occupied, both
    /// contribution percents must NOT exceed OccupiedContributionPercent
    /// (35%) — read or write. Today the property setters only enforce
    /// it on status assignment, not when contribution itself is
    /// assigned.
    /// </summary>
    public sealed class OccupationContributionInvariantBugTests
    {
        private readonly ITestOutputHelper output;

        public OccupationContributionInvariantBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void Occupied_Region_Cannot_Have_Contribution_Above_Cap()
        {
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(1, out data);
            RegionState region = state.regions[0];

            // Set status first, then bash contribution above the cap.
            region.occupationStatus = OccupationStatus.Occupied;
            // After status change, the normaliser should have clamped
            // contribution to <= 35.
            int contribAfterStatus = region.taxContributionPercent;

            // Now the attack: assign 100 directly.
            region.taxContributionPercent = 100;
            int contribAfterAssign = region.taxContributionPercent;

            output.WriteLine("contribution after status=Occupied: " + contribAfterStatus);
            output.WriteLine("contribution after explicit =100:   " + contribAfterAssign);

            Assert.True(contribAfterAssign <= StrategyCausalRules.OccupiedContributionPercent,
                "Region in Occupied status reports tax contribution " + contribAfterAssign +
                "% which exceeds the Occupied cap of " + StrategyCausalRules.OccupiedContributionPercent + "%.");
        }
    }
}
