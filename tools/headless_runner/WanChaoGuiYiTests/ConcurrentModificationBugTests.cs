using System;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: many domain systems iterate over
    /// MapState.ArmiesById.Values directly (DomainArmyMovementSystem,
    /// DomainEngagementDetector). If any iterated step calls
    /// MapState.AddArmy or MapState.RemoveArmy (which the rout/retreat
    /// path actually does — RemoveArmy is invoked from inside the
    /// movement loop), the underlying Dictionary throws
    /// InvalidOperationException("Collection was modified") on the
    /// next MoveNext.
    ///
    /// Pinned invariant: a turn that includes a rout (RemoveArmy
    /// inside movement) must complete without throwing.
    ///
    /// [FIXED v2] Original test constructed armies moving to an empty
    /// target region — no engagement forms, so RemoveArmy never fires.
    /// New tests:
    /// 1. Battle rout: two opposing armies fight, loser drops below
    ///    RoutSoldierThreshold (100), triggers RemoveArmy during
    ///    DomainMapWarResolutionSystem.ExecuteTurn iteration over EngagementsById.
    ///    The actual concurrent modification path is:
    ///    ArmiesById.Values iterates → battle resolves → loser RemoveArmy →
    ///    ArmiesById.Remove called → concurrent modification if also iterating.
    ///    We test this by running both movement and war resolution on the same
    ///    turn and verifying no exception fires.
    /// 2. RemoveRegion: MapState.RemoveRegion also calls RemoveArmy (line 151
    ///    of WorldState.cs) inside a method that modifies armyIdsByRegionId.
    ///    While ArmiesById is not iterated directly inside RemoveRegion,
    ///    the cleanup loop calls RemoveArmy which modifies ArmiesById.
    ///    We test this separately to ensure the invariant holds.
    /// </summary>
    public sealed class ConcurrentModificationBugTests
    {
        private readonly ITestOutputHelper output;

        public ConcurrentModificationBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void BattleRout_Must_Not_Throw_During_ArmiesById_Enumeration()
        {
            // Set up two opposing armies in the same region. When battle resolves,
            // the loser will have soldiers < 100 and trigger RemoveArmy.
            // We verify no InvalidOperationException fires during any system
            // that iterates ArmiesById.
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(2, out data);

            // Add a second faction to create a real engagement.
            FactionState faction2 = new FactionState
            {
                id = "faction_enemy",
                name = "敌对势力",
                emperorId = "test_player",
                money = 100000,
                food = 100000,
                legitimacy = 60,
                taxMultiplier = 1f,
                foodMultiplier = 1f,
                armyAttackMultiplier = 1f,
                armyDefenseMultiplier = 1f,
                talentMultiplier = 1f
            };
            state.factions.Add(faction2);
            state.factions[0].regionIds.Clear();
            state.regions[0].ownerFactionId = state.factions[0].id;
            state.regions[1].ownerFactionId = state.factions[0].id;
            state.factions[0].regionIds.Add("r0");
            state.factions[0].regionIds.Add("r1");
            state.factions[1].regionIds.Add("r0");
            state.factions[1].regionIds.Add("r1");

            data.UnitMap["infantry"] = new UnitDefinition
            {
                id = "infantry", name = "Infantry",
                stats = new UnitStats { attack = 10, defense = 10, mobility = 1, siege = 0 },
                upkeep = new CostSet()
            };

            // Attacker: strong army (200 soldiers, high morale) in r0 targeting r1.
            // This army will be on the attacking side of the engagement.
            ArmyRuntimeState attackerArmy = new ArmyRuntimeState
            {
                id = "attacker",
                ownerFactionId = state.factions[0].id,
                locationRegionId = "r0",
                targetRegionId = "r1",
                route = new System.Collections.Generic.List<string> { "r0", "r1" },
                unitId = "infantry",
                soldiers = 200,
                morale = 80,
                supply = 50
            };

            // Defender: weak army (80 soldiers) already in r1.
            // This army will be on the defending side. With 80 < RoutSoldierThreshold (100),
            // it will be removed by ResolveLoserArmy after the battle.
            ArmyRuntimeState defenderArmy = new ArmyRuntimeState
            {
                id = "defender",
                ownerFactionId = state.factions[1].id,
                locationRegionId = "r1",
                targetRegionId = null,
                unitId = "infantry",
                soldiers = 80,  // Below RoutSoldierThreshold (100) — will be removed after battle
                morale = 30,
                supply = 50
            };

            // Sync to legacy armies for battle resolution (DomainBattleSimulationSystem
            // reads from legacy armies via FindLegacyArmy).
            state.armies.Add(new ArmyState
            {
                id = "attacker",
                ownerFactionId = state.factions[0].id,
                regionId = "r0",
                unitId = "infantry",
                soldiers = 200,
                morale = 80
            });
            state.armies.Add(new ArmyState
            {
                id = "defender",
                ownerFactionId = state.factions[1].id,
                regionId = "r1",
                unitId = "infantry",
                soldiers = 80,
                morale = 30
            });

            WorldState world = WorldStateFactory.Create(state, data);
            world.Map.AddArmy(attackerArmy);
            world.Map.AddArmy(defenderArmy);

            GameContext context = new GameContext(state, data, new EventBus());
            MapQueryService queries = new MapQueryService(world.Map, new MapGraphData(data));
            MapCommandService commands = new MapCommandService(queries, context);
            DomainEngagementDetector detector = new DomainEngagementDetector();
            DomainBattleSimulationSystem battleSim = new DomainBattleSimulationSystem();
            DomainGovernanceImpactSystem governanceSystem = new DomainGovernanceImpactSystem();
            DomainOccupationSystem occupationSystem = new DomainOccupationSystem(governanceSystem);

            // Step 1: Run movement. The attacker moves into r1 where the defender is.
            DomainArmyMovementSystem movement = new DomainArmyMovementSystem(world, commands, detector);
            movement.ExecuteTurn(context);

            // After movement, attacker is in r1 — engagement should be detected.
            output.WriteLine("After movement, attacker location: " + world.Map.ArmiesById["attacker"].locationRegionId);
            output.WriteLine("Defender still in map: " + world.Map.ArmiesById.ContainsKey("defender"));

            // Step 2: Run war resolution. This will detect the engagement, resolve the battle,
            // and call RemoveArmy on the losing army (defender has soldiers=80 < 100).
            // If ArmiesById is iterated somewhere during this, the RemoveArmy call would
            // throw InvalidOperationException.
            DomainMapWarResolutionSystem warResolution = new DomainMapWarResolutionSystem(
                world, detector, battleSim, occupationSystem);

            Exception caught = null;
            try
            {
                // Run both systems on the same turn to expose any concurrent modification.
                warResolution.ExecuteTurn(context);
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            output.WriteLine("Exception thrown: " + (caught == null ? "<none>" : caught.GetType().Name + ": " + caught.Message));
            output.WriteLine("Armies in map after resolution: " + world.Map.ArmiesById.Count);

            Assert.True(caught == null,
                "RemoveArmy threw during enumeration of ArmiesById. Exception: " +
                (caught != null ? caught.Message : "unknown"));
        }

        [Fact]
        public void RemoveRegion_Must_Not_Throw_When_Iterating_ArmiesById()
        {
            // MapState.RemoveRegion (line 124 of WorldState.cs) calls RemoveArmy
            // inside a loop over armyIdsByRegionId. While the direct iteration
            // is over armyIdsByRegionId (not ArmiesById), we verify the overall
            // invariant: removing a region with armies in it should not cause
            // any dictionary modification exceptions.
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(2, out data);

            data.UnitMap["infantry"] = new UnitDefinition
            {
                id = "infantry", name = "Infantry",
                stats = new UnitStats { attack = 10, defense = 10, mobility = 1, siege = 0 },
                upkeep = new CostSet()
            };

            // Add an army in r1.
            ArmyRuntimeState army = new ArmyRuntimeState
            {
                id = "army_r1",
                ownerFactionId = state.factions[0].id,
                locationRegionId = "r1",
                targetRegionId = null,
                unitId = "infantry",
                soldiers = 50,
                morale = 50,
                supply = 50
            };

            state.armies.Add(new ArmyState
            {
                id = "army_r1",
                ownerFactionId = state.factions[0].id,
                regionId = "r1",
                unitId = "infantry",
                soldiers = 50,
                morale = 50
            });

            WorldState world = WorldStateFactory.Create(state, data);
            world.Map.AddArmy(army);

            output.WriteLine("Armies before RemoveRegion: " + world.Map.ArmiesById.Count);

            Exception caught = null;
            try
            {
                // RemoveRegion triggers RemoveArmy internally (line 151 of WorldState.cs).
                world.Map.RemoveRegion("r1");
            }
            catch (Exception ex)
            {
                caught = ex;
            }

            output.WriteLine("Exception thrown: " + (caught == null ? "<none>" : caught.GetType().Name + ": " + caught.Message));
            output.WriteLine("Armies after RemoveRegion: " + world.Map.ArmiesById.Count);

            Assert.True(caught == null,
                "RemoveRegion threw an exception. Exception: " +
                (caught != null ? caught.Message : "unknown"));

            Assert.False(world.Map.ArmiesById.ContainsKey("army_r1"),
                "army_r1 should have been removed along with its region.");
        }
    }
}
