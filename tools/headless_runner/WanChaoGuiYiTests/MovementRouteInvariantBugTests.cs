using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: DomainArmyMovementSystem.AdvanceMapLedArmies
    /// trusts army.route blindly — it pulls route[1] and calls
    /// MapState.MoveArmyToRegion without verifying that:
    ///  (a) route[0] equals the current locationRegionId
    ///  (b) route[1] is a neighbour of route[0]
    /// If anything has nudged the army's location since the route was
    /// computed (events, scripted scenarios, save/load, AI), the army can
    /// teleport across the map.  The desired invariant: movement must
    /// reject mismatched routes instead of teleporting.
    /// </summary>
    public sealed class MovementRouteInvariantBugTests
    {
        private readonly ITestOutputHelper output;

        public MovementRouteInvariantBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Army_With_Stale_Route_Must_Not_Teleport_To_NonAdjacent_Region()
        {
            // Build a triangle with a non-adjacent island:
            //   home -- mid -- far     (linear chain, all adjacent)
            //   island                  (no edges)
            FakeDataRepository data = new FakeDataRepository();
            data.EmperorMap["p"] = new EmperorDefinition { id = "p", name = "P", stats = new EmperorStats() };
            data.UnitMap["infantry"] = new UnitDefinition
            {
                id = "infantry",
                name = "Infantry",
                stats = new UnitStats { attack = 10, defense = 10, mobility = 1, siege = 0 },
                upkeep = new CostSet { money = 0, food = 0 }
            };
            string[] all = { "home", "mid", "far", "island" };
            data.RegionMap["home"] = new RegionDefinition
            {
                id = "home",
                landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                legitimacyMemory = new[] { "civilian" },
                neighbors = new[] { "mid" }
            };
            data.RegionMap["mid"] = new RegionDefinition
            {
                id = "mid",
                landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                legitimacyMemory = new[] { "civilian" },
                neighbors = new[] { "home", "far" }
            };
            data.RegionMap["far"] = new RegionDefinition
            {
                id = "far",
                landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                legitimacyMemory = new[] { "civilian" },
                neighbors = new[] { "mid" }
            };
            data.RegionMap["island"] = new RegionDefinition
            {
                id = "island",
                landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                legitimacyMemory = new[] { "civilian" },
                neighbors = new string[0]
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
            for (int i = 0; i < all.Length; i++)
            {
                state.regions.Add(new RegionState
                {
                    id = all[i],
                    ownerFactionId = player.id,
                    integration = 100,
                    occupationStatus = OccupationStatus.Controlled,
                    taxContributionPercent = 100,
                    foodContributionPercent = 100,
                    landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                    customs = new[] { "agrarian" }
                });
                player.regionIds.Add(all[i]);
            }

            state.armies.Add(new ArmyState
            {
                id = "ghost",
                ownerFactionId = player.id,
                regionId = "home",
                unitId = "infantry",
                soldiers = 1000,
                morale = 70
            });

            WorldState world = WorldStateFactory.Create(state, data);
            GameContext context = new GameContext(state, data, new EventBus());

            // Inject a route that does NOT start at the army's current
            // location and whose second hop is not adjacent to the
            // first.  Mimics a stale plan: route was computed from
            // 'mid', then the army was bumped back to 'home', then
            // someone forgot to recalc.
            ArmyRuntimeState ghost = world.Map.ArmiesById["ghost"];
            ghost.task = ArmyTask.Move;
            ghost.targetRegionId = "island";
            ghost.route = new List<string> { "mid", "island", "far" };

            string locationBefore = ghost.locationRegionId;

            MapQueryService queries = new MapQueryService(world.Map, new MapGraphData(data));
            MapCommandService commands = new MapCommandService(queries, context);
            DomainEngagementDetector detector = new DomainEngagementDetector();
            DomainArmyMovementSystem movement = new DomainArmyMovementSystem(world, commands, detector);
            movement.ExecuteTurn(context);

            output.WriteLine("location before: " + locationBefore);
            output.WriteLine("location after:  " + ghost.locationRegionId);
            output.WriteLine("route after:     [" + string.Join(",", ghost.route.ToArray()) + "]");

            // The army was at 'home'; the stale route claimed the next
            // hop was 'island'.  'home' and 'island' are not neighbours.
            // The desired invariant: the army must NOT have teleported
            // there.  Either it stays at 'home' (invariant violation
            // detected and command stopped) or it moves to a real
            // neighbour ('mid'), but never to 'island'.
            Assert.NotEqual("island", ghost.locationRegionId);
        }
    }
}
