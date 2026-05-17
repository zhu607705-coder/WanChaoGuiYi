using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: GameState is [Serializable] and the Web
    /// strategy map ships export/import that round-trips it through JSON
    /// (localStorage, save slots).  But several invariants critical to
    /// gameplay are NOT guarded by serialization:
    ///   1. GameState.runtimeMap is [NonSerialized] — round-tripping
    ///      drops the runtime mirror entirely. Any code that touches
    ///      MapState immediately after a load operates on null.
    ///   2. There is no version tag on the JSON shape.  Schema drift
    ///      between releases (e.g. renaming a faction multiplier) will
    ///      silently coerce or zero-fill missing properties.
    ///   3. FactionState.regionIds, RegionState.customs etc. are
    ///      List<string>/string[] — System.Text.Json deserializes them
    ///      as new instances, so reference-equality assumptions break.
    ///
    /// Pinned invariant: serialize a GameState to JSON, deserialize it
    /// back, and require that the recovered state passes a basic
    /// integrity check:
    ///   - factions count and ids match
    ///   - regions count and ids match, and each region's ownerFactionId
    ///     still resolves to a faction
    ///   - turnLog round-trips equal-length
    ///
    /// We use System.Text.Json with IncludeFields (the same options the
    /// real headless harness uses) to mirror the deployed code path.
    /// </summary>
    public sealed class GameStateRoundTripBugTests
    {
        private readonly ITestOutputHelper output;

        public GameStateRoundTripBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void GameState_Must_Survive_Json_RoundTrip_With_Integrity()
        {
            FakeDataRepository data;
            GameState original = TestFixtures.BuildSinglePlayerWorld(3, out data);
            original.AddLog("test", "round-trip line A");
            original.AddLog("test", "round-trip line B");

            JsonSerializerOptions opts = new JsonSerializerOptions
            {
                IncludeFields = true,
                WriteIndented = false
            };

            string json = JsonSerializer.Serialize(original, opts);
            GameState revived = JsonSerializer.Deserialize<GameState>(json, opts);

            output.WriteLine("json length: " + json.Length);
            output.WriteLine("original factions: " + original.factions.Count + " regions: " + original.regions.Count + " logs: " + original.turnLog.Count);
            output.WriteLine("revived  factions: " + (revived?.factions?.Count ?? -1) + " regions: " + (revived?.regions?.Count ?? -1) + " logs: " + (revived?.turnLog?.Count ?? -1));

            Assert.NotNull(revived);
            Assert.Equal(original.factions.Count, revived.factions.Count);
            Assert.Equal(original.regions.Count, revived.regions.Count);
            Assert.Equal(original.turnLog.Count, revived.turnLog.Count);

            // Cross-reference: every revived region's owner must still
            // resolve to a revived faction.
            HashSet<string> revivedFactionIds = new HashSet<string>();
            for (int i = 0; i < revived.factions.Count; i++) revivedFactionIds.Add(revived.factions[i].id);
            for (int i = 0; i < revived.regions.Count; i++)
            {
                string ownerId = revived.regions[i].ownerFactionId;
                Assert.True(revivedFactionIds.Contains(ownerId),
                    "Revived region '" + revived.regions[i].id + "' owner '" + ownerId + "' is not in revived factions.");
            }

            // Cross-reference: every faction's regionIds must resolve to
            // a revived region.
            HashSet<string> revivedRegionIds = new HashSet<string>();
            for (int i = 0; i < revived.regions.Count; i++) revivedRegionIds.Add(revived.regions[i].id);
            for (int i = 0; i < revived.factions.Count; i++)
            {
                List<string> ids = revived.factions[i].regionIds;
                if (ids == null) continue;
                for (int j = 0; j < ids.Count; j++)
                {
                    Assert.True(revivedRegionIds.Contains(ids[j]),
                        "Revived faction '" + revived.factions[i].id + "' references region '" + ids[j] + "' which is not in revived regions.");
                }
            }
        }
    }
}
