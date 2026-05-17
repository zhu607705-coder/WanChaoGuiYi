using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: MapState.MoveArmyToRegion accepts ANY
    /// targetRegionId, including ids never registered via AddRegion.
    /// IndexArmyLocation will lazily create a new entry in
    /// armyIdsByRegionId for the unknown id, leaving the army
    /// "orbiting" a phantom region that no other system knows about.
    /// Every subsequent GetArmiesInRegion(realRegionId) misses this
    /// army; UI doesn't see it; war/economy don't tax it.
    ///
    /// Pinned invariant: MoveArmyToRegion to an unregistered region
    /// must either reject the move (return false) or throw — never
    /// silently land the army in a phantom region.
    /// </summary>
    public sealed class MapStateMoveToUnknownRegionBugTests
    {
        private readonly ITestOutputHelper output;

        public MapStateMoveToUnknownRegionBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void Moving_Army_To_Unknown_Region_Must_Not_Silently_Succeed()
        {
            MapState map = new MapState();
            map.AddRegion(new RegionRuntimeState { id = "home", ownerFactionId = "p" });
            map.AddArmy(new ArmyRuntimeState { id = "a1", ownerFactionId = "p", locationRegionId = "home" });

            string locBefore = map.ArmiesById["a1"].locationRegionId;
            // 'phantom' was never registered with AddRegion.
            map.MoveArmyToRegion("a1", "phantom");
            string locAfter = map.ArmiesById["a1"].locationRegionId;

            output.WriteLine("locBefore: " + locBefore);
            output.WriteLine("locAfter:  " + locAfter);

            // The desired invariant: location must remain "home" if
            // 'phantom' is unknown. Today: location becomes 'phantom'
            // and the reverse index quietly grows a new entry.
            Assert.Equal("home", locAfter);
        }
    }
}
