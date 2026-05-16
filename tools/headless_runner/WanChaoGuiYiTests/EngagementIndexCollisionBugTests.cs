using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: MapState.AddEngagement silently overwrites
    /// engagementIdByRegionId when a second engagement is registered in
    /// the same region. The first engagement becomes orphaned (still
    /// present in EngagementsById, still referenced by armies'
    /// engagementId, but unreachable through TryGetEngagementInRegion).
    /// This test pins down the desired invariant: at most one active
    /// engagement per region (or, equivalently, AddEngagement must reject
    /// or replace defensively, not corrupt the index).
    /// </summary>
    public sealed class EngagementIndexCollisionBugTests
    {
        private readonly ITestOutputHelper output;

        public EngagementIndexCollisionBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Adding_Two_Engagements_To_Same_Region_Must_Not_Orphan_First_One()
        {
            MapState mapState = new MapState();
            mapState.AddRegion(new RegionRuntimeState
            {
                id = "r0",
                ownerFactionId = "faction_a",
                occupationStatus = OccupationStatus.Controlled
            });

            EngagementRuntimeState first = new EngagementRuntimeState
            {
                id = "engagement_first",
                regionId = "r0",
                phase = EngagementPhase.Forming,
                createdTurn = 1
            };
            first.attackerArmyIds.Add("army_a");
            first.defenderArmyIds.Add("army_b");
            mapState.AddEngagement(first);

            EngagementRuntimeState second = new EngagementRuntimeState
            {
                id = "engagement_second",
                regionId = "r0",
                phase = EngagementPhase.Forming,
                createdTurn = 1
            };
            second.attackerArmyIds.Add("army_c");
            second.defenderArmyIds.Add("army_d");
            mapState.AddEngagement(second);

            // After adding the second engagement, the region index should
            // unambiguously identify the active engagement; the first one
            // should either have been removed from EngagementsById or
            // AddEngagement should have rejected the duplicate.  Anything
            // else is a leak.
            EngagementRuntimeState resolved;
            bool found = mapState.TryGetEngagementInRegion("r0", out resolved);

            output.WriteLine("first.id  = " + first.id);
            output.WriteLine("second.id = " + second.id);
            output.WriteLine("TryGetEngagementInRegion -> " + (found ? resolved.id : "<null>"));
            output.WriteLine("EngagementsById.Count    = " + mapState.EngagementsById.Count);

            // The bug shows up as: index points to second, but first
            // remains in EngagementsById (count == 2). Strong invariant:
            // at most one engagement per region in EngagementsById.
            Assert.True(found);
            Assert.Single(mapState.EngagementsById);
        }
    }
}
