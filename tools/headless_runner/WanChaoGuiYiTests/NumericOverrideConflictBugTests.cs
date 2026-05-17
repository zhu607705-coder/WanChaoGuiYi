using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: NumericEngine.Evaluate processes Override
    /// modifiers in iteration order; each override silently replaces the
    /// previous one.  When two systems (e.g. an event AND a tech) both
    /// publish an Override for the same stat, the second wins without
    /// any signal that a conflict happened.  In a strategy game this is
    /// exactly the situation that produces unreproducible balance bugs.
    ///
    /// Pinned invariant:
    ///   When two non-equal Overrides apply to the same (domain, stat),
    ///   the system must either reject the conflict, log it, or expose
    ///   the count of overrides on the NumericResult.  Today the
    ///   NumericResult only carries `hasOverride` (bool) and
    ///   `overrideValue` (single float).  There is no way for any caller
    ///   to detect that two overrides collided.
    /// </summary>
    public sealed class NumericOverrideConflictBugTests
    {
        private readonly ITestOutputHelper output;

        public NumericOverrideConflictBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Two_Conflicting_Overrides_Must_Be_Detectable()
        {
            NumericContext ctx = new NumericContext();
            ctx.Add(NumericDomain.Economy, NumericStat.TaxIncome, NumericModifierType.Override, 50f, "tech_legalism");
            ctx.Add(NumericDomain.Economy, NumericStat.TaxIncome, NumericModifierType.Override, 200f, "event_grand_minister");

            NumericResult result = ctx.Evaluate(NumericDomain.Economy, NumericStat.TaxIncome, 100f);

            output.WriteLine("hasOverride: " + result.hasOverride);
            output.WriteLine("overrideValue: " + result.overrideValue);
            output.WriteLine("finalValue: " + result.finalValue);

            // Today: hasOverride=true, overrideValue=200 (last wins),
            // finalValue=200.  No way to know that 50 was thrown away
            // and which source produced the survivor.
            //
            // We pin the desired observable property: when conflicting
            // overrides exist, the NumericResult must EITHER expose
            // them as distinct fields (e.g. override count > 1, or a
            // list of contributing sources), OR the second Add call
            // must throw / return false to signal conflict.
            //
            // Since the current API returns void from Add and has no
            // override count, this assertion fails outright.  When
            // someone wants to fix the bug, they can either add an
            // overrideCount field or change Add to return a bool.

            // Heuristic check: if both overrides really registered,
            // there should be at least *some* mechanism to inspect
            // that (count, source list, or rejection on second Add).
            // None of these exist today, so the assertion fails.

            // Use reflection to detect a count-style field.  If the
            // bugfix landed by exposing overrideCount or
            // overrideSources, this test will start passing
            // automatically.
            System.Reflection.FieldInfo countField = typeof(NumericResult).GetField("overrideCount");
            System.Reflection.FieldInfo sourcesField = typeof(NumericResult).GetField("overrideSources");

            bool anyConflictSignal = countField != null || sourcesField != null;
            output.WriteLine("NumericResult.overrideCount field present?   " + (countField != null));
            output.WriteLine("NumericResult.overrideSources field present? " + (sourcesField != null));

            Assert.True(anyConflictSignal,
                "NumericResult exposes no way to detect that two Overrides collided. " +
                "Either add `overrideCount`/`overrideSources`, or make `Add` return false on conflict.");
        }
    }
}
