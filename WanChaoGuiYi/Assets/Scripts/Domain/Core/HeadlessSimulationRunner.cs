using System.Collections.Generic;

namespace WanChaoGuiYi
{
    public sealed class HeadlessSimulationResult
    {
        public string scenarioName;
        public bool passed;
        public string failureReason;
        public GameState state;
        public WorldState worldState;
        public int turnsExecuted;
    }

    public sealed class HeadlessSimulationSuiteResult
    {
        public bool passed;
        public List<HeadlessSimulationResult> scenarios = new List<HeadlessSimulationResult>();
    }

    public sealed class HeadlessSimulationRunner
    {
        private const int MaxTurns = 24;
        private const int OccupiedIntegration = 25;
        private const int OccupiedContributionPercent = 35;

        public HeadlessSimulationSuiteResult RunAllScenarios(IDataRepository data, string playerFactionId)
        {
            HeadlessSimulationSuiteResult suite = new HeadlessSimulationSuiteResult();
            suite.scenarios.Add(RunSingleLaneWar(data, playerFactionId));
            suite.scenarios.Add(RunAttackerOccupation(data, playerFactionId));
            suite.scenarios.Add(RunReinforcementMembership(data, playerFactionId));
            suite.scenarios.Add(RunActiveRetreatFromEngagement(data, playerFactionId));

            suite.passed = true;
            for (int i = 0; i < suite.scenarios.Count; i++)
            {
                if (!suite.scenarios[i].passed)
                {
                    suite.passed = false;
                    break;
                }
            }

            return suite;
        }

        public HeadlessSimulationResult RunSingleLaneWar(IDataRepository data, string playerFactionId)
        {
            HeadlessSimulationResult result = CreateResult("defender_holds_and_attacker_retreats");
            SimulationRuntime runtime;
            if (!TryCreateRuntime(data, playerFactionId, result, out runtime)) return result;

            ArmyRuntimeState playerArmy;
            ArmyRuntimeState enemyArmy;
            if (!TryGetSmokeArmies(result, runtime.worldState, out playerArmy, out enemyArmy)) return result;

            string targetRegionId = enemyArmy.locationRegionId;
            string originalOwnerFactionId = enemyArmy.ownerFactionId;
            bool issued = runtime.commands.MoveArmy(playerArmy.id, targetRegionId);
            if (!issued)
            {
                return Fail(result, runtime.state, runtime.worldState, "MapCommandService.MoveArmy failed for the smoke route.");
            }

            bool battleFinished = RunTurnsUntilBattle(result, runtime);
            if (!battleFinished)
            {
                return Fail(result, runtime.state, runtime.worldState, "Battle did not finish within " + MaxTurns + " turns.");
            }

            RegionRuntimeState targetRegion;
            bool regionCaptured = runtime.worldState.Map.TryGetRegion(targetRegionId, out targetRegion) && targetRegion.ownerFactionId != originalOwnerFactionId;
            bool regionDefended = runtime.worldState.Map.TryGetRegion(targetRegionId, out targetRegion) && targetRegion.ownerFactionId == originalOwnerFactionId;

            string failure = ValidateCommonWarLogs(runtime.state, regionCaptured, regionDefended);
            if (!string.IsNullOrEmpty(failure)) return Fail(result, runtime.state, runtime.worldState, failure);

            if (!HasLogContaining(runtime.state, "战败后撤退") && !HasLogContaining(runtime.state, "溃散"))
            {
                return Fail(result, runtime.state, runtime.worldState, "Expected loser retreat or rout log.");
            }

            return Pass(result, runtime.state, runtime.worldState);
        }

