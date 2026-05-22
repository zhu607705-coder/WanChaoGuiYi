using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: DomainBattleSimulationSystem.ResolveEngagement
    /// currently uses 'attackerWon = attackerPower > defenderPower'.  This
    /// means exact ties are defender holds.  The regression risk is changing
    /// the comparison back to '>=' or otherwise auto-awarding a no-margin
    /// result to the attacker while ApplySideLosses still imposes asymmetric
    /// winner/loser casualty fractions.
    ///
    /// Pinned invariant: an exact tie must NOT auto-attribute the region to
    /// the attacker.
    /// </summary>
    public sealed class BattleTieBreakBugTests
    {
        private readonly ITestOutputHelper output;

        public BattleTieBreakBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Exact_Tie_Must_Not_Default_To_Attacker_Victory()
        {
            FakeDataRepository data = new FakeDataRepository();
            data.EmperorMap["a"] = new EmperorDefinition { id = "a", name = "A", stats = new EmperorStats() };
            data.EmperorMap["d"] = new EmperorDefinition { id = "d", name = "D", stats = new EmperorStats() };
            data.UnitMap["infantry"] = new UnitDefinition
            {
                id = "infantry",
                name = "Infantry",
                stats = new UnitStats { attack = 10, defense = 10, mobility = 1, siege = 0 },
                upkeep = new CostSet { money = 0, food = 0 }
            };
            data.RegionMap["r0"] = new RegionDefinition
            {
                id = "r0",
                name = "r0",
                landStructure = new LandStructure
                {
                    smallFarmers = 0.6f,
                    localElites = 0.1f,
                    stateLand = 0.2f,
                    religiousLand = 0.1f
                },
                legitimacyMemory = new[] { "civilian" },
                neighbors = new string[0]
            };

            GameState state = new GameState
            {
                turn = 1,
                year = 1,
                season = Season.Spring,
                playerFactionId = "faction_a"
            };
            FactionState attacker = new FactionState
            {
                id = "faction_a", name = "A", emperorId = "a",
                taxMultiplier = 1f, foodMultiplier = 1f,
                armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f
            };
            FactionState defender = new FactionState
            {
                id = "faction_d", name = "D", emperorId = "d",
                taxMultiplier = 1f, foodMultiplier = 1f,
                armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f
            };
            state.factions.Add(attacker);
            state.factions.Add(defender);

            state.regions.Add(new RegionState
            {
                id = "r0",
                ownerFactionId = defender.id,
                integration = 100,
                occupationStatus = OccupationStatus.Controlled,
                taxContributionPercent = 100,
                foodContributionPercent = 100,
                landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                customs = new[] { "agrarian" }
            });
            defender.regionIds.Add("r0");

            // Identical armies on identical units, identical morale.
            state.armies.Add(new ArmyState { id = "atk", ownerFactionId = attacker.id, regionId = "r0", unitId = "infantry", soldiers = 1000, morale = 70 });
            state.armies.Add(new ArmyState { id = "def", ownerFactionId = defender.id, regionId = "r0", unitId = "infantry", soldiers = 1000, morale = 70 });

            WorldState world = WorldStateFactory.Create(state, data);
            GameContext context = new GameContext(state, data, new EventBus());

            // Force identical supplies so the supply-pressure pipeline
            // cannot tilt the comparison.
            world.Map.ArmiesById["atk"].supply = 80;
            world.Map.ArmiesById["def"].supply = 80;

            DomainEngagementDetector detector = new DomainEngagementDetector();
            EngagementRuntimeState engagement = detector.DetectRegion(context, world.Map, "r0", "atk");
            Assert.NotNull(engagement);

            DomainBattleSimulationSystem battle = new DomainBattleSimulationSystem();
            BattleResult result = battle.ResolveEngagement(context, world.Map, engagement.id);
            Assert.NotNull(result);

            output.WriteLine("attacker power: " + result.attackerPower);
            output.WriteLine("defender power: " + result.defenderPower);
            output.WriteLine("attacker won?   " + result.attackerWon);

            // Confirm we really constructed a tie (otherwise the test
            // would be testing nothing meaningful).
            Assert.Equal(result.defenderPower, result.attackerPower);

            // Tie policy: defender hold.  If design later changes to an
            // explicit draw/probabilistic result, this assertion should be
            // replaced with a richer outcome field rather than >=.
            Assert.False(result.attackerWon, "Exact tie auto-awarded to attacker. Defender lost the region with no margin.");
        }
    }
}
