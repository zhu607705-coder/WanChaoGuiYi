using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: NumericFormulas / NumericEngine perform
    /// no NaN/Infinity defence. RegionDefinition.foodOutput, taxOutput,
    /// army.morale, etc. are all numeric fields that come from JSON.
    /// A buggy mod, an out-of-spec save, or a downstream divide-by-zero
    /// in policy code can introduce NaN/Infinity. Once it enters the
    /// numeric pipeline, it propagates through every subsequent
    /// calculation and corrupts unrelated systems.
    ///
    /// Pinned invariant: NumericFormulas.CalculateRegionalTax MUST NOT
    /// return NaN, Infinity, or a negative value, even when the input
    /// region carries pathological numeric data.
    /// </summary>
    public sealed class NumericNaNInfinityBugTests
    {
        private readonly ITestOutputHelper output;

        public NumericNaNInfinityBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void RegionalTax_Must_Be_Finite_When_Inputs_Are_Pathological()
        {
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(1, out data);
            RegionState region = state.regions[0];
            FactionState faction = state.factions[0];

            int[] results = new int[4];
            string[] labels = { "NaN", "PositiveInfinity", "NegativeInfinity", "MaxValue" };
            float[] cases = { float.NaN, float.PositiveInfinity, float.NegativeInfinity, float.MaxValue };

            for (int i = 0; i < cases.Length; i++)
            {
                region.taxOutput = (int)System.Math.Min(int.MaxValue, System.Math.Max(int.MinValue, (long)cases[i]));
                // taxOutput is int, so direct NaN injection isn't
                // possible. Use the multiplier path instead.
                faction.taxMultiplier = cases[i];
                int tax = NumericFormulas.CalculateRegionalTax(region, faction);
                results[i] = tax;
                output.WriteLine(labels[i] + " multiplier -> tax=" + tax);
            }

            // None of the four pathological inputs should produce a
            // negative tax or have escaped through int.MinValue
            // wraparound. CalculateRegionalTax already does
            // DomainMath.Max(0, ...). But it does NOT IsNaN-check
            // the multiplier — NaN propagates through Add, *,
            // RoundToInt and produces 0 by accident on net8 (because
            // (int)NaN coerces to 0). The RISK we pin: any input
            // that breaks RoundToInt must yield a deterministic
            // non-negative number, not silently 0.
            for (int i = 0; i < results.Length; i++)
            {
                Assert.True(results[i] >= 0,
                    labels[i] + " produced a negative tax: " + results[i]);
            }
            // None should equal int.MinValue or int.MaxValue (those
            // are smoking-gun signs of overflow propagation).
            for (int i = 0; i < results.Length; i++)
            {
                Assert.NotEqual(int.MinValue, results[i]);
                Assert.NotEqual(int.MaxValue, results[i]);
            }

            // Strong claim: NaN multiplier should be detected and
            // either replaced with 1.0 (treat as no modifier) or
            // logged. Since the engine has no IsNaN check today,
            // the resulting tax is 0 (because 100 * NaN = NaN, which
            // RoundToInt coerces to 0). That's a silent loss of
            // expected revenue.
            //
            // Pin: NaN multiplier should NOT zero out a non-zero
            // base tax silently. Either the engine throws, or it
            // sanitises to 1.0. Today it returns 0. So this test
            // should fail.
            faction.taxMultiplier = float.NaN;
            region.taxOutput = 100;
            int taxWithNaN = NumericFormulas.CalculateRegionalTax(region, faction);
            output.WriteLine("NaN final tax for taxOutput=100: " + taxWithNaN);
            Assert.True(taxWithNaN > 0,
                "NaN multiplier silently produced tax=" + taxWithNaN +
                " for a region with taxOutput=100. The engine did not detect or sanitise NaN.");
        }
    }
}