        public HeadlessSimulationResult RunAttackerOccupation(IDataRepository data, string playerFactionId)
        {
            HeadlessSimulationResult result = CreateResult("attacker_wins_and_occupies");
            SimulationRuntime runtime;
            if (!TryCreateRuntime(data, playerFactionId, result, out runtime)) return result;

            ArmyRuntimeState playerArmy;
            ArmyRuntimeState enemyArmy;
            if (!TryGetSmokeArmies(result, runtime.worldState, out playerArmy, out enemyArmy)) return result;

            playerArmy.soldiers = 5200;
            playerArmy.morale = 95;
            enemyArmy.soldiers = 900;
            enemyArmy.morale = 35;
            SyncLegacyArmy(runtime.context, playerArmy);
            SyncLegacyArmy(runtime.context, enemyArmy);

            string targetRegionId = enemyArmy.locationRegionId;
            string originalOwnerFactionId = enemyArmy.ownerFactionId;
            if (!runtime.commands.MoveArmy(playerArmy.id, targetRegionId))
            {
                return Fail(result, runtime.state, runtime.worldState, "Move command failed for attacker occupation scenario.");
            }

            if (!RunTurnsUntilBattle(result, runtime))
            {
                return Fail(result, runtime.state, runtime.worldState, "Battle did not finish within " + MaxTurns + " turns.");
            }

            RegionRuntimeState targetRegion;
            bool regionCaptured = runtime.worldState.Map.TryGetRegion(targetRegionId, out targetRegion) && targetRegion.ownerFactionId != originalOwnerFactionId;
            if (!regionCaptured)
            {
                return Fail(result, runtime.state, runtime.worldState, "Expected attacker to occupy target region.");
            }

            string failure = ValidateCommonWarLogs(runtime.state, true, false);
            if (!string.IsNullOrEmpty(failure)) return Fail(result, runtime.state, runtime.worldState, failure);

            failure = ValidateCapturedRegionState(runtime, targetRegionId, originalOwnerFactionId, playerArmy.ownerFactionId);
            if (!string.IsNullOrEmpty(failure)) return Fail(result, runtime.state, runtime.worldState, failure);

            failure = ValidateEconomyContributionState(runtime, targetRegionId, playerArmy.ownerFactionId);
            if (!string.IsNullOrEmpty(failure)) return Fail(result, runtime.state, runtime.worldState, failure);

            if (!HasLogContaining(runtime.state, "进攻方获胜")) return Fail(result, runtime.state, runtime.worldState, "Expected attacker victory wording.");
            if (!HasLogContaining(runtime.state, "新占领")) return Fail(result, runtime.state, runtime.worldState, "Expected governance impact for newly occupied region.");

            return Pass(result, runtime.state, runtime.worldState);
        }

        public HeadlessSimulationResult RunReinforcementMembership(IDataRepository data, string playerFactionId)
        {
            HeadlessSimulationResult result = CreateResult("reinforcement_joins_existing_engagement");
            SimulationRuntime runtime;
            if (!TryCreateRuntime(data, playerFactionId, result, out runtime)) return result;

            ArmyRuntimeState playerArmy;
            ArmyRuntimeState enemyArmy;
            if (!TryGetSmokeArmies(result, runtime.worldState, out playerArmy, out enemyArmy)) return result;

            ArmyRuntimeState reinforcement = new ArmyRuntimeState
            {
                id = "army_player_reinforcement_1",
                ownerFactionId = playerArmy.ownerFactionId,
                locationRegionId = playerArmy.locationRegionId,
                targetRegionId = null,
                route = new List<string>(),
                task = ArmyTask.Idle,
                unitId = playerArmy.unitId,
                soldiers = 1200,
                morale = 70,
                supply = 80,
                movementPoints = playerArmy.movementPoints,
                engagementId = null
            };
            runtime.worldState.Map.AddArmy(reinforcement);
            runtime.state.armies.Add(new ArmyState
            {
                id = reinforcement.id,
                ownerFactionId = reinforcement.ownerFactionId,
                regionId = reinforcement.locationRegionId,
                unitId = reinforcement.unitId,
                soldiers = reinforcement.soldiers,
                morale = reinforcement.morale,
                movementProgress = 0
            });

            string targetRegionId = enemyArmy.locationRegionId;
            if (!runtime.commands.MoveArmy(playerArmy.id, targetRegionId))
            {
                return Fail(result, runtime.state, runtime.worldState, "Initial move command failed for reinforcement scenario.");
            }

            runtime.movementSystem.ExecuteTurn(runtime.context);

            EngagementRuntimeState engagement;
            if (!runtime.worldState.Map.TryGetEngagementInRegion(targetRegionId, out engagement))
            {
                return Fail(result, runtime.state, runtime.worldState, "Expected engagement after first army arrival.");
            }

            int attackerCountBefore = engagement.attackerArmyIds.Count;
            int contactLogsBefore = CountLogsContaining(runtime.state, "发生接敌");

            if (!runtime.commands.ReinforceArmy(reinforcement.id, targetRegionId))
            {
                return Fail(result, runtime.state, runtime.worldState, "Reinforce command failed for reinforcement scenario.");
            }

            if (reinforcement.task != ArmyTask.Reinforce)
            {
                return Fail(result, runtime.state, runtime.worldState, "Reinforce command did not set ArmyTask.Reinforce.");
            }

            if (reinforcement.targetRegionId != targetRegionId)
            {
                return Fail(result, runtime.state, runtime.worldState, "Reinforce command did not retain target engagement region.");
            }

            if (reinforcement.route == null || reinforcement.route.Count < 2 || reinforcement.route[reinforcement.route.Count - 1] != targetRegionId)
            {
                return Fail(result, runtime.state, runtime.worldState, "Reinforce command did not create a route to the engagement region.");
            }

            if (!HasLogContaining(runtime.state, "增援") || !HasLogContaining(runtime.state, "加入当地接敌"))
            {
                return Fail(result, runtime.state, runtime.worldState, "Expected reinforcement intent logs.");
            }

            runtime.movementSystem.ExecuteTurn(runtime.context);

            if (!runtime.worldState.Map.TryGetEngagementInRegion(targetRegionId, out engagement))
            {
                return Fail(result, runtime.state, runtime.worldState, "Expected engagement to remain after reinforcement arrival.");
            }

            if (!engagement.attackerArmyIds.Contains(reinforcement.id))
            {
                return Fail(result, runtime.state, runtime.worldState, "Reinforcement did not join attacker side membership.");
            }

            if (engagement.attackerArmyIds.Count <= attackerCountBefore)
            {
                return Fail(result, runtime.state, runtime.worldState, "Attacker membership did not increase after reinforcement.");
            }

            int contactLogsAfter = CountLogsContaining(runtime.state, "发生接敌");
            if (contactLogsAfter != contactLogsBefore + 1)
            {
                return Fail(result, runtime.state, runtime.worldState, "Expected exactly one additional contact log for new reinforcement membership.");
            }

            runtime.warResolutionSystem.ExecuteTurn(runtime.context);
            runtime.economySystem.ExecuteTurn(runtime.context);
            result.turnsExecuted = 1;

            string failure = ValidateCommonWarLogs(runtime.state, HasLogContaining(runtime.state, "占领"), !HasLogContaining(runtime.state, "占领"));
            if (!string.IsNullOrEmpty(failure)) return Fail(result, runtime.state, runtime.worldState, failure);

            return Pass(result, runtime.state, runtime.worldState);
        }

