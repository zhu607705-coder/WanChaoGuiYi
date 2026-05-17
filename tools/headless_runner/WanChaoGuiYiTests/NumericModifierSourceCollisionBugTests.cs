using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: NumericContext.Add appends modifiers
    /// to a list and never deduplicates by source.  If the same source
    /// (e.g. an event id, a tech id, an AI flag) is published twice in
    /// the same turn — which is a real risk because EventBus has no
    /// idempotency guarantee — its Additive value is doubled, its
    /// Multiplicative is doubled (since multiplicative is *summed*
    /// into a +delta in NumericEngine!), and its Override is silently
    /// shadowed by whichever Add came last.
    ///
    /// Pinned invariant: Adding the SAME (domain, stat, type, source)
    /// twice with the same value must not double its effective impact.
    /// Either NumericContext rejects the duplicate, or it must collapse
    /// duplicates by source on Evaluate.
    ///
    /// Note: this is independent of the override-conflict signal added
    /// in the previous round.  That one detected DIFFERENT overrides;
    /// this detects IDENTICAL re-additions.
    /// </summary>
    public sealed class NumericModifierSourceCollisionBugTests
    {
        private readonly ITestOutputHelper output;

        public NumericModifierSourceCollisionBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Same_Source_Added_Twice_Must_Not_Double_The_Effect()
        {
            NumericContext ctx = new NumericContext();

            // Same source, same value, added twice. Should be idempotent.
            ctx.Add(NumericDomain.Economy, NumericStat.TaxIncome, NumericModifierType.Additive, 30f, "tech_legalism");
            ctx.Add(NumericDomain.Economy, NumericStat.TaxIncome, NumericModifierType.Additive, 30f, "tech_legalism");

            NumericResult result = ctx.Evaluate(NumericDomain.Economy, NumericStat.TaxIncome, 100f);

            output.WriteLine("baseValue: " + result.baseValue);
            output.WriteLine("additive:  " + result.additive);
            output.WriteLine("finalValue: " + result.finalValue);

            // Today: additive = 60, finalValue = 160.
            // Desired: additive = 30, finalValue = 130 (idempotent on
            // duplicate (domain, stat, type, source)).
            Assert.Equal(30f, result.additive);
            Assert.Equal(130f, result.finalValue);
        }
    }
}
