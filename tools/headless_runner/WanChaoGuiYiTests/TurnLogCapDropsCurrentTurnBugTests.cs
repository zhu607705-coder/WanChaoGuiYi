using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: GameState.AddLog now caps turnLog at
    /// MaxTurnLogEntries=2000 by RemoveAt(0).  But the cap is applied
    /// blindly: if a single turn writes >2000 entries (a contrived
    /// but possible case in heavy logistics scenarios with hundreds
    /// of armies), the eviction throws away THIS turn's earliest
    /// entries, not last turn's.  The "battle report" or "war log"
    /// UI that filters by current turn would then show a partial
    /// truncated list that begins mid-narrative.
    ///
    /// Pinned invariant: when entries from the current turn are added
    /// past the cap, the eviction policy must NOT delete entries
    /// whose `turn` equals the current GameState.turn.  Older turns
    /// must be evicted first.
    /// </summary>
    public sealed class TurnLogCapDropsCurrentTurnBugTests
    {
        private readonly ITestOutputHelper output;

        public TurnLogCapDropsCurrentTurnBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TurnLog_Cap_Must_Not_Drop_Current_Turn_Entries_While_Older_Exist()
        {
            GameState state = new GameState { turn = 1 };

            // Fill with 1500 entries from turn 1.
            for (int i = 0; i < 1500; i++) state.AddLog("test", "t1 entry " + i);

            // Advance to turn 2, log another 1500. Total writes = 3000;
            // cap is 2000, so 1000 entries got evicted.
            state.turn = 2;
            for (int i = 0; i < 1500; i++) state.AddLog("test", "t2 entry " + i);

            int turn1Surviving = 0;
            int turn2Surviving = 0;
            for (int i = 0; i < state.turnLog.Count; i++)
            {
                if (state.turnLog[i].turn == 1) turn1Surviving++;
                else if (state.turnLog[i].turn == 2) turn2Surviving++;
            }

            output.WriteLine("turn1 surviving: " + turn1Surviving);
            output.WriteLine("turn2 surviving: " + turn2Surviving);
            output.WriteLine("total: " + state.turnLog.Count);

            // The cap is soft across turns: preserving whole turn
            // chains is more important than slicing a past turn just to
            // hit 2000 exactly.  The hard safety bound is the
            // current-turn flood ceiling.
            Assert.True(state.turnLog.Count <= GameState.MaxCurrentTurnLogEntries);
            Assert.Equal(1500, turn2Surviving);
            Assert.True(turn1Surviving == 0 || turn1Surviving == 1500,
                "Older turn should be evicted as a whole group, not partially sliced.");

            // The real failure is when current-turn writes themselves
            // exceed the global soft cap during a single turn:
            state.turn = 3;
            int beforeFlood = state.turnLog.Count;
            for (int i = 0; i < 2500; i++) state.AddLog("test", "t3 entry " + i);

            int turn3Surviving = 0;
            for (int i = 0; i < state.turnLog.Count; i++)
            {
                if (state.turnLog[i].turn == 3) turn3Surviving++;
            }

            output.WriteLine("after t3 flood (2500 writes), t3 surviving: " + turn3Surviving + " of 2500 written");
            output.WriteLine("turn-2 entries remaining: " + (state.turnLog.Count - turn3Surviving));

            // A turn-aware soft cap should evict older turns first, then
            // temporarily allow the current turn to exceed the global cap
            // instead of starting the saved log mid-turn.
            int olderRemaining = state.turnLog.Count - turn3Surviving;
            Assert.Equal(0, olderRemaining);
            Assert.Equal(2500, turn3Surviving);
            Assert.Equal(2500, state.turnLog.Count);
            Assert.Equal("t3 entry 0", state.turnLog[0].message);
            Assert.Equal("t3 entry 2499", state.turnLog[state.turnLog.Count - 1].message);
        }
    }
}