        public HeadlessSimulationResult RunActiveRetreatFromEngagement(IDataRepository data, string playerFactionId)
        {
            HeadlessSimulationResult result = CreateResult("active_retreat_leaves_engagement");
            SimulationRuntime runtime;
            if (!TryCreateRuntime(data, playerFactionId, result, out runtime)) return result;

            ArmyRuntimeState playerArmy;
            ArmyRuntimeState enemyArmy;
            if (!TryGetSmokeArmies(result, runtime.worldState, out playerArmy, out enemyArmy)) return result;

            string originRegionId = playerArmy.locationRegionId;
            string targetRegionId = enemyArmy.locationRegionId;
            if (!runtime.commands.MoveArmy(playerArmy.id, targetRegionId))
            {
                return Fail(result, runtime.state, runtime.worldState, "Initial move command failed for retreat scenario.");
            }

            runtime.movementSystem.ExecuteTurn(runtime.context);

            EngagementRuntimeState engagement;
            if (!runtime.worldState.Map.TryGetEngagementInRegion(targetRegionId, out engagement))
            {
                return Fail(result, runtime.state, runtime.worldState, "Expected engagement before active retreat.");
            }

            if (playerArmy.engagementId == null || !engagement.attackerArmyIds.Contains(playerArmy.id))
            {
                return Fail(result, runtime.state, runtime.worldState, "Moving army did not enter engagement before retreat.");
            }

            if (!runtime.commands.RetreatArmy(playerArmy.id, originRegionId))
            {
                return Fail(result, runtime.state, runtime.worldState, "Retreat command failed for engaged army.");
            }

            if (playerArmy.task != ArmyTask.Retreat)
            {
                return Fail(result, runtime.state, runtime.worldState, "Retreat command did not set ArmyTask.Retreat.");
            }

            if (playerArmy.targetRegionId != originRegionId)
            {
                return Fail(result, runtime.state, runtime.worldState, "Retreat command did not retain retreat target region.");
            }

            runtime.movementSystem.ExecuteTurn(runtime.context);
            result.turnsExecuted = 1;

            if (playerArmy.locationRegionId != originRegionId)
            {
                return Fail(result, runtime.state, runtime.worldState, "Retreating army did not move to retreat region while engaged.");
            }

            if (playerArmy.engagementId != null)
            {
                return Fail(result, runtime.state, runtime.worldState, "Retreating army kept stale engagementId after retreat movement.");
            }

            if (playerArmy.task != ArmyTask.Idle || playerArmy.targetRegionId != null || playerArmy.route.Count != 0)
            {
                return Fail(result, runtime.state, runtime.worldState, "Retreating army did not return to idle state after reaching retreat target.");
            }

            ArmyState legacyArmy = FindLegacyArmy(runtime.context, playerArmy.id);
            if (legacyArmy == null || legacyArmy.regionId != originRegionId)
            {
                return Fail(result, runtime.state, runtime.worldState, "Legacy army region did not mirror retreat movement.");
            }

            if (runtime.worldState.Map.TryGetEngagementInRegion(targetRegionId, out engagement))
            {
                return Fail(result, runtime.state, runtime.worldState, "Engagement remained after attacker actively retreated.");
            }

            RegionRuntimeState battleRegion;
            if (!runtime.worldState.Map.TryGetRegion(targetRegionId, out battleRegion) || battleRegion.occupationStatus != OccupationStatus.Controlled)
            {
                return Fail(result, runtime.state, runtime.worldState, "Battle region did not return to controlled state after active retreat.");
            }

            runtime.warResolutionSystem.ExecuteTurn(runtime.context);
            if (HasLogContaining(runtime.state, "战斗结束"))
            {
                return Fail(result, runtime.state, runtime.worldState, "War resolution should not resolve a battle after active retreat cleared the engagement.");
            }

            if (!HasLogContaining(runtime.state, "尝试脱离接敌") || !HasLogContaining(runtime.state, "脱离接敌并撤退"))
            {
                return Fail(result, runtime.state, runtime.worldState, "Expected active retreat intent and completion logs.");
            }

            return Pass(result, runtime.state, runtime.worldState);
        }

