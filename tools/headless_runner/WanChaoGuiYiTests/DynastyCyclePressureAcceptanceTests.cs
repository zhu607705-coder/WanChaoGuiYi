using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// MVP closure acceptance tests for the dynasty-cycle pressure chain.
    /// These tests pin existing systems into a playable long-horizon loop:
    /// expansion, occupation governance, logistics cost, fiscal pressure,
    /// land unrest, and succession risk must become visible together.
    /// </summary>
    public sealed class DynastyCyclePressureAcceptanceTests
    {
        private readonly ITestOutputHelper output;

        public DynastyCyclePressureAcceptanceTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Expansion_Overheat_Must_Create_Readable_Governance_And_Succession_Pressure()
        {
            FakeDataRepository data;
            FactionState player;
            FactionState rival;
            GameState state = BuildPressureWorld(9, 6, out data, out player, out rival);
            WorldState world = TestFixtures.BuildWorldState(state, data);
            GameContext context = TestFixtures.BuildContext(state, data);
            ArmyRuntimeState mainArmy;
            Assert.True(world.Map.TryGetArmy("main_army", out mainArmy));

            MapCommandService commands = new MapCommandService(
                new MapQueryService(world.Map, new MapGraphData(data)),
                context);

            int regionsBefore = player.regionIds.Count;
            int legitimacyBefore = player.legitimacy;
            int successionBefore = player.successionRisk;
            int governanceMoneyUpkeepBefore = NumericFormulas.CalculateGovernanceUpkeep(player, false);
            int governanceFoodUpkeepBefore = NumericFormulas.CalculateGovernanceUpkeep(player, true);
            int foodBeforePreparation = player.food;
            int rebellionBefore = state.FindRegion("r6").rebellionRisk;
            int localPowerBefore = state.FindRegion("r6").localPower;
            int annexationBefore = state.FindRegion("r6").annexationPressure;

            bool prepared = commands.PrepareFrontline("main_army", "r6");
            int foodAfterPreparation = player.food;
            int reservedFood = mainArmy.frontlineReservedFood;

            Assert.True(prepared, "Frontline preparation should be available before adjacent expansion.");
            Assert.True(foodAfterPreparation < foodBeforePreparation,
                "Expansion must pay visible food before occupation; otherwise war logistics are not part of the dynasty-cycle pressure.");
            Assert.True(reservedFood >= StrategyMapRulebook.OccupationAdministrationFoodCost,
                "Prepared campaign should reserve enough food for occupation administration.");

            state.ChangeRegionOwner("r6", player.id);
            state.ChangeRegionOwner("r7", player.id);
            state.ChangeRegionOwner("r8", player.id);

            DomainGovernanceImpactSystem governance = new DomainGovernanceImpactSystem();
            GovernanceImpactPayload firstPayload = governance.ApplyOccupationImpact(context, world.Map, "r6", reservedFood);
            GovernanceImpactPayload secondPayload = governance.ApplyOccupationImpact(context, world.Map, "r7", 0);
            GovernanceImpactPayload thirdPayload = governance.ApplyOccupationImpact(context, world.Map, "r8", 0);
            Assert.NotNull(firstPayload);
            Assert.NotNull(secondPayload);
            Assert.NotNull(thirdPayload);

            int governanceMoneyUpkeepAfter = NumericFormulas.CalculateGovernanceUpkeep(player, false);
            int governanceFoodUpkeepAfter = NumericFormulas.CalculateGovernanceUpkeep(player, true);
            DomainEconomySystem economy = new DomainEconomySystem(world);
            for (int turn = 0; turn < 20; turn++)
            {
                economy.ExecuteTurn(context);
                state.AdvanceHalfYear();
            }

            RegionState core = state.FindRegion("r0");
            RegionState occupied = state.FindRegion("r6");
            output.WriteLine("regions: " + regionsBefore + " -> " + player.regionIds.Count);
            output.WriteLine("legitimacy: " + legitimacyBefore + " -> " + player.legitimacy);
            output.WriteLine("successionRisk: " + successionBefore + " -> " + player.successionRisk);
            output.WriteLine("governance money upkeep: " + governanceMoneyUpkeepBefore + " -> " + governanceMoneyUpkeepAfter);
            output.WriteLine("governance food upkeep: " + governanceFoodUpkeepBefore + " -> " + governanceFoodUpkeepAfter);
            output.WriteLine("occupied tax contribution: " + occupied.taxContributionPercent + " core=" + core.taxContributionPercent);

            Assert.Equal(regionsBefore + 3, player.regionIds.Count);
            Assert.True(player.legitimacy < legitimacyBefore,
                "Occupation should reduce legitimacy so expansion is not a pure reward.");
            Assert.True(player.successionRisk > successionBefore,
                "A larger realm should accumulate succession risk across a 20-turn dynasty-cycle window.");
            Assert.True(governanceMoneyUpkeepAfter > governanceMoneyUpkeepBefore);
            Assert.True(governanceFoodUpkeepAfter > governanceFoodUpkeepBefore);
            Assert.Equal(OccupationStatus.Occupied, occupied.occupationStatus);
            Assert.Equal(ControlStage.NewlyAttached, occupied.controlStage);
            Assert.True(occupied.taxContributionPercent < core.taxContributionPercent);
            Assert.True(occupied.foodContributionPercent < core.foodContributionPercent);
            Assert.True(
                occupied.rebellionRisk > rebellionBefore ||
                occupied.localPower > localPowerBefore ||
                occupied.annexationPressure > annexationBefore,
                "New occupation should raise at least one local pressure vector.");
            Assert.True(firstPayload.occupationReservedFoodAvailable > 0);
            Assert.True(HasLogContaining(state, "前线整备"));
            Assert.True(HasLogContaining(state, "新占领"));
            Assert.True(HasLogContaining(state, "地区贡献率折算产出"));
        }

        [Fact]
        public void Fiscal_Military_Land_Squeeze_Must_Show_Resources_And_Risk_Moving_Together()
        {
            FakeDataRepository data;
            FactionState player;
            FactionState rival;
            GameState state = BuildPressureWorld(4, 3, out data, out player, out rival);
            WorldState world = TestFixtures.BuildWorldState(state, data);
            GameContext context = TestFixtures.BuildContext(state, data);
            RegionState core = state.FindRegion("r1");
            ArmyRuntimeState mainArmy;
            Assert.True(world.Map.TryGetArmy("main_army", out mainArmy));

            MapCommandService commands = new MapCommandService(
                new MapQueryService(world.Map, new MapGraphData(data)),
                context);

            int moneyBeforeTax = player.money;
            int legitimacyBeforeTax = player.legitimacy;
            int rebellionBeforeTax = core.rebellionRisk;
            int acceptanceBeforeTax = core.localAcceptance;
            GovernanceActionForecast tax = StrategyMapRulebook.ApplyGovernanceAction(
                context,
                data.GetRegion(core.id),
                core,
                player,
                GovernanceActionKind.TaxPressure);

            Assert.True(tax.canApply);
            Assert.True(player.money > moneyBeforeTax, "Emergency tax should improve money.");
            Assert.True(player.legitimacy < legitimacyBeforeTax, "Emergency tax should damage legitimacy.");
            Assert.True(core.rebellionRisk > rebellionBeforeTax, "Emergency tax should raise local unrest.");
            Assert.True(core.localAcceptance < acceptanceBeforeTax, "Emergency tax should lower local acceptance.");
            Assert.Contains("raises money", tax.reason);

            int populationBeforeDraft = core.population;
            int manpowerBeforeDraft = core.manpower;
            int rebellionBeforeDraft = core.rebellionRisk;
            GovernanceActionForecast draft = StrategyMapRulebook.ApplyGovernanceAction(
                context,
                data.GetRegion(core.id),
                core,
                player,
                GovernanceActionKind.Conscription);

            Assert.True(draft.canApply);
            Assert.True(core.population < populationBeforeDraft, "Conscription should consume households.");
            Assert.True(core.manpower < manpowerBeforeDraft, "Conscription should consume manpower.");
            Assert.True(core.rebellionRisk > rebellionBeforeDraft, "Conscription should increase unrest.");
            Assert.Contains("drafting converts households", draft.reason);

            int foodBeforePreparation = player.food;
            bool prepared = commands.PrepareFrontline("main_army", "r3");
            int foodAfterPreparation = player.food;

            Assert.True(prepared, "The main army should be able to prepare an adjacent campaign.");
            Assert.True(foodAfterPreparation < foodBeforePreparation,
                "Frontline preparation should spend food, completing the fiscal/military/land squeeze.");
            Assert.True(HasLogContaining(state, "急征"));
            Assert.True(HasLogContaining(state, "征兵"));
            Assert.True(HasLogContaining(state, "前线整备"));
        }

        private static GameState BuildPressureWorld(
            int regionCount,
            int initialPlayerRegions,
            out FakeDataRepository data,
            out FactionState player,
            out FactionState rival)
        {
            GameState state = TestFixtures.BuildSinglePlayerWorld(regionCount, out data);
            player = state.FindFaction("faction_test_player");
            player.name = "Test Dynasty";
            player.money = 1200;
            player.food = 1200;
            player.legitimacy = 82;
            player.successionRisk = 0;
            player.courtFactionPressure = 0;

            rival = new FactionState
            {
                id = "faction_rival",
                name = "Rival Court",
                emperorId = "rival_emperor",
                money = 500,
                food = 500,
                legitimacy = 60,
                taxMultiplier = 1f,
                foodMultiplier = 1f,
                armyAttackMultiplier = 1f,
                armyDefenseMultiplier = 1f,
                talentMultiplier = 1f
            };
            state.factions.Add(rival);
            data.EmperorMap["rival_emperor"] = new EmperorDefinition
            {
                id = "rival_emperor",
                name = "Rival",
                stats = new EmperorStats()
            };

            for (int i = 0; i < regionCount; i++)
            {
                RegionState region = state.FindRegion("r" + i);
                RegionDefinition definition = data.GetRegion(region.id);
                region.population = 180000 + i * 10000;
                region.taxOutput = 90 + i * 3;
                region.foodOutput = 95 + i * 2;
                region.manpower = 70;
                region.localPower = i >= initialPlayerRegions ? 18 : 12;
                region.rebellionRisk = i >= initialPlayerRegions ? 16 : 8;
                region.annexationPressure = i >= initialPlayerRegions ? 10 : 4;
                region.integration = 100;
                region.occupationStatus = OccupationStatus.Controlled;
                region.controlStage = ControlStage.Controlled;
                region.taxContributionPercent = 100;
                region.foodContributionPercent = 100;
                region.localAcceptance = 72;
                region.customs = new[] { "agrarian" };
                region.customStability = 65;

                definition.population = region.population;
                definition.taxOutput = region.taxOutput;
                definition.foodOutput = region.foodOutput;
                definition.manpower = region.manpower;
                definition.localPower = region.localPower;
                definition.rebellionRisk = region.rebellionRisk;
            }

            for (int i = initialPlayerRegions; i < regionCount; i++)
            {
                state.ChangeRegionOwner("r" + i, rival.id);
            }

            data.UnitMap["infantry"] = new UnitDefinition
            {
                id = "infantry",
                name = "Infantry",
                stats = new UnitStats { attack = 12, defense = 10, mobility = 1, siege = 0 },
                upkeep = new CostSet { money = 6, food = 7 }
            };

            state.armies.Add(new ArmyState
            {
                id = "main_army",
                ownerFactionId = player.id,
                regionId = "r" + (initialPlayerRegions - 1),
                unitId = "infantry",
                soldiers = 4500,
                morale = 82
            });

            return state;
        }

        private static bool HasLogContaining(GameState state, string token)
        {
            if (state == null || state.turnLog == null || string.IsNullOrEmpty(token)) return false;
            for (int i = 0; i < state.turnLog.Count; i++)
            {
                TurnLogEntry entry = state.turnLog[i];
                if (entry != null && entry.message != null && entry.message.Contains(token)) return true;
            }

            return false;
        }
    }
}
