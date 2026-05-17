using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: MapState exposes AddRegion / AddArmy /
    /// AddEngagement but has no symmetric Remove for regions. This is
    /// an asymmetry — RegionDefinition can be deleted from a JSON
    /// data table at design time, and after a hot-reload the runtime
    /// MapState still carries the orphan region. Worse, the orphan
    /// region's ownerFactionId may still appear in
    /// faction.regionIds — so economy will keep trying to tax a
    /// region that no longer has a definition.
    ///
    /// Pinned invariant: there must be an API to remove a region
    /// from MapState that purges (a) the region itself, (b) any
    /// armies stationed there, (c) any engagement in that region,
    /// and (d) the reverse index. Today no such method exists.
    /// </summary>
    public sealed class MapStateRegionRemovalBugTests
    {
        private readonly ITestOutputHelper output;

        public MapStateRegionRemovalBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void MapState_Must_Provide_Region_Removal_API()
        {
            // Today MapState has no public RemoveRegion method.
            // This test will compile-fail when the API is added,
            // signalling that the bug was addressed. We probe via
            // reflection to keep the test compilable today.
            System.Reflection.MethodInfo[] methods = typeof(MapState).GetMethods();
            bool hasRemoveRegion = false;
            foreach (System.Reflection.MethodInfo m in methods)
            {
                if (m.Name == "RemoveRegion" && m.GetParameters().Length == 1)
                {
                    hasRemoveRegion = true;
                    break;
                }
            }

            output.WriteLine("MapState has RemoveRegion(string) method? " + hasRemoveRegion);
            Assert.True(hasRemoveRegion,
                "MapState provides AddRegion but not RemoveRegion. Hot-reloads, AI deletions, " +
                "and modder workflows produce orphan regions that the runtime cannot purge cleanly.");
        }
    }
}
