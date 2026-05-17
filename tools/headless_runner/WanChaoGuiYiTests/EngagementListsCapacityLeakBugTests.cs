using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: EngagementRuntimeState holds two
    /// List<string> fields (attackerArmyIds, defenderArmyIds) that
    /// the war pipeline mutates each turn — Add, Remove, Clear. The
    /// pipeline never calls TrimExcess, and List<T> never shrinks
    /// its backing array on Remove/Clear. Over a 60-turn game with
    /// many engagements created and destroyed, peak list capacity
    /// remains pinned at the largest historical size even though
    /// the lists are usually empty.
    ///
    /// In the headless save (latest-war-report.json) and any UI
    /// that serialises a snapshot, the LIST CONTENTS are exported,
    /// so capacity leakage is invisible to the reporter. But the
    /// runtime memory cost is real: an active battle royale
    /// scenario could hold a 500-slot defenderArmyIds capacity for
    /// the rest of the game.
    ///
    /// Pinned invariant: when an engagement transitions from
    /// resolved to removed (cleared), its lists' backing arrays
    /// should be released or trimmed. Since EngagementRuntimeState
    /// is removed wholesale via MapState.RemoveEngagement, this
    /// test focuses on the simpler upstream invariant: after Clear,
    /// a List<string> in our domain types should not retain a
    /// >0 capacity for the rest of the engagement lifetime.
    /// </summary>
    public sealed class EngagementListsCapacityLeakBugTests
    {
        private readonly ITestOutputHelper output;

        public EngagementListsCapacityLeakBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void EngagementRuntimeState_Lists_Must_Not_Retain_Capacity_After_Cleanup()
        {
            EngagementRuntimeState eng = new EngagementRuntimeState
            {
                id = "e1",
                regionId = "r0"
            };

            // Stuff a battle royale's worth of armies into the lists.
            for (int i = 0; i < 500; i++) eng.attackerArmyIds.Add("atk_" + i);
            for (int i = 0; i < 500; i++) eng.defenderArmyIds.Add("def_" + i);

            int attackerCapacityAtPeak = eng.attackerArmyIds.Capacity;
            int defenderCapacityAtPeak = eng.defenderArmyIds.Capacity;

            // The DomainEngagementCleanup path calls RemoveAt for each
            // member as armies retreat or rout, ending up at 0 count.
            eng.attackerArmyIds.Clear();
            eng.defenderArmyIds.Clear();

            int attackerCapacityAfterClear = eng.attackerArmyIds.Capacity;
            int defenderCapacityAfterClear = eng.defenderArmyIds.Capacity;

            output.WriteLine("attacker capacity at peak: " + attackerCapacityAtPeak +
                             ", after clear: " + attackerCapacityAfterClear);
            output.WriteLine("defender capacity at peak: " + defenderCapacityAtPeak +
                             ", after clear: " + defenderCapacityAfterClear);

            // Either the war pipeline must call TrimExcess on Clear,
            // or EngagementRuntimeState must own the lists privately
            // and expose accessors that trim. Today the lists are
            // public List<string> fields — leaking 500-slot arrays
            // for the rest of GC's choosing.
            Assert.True(attackerCapacityAfterClear < 64,
                "attackerArmyIds retained capacity " + attackerCapacityAfterClear +
                " after Clear; this leaks for the engagement's lifetime.");
            Assert.True(defenderCapacityAfterClear < 64,
                "defenderArmyIds retained capacity " + defenderCapacityAfterClear +
                " after Clear; this leaks for the engagement's lifetime.");
        }
    }
}
