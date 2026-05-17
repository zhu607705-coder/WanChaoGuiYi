using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: ArmyState.morale and
    /// ArmyRuntimeState.morale are public int fields with no clamp.
    /// Battle losses, frontier supply attrition, and emperor traits
    /// modify morale through assignment; nothing stops it from going
    /// negative (despondent past death) or above 100 (rallying past
    /// max). NumericFormulas.CalculateBattlePower clamps via
    /// Clamp01(morale/100f) on read, but that's only one consumer.
    /// UI, save export, and AI scoring read raw morale.
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
    }
}
