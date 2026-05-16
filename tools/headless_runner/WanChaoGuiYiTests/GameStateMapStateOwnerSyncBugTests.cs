using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: GameContext.ChangeRegionOwner mutates
    /// GameState.regions and FactionState.regionIds, but does not touch
    /// MapState.RegionsById[regionId].ownerFactionId.  Any non-battle
    /// caller (events, scripted scenarios, tests) therefore leaves the
    /// runtime map in an inconsistent state.  The desired invariant is
    /// that GameContext.ChangeRegionOwner is the single source of truth
    /// and the runtime mirror is updated atomically.
    /// </summary>
    public sealed class GameStateMapStateOwnerSyncBugTests
    {
        private readonly ITestOutputHelper output;

        public GameStateMapStateOwnerSyncBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void ChangeRegionOwner_Must_Sync_MapState_Runtime_Region()
        {
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(2, out data);

            // Add a second faction so we have somewhere to transfer to.
            data.EmperorMap["enemy"] = new EmperorDefinition { id = "enemy", name = "E", stats = new EmperorStats() };
            FactionState enemy = new FactionState
            {
                id = "faction_enemy",
                name = "E",
                emperorId = "enemy",
                taxMultiplier = 1f, foodMultiplier = 1f,
                armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f
            };
            state.factions.Add(enemy);

            WorldState world = WorldStateFactory.Create(state, data);
            GameContext context = new GameContext(state, data, new EventBus());

            string regionId = "r0";
            string newOwner = enemy.id;
            RegionRuntimeState runtimeRegion = world.Map.RegionsById[regionId];
            string runtimeOwnerBefore = runtimeRegion.ownerFactionId;

            RegionOwnerChangedPayload payload = context.ChangeRegionOwner(regionId, newOwner);

            output.WriteLine("payload null?            " + (payload == null));
            output.WriteLine("legacy region owner now: " + state.FindRegion(regionId).ownerFactionId);
            output.WriteLine("runtime owner before:    " + runtimeOwnerBefore);
            output.WriteLine("runtime owner after:     " + runtimeRegion.ownerFactionId);

            Assert.NotNull(payload);
            Assert.Equal(newOwner, state.FindRegion(regionId).ownerFactionId);
            Assert.Equal(newOwner, runtimeRegion.ownerFactionId);
        }
    }
}
