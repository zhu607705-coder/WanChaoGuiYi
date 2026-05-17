using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: GameState.AddLog now uses turn-aware
    /// pruning. But the cap is enforced only on the WRITE path. When a
    /// serialized save is deserialized, the turnLog can already be
    /// well past 2000 entries (unbounded from a pre-cap save, or from
    /// a tampered save). The first AddLog after deserialization will
    /// then prune — but the prune logic targets entries whose
    /// `turn != current turn`. If the save's turn marker doesn't
    /// match the entries' turn fields (which is exactly what happens
    /// after a load: GameState.turn might be 50 while the log has
    /// entries from turns 1..50), the prune will work fine here. But
    /// the soft cap of 2000 will still drop entries from MULTIPLE
    /// past turns in one shot, potentially evicting an entire turn's
    /// causal chain (e.g. all turn-12 governance entries are gone
    /// while turn-11 entries survive partially).
    ///
    /// Pinned invariant: on the first prune after deserialization,
    /// the eviction policy must not partially evict a single past
    /// turn; either ALL entries from that turn survive or NONE do.
    /// </summary>
    public sealed class TurnLogPruneAfterDeserializationBugTests
    {
        private readonly ITestOutputHelper output;

        public TurnLogPruneAfterDeserializationBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void First_AddLog_After_Load_Must_Not_Partially_Evict_A_Past_Turn()
        {
            // Build a state whose turnLog has 2500 entries spread
            // evenly across turns 1..5 (500 each), then jump to turn
            // 50 and AddLog once — simulating a load + first action.
            GameState state = new GameState { turn = 5 };
            for (int t = 1; t <= 5; t++)
            {
                state.turn = t;
                for (int i = 0; i < 500; i++) state.AddLog("test", "t" + t + " e" + i);
            }
            // Sanity check: no eviction yet because we are within 2000
            // current-turn cap and total ≤ 2500 stays under hard cap.
            int totalAfterFill = state.turnLog.Count;
            output.WriteLine("after fill: " + totalAfterFill + " entries");

            // Simulate "loaded save with turn=50, log carries 2500 historic entries"
            state.turn = 50;
            state.AddLog("test", "first action after load");

            int total = state.turnLog.Count;
            int[] perTurn = new int[60];
            for (int i = 0; i < state.turnLog.Count; i++)
            {
                int t = state.turnLog[i].turn;
                if (t >= 0 && t < perTurn.Length) perTurn[t]++;
            }

            output.WriteLine("after first add: total=" + total);
            for (int t = 0; t < perTurn.Length; t++)
            {
                if (perTurn[t] > 0) output.WriteLine("  turn " + t + " surviving: " + perTurn[t]);
            }

            // Find any past turn that has been *partially* evicted:
            // some entries survive but not all 500.
            int partialTurn = -1;
            int partialCount = 0;
            for (int t = 1; t <= 5; t++)
            {
                if (perTurn[t] > 0 && perTurn[t] < 500)
                {
                    partialTurn = t;
                    partialCount = perTurn[t];
                    break;
                }
            }

            output.WriteLine("partially evicted turn: " + partialTurn + " (count=" + partialCount + ")");

            Assert.True(partialTurn == -1,
                "Turn " + partialTurn + " has " + partialCount + " surviving entries (expected all-or-nothing). " +
                "The save log was partially evicted, breaking causal chains for that turn's report.");
        }
    }
}
