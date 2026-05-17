using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: DomainArmyMovementSystem.AdvanceMapLedArmies
    /// iterates `mapState.ArmiesById.Values` directly.  System.Collections.
    /// Generic.Dictionary documents enumeration order as "implementation
    /// detail; do not rely on it" — and on .NET 8 it changes when the
    /// dictionary is modified mid-enumeration (which the movement system
    /// does indirectly via MoveArmyToRegion -> RemoveArmyLocation /
    /// IndexArmyLocation, which mutate inner lists keyed off the dict).
    ///
    /// The strategic effect: if two friendly armies converge on the same
    /// target region with the same route on the same turn, the order in
    /// which they arrive — and therefore who triggers engagement
    /// detection first — depends on dictionary insertion order, which is
    /// non-deterministic across save/load and across .NET runtime
    /// implementations.
    ///
    /// Pinned invariant:
    ///   Build a deterministic scenario, run the movement turn twice
    ///   from two equivalent but reversed army-creation orders, and
    ///   require the resulting world state (army locations) to be
    ///   identical.
    /// </summary>
    public sealed class DictionaryEnumerationOrderBugTests
    {
        private readonly ITestOutputHelper output;

        public DictionaryEnumerationOrderBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Movement_System_Output_Must_Not_Depend_On_Army_Insertion_Order()
        {
            string[] sortedFinal1 = RunWithInsertionOrder(new[] { "alpha", "beta" });
            string[] sortedFinal2 = RunWithInsertionOrder(new[] { "beta", "alpha" });

            output.WriteLine("alpha-first final:  " + string.Join(",", sortedFinal1));
            output.WriteLine("beta-first final:   " + string.Join(",", sortedFinal2));

            Assert.Equal(sortedFinal1, sortedFinal2);
        }

        private static string[] RunWithInsertionOrder(string[] armyIds)
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
            foreach (string r in new[] { "home", "front" })
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

            // Insert armies in the requested order.  All armies share
            // the same route home->front.
            foreach (string id in armyIds)
            {
                state.armies.Add(new ArmyState
                {
                    id = id,
                    ownerFactionId = player.id,
                    regionId = "home",
                    unitId = "infantry",
                    soldiers = 1000,
                    morale = 70
                });
            }

            WorldState world = WorldStateFactory.Create(state, data);
            GameContext context = new GameContext(state, data, new EventBus());
            MapQueryService queries = new MapQueryService(world.Map, new MapGraphData(data));
            MapCommandService commands = new MapCommandService(queries, context);
            DomainEngagementDetector detector = new DomainEngagementDetector();

            foreach (string id in armyIds)
            {
                ArmyRuntimeState army = world.Map.ArmiesById[id];
                army.task = ArmyTask.Move;
                army.targetRegionId = "front";
                army.route = new List<string> { "home", "front" };
            }

            DomainArmyMovementSystem movement = new DomainArmyMovementSystem(world, commands, detector);
            movement.ExecuteTurn(context);

            // Snapshot final state in a canonical, order-independent form.
            return armyIds
                .Select(id => id + "@" + world.Map.ArmiesById[id].locationRegionId + ":" + world.Map.ArmiesById[id].task)
                .OrderBy(s => s)
                .ToArray();
        }
    }
}
