using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: MapQueryService.FindRoute is a BFS over
    /// the map graph. It works correctly on connected graphs. But:
    ///   - On a disconnected graph (target is unreachable), it returns
    ///     an empty list. Callers must check Count > 0. If a caller
    ///     does route[0] to extract origin without checking, it
    ///     IndexOutOfRange.
    ///   - On a graph with a cycle, BFS still terminates because
    ///     cameFrom is keyed on visited regions.
    ///   - On startRegionId == targetRegionId, FindRoute returns
    ///     [startRegionId] (length 1). Some downstream code expects
    ///     length >= 2 for a "real route". Today IssueRouteCommand
    ///     rejects route.Count < 2, so this is fine — but tested?
    ///
    /// Pinned invariants:
    ///   1. FindRoute on disconnected graph returns empty list.
    ///   2. FindRoute on same start==target returns single-element list.
    ///   3. FindRoute does not throw on null/empty inputs.
    /// </summary>
    public sealed class MapQueryFindRoutePathologicalBugTests
    {
        private readonly ITestOutputHelper output;

        public MapQueryFindRoutePathologicalBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void FindRoute_Disconnected_Graph_Returns_Empty()
        {
            FakeDataRepository data = new FakeDataRepository();
            data.RegionMap["a"] = new RegionDefinition { id = "a", neighbors = new[] { "b" } };
            data.RegionMap["b"] = new RegionDefinition { id = "b", neighbors = new[] { "a" } };
            data.RegionMap["island"] = new RegionDefinition { id = "island", neighbors = new string[0] };

            MapState map = new MapState();
            map.AddRegion(new RegionRuntimeState { id = "a" });
            map.AddRegion(new RegionRuntimeState { id = "b" });
            map.AddRegion(new RegionRuntimeState { id = "island" });

            MapQueryService q = new MapQueryService(map, new MapGraphData(data));
            List<string> route = q.FindRoute("a", "island");
            output.WriteLine("disconnected route count: " + route.Count);
            Assert.Empty(route);
        }

        [Fact]
        public void FindRoute_Same_Start_And_Target_Returns_Single_Element()
        {
            FakeDataRepository data = new FakeDataRepository();
            data.RegionMap["a"] = new RegionDefinition { id = "a", neighbors = new string[0] };
            MapState map = new MapState();
            map.AddRegion(new RegionRuntimeState { id = "a" });

            MapQueryService q = new MapQueryService(map, new MapGraphData(data));
            List<string> route = q.FindRoute("a", "a");
            output.WriteLine("self-route: [" + string.Join(",", route) + "]");
            Assert.Single(route);
            Assert.Equal("a", route[0]);
        }

        [Fact]
        public void FindRoute_Null_Or_Empty_Inputs_Do_Not_Throw()
        {
            FakeDataRepository data = new FakeDataRepository();
            MapState map = new MapState();
            MapQueryService q = new MapQueryService(map, new MapGraphData(data));

            System.Exception caught = null;
            List<string> r1 = null, r2 = null, r3 = null, r4 = null;
            try { r1 = q.FindRoute(null, null); }
            catch (System.Exception ex) { caught = ex; }
            try { r2 = q.FindRoute("a", null); }
            catch (System.Exception ex) { if (caught == null) caught = ex; }
            try { r3 = q.FindRoute(null, "b"); }
            catch (System.Exception ex) { if (caught == null) caught = ex; }
            try { r4 = q.FindRoute("", ""); }
            catch (System.Exception ex) { if (caught == null) caught = ex; }

            output.WriteLine("threw? " + (caught == null ? "<no>" : caught.GetType().Name));

            Assert.Null(caught);
            Assert.NotNull(r1);
            Assert.NotNull(r2);
            Assert.NotNull(r3);
            Assert.NotNull(r4);
        }
    }
}
