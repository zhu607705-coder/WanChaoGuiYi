using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: DomainBattleSimulationSystem is
    /// expected to be deterministic — same input, same output.
    /// Recent changes added supply-pressure modifiers and dominance
    /// ratios. Verify the determinism contract by running the same
    /// engagement TWICE on two freshly-built worlds and comparing
    /// not just attackerWon, but the exact attackerSoldiersAfter
    /// and defenderSoldiersAfter.
    ///
    /// Pinned invariant: identical initial conditions produce
    /// byte-equal BattleResult.
    /// </summary>
    public sealed class BattleSimulationDeterministicBugTests
    {
        private readonly ITestOutputHelper output;

        public BattleSimulationDeterministicBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void Same_Engagement_Produces_Same_BattleResult_Twice()
        {
            BattleResult run1 = RunOne();
            BattleResult run2 = RunOne();
            output.WriteLine("run1: power " + run1.attackerPower + " v " + run1.defenderPower +
                ", soldiers after " + run1.attackerSoldiersAfter + " v " + run1.defenderSoldiersAfter);
            output.WriteLine("run2: power " + run2.attackerPower + " v " + run2.defenderPower +
                ", soldiers after " + run2.attackerSoldiersAfter + " v " + run2.defenderSoldiersAfter);

            Assert.Equal(run1.attackerWon, run2.attackerWon);
            Assert.Equal(run1.attackerPower, run2.attackerPower);
            Assert.Equal(run1.defenderPower, run2.defenderPower);
            Assert.Equal(run1.attackerSoldiersAfter, run2.attackerSoldiersAfter);
            Assert.Equal(run1.defenderSoldiersAfter, run2.defenderSoldiersAfter);
        }

        private static BattleResult RunOne()
        {
            FakeDataRepository data = new FakeDataRepository();
            data.EmperorMap["a"] = new EmperorDefinition { id = "a", name = "A", stats = new EmperorStats() };
            data.EmperorMap["d"] = new EmperorDefinition { id = "d", name = "D", stats = new EmperorStats() };
            data.UnitMap["infantry"] = new UnitDefinition
            {
                id = "infantry", name = "Infantry",
                stats = new UnitStats { attack = 10, defense = 10, mobility = 1, siege = 0 },
                upkeep = new CostSet()
            };
            data.RegionMap["r0"] = new RegionDefinition
            {
                id = "r0", name = "r0",
                landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                legitimacyMemory = new[] { "civilian" },
                neighbors = new string[0]
            };
            GameState state = new GameState
            {
                turn = 1, year = 1, season = Season.Spring, playerFactionId = "faction_a"
            };
            FactionState a = new FactionState
            {
                id = "faction_a", name = "A", emperorId = "a",
                taxMultiplier = 1f, foodMultiplier = 1f,
                armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f
            };
            FactionState d = new FactionState
            {
                id = "faction_d", name = "D", emperorId = "d",
                taxMultiplier = 1f, foodMultiplier = 1f,
                armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f
            };
            state.factions.Add(a);
            state.factions.Add(d);
            state.regions.Add(new RegionState
            {
                id = "r0", ownerFactionId = d.id,
                integration = 100, taxContributionPercent = 100, foodContributionPercent = 100,
                landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                customs = new[] { "agrarian" }
            });
            d.regionIds.Add("r0");
            state.armies.Add(new ArmyState { id = "atk", ownerFactionId = a.id, regionId = "r0", unitId = "infantry", soldiers = 5000, morale = 80 });
            state.armies.Add(new ArmyState { id = "def", ownerFactionId = d.id, regionId = "r0", unitId = "infantry", soldiers = 3000, morale = 70 });

            WorldState world = WorldStateFactory.Create(state, data);
            world.Map.ArmiesById["atk"].supply = 80;
            world.Map.ArmiesById["def"].supply = 80;

            GameContext context = new GameContext(state, data, new EventBus());
            DomainEngagementDetector detector = new DomainEngagementDetector();
            EngagementRuntimeState eng = detector.DetectRegion(context, world.Map, "r0", "atk");
            DomainBattleSimulationSystem battle = new DomainBattleSimulationSystem();
            return battle.ResolveEngagement(context, world.Map, eng.id);
        }
    }
}
