using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: MapCommandService.RetreatArmy accepts any
    /// reachable target region as a retreat destination. Combined with
    /// IssueRouteCommand skipping the campaign forecast for retreats,
    /// this lets an engaged army "retreat" deep into hostile territory
    /// in a single command.  The desired invariant is that retreat is
    /// only allowed toward an immediately adjacent region whose owner
    /// belongs to the army's own faction (mirroring the post-battle
    /// rout logic in DomainMapWarResolutionSystem.FindOwnedNeighborRegion).
    /// </summary>
    public sealed class RetreatTargetValidationBugTests
    {
        private readonly ITestOutputHelper output;

        public RetreatTargetValidationBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Retreat_Into_Hostile_Region_Must_Be_Rejected()
        {
            // Three regions in a line, owned by player / enemy / enemy.
            FakeDataRepository data = new FakeDataRepository();
            data.EmperorMap["p"] = new EmperorDefinition { id = "p", name = "P", stats = new EmperorStats() };
            data.EmperorMap["e"] = new EmperorDefinition { id = "e", name = "E", stats = new EmperorStats() };
            data.UnitMap["infantry"] = new UnitDefinition
            {
                id = "infantry",
                name = "Infantry",
                stats = new UnitStats { attack = 10, defense = 10, mobility = 1, siege = 0 },
                upkeep = new CostSet { money = 0, food = 0 }
            };
            string[] ids = { "home", "front", "deep" };
            for (int i = 0; i < ids.Length; i++)
            {
                System.Collections.Generic.List<string> nbrs = new System.Collections.Generic.List<string>();
                if (i > 0) nbrs.Add(ids[i - 1]);
                if (i < ids.Length - 1) nbrs.Add(ids[i + 1]);
                data.RegionMap[ids[i]] = new RegionDefinition
                {
                    id = ids[i],
                    name = ids[i],
                    landStructure = new LandStructure
                    {
                        smallFarmers = 0.6f,
                        localElites = 0.1f,
                        stateLand = 0.2f,
                        religiousLand = 0.1f
                    },
                    legitimacyMemory = new[] { "civilian" },
                    neighbors = nbrs.ToArray()
                };
            }

            GameState state = new GameState
            {
                turn = 1,
                year = 1,
                season = Season.Spring,
                playerFactionId = "faction_p"
            };
            FactionState player = new FactionState
            {
                id = "faction_p",
                name = "P",
                emperorId = "p",
                taxMultiplier = 1f,
                foodMultiplier = 1f,
                armyAttackMultiplier = 1f,
                armyDefenseMultiplier = 1f,
                talentMultiplier = 1f
            };
            FactionState enemy = new FactionState
            {
                id = "faction_e",
                name = "E",
                emperorId = "e",
                taxMultiplier = 1f,
                foodMultiplier = 1f,
                armyAttackMultiplier = 1f,
                armyDefenseMultiplier = 1f,
                talentMultiplier = 1f
            };
            state.factions.Add(player);
            state.factions.Add(enemy);

            state.regions.Add(new RegionState
            {
                id = "home", ownerFactionId = player.id, integration = 100,
                occupationStatus = OccupationStatus.Controlled,
                taxContributionPercent = 100, foodContributionPercent = 100,
                landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                customs = new[] { "agrarian" }
            });
            player.regionIds.Add("home");
            state.regions.Add(new RegionState
            {
                id = "front", ownerFactionId = enemy.id, integration = 100,
                occupationStatus = OccupationStatus.Controlled,
                taxContributionPercent = 100, foodContributionPercent = 100,
                landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                customs = new[] { "agrarian" }
            });
            enemy.regionIds.Add("front");
            state.regions.Add(new RegionState
            {
                id = "deep", ownerFactionId = enemy.id, integration = 100,
                occupationStatus = OccupationStatus.Controlled,
                taxContributionPercent = 100, foodContributionPercent = 100,
                landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                customs = new[] { "agrarian" }
            });
            enemy.regionIds.Add("deep");

            state.armies.Add(new ArmyState
            {
                id = "army_player",
                ownerFactionId = player.id,
                regionId = "front",
                unitId = "infantry",
                soldiers = 1000,
                morale = 70
            });
            state.armies.Add(new ArmyState
            {
                id = "army_enemy",
                ownerFactionId = enemy.id,
                regionId = "front",
                unitId = "infantry",
                soldiers = 1000,
                morale = 70
            });

            WorldState world = WorldStateFactory.Create(state, data);
            GameContext context = new GameContext(state, data, new EventBus());
            MapQueryService queries = new MapQueryService(world.Map, new MapGraphData(data));
            MapCommandService commands = new MapCommandService(queries, context);

            // Form an engagement by hand: place both armies in `front`
            // and assign engagementId so RetreatArmy's pre-check passes.
            ArmyRuntimeState player1 = world.Map.ArmiesById["army_player"];
            ArmyRuntimeState enemy1 = world.Map.ArmiesById["army_enemy"];
            EngagementRuntimeState eng = new EngagementRuntimeState
            {
                id = "engagement_test",
                regionId = "front",
                phase = EngagementPhase.Forming,
                initiatingArmyId = player1.id,
                initiatingFactionId = player1.ownerFactionId,
                createdTurn = 1
            };
            eng.attackerArmyIds.Add(player1.id);
            eng.defenderArmyIds.Add(enemy1.id);
            world.Map.AddEngagement(eng);
            player1.engagementId = eng.id;
            enemy1.engagementId = eng.id;

            // Attempt to "retreat" deeper into enemy territory. The
            // route exists (front -> deep is adjacent, both belong to
            // enemy), so today the command service accepts it. The
            // desired behaviour is rejection because `deep` is not
            // owned by the player.
            bool accepted = commands.RetreatArmy(player1.id, "deep");

            output.WriteLine("retreat into hostile region accepted? " + accepted);
            output.WriteLine("army.targetRegionId after call:        " + player1.targetRegionId);
            output.WriteLine("army.task after call:                  " + player1.task);

            Assert.False(accepted);
        }
    }
}
