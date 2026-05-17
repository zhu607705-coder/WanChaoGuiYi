using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: MapCommandService.SiegeRegion delegates
    /// directly to IssueRouteCommand without verifying that the target
    /// is owned by a hostile faction. A player can issue Siege against
    /// their own region, against an empty region, or against a
    /// neutral border zone — the command is accepted, "围攻" log line
    /// is written, ArmyMoveStarted event fires.
    ///
    /// Pinned invariant: Siege must reject any target whose owner is
    /// the same faction as the army's owner.
    /// </summary>
    public sealed class SiegeCommandValidationBugTests
    {
        private readonly ITestOutputHelper output;

        public SiegeCommandValidationBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void Siege_Cannot_Target_Own_Region()
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
            state.armies.Add(new ArmyState
            {
                id = "siegeer",
                ownerFactionId = player.id,
                regionId = "r0",
                unitId = "infantry",
                soldiers = 1000,
                morale = 70
            });

            WorldState world = WorldStateFactory.Create(state, data);
            GameContext context = new GameContext(state, data, new EventBus());
            MapQueryService queries = new MapQueryService(world.Map, new MapGraphData(data));
            MapCommandService commands = new MapCommandService(queries, context);

            // r1 is owned by the same faction (TestFixtures gave both
            // r0 and r1 to player). Siege must reject.
            bool accepted = commands.SiegeRegion("siegeer", "r1");

            output.WriteLine("siege accepted on own region? " + accepted);
            Assert.False(accepted);
        }
    }
}
