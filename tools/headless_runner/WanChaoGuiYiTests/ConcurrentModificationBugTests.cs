using System;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: many domain systems iterate over
    /// MapState.ArmiesById.Values directly (DomainArmyMovementSystem,
    /// DomainEngagementDetector). If any iterated step calls
    /// MapState.AddArmy or MapState.RemoveArmy (which the rout/retreat
    /// path actually does — RemoveArmy is invoked from inside the
    /// movement loop), the underlying Dictionary throws
    /// InvalidOperationException("Collection was modified") on the
    /// next MoveNext.
    ///
    /// Pinned invariant: a turn that includes a rout (RemoveArmy
    /// inside movement) must complete without throwing.
    /// </summary>
    public sealed class ConcurrentModificationBugTests
    {
        private readonly ITestOutputHelper output;

        public ConcurrentModificationBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void Movement_Turn_Surviving_RemoveArmy_Must_Not_Throw()
        {
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(2, out data);
            data.UnitMap["infantry"] = new UnitDefinition
            {
                id = "infantry", name = "Infantry",
                stats = new UnitStats { attack = 10, defense = 10, mobility = 1, siege = 0 },
                upkeep = new CostSet()
            };
            FactionState player = state.factions[0];
            // Add many armies with stale routes that cause RemoveArmy
            // to fire mid-iteration. Use trivial routes since we're
            // testing iteration safety, not battle logic.
            for (int i = 0; i < 20; i++)
            {
                state.armies.Add(new ArmyState
                {
                    id = "a" + i,
                    ownerFactionId = player.id,
                    regionId = "r0",
                    unitId = "infantry",
                    soldiers = 50,    // low soldiers => rout-eligible if ever fights
                    morale = 5
                });
            }

            WorldState world = WorldStateFactory.Create(state, data);
            GameContext context = new GameContext(state, data, new EventBus());
            MapQueryService queries = new MapQueryService(world.Map, new MapGraphData(data));
            MapCommandService commands = new MapCommandService(queries, context);
            DomainEngagementDetector detector = new DomainEngagementDetector();

            foreach (string aid in new[] { "a0", "a1", "a2", "a3", "a4" })
            {
                ArmyRuntimeState a = world.Map.ArmiesById[aid];
                a.task = ArmyTask.Move;
                a.targetRegionId = "r1";
                a.route = new System.Collections.Generic.List<string> { "r0", "r1" };
            }

            DomainArmyMovementSystem movement = new DomainArmyMovementSystem(world, commands, detector);
            Exception caught = null;
            try { movement.ExecuteTurn(context); }
            catch (Exception ex) { caught = ex; }

            output.WriteLine("threw? " + (caught == null ? "<no>" : caught.GetType().Name + ": " + caught.Message));
            Assert.Null(caught);
        }
    }
}
