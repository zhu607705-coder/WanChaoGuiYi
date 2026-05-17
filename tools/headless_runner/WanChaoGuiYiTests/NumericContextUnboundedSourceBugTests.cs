using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: NumericContext.AddOrReplace was added
    /// to make duplicate (domain, stat, type, source) tuples idempotent.
    /// But the public Add(...) overload still accepts arbitrary string
    /// sources. A malicious or buggy event loop can flood the context
    /// with thousands of UNIQUE source ids, causing:
    ///
    ///   1. modifiers list grows linearly
    ///   2. NumericResult.overrideSources string array grows linearly
    ///      (one allocation per Evaluate, copied to a new array each
    ///      call)
    ///   3. AddOrReplace is O(n) per call; n^2 total when N events fire
    ///
    /// In the headless 16-scenario run today this is dormant. But
    /// chronicle events + tech effects + AI plans can easily reach
    /// hundreds of modifiers. The point of this test is to pin a
    /// soft upper bound: a single context after 10,000 unique-source
    /// adds should still respond within a sane time AND not grow
    /// overrideSources linearly on every Evaluate.
    ///
    /// Pinned invariant (focused on the visible API): Evaluate() should
    /// return overrideSources whose length is at most overrideCount —
    /// if a future fix collapses sources, this still holds; today it
    /// holds. The real attack: stack 10,000 unique Override sources
    /// and require that overrideSources reports an unambiguous winner
    /// without leaking memory by retaining all 10,000 source strings.
    /// </summary>
    public sealed class NumericContextUnboundedSourceBugTests
    {
        private readonly ITestOutputHelper output;

        public NumericContextUnboundedSourceBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Many_Unique_Override_Sources_Must_Not_Bloat_OverrideSources()
        {
            NumericContext ctx = new NumericContext();
            const int count = 10_000;
            for (int i = 0; i < count; i++)
            {
                ctx.Add(NumericDomain.Economy, NumericStat.TaxIncome, NumericModifierType.Override,
                    100f + i, "src_" + i);
            }

            NumericResult result = ctx.Evaluate(NumericDomain.Economy, NumericStat.TaxIncome, 0f);
            output.WriteLine("overrideCount: " + result.overrideCount);
            output.WriteLine("overrideSources.Length: " + result.overrideSources.Length);
            output.WriteLine("overrideValue: " + result.overrideValue);

            // 10,000 unique override sources today produce
            //   overrideCount = 10000
            //   overrideSources.Length = 10000
            //   one allocation per Evaluate() call.
            //
            // Pin a sane upper bound: even when 10,000 sources have
            // been added, the result should not ship 10,000 source
            // strings to every consumer. Either the engine collapses
            // by source on Add, or Evaluate caps the reported list to
            // a digestible size (e.g. <=64 unique winners).
            Assert.True(result.overrideSources.Length <= 64,
                "overrideSources reports " + result.overrideSources.Length +
                " entries; consumers (UI, save, diagnostics) cannot reasonably display that. " +
                "Cap or deduplicate.");
        }
    }
}
