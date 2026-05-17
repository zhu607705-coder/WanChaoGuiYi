using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: MapState.RemoveArmy uses
    ///     RemoveArmyLocation(armyId, army.locationRegionId);
    /// to clean up the reverse index, where `army.locationRegionId` is
    /// the CURRENT location at removal time. If anything has nudged
    /// the army's location previously without going through
    /// MoveArmyToRegion (a save/load, an event handler that mutated
    /// the field directly, the post-battle rout path that touches
    /// legacy state), the OLD region's army list still contains the
    /// army id even after RemoveArmy reports success.
    ///
    /// Pinned invariant: after a successful RemoveArmy, NO region's
    /// army list may still reference that army id.
    /// </summary>
    public sealed class MapStateRemoveArmyOrphanBugTests
    {
        private readonly ITestOutputHelper output;

        public MapStateRemoveArmyOrphanBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void RemoveArmy_Must_Purge_Army_From_All_Region_Indexes()
        {
            MapState map = new MapState();
            map.AddRegion(new RegionRuntimeState { id = "home", ownerFactionId = "p" });
            map.AddRegion(new RegionRuntimeState { id = "front", ownerFactionId = "p" });

            ArmyRuntimeState army = new ArmyRuntimeState
            {
                id = "ghost",
                ownerFactionId = "p",
                locationRegionId = "home",
                task = ArmyTask.Idle
            };
            map.AddArmy(army);

            // Direct mutation (the same vector tested in
            // MapStateArmyIndexCoherenceBugTests). Even after the
            // recent fix, RemoveArmy must still cope with this case.
            army.locationRegionId = "front";

            bool removed = map.RemoveArmy("ghost");
            Assert.True(removed);

            // Verify NO region's army list contains this id.  We
            // cannot reach armyIdsByRegionId directly, but
            // GetArmiesInRegion is the public read path. If the fix
            // for the "two truths" issue routes through ArmiesById,
            // both regions report empty (because the army is gone
            // from ArmiesById too). If the fix kept the reverse
            // index but only cleaned the CURRENT location, then
            // 'home' still references it.
            int homeCount = map.GetArmiesInRegion("home").Count;
            int frontCount = map.GetArmiesInRegion("front").Count;

            output.WriteLine("after RemoveArmy: home count=" + homeCount + " front count=" + frontCount);

            Assert.Equal(0, homeCount);
            Assert.Equal(0, frontCount);
        }
    }
}
