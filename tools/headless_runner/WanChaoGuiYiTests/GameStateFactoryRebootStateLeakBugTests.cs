using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: calling GameStateFactory.CreateDefault
    /// twice in a row (e.g. "restart game") produces two GameStates.
    /// We pinned previously that runtime maps don't share instances.
    /// But what about tableau-level state: NumericTuning constants are
    /// fine (truly const), but any STATIC mutable field inside Domain
    /// would persist across "games".
    ///
    /// Pinned invariant: after CreateDefault then CreateDefault, the
    /// second state's faction.legitimacy/money/food MUST equal a
    /// fresh instance's defaults — no carry-over from the first
    /// state's mutations.
    /// </summary>
    public sealed class GameStateFactoryRebootStateLeakBugTests
    {
        private readonly ITestOutputHelper output;

        public GameStateFactoryRebootStateLeakBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void Second_CreateDefault_Must_Not_Inherit_First_GameState_Mutations()
        {
            FakeDataRepository data;
            GameState first = TestFixtures.BuildSinglePlayerWorld(2, out data);

            // Mutate the first state.
            FactionState firstFaction = first.factions[0];
            int firstMoneyOriginal = firstFaction.money;
            firstFaction.money = -9999;
            firstFaction.legitimacy = 5;
            firstFaction.successionRisk = 95;

            // Build a second state with the SAME data repo.
            FakeDataRepository data2;
            GameState second = TestFixtures.BuildSinglePlayerWorld(2, out data2);
            FactionState secondFaction = second.factions[0];

            output.WriteLine("first faction.money: " + firstFaction.money);
            output.WriteLine("second faction.money: " + secondFaction.money);

            Assert.Equal(firstMoneyOriginal, secondFaction.money);
            Assert.Equal(60, secondFaction.legitimacy); // default from BuildSinglePlayerWorld
            Assert.NotEqual(95, secondFaction.successionRisk);
        }
    }
}
