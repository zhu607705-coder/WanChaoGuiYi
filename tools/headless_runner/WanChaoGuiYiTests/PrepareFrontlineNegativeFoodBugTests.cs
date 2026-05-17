using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: MapCommandService.PrepareFrontline
    /// pays food by `faction.food -= foodCost`. If a previous turn's
    /// economy already left faction.food at -100 (grain shortage),
    /// PrepareFrontline currently rejects with "粮食不足" because
    /// faction.food (-100) < foodCost (positive). Good. But what if
    /// the upstream check uses `faction.food >= foodCost`? Then
    /// negative food still permits prepare. Let's test the boundary.
    /// </summary>
    public sealed class PrepareFrontlineNegativeFoodBugTests
    {
        private readonly ITestOutputHelper output;

        public PrepareFrontlineNegativeFoodBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void PrepareFrontline_With_Negative_Food_Must_Be_Rejected()
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
            faction.food = -50; // grain shortage carry-over

            state.armies.Add(new ArmyState
            {
                id = "starver",
                ownerFactionId = faction.id,
                regionId = "r0",
                unitId = "infantry",
                soldiers = 1000,
                morale = 70
            });

            WorldState world = WorldStateFactory.Create(state, data);
            GameContext context = new GameContext(state, data, new EventBus());
            MapQueryService queries = new MapQueryService(world.Map, new MapGraphData(data));
            MapCommandService commands = new MapCommandService(queries, context);

            int foodBefore = faction.food;
            bool prepared = commands.PrepareFrontline("starver", "r1");
            int foodAfter = faction.food;

            output.WriteLine("prepared? " + prepared);
            output.WriteLine("food before/after: " + foodBefore + " -> " + foodAfter);

            // Either reject (faction.food unchanged), or accept but
            // refuse to make food MORE negative.
            Assert.True(!prepared || foodAfter == foodBefore,
                "PrepareFrontline succeeded on a negative-food faction without clamping.");
        }
    }
}
