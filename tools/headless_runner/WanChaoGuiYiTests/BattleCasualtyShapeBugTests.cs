using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: DomainBattleSimulationSystem.ApplySideLosses
    /// uses fixed multipliers for the winner (0.85x soldiers) and the
    /// loser (0.45x soldiers) regardless of the power margin, the
    /// soldier count gap, or the supply pressure that already shaped
    /// the result.
    ///
    /// Real-world strategy expectation:
    ///   - A 100k vs 1k crushing victory should leave the winner
    ///     near-untouched (well under 5% loss).  Today they lose 15%,
    ///     which is 15,000 troops gone for nothing.
    ///   - Conversely, a near-tie victory should cost the winner more
    ///     than 15%.
    ///
    /// Pinned invariant: the winner's casualty fraction must be a
    /// monotone decreasing function of (winnerPower / loserPower).
    /// In particular, a 100x power advantage should produce LESS than
    /// 15% losses, and a 1.01x marginal advantage should produce MORE
    /// than 5% losses.
    /// </summary>
    public sealed class BattleCasualtyShapeBugTests
    {
        private readonly ITestOutputHelper output;

        public BattleCasualtyShapeBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Crushing_Victory_Must_Cost_Less_Than_FixedFloor_15Percent()
        {
            FakeDataRepository data = BuildBattleData();
            GameState state = BuildBattleWorld(data, attackerSoldiers: 100_000, defenderSoldiers: 1_000);

            WorldState world = WorldStateFactory.Create(state, data);
            GameContext context = new GameContext(state, data, new EventBus());

            world.Map.ArmiesById["atk"].supply = 80;
            world.Map.ArmiesById["def"].supply = 80;

            DomainEngagementDetector detector = new DomainEngagementDetector();
            EngagementRuntimeState eng = detector.DetectRegion(context, world.Map, "r0", "atk");
            Assert.NotNull(eng);

            int attackerSoldiersBefore = world.Map.ArmiesById["atk"].soldiers;
            DomainBattleSimulationSystem battle = new DomainBattleSimulationSystem();
            BattleResult result = battle.ResolveEngagement(context, world.Map, eng.id);
            Assert.NotNull(result);
            Assert.True(result.attackerWon, "attacker with 100x soldiers must have won the battle");

            int attackerSoldiersAfter = world.Map.ArmiesById["atk"].soldiers;
            float casualtyFraction = 1f - (attackerSoldiersAfter / (float)attackerSoldiersBefore);

            output.WriteLine("attacker before: " + attackerSoldiersBefore);
            output.WriteLine("attacker after:  " + attackerSoldiersAfter);
            output.WriteLine("attacker casualty fraction: " + casualtyFraction.ToString("0.000"));

            // Today this is exactly 0.15 (or 0.15 ± 0.5/100000 due to
            // RoundToInt). A real curve should drop well below that
            // when the power margin is 100x.
            Assert.True(casualtyFraction < 0.10f,
                "Crushing victor (100x) lost " + casualtyFraction.ToString("0.000") +
                " of troops; expected < 0.10. Fixed 0.85 multiplier ignores margin.");
        }

        [Fact]
        public void Marginal_Victory_Must_Cost_More_Than_FixedFloor_15Percent()
        {
            FakeDataRepository data = BuildBattleData();
            // 1010 vs 1000 — attacker barely edges out, but auto-resolution
            // currently still gives them the same flat 0.85 surviving rate.
            GameState state = BuildBattleWorld(data, attackerSoldiers: 1010, defenderSoldiers: 1000);

            WorldState world = WorldStateFactory.Create(state, data);
            GameContext context = new GameContext(state, data, new EventBus());
            world.Map.ArmiesById["atk"].supply = 80;
            world.Map.ArmiesById["def"].supply = 80;

            DomainEngagementDetector detector = new DomainEngagementDetector();
            EngagementRuntimeState eng = detector.DetectRegion(context, world.Map, "r0", "atk");
            Assert.NotNull(eng);

            int attackerSoldiersBefore = world.Map.ArmiesById["atk"].soldiers;
            DomainBattleSimulationSystem battle = new DomainBattleSimulationSystem();
            BattleResult result = battle.ResolveEngagement(context, world.Map, eng.id);
            Assert.NotNull(result);

            // We don't strictly require the marginal victor to be the
            // attacker — a probabilistic resolution might flip it. But
            // we do require that whichever side wins pays more than 15%.
            ArmyRuntimeState winner = result.attackerWon
                ? world.Map.ArmiesById["atk"]
                : world.Map.ArmiesById["def"];
            int winnerBefore = result.attackerWon ? attackerSoldiersBefore : 1000;
            int winnerAfter = winner.soldiers;
            float casualtyFraction = 1f - (winnerAfter / (float)winnerBefore);

            output.WriteLine("winner before: " + winnerBefore);
            output.WriteLine("winner after:  " + winnerAfter);
            output.WriteLine("winner casualty fraction: " + casualtyFraction.ToString("0.000"));

            // A 1% power margin should not be cheaper than a 100x margin.
            Assert.True(casualtyFraction > 0.20f,
                "Marginal victor (1.01x) lost only " + casualtyFraction.ToString("0.000") +
                " of troops; expected > 0.20 because the fight was close.");
        }

        private static FakeDataRepository BuildBattleData()
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
                landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                legitimacyMemory = new[] { "civilian" },
                neighbors = new string[0]
            };
            return data;
        }

        private static GameState BuildBattleWorld(FakeDataRepository data, int attackerSoldiers, int defenderSoldiers)
        {
            GameState state = new GameState
            {
                turn = 1,
                year = 1,
                season = Season.Spring,
                playerFactionId = "faction_a"
            };
            FactionState atk = new FactionState
            {
                id = "faction_a", name = "A", emperorId = "a",
                taxMultiplier = 1f, foodMultiplier = 1f,
                armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f
            };
            FactionState def = new FactionState
            {
                id = "faction_d", name = "D", emperorId = "d",
                taxMultiplier = 1f, foodMultiplier = 1f,
                armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f
            };
            state.factions.Add(atk);
            state.factions.Add(def);
            state.regions.Add(new RegionState
            {
                id = "r0",
                ownerFactionId = def.id,
                integration = 100,
                occupationStatus = OccupationStatus.Controlled,
                taxContributionPercent = 100,
                foodContributionPercent = 100,
                landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                customs = new[] { "agrarian" }
            });
            def.regionIds.Add("r0");
            state.armies.Add(new ArmyState { id = "atk", ownerFactionId = atk.id, regionId = "r0", unitId = "infantry", soldiers = attackerSoldiers, morale = 70 });
            state.armies.Add(new ArmyState { id = "def", ownerFactionId = def.id, regionId = "r0", unitId = "infantry", soldiers = defenderSoldiers, morale = 70 });
            return state;
        }
    }
}
