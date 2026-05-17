using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: GameState.ChangeRegionOwner uses
    /// `previousOwner.regionIds.Remove(regionId)` on a List&lt;string&gt;.
    /// List.Remove only removes the FIRST occurrence.  If a region id
    /// gets accidentally double-added to a faction's regionIds
    /// (factory bug, save/load round-trip, modder data, etc.), then
    /// a single ownership transfer leaves the duplicate behind.  Every
    /// economy/military pass that loops over regionIds will then double
    /// count that region.
    ///
    /// Pinned invariant: after ChangeRegionOwner, neither the previous
    /// owner nor the new owner should contain the region id more than
    /// once in their regionIds list.  And the previous owner should
    /// not contain it AT ALL.
    /// </summary>
    public sealed class FactionRegionIdsDuplicationBugTests
    {
        private readonly ITestOutputHelper output;

        public FactionRegionIdsDuplicationBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void ChangeRegionOwner_Must_Purge_All_Duplicate_Entries_From_Previous_Owner()
        {
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(2, out data);
            FactionState player = state.factions[0];

            data.EmperorMap["enemy"] = new EmperorDefinition { id = "enemy", name = "E", stats = new EmperorStats() };
            FactionState enemy = new FactionState
            {
                id = "faction_enemy", name = "E", emperorId = "enemy",
                taxMultiplier = 1f, foodMultiplier = 1f,
                armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f
            };
            state.factions.Add(enemy);

            // Inject a duplicate (mimics a save/load or factory glitch).
            // TestFixtures.BuildSinglePlayerWorld already has player owning ["r0", "r1"].
            player.regionIds.Add("r0");
            int duplicatesBefore = CountOccurrences(player.regionIds, "r0");
            output.WriteLine("duplicates before transfer: " + duplicatesBefore);
            Assert.Equal(2, duplicatesBefore);

            state.ChangeRegionOwner("r0", enemy.id);

            int duplicatesAfter = CountOccurrences(player.regionIds, "r0");
            int newOwnerCount = CountOccurrences(enemy.regionIds, "r0");
            output.WriteLine("duplicates remaining on previous owner: " + duplicatesAfter);
            output.WriteLine("count on new owner:                     " + newOwnerCount);

            Assert.Equal(0, duplicatesAfter);
            Assert.Equal(1, newOwnerCount);
        }

        private static int CountOccurrences(System.Collections.Generic.List<string> list, string value)
        {
            int n = 0;
            for (int i = 0; i < list.Count; i++) if (list[i] == value) n++;
            return n;
        }
    }
}
