using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: EngagementRuntimeState holds two
    /// CompactStringList fields (attackerArmyIds, defenderArmyIds)
    /// that the war pipeline mutates each turn — Add, Remove, Clear.
    /// Clear must release the backing array so old peak engagement
    /// sizes do not pin memory for the rest of the engagement lifetime.
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
    /// should be released. Since EngagementRuntimeState is removed
    /// wholesale via MapState.RemoveEngagement, this test focuses on
    /// the simpler upstream invariant: after Clear, CompactStringList
    /// capacity must return to 0.
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

            // CompactStringList.Clear() must trim the backing array,
            // not merely reduce Count while retaining peak capacity.
            Assert.True(attackerCapacityAfterClear == 0,
                "attackerArmyIds retained capacity " + attackerCapacityAfterClear +
                " after Clear; this leaks for the engagement's lifetime.");
            Assert.True(defenderCapacityAfterClear == 0,
                "defenderArmyIds retained capacity " + defenderCapacityAfterClear +
                " after Clear; this leaks for the engagement's lifetime.");
        }
    }
}
