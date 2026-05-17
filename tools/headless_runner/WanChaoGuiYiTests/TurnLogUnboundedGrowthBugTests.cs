using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: GameState.AddLog appends to turnLog
    /// without any cap, eviction policy, or category-aware filter.
    /// In a long-running simulation (50+ turns at MVP volume) this
    /// list grows unbounded.  Each economy pass alone can write
    /// ~factionCount * 1 entry per turn; war scenarios can write 10+
    /// entries per army per turn.  Save files that round-trip the
    /// state (Web export/import, SaveManager) embed the entire turnLog
    /// and grow O(turns).
    ///
    /// Pinned invariant: turnLog must have a soft cap.  When entries
    /// exceed the cap, the oldest non-current-turn entries should be
    /// pruned.  A reasonable cap for an MVP demo is ~2000 entries
    /// (well above one full turn's worth, well below memory pressure).
    ///
    /// We test by writing 5000 distinct log lines.  After that the
    /// length should be bounded.  Today turnLog.Count == 5000.
    /// </summary>
    public sealed class TurnLogUnboundedGrowthBugTests
    {
        private readonly ITestOutputHelper output;

        public TurnLogUnboundedGrowthBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TurnLog_Must_Have_A_Soft_Cap_To_Prevent_Save_Bloat()
        {
            GameState state = new GameState();
            const int totalLogs = 5000;
            for (int i = 0; i < totalLogs; i++)
            {
                state.AddLog("test", "entry " + i);
            }

            output.WriteLine("turnLog.Count after " + totalLogs + " writes: " + state.turnLog.Count);

            // 5000 raw entries with no eviction means a save file at
            // turn 50 with 100 entries/turn is 5000 strings carried
            // forward in memory and serialized into JSON.  The log cap
            // is soft across turns, but a single noisy turn still needs
            // a safety ceiling.
            Assert.True(state.turnLog.Count < totalLogs,
                "GameState.turnLog accepted " + state.turnLog.Count +
                " entries with zero eviction. Long simulations and save files will bloat unboundedly.");
            Assert.True(state.turnLog.Count <= GameState.MaxCurrentTurnLogEntries,
                "Current-turn log flood exceeded MaxCurrentTurnLogEntries: " + state.turnLog.Count);
            Assert.Equal("entry " + (totalLogs - 1), state.turnLog[state.turnLog.Count - 1].message);
        }
    }
}