        private static HeadlessSimulationResult CreateResult(string scenarioName)
        {
            return new HeadlessSimulationResult { scenarioName = scenarioName };
        }

        private static bool TryCreateRuntime(IDataRepository data, string playerFactionId, HeadlessSimulationResult result, out SimulationRuntime runtime)
        {
            runtime = null;
            if (data == null)
            {
                Fail(result, null, null, "IDataRepository is required.");
                return false;
            }

            GameState state = GameStateFactory.CreateDefault(data, playerFactionId);
            WorldState worldState = WorldStateFactory.Create(state, data);
            EventBus events = new EventBus();
            GameContext context = new GameContext(state, data, events);
            MapGraphData graph = new MapGraphData(data);
            MapQueryService queries = new MapQueryService(worldState.Map, graph);
            MapCommandService commands = new MapCommandService(worldState, queries, context);

            runtime = new SimulationRuntime
            {
                state = state,
                worldState = worldState,
                context = context,
                commands = commands,
                movementSystem = new DomainArmyMovementSystem(worldState, commands, new DomainEngagementDetector()),
                warResolutionSystem = new DomainMapWarResolutionSystem(
                    worldState,
                    new DomainEngagementDetector(),
                    new DomainBattleSimulationSystem(),
                    new DomainOccupationSystem(new DomainGovernanceImpactSystem())),
                economySystem = new DomainEconomySystem(worldState)
            };
            return true;
        }

        private static bool TryGetSmokeArmies(HeadlessSimulationResult result, WorldState worldState, out ArmyRuntimeState playerArmy, out ArmyRuntimeState enemyArmy)
        {
            playerArmy = null;
            enemyArmy = null;
            if (!worldState.Map.TryGetArmy("army_player_1", out playerArmy) || !worldState.Map.TryGetArmy("army_enemy_1", out enemyArmy))
            {
                Fail(result, worldState.LegacyState, worldState, "Initial smoke armies are missing.");
                return false;
            }

            return true;
        }

