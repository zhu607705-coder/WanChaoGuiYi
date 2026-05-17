using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: GameState.diplomaticRelations is a
    /// List<DiplomaticRelation>, public, no uniqueness invariant.
    /// Two relations between (factionA, factionB) and (factionB,
    /// factionA) can be created independently. Or two events firing
    /// in the same turn can both publish a "war declared" relation
    /// without dedup. The relation lookup code (when it exists) will
    /// pick whichever is first, leaving the second as a phantom.
    ///
    /// Pinned invariant: there must be at most ONE relation per
    /// unordered pair (factionA, factionB). Today nothing enforces.
    /// </summary>
    public sealed class DiplomaticRelationDuplicateBugTests
    {
        private readonly ITestOutputHelper output;

        public DiplomaticRelationDuplicateBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void Adding_Reciprocal_Relations_Must_Not_Create_Duplicates()
        {
            GameState state = new GameState();
            state.diplomaticRelations.Add(new DiplomaticRelation
            {
                factionA = "alpha", factionB = "beta",
                status = DiplomacyStatus.Alliance, opinion = 50
            });
            state.diplomaticRelations.Add(new DiplomaticRelation
            {
                factionA = "beta", factionB = "alpha",
                status = DiplomacyStatus.AtWar, opinion = -80
            });

            // After adding two relations on the same unordered pair,
            // the count should be 1 — the second add should either
            // reject or replace.
            int count = 0;
            for (int i = 0; i < state.diplomaticRelations.Count; i++)
            {
                DiplomaticRelation r = state.diplomaticRelations[i];
                if ((r.factionA == "alpha" && r.factionB == "beta") ||
                    (r.factionA == "beta" && r.factionB == "alpha"))
                {
                    count++;
                }
            }

            output.WriteLine("relations on alpha-beta pair: " + count);
            Assert.Equal(1, count);
        }
    }
}
