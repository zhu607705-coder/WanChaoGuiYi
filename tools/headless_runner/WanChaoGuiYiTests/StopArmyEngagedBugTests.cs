using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: MapCommandService.StopArmy already
    /// rejects engaged armies. Good. But it doesn't reject armies
    /// that are MID-ROUTE. If an army is task=Move with a 5-step
    /// route and the player issues StopArmy, the entire route is
    /// cleared — there's no warning that the army will sit idle
    /// in enemy territory.
    ///
    /// Pinned invariant: StopArmy on an army NOT at its origin
    /// (army.locationRegionId != some "home" or owned region) must
    /// at least log "stranded in non-owned territory" so the player
    /// has a chance to react.
    /// </summary>
    public sealed class StopArmyEngagedBugTests
    {
        private readonly ITestOutputHelper output;

        public StopArmyEngagedBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void StopArmy_Mid_Enemy_Territory_Must_Log_Warning()
        {
            FakeDataRepository data = new FakeDataRepository();
            data.EmperorMap["p"] = new EmperorDefinition { id = "p", name = "P", stats = new EmperorStats() };
            data.EmperorMap["e"] = new EmperorDefinition { id = "e", name = "E", stats = new EmperorStats() };
            data.UnitMap["infantry"] = new UnitDefinition
            {
                id = "infantry", name = "Infantry",
                stats = new UnitStats { attack = 10, defense = 10, mobility = 1, siege = 0 },
                upkeep = new CostSet()
            };
            data.RegionMap["home"] = new RegionDefinition
            {
                id = "home",
                landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                legitimacyMemory = new[] { "civilian" },
                neighbors = new[] { "enemy_land" }
            };
            data.RegionMap["enemy_land"] = new RegionDefinition
            {
                id = "enemy_land",
                landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                legitimacyMemory = new[] { "civilian" },
                neighbors = new[] { "home" }
            };

            GameState state = new GameState
            {
                turn = 1, year = 1, season = Season.Spring,
                playerFactionId = "faction_p"
            };
            FactionState p = new FactionState { id = "faction_p", name = "P", emperorId = "p", taxMultiplier = 1f, foodMultiplier = 1f, armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f };
            FactionState e = new FactionState { id = "faction_e", name = "E", emperorId = "e", taxMultiplier = 1f, foodMultiplier = 1f, armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f };
            state.factions.Add(p);
            state.factions.Add(e);
            state.regions.Add(new RegionState { id = "home", ownerFactionId = p.id, integration = 100, taxContributionPercent = 100, foodContributionPercent = 100, landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f }, customs = new[] { "agrarian" } });
            state.regions.Add(new RegionState { id = "enemy_land", ownerFactionId = e.id, integration = 100, taxContributionPercent = 100, foodContributionPercent = 100, landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f }, customs = new[] { "agrarian" } });
            p.regionIds.Add("home");
            e.regionIds.Add("enemy_land");
            state.armies.Add(new ArmyState { id = "scout", ownerFactionId = p.id, regionId = "enemy_land", unitId = "infantry", soldiers = 1000, morale = 70 });

            WorldState world = WorldStateFactory.Create(state, data);
            GameContext context = new GameContext(state, data, new EventBus());
            MapQueryService queries = new MapQueryService(world.Map, new MapGraphData(data));
            MapCommandService commands = new MapCommandService(queries, context);

            ArmyRuntimeState scout = world.Map.ArmiesById["scout"];
            scout.task = ArmyTask.Move;
            scout.targetRegionId = "home";

            int logsBefore = state.turnLog.Count;
            bool stopped = commands.StopArmy("scout");
            string lastLog = state.turnLog.Count > logsBefore ? state.turnLog[state.turnLog.Count - 1].message : "";

            output.WriteLine("stopped? " + stopped);
            output.WriteLine("last log: " + lastLog);

            Assert.True(stopped, "StopArmy should accept the command.");
            // Pin: the log must mention 敌方/搁浅/异域 — anything that
            // signals "you just stranded an army in enemy land".
            // Today the log just says "停止行动" without distinction.
            bool warned = lastLog.Contains("敌方") || lastLog.Contains("搁浅") || lastLog.Contains("异域") ||
                          lastLog.Contains("stranded") || lastLog.Contains("非己方");
            Assert.True(warned,
                "StopArmy on an army in non-owned territory wrote no stranded-warning log. " +
                "Last log: " + lastLog);
        }
    }
}