        private static bool RunTurnsUntilBattle(HeadlessSimulationResult result, SimulationRuntime runtime)
        {
            for (int turn = 0; turn < MaxTurns; turn++)
            {
                runtime.movementSystem.ExecuteTurn(runtime.context);
                runtime.warResolutionSystem.ExecuteTurn(runtime.context);
                runtime.economySystem.ExecuteTurn(runtime.context);
                result.turnsExecuted = turn + 1;

                if (HasLogContaining(runtime.state, "战斗结束"))
                {
                    return true;
                }
            }

            return false;
        }

        private static string ValidateCommonWarLogs(GameState state, bool regionCaptured, bool regionDefended)
        {
            if (!HasLogContaining(state, "行军")) return "Missing required log: 行军";
            if (!HasLogContaining(state, "抵达")) return "Missing required log: 抵达";
            if (!HasLogContaining(state, "接敌")) return "Missing required log: 接敌";
            if (HasDuplicateContactLogWithoutMembershipChange(state)) return "Duplicate unchanged contact log detected.";
            if (!HasLogContaining(state, "战斗结束")) return "Missing required log: 战斗结束";
            if (!regionCaptured && !regionDefended) return "Missing required outcome: 占领 / 防守";
            if (regionCaptured && !HasLogContaining(state, "占领")) return "Missing required log: 占领";
            if (regionCaptured && HasLogContaining(state, "防守方获胜")) return "Contradictory logs: defender victory followed by occupation.";
            if (regionDefended && HasLogContaining(state, "进攻方获胜")) return "Contradictory logs: attacker victory without occupation.";
            if (regionCaptured && !HasLogContaining(state, "新占领")) return "Missing required log: 治理折损";
            if (!HasLogContaining(state, "收入 金钱")) return "Missing required log: 经济结算";
            return null;
        }

        private static string ValidateCapturedRegionState(SimulationRuntime runtime, string regionId, string previousOwnerFactionId, string expectedOwnerFactionId)
        {
            RegionRuntimeState runtimeRegion;
            if (!runtime.worldState.Map.TryGetRegion(regionId, out runtimeRegion)) return "Captured runtime region is missing.";
            if (runtimeRegion.ownerFactionId != expectedOwnerFactionId) return "Runtime region owner did not change to attacker faction.";
            if (runtimeRegion.occupationStatus != OccupationStatus.Occupied) return "Captured runtime region is not marked Occupied.";
            if (runtimeRegion.integration != OccupiedIntegration) return "Captured runtime region integration should be 25.";
            if (runtimeRegion.taxContributionPercent != OccupiedContributionPercent) return "Captured runtime region tax contribution should be 35%.";
            if (runtimeRegion.foodContributionPercent != OccupiedContributionPercent) return "Captured runtime region food contribution should be 35%.";
            if (runtimeRegion.rebellionRisk < 20) return "Captured runtime region rebellion risk did not include occupation pressure.";
            if (runtimeRegion.localPower < 8) return "Captured runtime region local power did not include occupation pressure.";
            if (runtimeRegion.annexationPressure < 10) return "Captured runtime region annexation pressure did not include occupation pressure.";

            RegionState legacyRegion = runtime.state.FindRegion(regionId);
            if (legacyRegion == null) return "Captured legacy region is missing.";
            if (legacyRegion.ownerFactionId != expectedOwnerFactionId) return "Legacy region owner did not mirror occupation.";
            if (legacyRegion.integration != runtimeRegion.integration) return "Legacy region integration did not mirror runtime governance impact.";
            if (legacyRegion.rebellionRisk != runtimeRegion.rebellionRisk) return "Legacy region rebellion risk did not mirror runtime governance impact.";
            if (legacyRegion.localPower != runtimeRegion.localPower) return "Legacy region local power did not mirror runtime governance impact.";
            if (legacyRegion.annexationPressure != runtimeRegion.annexationPressure) return "Legacy region annexation pressure did not mirror runtime governance impact.";

            FactionState previousOwner = runtime.state.FindFaction(previousOwnerFactionId);
            if (previousOwner != null && previousOwner.regionIds.Contains(regionId)) return "Previous owner still lists captured region.";

            FactionState newOwner = runtime.state.FindFaction(expectedOwnerFactionId);
            if (newOwner == null) return "Capturing faction is missing.";
            if (!newOwner.regionIds.Contains(regionId)) return "Capturing faction does not list captured region.";

            return null;
        }

