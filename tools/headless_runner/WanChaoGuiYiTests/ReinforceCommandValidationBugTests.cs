using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: MapCommandService.ReinforceArmy delegates
    /// straight to IssueRouteCommand with task=Reinforce. The promise
    /// of "reinforce" is "join an existing engagement at targetRegionId".
    /// But ReinforceArmy never validates that:
    ///   (a) targetRegionId actually has an engagement, OR
    ///   (b) the engagement has a side that the reinforcing army can
    ///       legitimately join (same faction).
    ///
    /// So a player can issue Reinforce to a peaceful frontier region
    /// or to a region currently hosting an enemy-only engagement, and
    /// the command service emits "正在前往 X 增援" log + ArmyMoveStarted
    /// event without any indication of impossibility. The army then
    /// behaves like a normal Move once it arrives.
    ///
    /// Pinned invariant: ReinforceArmy must reject the command (and
    /// not log a misleading "增援" message) if the target has no
    /// engagement OR the engagement has no friendly side.
    /// </summary>
    public sealed class ReinforceCommandValidationBugTests
    {
        private readonly ITestOutputHelper output;

        public ReinforceCommandValidationBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Reinforce_To_Peaceful_Region_Must_Be_Rejected()
        {
            FakeDataRepository data = new FakeDataRepository();
            data.EmperorMap["p"] = new EmperorDefinition { id = "p", name = "P", stats = new EmperorStats() };
            data.UnitMap["infantry"] = new UnitDefinition
            {
                id = "infantry",
                name = "Infantry",
                stats = new UnitStats { attack = 10, defense = 10, mobility = 1, siege = 0 },
                upkeep = new CostSet { money = 0, food = 0 }
            };
            string[] all = { "home", "front" };
            data.RegionMap["home"] = new RegionDefinition
            {
                id = "home",
                landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                legitimacyMemory = new[] { "civilian" },
                neighbors = new[] { "front" }
            };
            data.RegionMap["front"] = new RegionDefinition
            {
                id = "front",
                landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                legitimacyMemory = new[] { "civilian" },
                neighbors = new[] { "home" }
            };

            GameState state = new GameState
            {
                turn = 1,
                year = 1,
                season = Season.Spring,
                playerFactionId = "faction_p"
            };
            FactionState player = new FactionState
            {
                id = "faction_p", name = "P", emperorId = "p",
                taxMultiplier = 1f, foodMultiplier = 1f,
                armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f
            };
            state.factions.Add(player);
            foreach (string r in all)
            {
                state.regions.Add(new RegionState
                {
                    id = r,
                    ownerFactionId = player.id,
                    integration = 100,
                    occupationStatus = OccupationStatus.Controlled,
                    taxContributionPercent = 100,
                    foodContributionPercent = 100,
                    landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                    customs = new[] { "agrarian" }
                });
                player.regionIds.Add(r);
            }
            state.armies.Add(new ArmyState
            {
                id = "reinforcer",
                ownerFactionId = player.id,
                regionId = "home",
                unitId = "infantry",
                soldiers = 1000,
                morale = 70
            });

            WorldState world = WorldStateFactory.Create(state, data);
            GameContext context = new GameContext(state, data, new EventBus());
            MapQueryService queries = new MapQueryService(world.Map, new MapGraphData(data));
            MapCommandService commands = new MapCommandService(queries, context);

            // 'front' is peaceful (no engagement, no enemy army).
            // ReinforceArmy("reinforcer", "front") promises "join an
            // engagement" but no engagement exists. The command
            // should reject — otherwise the log says "增援" without
            // a target to join.
            int logsBefore = state.turnLog.Count;
            bool accepted = commands.ReinforceArmy("reinforcer", "front");
            string lastLog = state.turnLog.Count > logsBefore
                ? state.turnLog[state.turnLog.Count - 1].message
                : "";

            output.WriteLine("reinforce to peaceful region accepted? " + accepted);
            output.WriteLine("last log: " + lastLog);

            Assert.False(accepted,
                "ReinforceArmy accepted a command targeting a peaceful region; " +
                "no engagement exists to reinforce. Last log: " + lastLog);
        }
    }
}
