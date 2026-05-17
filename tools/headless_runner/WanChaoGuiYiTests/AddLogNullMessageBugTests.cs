using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: GameState.AddLog(category, message)
    /// accepts arbitrary strings including null/empty. Worst case:
    /// a buggy formatter passes null when string interpolation hits
    /// a missing field. The TurnLogEntry then carries null which
    /// breaks any reader that does message.Contains(...).
    /// </summary>
    public sealed class AddLogNullMessageBugTests
    {
        private readonly ITestOutputHelper output;

        public AddLogNullMessageBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void AddLog_With_Null_Message_Must_Not_Be_Recorded_As_Null()
        {
            GameState state = new GameState { turn = 1 };
            state.AddLog(null, null);
            state.AddLog("test", null);
            state.AddLog(null, "test");

            for (int i = 0; i < state.turnLog.Count; i++)
            {
                TurnLogEntry e = state.turnLog[i];
                output.WriteLine("entry " + i + ": cat='" + (e.category ?? "<null>") + "' msg='" + (e.message ?? "<null>") + "'");

                Assert.NotNull(e.category);
                Assert.NotNull(e.message);
            }
        }
    }
}
