using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: NumericFormulas.CalculateArmyUpkeep
    /// returns 0 silently when the unit definition is missing or has
    /// no upkeep info:
    ///
    ///     if (army == null || unit == null || unit.upkeep == null) return 0;
    ///
    /// In production this means an army whose unitId was deleted or
    /// renamed (typo, mod, save migration) costs zero money and zero
    /// food forever. The economy summary log won't even mention this
    /// army's contribution. Worst-case: the game-balance breaks
    /// quietly and players get free armies that are not visible
    /// without grepping logs.
    ///
    /// Pinned invariant: an army whose unit definition is missing
    /// should EITHER produce a deterministic fallback upkeep (e.g.
    /// last-known good unit) AND log a warning, OR force the economy
    /// system to flag it (so a UI hint appears). Today neither
    /// happens.
    ///
    /// Test approach: build a faction with one army whose unitId
    /// doesn't exist in data.Units, run the economy turn, and check
    /// that some observable signal of "this is wrong" exists in the
    /// turn log.
    /// </summary>
    public sealed class MissingUnitDefinitionFreeArmyBugTests
    {
        private readonly ITestOutputHelper output;

        public MissingUnitDefinitionFreeArmyBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void Army_With_Missing_Unit_Must_Be_Flagged_Not_Free()
        {
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(1, out data);
            FactionState faction = state.factions[0];
            // Note: data.UnitMap is empty.
            state.armies.Add(new ArmyState
            {
                id = "ghost_army",
                ownerFactionId = faction.id,
                regionId = "r0",
                unitId = "nonexistent_unit",
                soldiers = 5000,
                morale = 80
            });

            int logsBefore = state.turnLog.Count;
            GameContext context = new GameContext(state, data, new EventBus());
            new DomainEconomySystem(null).ExecuteTurn(context);

            bool flagged = false;
            for (int i = logsBefore; i < state.turnLog.Count; i++)
            {
                string msg = state.turnLog[i].message ?? "";
                if (msg.Contains("nonexistent_unit") || msg.Contains("missing unit") || msg.Contains("ghost_army"))
                {
                    flagged = true;
                    output.WriteLine("flagged log: " + msg);
                }
            }

            Assert.True(flagged,
                "Army 'ghost_army' with unitId 'nonexistent_unit' was billed at 0 upkeep " +
                "with no warning, no log, and no flag. Free army glitch.");
        }
    }
}
