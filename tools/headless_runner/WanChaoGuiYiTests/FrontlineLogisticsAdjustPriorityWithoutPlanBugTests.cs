using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: MapCommandService.AdjustFrontlineLogisticsPriority
    /// gates on HasActiveLogistics(army), so calling it on an army with
    /// no active plan returns false. But what about
    /// CancelFrontlineLogistics on an army whose plan was already
    /// completed (FoodRemaining=0, TurnsRemaining=0, but Convoy still
    /// has the route id)? FinishFrontlineLogisticsIfReady clears the
    /// plan, but only when explicitly triggered. There's no
    /// invariant that says "any army past completion has zero
    /// frontlineLogistics* fields" — race conditions in
    /// AdvanceFrontlineLogistics may have skipped the cleanup if
    /// AddLog warning short-circuited.
    ///
    /// This test pins a simpler invariant: ToggleFrontlineLogisticsPause
    /// must not flip the paused flag on an army with TurnsRemaining=0
    /// (the plan is over; pause is meaningless and only adds noise).
    /// </summary>
    public sealed class FrontlineLogisticsAdjustPriorityWithoutPlanBugTests
    {
        private readonly ITestOutputHelper output;

        public FrontlineLogisticsAdjustPriorityWithoutPlanBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void Toggle_Pause_On_Completed_Plan_Must_Be_Rejected()
        {
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(2, out data);
            data.UnitMap["infantry"] = new UnitDefinition
            {
                id = "infantry", name = "Infantry",
                stats = new UnitStats { attack = 10, defense = 10, mobility = 1, siege = 0 },
                upkeep = new CostSet()
            };
            FactionState faction = state.factions[0];
            state.armies.Add(new ArmyState
            {
                id = "army_done",
                ownerFactionId = faction.id,
                regionId = "r0",
                unitId = "infantry",
                soldiers = 1000,
                morale = 70
            });

            WorldState world = WorldStateFactory.Create(state, data);
            ArmyRuntimeState army = world.Map.ArmiesById["army_done"];

            // Manually configure as if a plan completed but flags weren't cleared.
            army.frontlineLogisticsTargetRegionId = "r1";
            army.frontlineLogisticsTurnsRemaining = 0;
            army.frontlineLogisticsFoodRemaining = 0;
            army.frontlineLogisticsFoodPerTurn = 0;
            army.frontlineLogisticsConvoyId = "convoy_dead";

            GameContext context = new GameContext(state, data, new EventBus());
            MapQueryService queries = new MapQueryService(world.Map, new MapGraphData(data));
            MapCommandService commands = new MapCommandService(queries, context);

            bool toggled = commands.ToggleFrontlineLogisticsPause("army_done");

            output.WriteLine("toggle accepted? " + toggled);
            output.WriteLine("paused after toggle? " + army.frontlineLogisticsPaused);

            // A completed plan should not be pausable. Today
            // HasActiveLogistics returns true because targetRegionId
            // is non-empty — but TurnsRemaining=0 means it's a
            // zombie. Pausing should be a no-op or false.
            Assert.False(toggled,
                "ToggleFrontlineLogisticsPause accepted on a plan with TurnsRemaining=0. " +
                "This creates a zombie state where pause flag is set on a completed plan.");
        }
    }
}
