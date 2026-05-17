using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: GameContext.ChangeRegionOwner is the
    /// recommended entry point.  It calls
    ///     State.ChangeRegionOwner(...)
    ///     State.SyncRuntimeRegionOwner(...)
    ///     Events.Publish(RegionOwnerChanged)
    ///
    /// However, GameState.ChangeRegionOwner is still PUBLIC and any
    /// caller — events, scripted scenarios, save/load, AI systems, the
    /// Web bridge — that uses it directly skips both the runtime sync
    /// AND the event publish.  Today's "GameStateMapStateOwnerSync"
    /// test only covers the GameContext path.  This test pins the
    /// invariant for the State path:
    ///
    ///   Whichever path mutates ownership, the runtime MapState mirror
    ///   must reflect the change before any external observer can
    ///   read inconsistent state.
    ///
    /// Either GameState.ChangeRegionOwner becomes internal/private (so
    /// callers MUST go through GameContext), OR it must perform the
    /// runtime sync itself.
    /// </summary>
    public sealed class DirectStateOwnerChangeBypassBugTests
    {
        private readonly ITestOutputHelper output;

        public DirectStateOwnerChangeBypassBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Direct_State_ChangeRegionOwner_Must_Not_Leave_MapState_Inconsistent()
        {
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(2, out data);
            data.EmperorMap["enemy"] = new EmperorDefinition { id = "enemy", name = "E", stats = new EmperorStats() };
            FactionState enemy = new FactionState
            {
                id = "faction_enemy", name = "E", emperorId = "enemy",
                taxMultiplier = 1f, foodMultiplier = 1f,
                armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f
            };
            state.factions.Add(enemy);

            // Build the runtime world the same way GameContext bootstrap does.
            WorldState world = WorldStateFactory.Create(state, data);

            // Call the LEGACY entry point directly.  This is what an
            // event handler, save loader, or AI scripted scenario
            // could realistically do today.
            RegionOwnerChangedPayload payload = state.ChangeRegionOwner("r0", enemy.id);
            Assert.NotNull(payload);

            string legacyOwner = state.FindRegion("r0").ownerFactionId;
            string runtimeOwner = world.Map.RegionsById["r0"].ownerFactionId;

            output.WriteLine("legacy owner after direct State call:  " + legacyOwner);
            output.WriteLine("runtime owner after direct State call: " + runtimeOwner);

            // Today: legacyOwner = faction_enemy, runtimeOwner = faction_test_player.
            // The runtime mirror has drifted — any system reading
            // MapState (war resolution, queries, governance) sees the
            // wrong owner.
            Assert.Equal(legacyOwner, runtimeOwner);
        }
    }
}
