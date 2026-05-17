using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: NumericFormulas.CalculateBattlePower has
    /// never been PBT-tested. It applies (a) base attack/defense from
    /// unit, (b) equipment bonus, (c) log-soldier multiplier with floor,
    /// (d) morale-based multiplier (clamp01), (e) faction modifiers via
    /// NumericContext, (f) battle power scale.
    ///
    /// Pinned invariants (universal):
    ///   1. Result is always >= 0.
    ///   2. Result is always finite.
    ///   3. Zero soldiers => result still produces a non-negative
    ///      power (because of soldier floor multiplier).
    ///   4. Doubling soldiers (with same morale, same unit) does not
    ///      DECREASE power. (Monotone in soldiers above the floor.)
    /// </summary>
    public sealed class NumericFormulasBattlePowerPbtBugTests
    {
        private readonly ITestOutputHelper output;

        public NumericFormulasBattlePowerPbtBugTests(ITestOutputHelper output) { this.output = output; }

        private static int Power(int soldiers, int morale)
        {
            ArmyState army = new ArmyState { id = "a", soldiers = soldiers, morale = morale };
            UnitDefinition unit = new UnitDefinition
            {
                id = "u",
                stats = new UnitStats { attack = 10, defense = 10, mobility = 1, siege = 0 },
                upkeep = new CostSet()
            };
            FactionState faction = new FactionState
            {
                id = "f",
                taxMultiplier = 1f, foodMultiplier = 1f,
                armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f
            };
            return NumericFormulas.CalculateBattlePower(army, unit, faction, default(EquipmentBonus), true);
        }

        [Fact]
        public void Power_Is_NonNegative_For_All_Reasonable_Inputs()
        {
            int[] soldierCases = { 0, 1, 100, 1000, 100_000, 1_000_000 };
            int[] moraleCases = { 0, 25, 50, 75, 100 };
            for (int s = 0; s < soldierCases.Length; s++)
            {
                for (int m = 0; m < moraleCases.Length; m++)
                {
                    int p = Power(soldierCases[s], moraleCases[m]);
                    output.WriteLine("soldiers=" + soldierCases[s] + " morale=" + moraleCases[m] + " => " + p);
                    Assert.True(p >= 0, "Power negative for soldiers=" + soldierCases[s] + " morale=" + moraleCases[m]);
                }
            }
        }

        [Fact]
        public void Power_Is_Monotone_In_Soldiers_At_Same_Morale()
        {
            int prev = -1;
            int[] cases = { 1000, 5000, 10_000, 50_000, 100_000, 500_000 };
            for (int i = 0; i < cases.Length; i++)
            {
                int p = Power(cases[i], 70);
                output.WriteLine("soldiers=" + cases[i] + " => " + p);
                if (i > 0)
                {
                    Assert.True(p >= prev,
                        "Power not monotone: " + cases[i - 1] + " -> " + cases[i] +
                        " produced " + prev + " -> " + p);
                }
                prev = p;
            }
        }

        [Fact]
        public void Zero_Soldiers_Returns_NonNegative_Finite()
        {
            int p = Power(0, 70);
            output.WriteLine("zero soldiers power: " + p);
            Assert.True(p >= 0);
            Assert.True(p < int.MaxValue);
        }

        [Fact]
        public void Negative_Soldiers_Does_Not_Crash_Or_Return_Negative()
        {
            // Pathological: morale negative or soldiers negative
            // (shouldn't happen since we now clamp morale, but
            // soldiers is still public int).
            int p = Power(-100, 70);
            output.WriteLine("negative soldiers power: " + p);
            Assert.True(p >= 0);
        }
    }
}
