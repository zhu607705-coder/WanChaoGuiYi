using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: GameStateFactory.CreateDefault returns a
    /// fresh GameState each call. But callers that "restart the game"
    /// often do so by creating a new GameContext with a new EventBus
    /// while keeping the OLD GameState reference around for cleanup.
    /// If anything in the old context still holds a reference to a
    /// runtimeMap (set via AttachRuntimeMap), that runtimeMap also
    /// references the old factions/regions. A subsequent
    /// ChangeRegionOwner on the OLD GameState then mutates the OLD
    /// runtimeMap — but no one observes it because the active
    /// GameContext is the new one.
    ///
    /// Pinned invariant: after creating a SECOND GameState with the
    /// same FakeDataRepository, mutations to the first GameState's
    /// regions must not appear in the second GameState's runtime map.
    /// In other words, creating two simultaneous "games" must not
    /// share state.
    /// </summary>
    public sealed class GameStateFactoryRebootIsolationBugTests
    {
        private readonly ITestOutputHelper output;

        public GameStateFactoryRebootIsolationBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Two_Concurrent_GameStates_Must_Not_Share_RuntimeMap()
        {
            FakeDataRepository data;
            GameState first = TestFixtures.BuildSinglePlayerWorld(2, out data);
            // BuildSinglePlayerWorld sets up data and one faction. We
            // need a second faction to perform a transfer.
            data.EmperorMap["enemy"] = new EmperorDefinition { id = "enemy", name = "E", stats = new EmperorStats() };
            FactionState enemyA = new FactionState
            {
                id = "faction_enemy", name = "E", emperorId = "enemy",
                taxMultiplier = 1f, foodMultiplier = 1f,
                armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f
            };
            first.factions.Add(enemyA);
            WorldState worldFirst = WorldStateFactory.Create(first, data);

            // Build a second GameState reusing the same data repository.
            FakeDataRepository data2;
            GameState second = TestFixtures.BuildSinglePlayerWorld(2, out data2);
            data2.EmperorMap["enemy"] = new EmperorDefinition { id = "enemy", name = "E", stats = new EmperorStats() };
            FactionState enemyB = new FactionState
            {
                id = "faction_enemy", name = "E", emperorId = "enemy",
                taxMultiplier = 1f, foodMultiplier = 1f,
                armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f
            };
            second.factions.Add(enemyB);
            WorldState worldSecond = WorldStateFactory.Create(second, data2);

            // Mutate ONLY the first.
            GameContext ctxFirst = new GameContext(first, data, new EventBus());
            ctxFirst.ChangeRegionOwner("r0", "faction_enemy");

            string secondRuntimeOwner = worldSecond.Map.RegionsById["r0"].ownerFactionId;
            string secondLegacyOwner = second.FindRegion("r0").ownerFactionId;

            output.WriteLine("first game runtime r0 owner:  " + worldFirst.Map.RegionsById["r0"].ownerFactionId);
            output.WriteLine("second game runtime r0 owner: " + secondRuntimeOwner);
            output.WriteLine("second game legacy r0 owner:  " + secondLegacyOwner);

            // The second game must be unaffected.  If GameState.AttachRuntimeMap
            // mistakenly stored a single static or shared map, this fails.
            Assert.Equal("faction_test_player", secondRuntimeOwner);
            Assert.Equal("faction_test_player", secondLegacyOwner);
        }
    }
}
