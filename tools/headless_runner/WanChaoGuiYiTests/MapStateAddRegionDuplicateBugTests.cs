using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: MapState.AddRegion silently overwrites
    /// when called twice with the same id. Same shape as the earlier
    /// duplicate-engagement bug. Now that production NonUnityJson
    /// rejects duplicate region ids and FakeRepository does too, the
    /// MapState runtime mirror should ALSO refuse to silently
    /// overwrite — otherwise a buggy data load could orphan armies
    /// indexed by the old runtime region.
    ///
    /// Pinned invariant: AddRegion with a duplicate id must either
    /// throw or be a no-op that preserves the original entry's
    /// armyIdsByRegionId list.
    /// </summary>
    public sealed class MapStateAddRegionDuplicateBugTests
    {
        private readonly ITestOutputHelper output;

        public MapStateAddRegionDuplicateBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void AddRegion_With_Duplicate_Id_Must_Not_Lose_Existing_Armies()
        {
            MapState map = new MapState();
            RegionRuntimeState first = new RegionRuntimeState { id = "r0", ownerFactionId = "p" };
            map.AddRegion(first);
            map.AddArmy(new ArmyRuntimeState { id = "a1", ownerFactionId = "p", locationRegionId = "r0" });

            int armiesBefore = map.GetArmiesInRegion("r0").Count;
            Assert.Equal(1, armiesBefore);

            // Second AddRegion with same id. The current behaviour
            // silently swaps the RegionRuntimeState reference,
            // potentially severing the army's index association.
            RegionRuntimeState second = new RegionRuntimeState { id = "r0", ownerFactionId = "q" };
            map.AddRegion(second);

            int armiesAfter = map.GetArmiesInRegion("r0").Count;
            output.WriteLine("armies in r0 before second AddRegion: " + armiesBefore);
            output.WriteLine("armies in r0 after second AddRegion:  " + armiesAfter);

            // The army should still be reachable; pinning the
            // weaker invariant that the index isn't quietly broken.
            Assert.Equal(armiesBefore, armiesAfter);
        }
    }
}