        private static string ValidateEconomyContributionState(SimulationRuntime runtime, string regionId, string ownerFactionId)
        {
            FactionState faction = runtime.state.FindFaction(ownerFactionId);
            RegionState region = runtime.state.FindRegion(regionId);
            RegionRuntimeState runtimeRegion;
            if (faction == null || region == null || !runtime.worldState.Map.TryGetRegion(regionId, out runtimeRegion))
            {
                return "Cannot validate economy contribution state for captured region.";
            }

            NumericContext numericContext = NumericModifierFactory.ForFaction(faction);
            int baseTax = NumericFormulas.CalculateRegionalTax(region, numericContext);
            int baseFood = NumericFormulas.CalculateRegionalFood(region, numericContext);
            int effectiveTax = DomainMath.RoundToInt(baseTax * runtimeRegion.taxContributionPercent / 100f);
            int effectiveFood = DomainMath.RoundToInt(baseFood * runtimeRegion.foodContributionPercent / 100f);

            if (runtimeRegion.taxContributionPercent != OccupiedContributionPercent || runtimeRegion.foodContributionPercent != OccupiedContributionPercent)
            {
                return "Captured region contribution percent is not the occupied 35% value.";
            }

            if (effectiveTax > baseTax || effectiveFood > baseFood)
            {
                return "Captured region effective economy contribution exceeds base output.";
            }

            if (baseTax > 0 && effectiveTax >= baseTax)
            {
                return "Captured region tax contribution was not reduced by runtime occupation multiplier.";
            }

            if (baseFood > 0 && effectiveFood >= baseFood)
            {
                return "Captured region food contribution was not reduced by runtime occupation multiplier.";
            }

            if (!HasLogContaining(runtime.state, "收入 金钱"))
            {
                return "Economy settlement log missing after captured contribution validation.";
            }

            return null;
        }

        private static bool HasDuplicateContactLogWithoutMembershipChange(GameState state)
        {
            Dictionary<string, bool> seen = new Dictionary<string, bool>();
            if (state == null || state.turnLog == null) return false;

            for (int i = 0; i < state.turnLog.Count; i++)
            {
                TurnLogEntry entry = state.turnLog[i];
                if (entry == null || entry.message == null || !entry.message.Contains("发生接敌")) continue;
                if (seen.ContainsKey(entry.message)) return true;
                seen[entry.message] = true;
            }

            return false;
        }

        private static HeadlessSimulationResult Pass(HeadlessSimulationResult result, GameState state, WorldState worldState)
        {
            result.passed = true;
            result.state = state;
            result.worldState = worldState;
            return result;
        }

        private static HeadlessSimulationResult Fail(HeadlessSimulationResult result, GameState state, WorldState worldState, string reason)
        {
            result.passed = false;
            result.failureReason = reason;
            result.state = state;
            result.worldState = worldState;
            return result;
        }

        private static bool HasLogContaining(GameState state, string token)
        {
            return CountLogsContaining(state, token) > 0;
        }

        private static int CountLogsContaining(GameState state, string token)
        {
            if (state == null || state.turnLog == null || string.IsNullOrEmpty(token)) return 0;

            int count = 0;
            for (int i = 0; i < state.turnLog.Count; i++)
            {
                TurnLogEntry entry = state.turnLog[i];
                if (entry != null && entry.message != null && entry.message.Contains(token)) count++;
            }

            return count;
        }

        private static void SyncLegacyArmy(GameContext context, ArmyRuntimeState runtimeArmy)
        {
            if (context == null || context.State == null || runtimeArmy == null) return;
            for (int i = 0; i < context.State.armies.Count; i++)
            {
                ArmyState army = context.State.armies[i];
                if (army.id != runtimeArmy.id) continue;
                army.regionId = runtimeArmy.locationRegionId;
                army.soldiers = runtimeArmy.soldiers;
                army.morale = runtimeArmy.morale;
                army.unitId = runtimeArmy.unitId;
                return;
            }
        }

        private static ArmyState FindLegacyArmy(GameContext context, string armyId)
        {
            if (context == null || context.State == null) return null;

            for (int i = 0; i < context.State.armies.Count; i++)
            {
                if (context.State.armies[i].id == armyId) return context.State.armies[i];
            }

            return null;
        }

        private sealed class SimulationRuntime
        {
            public GameState state;
            public WorldState worldState;
            public GameContext context;
            public MapCommandService commands;
            public DomainArmyMovementSystem movementSystem;
            public DomainMapWarResolutionSystem warResolutionSystem;
            public DomainEconomySystem economySystem;
        }
    }
}
