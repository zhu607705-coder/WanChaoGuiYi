using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Historical bug: ArmyState.morale and ArmyRuntimeState.morale
    /// used to be assignment paths where out-of-range values could
    /// survive until a later consumer clamped on read. Both legacy and
    /// runtime morale are now properties, so this pins the setter-level
    /// clamp directly.
    ///
    /// Pinned invariant: morale must stay in [0, 100] regardless of
    /// the assignment path.
    /// </summary>
    public sealed class ArmyMoraleClampBugTests
    {
        private readonly ITestOutputHelper output;

        public ArmyMoraleClampBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void Army_Morale_Must_Stay_In_Range_After_Assignment()
        {
            ArmyState army = new ArmyState { morale = 70 };
            army.morale = -50;
            int afterNegative = army.morale;

            army.morale = 250;
            int afterOverflow = army.morale;

            output.WriteLine("after -50: " + afterNegative);
            output.WriteLine("after 250: " + afterOverflow);

            Assert.True(afterNegative >= 0,
                "ArmyState.morale = -50 produced " + afterNegative + ". Negative morale is undefined.");
            Assert.True(afterOverflow <= 100,
                "ArmyState.morale = 250 produced " + afterOverflow + ". Morale > 100 breaks battle power formulas.");
        }

        [Fact]
        public void Runtime_Army_Morale_Must_Stay_In_Range_After_Assignment()
        {
            ArmyRuntimeState army = new ArmyRuntimeState { morale = 70 };
            army.morale = -50;
            int afterNegative = army.morale;

            army.morale = 250;
            int afterOverflow = army.morale;

            output.WriteLine("runtime after -50: " + afterNegative);
            output.WriteLine("runtime after 250: " + afterOverflow);

            Assert.True(afterNegative >= 0,
                "ArmyRuntimeState.morale = -50 produced " + afterNegative + ". Negative runtime morale is undefined.");
            Assert.True(afterOverflow <= 100,
                "ArmyRuntimeState.morale = 250 produced " + afterOverflow + ". Runtime morale > 100 breaks battle power formulas.");
        }
    }
}
