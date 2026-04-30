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
        public HeadlessScenarioReport report;
    }

    public sealed class HeadlessSimulationSuiteResult
    {
        public bool passed;
        public List<HeadlessSimulationResult> scenarios = new List<HeadlessSimulationResult>();
        public HeadlessWarReport report;
    }

    public sealed class HeadlessWarReport
    {
        public string runName;
        public bool passed;
        public int scenarioCount;
        public int passedCount;
        public int failedCount;
        public string generatedAt;
        public List<HeadlessScenarioReport> scenarios = new List<HeadlessScenarioReport>();
    }

    public sealed class HeadlessScenarioReport
    {
        public string name;
        public bool passed;
        public int turnsExecuted;
        public string summary;
        public string failureStage;
        public string failureReason;
        public List<HeadlessPhaseResult> phaseResults = new List<HeadlessPhaseResult>();
        public List<HeadlessAssertionResult> assertions = new List<HeadlessAssertionResult>();
        public List<HeadlessKeyDelta> keyDeltas = new List<HeadlessKeyDelta>();
        public List<HeadlessExplanation> explanations = new List<HeadlessExplanation>();
        public List<string> logs = new List<string>();
    }

    public sealed class HeadlessPhaseResult
    {
        public string phase;
        public string status;
        public Dictionary<string, object> before = new Dictionary<string, object>();
        public Dictionary<string, object> after = new Dictionary<string, object>();
        public string explanation;
    }

    public sealed class HeadlessAssertionResult
    {
        public string id;
        public string phase;
        public bool passed;
        public object expected;
        public object actual;
        public string message;
    }

    public sealed class HeadlessKeyDelta
    {
        public string field;
        public string entityId;
        public object before;
        public object after;
        public string impact;
    }

    public sealed class HeadlessExplanation
    {
        public string phase;
        public string message;
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
            suite.report = new HeadlessWarReport
            {
                runName = "headless_war_four_scenarios",
                passed = true
            };

            for (int i = 0; i < suite.scenarios.Count; i++)
            {
                if (!suite.scenarios[i].passed)
                {
                    suite.passed = false;
                }

                FinalizeScenarioReport(suite.scenarios[i]);
                suite.report.scenarios.Add(suite.scenarios[i].report);
                if (suite.scenarios[i].passed) suite.report.passedCount++;
                else suite.report.failedCount++;
            }

            suite.report.passed = suite.passed;
            suite.report.scenarioCount = suite.scenarios.Count;
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

            string originRegionId = playerArmy.locationRegionId;
            string targetRegionId = enemyArmy.locationRegionId;
            string originalOwnerFactionId = enemyArmy.ownerFactionId;
            int attackerSoldiersBefore = playerArmy.soldiers;
            int defenderSoldiersBefore = enemyArmy.soldiers;
            int attackerMoraleBefore = playerArmy.morale;
            int defenderMoraleBefore = enemyArmy.morale;
            bool issued = runtime.commands.MoveArmy(playerArmy.id, targetRegionId);
            AddPhase(result, "command", issued ? "pass" : "fail",
                Fields("armyId", playerArmy.id, "from", originRegionId),
                Fields("targetRegionId", targetRegionId, "task", playerArmy.task.ToString(), "routeLength", playerArmy.route.Count),
                "玩家发出进攻命令，系统为部队创建到目标地区的路线。");
            AddAssertion(result, "command.attack_route_created", "command", issued && playerArmy.route.Count >= 2 && playerArmy.targetRegionId == targetRegionId,
                true, issued, "Attack command should create route and retain target region.");
            if (!issued)
            {
                return Fail(result, runtime.state, runtime.worldState, "MapCommandService.MoveArmy failed for the smoke route.");
            }

            RunFullTurn(runtime);
            result.turnsExecuted = 1;

            EngagementRuntimeState engagement;
            bool engagementCreated = runtime.worldState.Map.TryGetEngagementInRegion(targetRegionId, out engagement);
            AddPhase(result, "movement", playerArmy.locationRegionId == targetRegionId ? "pass" : "fail",
                Fields("locationRegionId", originRegionId),
                Fields("locationRegionId", playerArmy.locationRegionId, "legacyRegionId", FindLegacyArmy(runtime.context, playerArmy.id).regionId),
                "进攻军抵达目标地区，legacy army 位置同步到同一地区。");
            AddAssertion(result, "movement.attacker_arrived_at_defended_region", "movement",
                playerArmy.locationRegionId == targetRegionId && FindLegacyArmy(runtime.context, playerArmy.id).regionId == targetRegionId,
                targetRegionId, playerArmy.locationRegionId, "Attacker should arrive at defended region before battle.");

            AddPhase(result, "engagement", engagementCreated ? "pass" : "fail",
                Fields("attackerArmyIds", 0, "defenderArmyIds", 0),
                Fields("engagementId", engagementCreated ? engagement.id : null, "attackerArmyIds", engagementCreated ? engagement.attackerArmyIds.Count : 0, "defenderArmyIds", engagementCreated ? engagement.defenderArmyIds.Count : 0),
                "敌对双方位于同一地区，系统创建接敌并把地区标记为争夺。");
            AddAssertion(result, "engagement.created_with_attacker_and_defender", "engagement",
                engagementCreated && engagement.attackerArmyIds.Contains(playerArmy.id) && engagement.defenderArmyIds.Contains(enemyArmy.id),
                true, engagementCreated, "Engagement should contain attacker and defender armies.");
            if (!engagementCreated)
            {
                return Fail(result, runtime.state, runtime.worldState, "Engagement was not created after attacker arrival.");
            }

            runtime.state.AdvanceHalfYear();
            RunFullTurn(runtime);
            result.turnsExecuted = 2;

            RegionRuntimeState targetRegion;
            bool regionCaptured = runtime.worldState.Map.TryGetRegion(targetRegionId, out targetRegion) && targetRegion.ownerFactionId != originalOwnerFactionId;
            bool regionDefended = runtime.worldState.Map.TryGetRegion(targetRegionId, out targetRegion) && targetRegion.ownerFactionId == originalOwnerFactionId;

            string failure = ValidateCommonWarLogs(runtime.state, regionCaptured, regionDefended);
            if (!string.IsNullOrEmpty(failure)) return Fail(result, runtime.state, runtime.worldState, failure);

            if (!HasLogContaining(runtime.state, "战败后撤退") && !HasLogContaining(runtime.state, "溃散"))
            {
                return Fail(result, runtime.state, runtime.worldState, "Expected loser retreat or rout log.");
            }

            ArmyRuntimeState remainingAttacker;
            bool attackerRemaining = runtime.worldState.Map.TryGetArmy(playerArmy.id, out remainingAttacker);
            bool attackerRetreatedOrRouted = !attackerRemaining || (remainingAttacker.locationRegionId != targetRegionId && remainingAttacker.engagementId == null);
            EngagementRuntimeState completedEngagement = engagement;
            EngagementRuntimeState leftoverEngagement;
            bool noEngagementLeft = !runtime.worldState.Map.TryGetEngagementInRegion(targetRegionId, out leftoverEngagement);
            AddPhase(result, "battle", completedEngagement.result != null && !completedEngagement.result.attackerWon ? "pass" : "fail",
                Fields("attackerSoldiers", attackerSoldiersBefore, "defenderSoldiers", defenderSoldiersBefore, "attackerMorale", attackerMoraleBefore, "defenderMorale", defenderMoraleBefore),
                Fields("attackerWon", completedEngagement.result != null && completedEngagement.result.attackerWon, "attackerSoldiers", attackerRemaining ? remainingAttacker.soldiers : 0, "defenderSoldiers", enemyArmy.soldiers),
                "防守方战力更高，自动战斗结算为防守方获胜。");
            AddAssertion(result, "battle.defender_won", "battle",
                completedEngagement.result != null && !completedEngagement.result.attackerWon,
                false, completedEngagement.result != null && completedEngagement.result.attackerWon, "Defender should win this scenario.");

            AddPhase(result, "outcome", regionDefended && attackerRetreatedOrRouted && noEngagementLeft ? "pass" : "fail",
                Fields("ownerFactionId", originalOwnerFactionId, "attackerLocation", targetRegionId),
                Fields("ownerFactionId", targetRegion.ownerFactionId, "attackerRemaining", attackerRemaining, "attackerLocation", attackerRemaining ? remainingAttacker.locationRegionId : "routed", "engagementLeft", !noEngagementLeft),
                "防守方守住地区，进攻军撤退或溃散，接敌被清理。");
            AddAssertion(result, "outcome.attacker_retreated_or_routed", "outcome", attackerRetreatedOrRouted,
                true, attackerRetreatedOrRouted, "Attacker should retreat or rout after defender victory.");
            AddAssertion(result, "outcome.region_owner_unchanged", "outcome", regionDefended,
                originalOwnerFactionId, targetRegion.ownerFactionId, "Region owner should remain defender faction.");
            AddAssertion(result, "outcome.no_resolved_engagement_left", "outcome", noEngagementLeft,
                true, noEngagementLeft, "Resolved engagement should be removed from map.");
            AddPhase(result, "governance", "skip", null, null, "防守成功没有产生新占领，治理折损阶段跳过。");
            AddPhase(result, "economy", HasLogContaining(runtime.state, "收入 金钱") ? "pass" : "fail",
                Fields("moneySnapshotAvailable", runtime.lastEconomyMoneyBeforeByFaction != null),
                Fields("moneySnapshotAvailable", runtime.lastEconomyMoneyAfterByFaction != null),
                "战斗结果进入本回合经济结算。");
            AddAssertion(result, "economy.settlement_ran_after_defense", "economy", runtime.lastEconomyMoneyAfterByFaction != null,
                true, runtime.lastEconomyMoneyAfterByFaction != null, "Economy settlement should run after defended battle.");
            AddKeyDelta(result, "region.ownerFactionId", targetRegionId, originalOwnerFactionId, targetRegion.ownerFactionId,
                "玩家未取得地区控制权，防守方继续获得该地区收益。");
            AddKeyDelta(result, "army.locationRegionId", playerArmy.id, targetRegionId, attackerRemaining ? remainingAttacker.locationRegionId : "routed",
                "进攻失败导致军队撤回己方相邻地区或溃散。");

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

            string originRegionId = playerArmy.locationRegionId;
            string targetRegionId = enemyArmy.locationRegionId;
            string originalOwnerFactionId = enemyArmy.ownerFactionId;
            int attackerSoldiersBefore = playerArmy.soldiers;
            int defenderSoldiersBefore = enemyArmy.soldiers;
            int attackerMoraleBefore = playerArmy.morale;
            int defenderMoraleBefore = enemyArmy.morale;
            bool issued = runtime.commands.MoveArmy(playerArmy.id, targetRegionId);
            AddPhase(result, "command", issued ? "pass" : "fail",
                Fields("armyId", playerArmy.id, "from", originRegionId),
                Fields("targetRegionId", targetRegionId, "task", playerArmy.task.ToString(), "routeLength", playerArmy.route.Count),
                "玩家发出进攻命令，强势进攻军获得到目标地区的路线。");
            AddAssertion(result, "command.attack_route_created", "command", issued && playerArmy.route.Count >= 2 && playerArmy.targetRegionId == targetRegionId,
                true, issued, "Attack command should create route and retain target region.");
            if (!issued)
            {
                return Fail(result, runtime.state, runtime.worldState, "Move command failed for attacker occupation scenario.");
            }

            RunFullTurn(runtime);
            result.turnsExecuted = 1;

            EngagementRuntimeState engagement;
            bool engagementCreated = runtime.worldState.Map.TryGetEngagementInRegion(targetRegionId, out engagement);
            AddPhase(result, "movement", playerArmy.locationRegionId == targetRegionId ? "pass" : "fail",
                Fields("locationRegionId", originRegionId),
                Fields("locationRegionId", playerArmy.locationRegionId, "legacyRegionId", FindLegacyArmy(runtime.context, playerArmy.id).regionId),
                "进攻军抵达敌方地区，等待下一回合战斗结算。");
            AddAssertion(result, "movement.attacker_arrived_at_target_region", "movement",
                playerArmy.locationRegionId == targetRegionId && FindLegacyArmy(runtime.context, playerArmy.id).regionId == targetRegionId,
                targetRegionId, playerArmy.locationRegionId, "Attacker should arrive at target region before occupation battle.");
            AddPhase(result, "engagement", engagementCreated ? "pass" : "fail",
                Fields("attackerArmyIds", 0, "defenderArmyIds", 0),
                Fields("engagementId", engagementCreated ? engagement.id : null, "attackerArmyIds", engagementCreated ? engagement.attackerArmyIds.Count : 0, "defenderArmyIds", engagementCreated ? engagement.defenderArmyIds.Count : 0),
                "敌对双方在同一地区形成接敌，目标地区进入争夺状态。");
            AddAssertion(result, "engagement.created_with_attacker_and_defender", "engagement",
                engagementCreated && engagement.attackerArmyIds.Contains(playerArmy.id) && engagement.defenderArmyIds.Contains(enemyArmy.id),
                true, engagementCreated, "Engagement should contain attacker and defender armies.");
            if (!engagementCreated)
            {
                return Fail(result, runtime.state, runtime.worldState, "Engagement was not created before attacker occupation battle.");
            }

            runtime.state.AdvanceHalfYear();
            RunFullTurn(runtime);
            result.turnsExecuted = 2;

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

            ArmyRuntimeState remainingEnemy;
            bool defenderRemaining = runtime.worldState.Map.TryGetArmy(enemyArmy.id, out remainingEnemy);
            EngagementRuntimeState completedEngagement = engagement;
            EngagementRuntimeState leftoverEngagement;
            bool noEngagementLeft = !runtime.worldState.Map.TryGetEngagementInRegion(targetRegionId, out leftoverEngagement);
            AddPhase(result, "battle", completedEngagement.result != null && completedEngagement.result.attackerWon ? "pass" : "fail",
                Fields("attackerSoldiers", attackerSoldiersBefore, "defenderSoldiers", defenderSoldiersBefore, "attackerMorale", attackerMoraleBefore, "defenderMorale", defenderMoraleBefore),
                Fields("attackerWon", completedEngagement.result != null && completedEngagement.result.attackerWon, "attackerSoldiers", playerArmy.soldiers, "defenderRemaining", defenderRemaining),
                "进攻方兵力和士气优势明显，自动战斗结算为进攻方获胜。");
            AddAssertion(result, "battle.attacker_won", "battle",
                completedEngagement.result != null && completedEngagement.result.attackerWon,
                true, completedEngagement.result != null && completedEngagement.result.attackerWon, "Attacker should win this occupation scenario.");

            AddPhase(result, "outcome", regionCaptured && noEngagementLeft ? "pass" : "fail",
                Fields("ownerFactionId", originalOwnerFactionId),
                Fields("ownerFactionId", targetRegion.ownerFactionId, "defenderRemaining", defenderRemaining, "engagementLeft", !noEngagementLeft),
                "进攻方获胜后取得地区控制权，防守军撤退或溃散，接敌被清理。");
            AddAssertion(result, "outcome.region_owner_changed_to_attacker", "outcome", regionCaptured,
                playerArmy.ownerFactionId, targetRegion.ownerFactionId, "Captured region owner should become attacker faction.");
            AddAssertion(result, "outcome.no_resolved_engagement_left", "outcome", noEngagementLeft,
                true, noEngagementLeft, "Resolved engagement should be removed after occupation.");

            AddPhase(result, "governance", targetRegion.integration == OccupiedIntegration && targetRegion.taxContributionPercent == OccupiedContributionPercent ? "pass" : "fail",
                Fields("integration", 70, "taxContributionPercent", 70, "foodContributionPercent", 70),
                Fields("integration", targetRegion.integration, "taxContributionPercent", targetRegion.taxContributionPercent, "foodContributionPercent", targetRegion.foodContributionPercent),
                "新占领地区立刻进入低整合状态，税粮贡献按治理折损比例计算。");
            AddAssertion(result, "governance.occupation_reduced_contribution", "governance",
                targetRegion.integration == OccupiedIntegration &&
                    targetRegion.taxContributionPercent == OccupiedContributionPercent &&
                    targetRegion.foodContributionPercent == OccupiedContributionPercent,
                OccupiedContributionPercent, targetRegion.taxContributionPercent, "Occupation should reduce tax and food contribution to 35%.");

            AddEconomyDeltaAssertions(result, runtime, playerArmy.ownerFactionId);
            AddPhase(result, "economy", HasAssertionPassed(result, "economy.money_delta_matches_runtime_contribution") && HasAssertionPassed(result, "economy.food_delta_matches_runtime_contribution") ? "pass" : "fail",
                Fields("money", GetSnapshotValue(runtime.lastEconomyMoneyBeforeByFaction, playerArmy.ownerFactionId), "food", GetSnapshotValue(runtime.lastEconomyFoodBeforeByFaction, playerArmy.ownerFactionId)),
                Fields("money", GetSnapshotValue(runtime.lastEconomyMoneyAfterByFaction, playerArmy.ownerFactionId), "food", GetSnapshotValue(runtime.lastEconomyFoodAfterByFaction, playerArmy.ownerFactionId)),
                "经济系统使用 runtime 贡献率折算收入，再扣除军费和军粮。");
            AddKeyDelta(result, "region.ownerFactionId", targetRegionId, originalOwnerFactionId, targetRegion.ownerFactionId,
                "玩家取得地区控制权，但新占领地区先按治理折损贡献收入。");

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
            bool initialIssued = runtime.commands.MoveArmy(playerArmy.id, targetRegionId);
            AddAssertion(result, "command.attack_route_created", "command", initialIssued,
                true, initialIssued, "Initial attack command should create the base engagement.");
            if (!initialIssued)
            {
                return Fail(result, runtime.state, runtime.worldState, "Initial move command failed for reinforcement scenario.");
            }

            RunFullTurn(runtime);
            result.turnsExecuted = 1;
            if (HasLogContaining(runtime.state, "战斗结束"))
            {
                return Fail(result, runtime.state, runtime.worldState, "Newly formed engagement resolved before a reinforcement command window.");
            }
            runtime.state.AdvanceHalfYear();

            EngagementRuntimeState engagement;
            if (!runtime.worldState.Map.TryGetEngagementInRegion(targetRegionId, out engagement))
            {
                return Fail(result, runtime.state, runtime.worldState, "Expected engagement after first army arrival.");
            }
            AddPhase(result, "movement", playerArmy.locationRegionId == targetRegionId ? "pass" : "fail",
                Fields("attackerLocation", reinforcement.locationRegionId),
                Fields("attackerLocation", playerArmy.locationRegionId, "engagementRegionId", targetRegionId),
                "进攻军先抵达敌方地区并形成接敌，为后续增援提供目标。");
            AddAssertion(result, "movement.attacker_arrived_at_defended_region", "movement",
                playerArmy.locationRegionId == targetRegionId, targetRegionId, playerArmy.locationRegionId,
                "Initial attacker should arrive before reinforcement command.");

            if (enemyArmy.task != ArmyTask.Idle)
            {
                return Fail(result, runtime.state, runtime.worldState, "Defending army task changed after contact.");
            }

            int attackerCountBefore = engagement.attackerArmyIds.Count;
            int defenderCountBefore = engagement.defenderArmyIds.Count;
            int attackerPowerBefore = CalculateReportSidePower(runtime.context, runtime.worldState.Map, engagement.attackerArmyIds, true);
            int defenderPowerBefore = CalculateReportSidePower(runtime.context, runtime.worldState.Map, engagement.defenderArmyIds, false);
            int contactLogsBefore = CountLogsContaining(runtime.state, "发生接敌");

            bool reinforcementIssued = runtime.commands.ReinforceArmy(reinforcement.id, targetRegionId);
            AddPhase(result, "command", reinforcementIssued ? "pass" : "fail",
                Fields("armyId", reinforcement.id, "from", reinforcement.locationRegionId),
                Fields("targetRegionId", targetRegionId, "task", reinforcement.task.ToString(), "routeLength", reinforcement.route.Count),
                "玩家发出增援命令，增援部队以既有接敌地区为目标行军。");
            AddAssertion(result, "command.reinforcement_route_created", "command",
                reinforcementIssued && reinforcement.route.Count >= 2 && reinforcement.targetRegionId == targetRegionId && reinforcement.task == ArmyTask.Reinforce,
                true, reinforcementIssued, "Reinforcement command should create route and set ArmyTask.Reinforce.");
            if (!reinforcementIssued)
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
            result.turnsExecuted = 2;

            if (!runtime.worldState.Map.TryGetEngagementInRegion(targetRegionId, out engagement))
            {
                return Fail(result, runtime.state, runtime.worldState, "Expected engagement to remain after reinforcement arrival.");
            }

            if (enemyArmy.task != ArmyTask.Idle)
            {
                return Fail(result, runtime.state, runtime.worldState, "Defending army task changed after reinforcement contact.");
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
            int attackerPowerAfter = CalculateReportSidePower(runtime.context, runtime.worldState.Map, engagement.attackerArmyIds, true);
            AddPhase(result, "engagement", engagement.attackerArmyIds.Contains(reinforcement.id) ? "pass" : "fail",
                Fields("attackerArmyIds", attackerCountBefore, "defenderArmyIds", defenderCountBefore, "attackerPower", attackerPowerBefore, "defenderPower", defenderPowerBefore),
                Fields("attackerArmyIds", engagement.attackerArmyIds.Count, "defenderArmyIds", engagement.defenderArmyIds.Count, "attackerPower", attackerPowerAfter, "defenderPower", defenderPowerBefore),
                "增援抵达后加入进攻方成员列表，进攻方战力随 membership 增加。");
            AddAssertion(result, "movement.reinforcement_arrived_at_engagement_region", "movement",
                reinforcement.locationRegionId == targetRegionId,
                targetRegionId, reinforcement.locationRegionId, "Reinforcement should arrive at engagement region.");
            AddAssertion(result, "engagement.membership_changed_after_reinforcement", "engagement",
                engagement.attackerArmyIds.Contains(reinforcement.id) && engagement.attackerArmyIds.Count > attackerCountBefore,
                attackerCountBefore + 1, engagement.attackerArmyIds.Count, "Attacker membership should increase after reinforcement arrival.");

            runtime.warResolutionSystem.ExecuteTurn(runtime.context);
            RunEconomyWithSnapshot(runtime);

            string failure = ValidateCommonWarLogs(runtime.state, HasLogContaining(runtime.state, "占领"), !HasLogContaining(runtime.state, "占领"));
            if (!string.IsNullOrEmpty(failure)) return Fail(result, runtime.state, runtime.worldState, failure);

            AddPhase(result, "battle", engagement.result != null ? "pass" : "fail",
                Fields("attackerPowerBeforeReinforcement", attackerPowerBefore, "attackerPowerAfterReinforcement", attackerPowerAfter, "defenderPower", defenderPowerBefore),
                Fields("attackerWon", engagement.result != null && engagement.result.attackerWon, "attackerArmyIds", engagement.attackerArmyIds.Count),
                "进攻方增援后战力上升，战斗结果由新的双方 membership 解释。");
            AddAssertion(result, "battle.result_changed_or_explained_by_membership", "battle",
                engagement.result != null && engagement.result.attackerWon && attackerPowerAfter > attackerPowerBefore,
                true, engagement.result != null && engagement.result.attackerWon, "Battle result should be explained by increased attacker membership and power.");

            AddPhase(result, "outcome", reinforcement.task == ArmyTask.Idle && reinforcement.engagementId == null ? "pass" : "fail",
                Fields("task", ArmyTask.Reinforce.ToString(), "engagementId", engagement.id),
                Fields("task", reinforcement.task.ToString(), "engagementId", reinforcement.engagementId),
                "战斗结束后增援军的临时增援任务和接敌引用被清理。");
            AddAssertion(result, "outcome.no_stale_reinforcement_task", "outcome",
                reinforcement.task == ArmyTask.Idle && reinforcement.engagementId == null,
                ArmyTask.Idle.ToString(), reinforcement.task.ToString(), "Reinforcement should not keep stale task after battle.");
            AddPhase(result, "governance", HasLogContaining(runtime.state, "新占领") ? "pass" : "skip", null, null,
                HasLogContaining(runtime.state, "新占领") ? "进攻方获胜后触发新占领治理折损。" : "该次增援未产生占领，治理折损阶段跳过。");
            AddPhase(result, "economy", runtime.lastEconomyMoneyAfterByFaction != null ? "pass" : "fail",
                Fields("moneySnapshotAvailable", runtime.lastEconomyMoneyBeforeByFaction != null),
                Fields("moneySnapshotAvailable", runtime.lastEconomyMoneyAfterByFaction != null),
                "增援后的战斗结果进入经济结算。");
            AddKeyDelta(result, "engagement.attackerArmyIds.Count", targetRegionId, attackerCountBefore, engagement.attackerArmyIds.Count,
                "增援加入进攻方后，战斗用新的成员列表计算。");
            AddExplanation(result, "battle", "增援前进攻方战力=" + attackerPowerBefore + "，增援后进攻方战力=" + attackerPowerAfter + "，防守方战力=" + defenderPowerBefore + "。");

            return Pass(result, runtime.state, runtime.worldState);
        }

        public HeadlessSimulationResult RunDefenderReinforcementMembership(IDataRepository data, string playerFactionId)
        {
            HeadlessSimulationResult result = CreateResult("defender_reinforcement_joins_existing_engagement");
            SimulationRuntime runtime;
            if (!TryCreateRuntime(data, playerFactionId, result, out runtime)) return result;

            ArmyRuntimeState playerArmy;
            ArmyRuntimeState enemyArmy;
            if (!TryGetSmokeArmies(result, runtime.worldState, out playerArmy, out enemyArmy)) return result;

            ArmyRuntimeState reinforcement = new ArmyRuntimeState
            {
                id = "army_enemy_reinforcement_1",
                ownerFactionId = enemyArmy.ownerFactionId,
                locationRegionId = playerArmy.locationRegionId,
                targetRegionId = null,
                route = new List<string>(),
                task = ArmyTask.Idle,
                unitId = enemyArmy.unitId,
                soldiers = 1200,
                morale = 70,
                supply = 80,
                movementPoints = enemyArmy.movementPoints,
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
                return Fail(result, runtime.state, runtime.worldState, "Initial move command failed for defender reinforcement scenario.");
            }

            RunFullTurn(runtime);
            result.turnsExecuted = 1;
            if (HasLogContaining(runtime.state, "战斗结束"))
            {
                return Fail(result, runtime.state, runtime.worldState, "Newly formed engagement resolved before defender reinforcement command window.");
            }
            runtime.state.AdvanceHalfYear();

            EngagementRuntimeState engagement;
            if (!runtime.worldState.Map.TryGetEngagementInRegion(targetRegionId, out engagement))
            {
                return Fail(result, runtime.state, runtime.worldState, "Expected engagement before defender reinforcement.");
            }

            int defenderCountBefore = engagement.defenderArmyIds.Count;
            int attackerCountBefore = engagement.attackerArmyIds.Count;

            if (!runtime.commands.ReinforceArmy(reinforcement.id, targetRegionId))
            {
                return Fail(result, runtime.state, runtime.worldState, "Defender reinforce command failed.");
            }

            runtime.movementSystem.ExecuteTurn(runtime.context);
            result.turnsExecuted = 2;

            if (!runtime.worldState.Map.TryGetEngagementInRegion(targetRegionId, out engagement))
            {
                return Fail(result, runtime.state, runtime.worldState, "Expected engagement to remain after defender reinforcement arrival.");
            }

            if (!engagement.defenderArmyIds.Contains(reinforcement.id))
            {
                return Fail(result, runtime.state, runtime.worldState, "Defender reinforcement did not join defender side membership.");
            }

            if (engagement.attackerArmyIds.Contains(reinforcement.id))
            {
                return Fail(result, runtime.state, runtime.worldState, "Defender reinforcement was incorrectly added to attacker side.");
            }

            if (engagement.defenderArmyIds.Count <= defenderCountBefore)
            {
                return Fail(result, runtime.state, runtime.worldState, "Defender membership did not increase after reinforcement.");
            }

            if (engagement.attackerArmyIds.Count != attackerCountBefore)
            {
                return Fail(result, runtime.state, runtime.worldState, "Attacker membership changed during defender reinforcement.");
            }

            runtime.warResolutionSystem.ExecuteTurn(runtime.context);
            RunEconomyWithSnapshot(runtime);

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
            bool initialIssued = runtime.commands.MoveArmy(playerArmy.id, targetRegionId);
            AddAssertion(result, "command.attack_route_created", "command", initialIssued,
                true, initialIssued, "Initial attack command should create engagement before retreat.");
            if (!initialIssued)
            {
                return Fail(result, runtime.state, runtime.worldState, "Initial move command failed for retreat scenario.");
            }

            RunFullTurn(runtime);
            result.turnsExecuted = 1;
            if (HasLogContaining(runtime.state, "战斗结束"))
            {
                return Fail(result, runtime.state, runtime.worldState, "Newly formed engagement resolved before active retreat command window.");
            }
            runtime.state.AdvanceHalfYear();

            EngagementRuntimeState engagement;
            if (!runtime.worldState.Map.TryGetEngagementInRegion(targetRegionId, out engagement))
            {
                return Fail(result, runtime.state, runtime.worldState, "Expected engagement before active retreat.");
            }
            AddPhase(result, "movement", playerArmy.locationRegionId == targetRegionId ? "pass" : "fail",
                Fields("locationRegionId", originRegionId),
                Fields("locationRegionId", playerArmy.locationRegionId, "targetRegionId", targetRegionId),
                "进攻军抵达敌方地区并进入接敌，随后可选择主动撤退。");
            AddPhase(result, "engagement", engagement.attackerArmyIds.Contains(playerArmy.id) ? "pass" : "fail",
                Fields("attackerArmyIds", 0, "defenderArmyIds", 0),
                Fields("engagementId", engagement.id, "attackerArmyIds", engagement.attackerArmyIds.Count, "defenderArmyIds", engagement.defenderArmyIds.Count),
                "敌对双方形成接敌，进攻军被记录在 attacker membership。");

            if (playerArmy.engagementId == null || !engagement.attackerArmyIds.Contains(playerArmy.id))
            {
                return Fail(result, runtime.state, runtime.worldState, "Moving army did not enter engagement before retreat.");
            }

            if (enemyArmy.task != ArmyTask.Idle)
            {
                return Fail(result, runtime.state, runtime.worldState, "Defending army task changed before active retreat.");
            }

            bool retreatIssued = runtime.commands.RetreatArmy(playerArmy.id, originRegionId);
            AddPhase(result, "command", retreatIssued ? "pass" : "fail",
                Fields("armyId", playerArmy.id, "from", targetRegionId, "engagementId", playerArmy.engagementId),
                Fields("targetRegionId", originRegionId, "task", playerArmy.task.ToString(), "routeLength", playerArmy.route.Count),
                "玩家对已接敌军队发出撤退命令，撤退会在行军阶段脱离接敌。");
            AddAssertion(result, "command.retreat_route_created", "command",
                retreatIssued && playerArmy.task == ArmyTask.Retreat && playerArmy.targetRegionId == originRegionId && playerArmy.route.Count >= 2,
                true, retreatIssued, "Retreat command should create route and set ArmyTask.Retreat.");
            if (!retreatIssued)
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

            RunFullTurn(runtime);
            result.turnsExecuted = 2;

            if (playerArmy.locationRegionId != originRegionId)
            {
                return Fail(result, runtime.state, runtime.worldState, "Retreating army did not move to retreat region while engaged.");
            }
            AddAssertion(result, "movement.retreating_army_left_region", "movement",
                playerArmy.locationRegionId == originRegionId,
                originRegionId, playerArmy.locationRegionId, "Retreating army should move back to origin region.");

            if (playerArmy.engagementId != null)
            {
                return Fail(result, runtime.state, runtime.worldState, "Retreating army kept stale engagementId after retreat movement.");
            }
            AddAssertion(result, "engagement.retreating_army_removed_from_membership", "engagement",
                playerArmy.engagementId == null,
                null, playerArmy.engagementId, "Retreating army should leave engagement membership.");

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
            AddAssertion(result, "outcome.no_residual_engagement_after_retreat", "outcome",
                true, true, true, "Engagement should be removed after attacker retreat leaves one side empty.");

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
            AddAssertion(result, "battle.no_battle_triggered_after_retreat", "battle",
                !HasLogContaining(runtime.state, "战斗结束"),
                false, HasLogContaining(runtime.state, "战斗结束"), "War resolution should not trigger battle after retreat clears engagement.");
            AddAssertion(result, "outcome.region_returns_to_controlled_when_uncontested", "outcome",
                battleRegion.occupationStatus == OccupationStatus.Controlled,
                OccupationStatus.Controlled.ToString(), battleRegion.occupationStatus.ToString(), "Uncontested region should return to Controlled.");

            if (!HasLogContaining(runtime.state, "尝试脱离接敌") || !HasLogContaining(runtime.state, "脱离接敌并撤退"))
            {
                return Fail(result, runtime.state, runtime.worldState, "Expected active retreat intent and completion logs.");
            }

            AddPhase(result, "outcome", playerArmy.task == ArmyTask.Idle && battleRegion.occupationStatus == OccupationStatus.Controlled ? "pass" : "fail",
                Fields("locationRegionId", targetRegionId, "engagementId", "set", "occupationStatus", OccupationStatus.Contested.ToString()),
                Fields("locationRegionId", playerArmy.locationRegionId, "engagementId", playerArmy.engagementId, "occupationStatus", battleRegion.occupationStatus.ToString()),
                "撤退军离开争夺地区后，接敌清空，目标地区恢复受控状态。");
            AddPhase(result, "battle", "skip", null, Fields("battleTriggered", false),
                "主动撤退已经清空接敌，战斗结算阶段没有可结算对象。");
            AddPhase(result, "governance", "skip", null, null, "主动撤退没有产生占领，治理折损阶段跳过。");
            AddPhase(result, "economy", runtime.lastEconomyMoneyAfterByFaction != null ? "pass" : "fail",
                Fields("moneySnapshotAvailable", runtime.lastEconomyMoneyBeforeByFaction != null),
                Fields("moneySnapshotAvailable", runtime.lastEconomyMoneyAfterByFaction != null),
                "撤退回合仍完成常规经济结算。");
            AddKeyDelta(result, "army.locationRegionId", playerArmy.id, targetRegionId, playerArmy.locationRegionId,
                "主动撤退让进攻军退出争夺地区，避免继续触发战斗。");
            AddKeyDelta(result, "region.occupationStatus", targetRegionId, OccupationStatus.Contested.ToString(), battleRegion.occupationStatus.ToString(),
                "接敌清空后地区恢复受控状态。");

            return Pass(result, runtime.state, runtime.worldState);
        }

        public HeadlessSimulationResult RunEngagedArmyRejectsNonRetreatCommands(IDataRepository data, string playerFactionId)
        {
            HeadlessSimulationResult result = CreateResult("engaged_army_rejects_non_retreat_commands");
            SimulationRuntime runtime;
            if (!TryCreateRuntime(data, playerFactionId, result, out runtime)) return result;

            ArmyRuntimeState playerArmy;
            ArmyRuntimeState enemyArmy;
            if (!TryGetSmokeArmies(result, runtime.worldState, out playerArmy, out enemyArmy)) return result;

            string originRegionId = playerArmy.locationRegionId;
            string targetRegionId = enemyArmy.locationRegionId;
            if (!runtime.commands.MoveArmy(playerArmy.id, targetRegionId))
            {
                return Fail(result, runtime.state, runtime.worldState, "Initial move command failed for engaged command guard scenario.");
            }

            RunFullTurn(runtime);
            result.turnsExecuted = 1;
            if (playerArmy.engagementId == null)
            {
                return Fail(result, runtime.state, runtime.worldState, "Army did not enter engagement before command guard assertions.");
            }

            if (runtime.commands.MoveArmy(playerArmy.id, originRegionId))
            {
                return Fail(result, runtime.state, runtime.worldState, "Engaged army accepted a normal move command.");
            }

            if (runtime.commands.ReinforceArmy(playerArmy.id, originRegionId))
            {
                return Fail(result, runtime.state, runtime.worldState, "Engaged army accepted a reinforce command.");
            }

            if (runtime.commands.SiegeRegion(playerArmy.id, originRegionId))
            {
                return Fail(result, runtime.state, runtime.worldState, "Engaged army accepted a siege command.");
            }

            if (runtime.commands.StopArmy(playerArmy.id))
            {
                return Fail(result, runtime.state, runtime.worldState, "Engaged army accepted a stop command that would leave stale engagement state.");
            }

            if (!runtime.commands.RetreatArmy(playerArmy.id, originRegionId))
            {
                return Fail(result, runtime.state, runtime.worldState, "Engaged army rejected retreat command.");
            }

            return Pass(result, runtime.state, runtime.worldState);
        }

        public HeadlessSimulationResult RunSameFactionContactDoesNotCreateEngagement(IDataRepository data, string playerFactionId)
        {
            HeadlessSimulationResult result = CreateResult("same_faction_contact_does_not_create_engagement");
            SimulationRuntime runtime;
            if (!TryCreateRuntime(data, playerFactionId, result, out runtime)) return result;

            ArmyRuntimeState playerArmy;
            ArmyRuntimeState enemyArmy;
            if (!TryGetSmokeArmies(result, runtime.worldState, out playerArmy, out enemyArmy)) return result;

            ArmyRuntimeState sameFactionArmy = new ArmyRuntimeState
            {
                id = "army_player_same_faction_1",
                ownerFactionId = playerArmy.ownerFactionId,
                locationRegionId = playerArmy.locationRegionId,
                targetRegionId = null,
                route = new List<string>(),
                task = ArmyTask.Idle,
                unitId = playerArmy.unitId,
                soldiers = 800,
                morale = 60,
                supply = 80,
                movementPoints = playerArmy.movementPoints,
                engagementId = null
            };
            runtime.worldState.Map.AddArmy(sameFactionArmy);

            EngagementRuntimeState engagement = new DomainEngagementDetector().DetectRegion(runtime.context, runtime.worldState.Map, playerArmy.locationRegionId);
            if (engagement != null)
            {
                return Fail(result, runtime.state, runtime.worldState, "Same-faction armies created an engagement.");
            }

            EngagementRuntimeState staleEngagement;
            if (runtime.worldState.Map.TryGetEngagementInRegion(playerArmy.locationRegionId, out staleEngagement))
            {
                return Fail(result, runtime.state, runtime.worldState, "Same-faction detection left a stale engagement indexed by region.");
            }

            RegionRuntimeState region;
            if (!runtime.worldState.Map.TryGetRegion(playerArmy.locationRegionId, out region) || region.occupationStatus != OccupationStatus.Controlled)
            {
                return Fail(result, runtime.state, runtime.worldState, "Same-faction detection changed region occupation status.");
            }

            return Pass(result, runtime.state, runtime.worldState);
        }

        private static HeadlessSimulationResult CreateResult(string scenarioName)
        {
            return new HeadlessSimulationResult
            {
                scenarioName = scenarioName,
                report = new HeadlessScenarioReport
                {
                    name = scenarioName,
                    summary = DefaultSummary(scenarioName)
                }
            };
        }

        private static string DefaultSummary(string scenarioName)
        {
            switch (scenarioName)
            {
                case "defender_holds_and_attacker_retreats":
                    return "防守方守住目标地区，进攻军撤退或溃散，地区归属不变并进入经济结算。";
                case "attacker_wins_and_occupies":
                    return "进攻军占领目标地区，治理折损立即生效，下一次经济结算按折损后贡献入账。";
                case "reinforcement_joins_existing_engagement":
                    return "增援抵达既有接敌地区并加入进攻方，成员变化改变战力解释并触发结算。";
                case "active_retreat_leaves_engagement":
                    return "已接敌军队主动撤退并脱离接敌，地区恢复受控状态，战斗不再误结算。";
                default:
                    return scenarioName;
            }
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
            MapCommandService commands = new MapCommandService(queries, context);

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
                RunFullTurn(runtime);
                result.turnsExecuted = turn + 1;

                if (HasLogContaining(runtime.state, "战斗结束"))
                {
                    return true;
                }

                runtime.state.AdvanceHalfYear();
            }

            return false;
        }

        private static void RunFullTurn(SimulationRuntime runtime)
        {
            runtime.movementSystem.ExecuteTurn(runtime.context);
            runtime.warResolutionSystem.ExecuteTurn(runtime.context);
            RunEconomyWithSnapshot(runtime);
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
            if (legacyRegion.occupationStatus != runtimeRegion.occupationStatus) return "Legacy region occupation status did not mirror runtime occupation.";
            if (legacyRegion.integration != runtimeRegion.integration) return "Legacy region integration did not mirror runtime governance impact.";
            if (legacyRegion.taxContributionPercent != runtimeRegion.taxContributionPercent) return "Legacy region tax contribution did not mirror runtime governance impact.";
            if (legacyRegion.foodContributionPercent != runtimeRegion.foodContributionPercent) return "Legacy region food contribution did not mirror runtime governance impact.";
            if (legacyRegion.rebellionRisk != runtimeRegion.rebellionRisk) return "Legacy region rebellion risk did not mirror runtime governance impact.";
            if (legacyRegion.localPower != runtimeRegion.localPower) return "Legacy region local power did not mirror runtime governance impact.";
            if (legacyRegion.annexationPressure != runtimeRegion.annexationPressure) return "Legacy region annexation pressure did not mirror runtime governance impact.";

            WorldState rebuiltWorldState = WorldStateFactory.Create(runtime.state, runtime.context.Data);
            RegionRuntimeState rebuiltRegion;
            if (!rebuiltWorldState.Map.TryGetRegion(regionId, out rebuiltRegion)) return "Rebuilt runtime region is missing after occupation.";
            if (rebuiltRegion.occupationStatus != OccupationStatus.Occupied) return "Rebuilt runtime region did not preserve occupied status.";
            if (rebuiltRegion.taxContributionPercent != OccupiedContributionPercent) return "Rebuilt runtime region did not preserve occupied tax contribution.";
            if (rebuiltRegion.foodContributionPercent != OccupiedContributionPercent) return "Rebuilt runtime region did not preserve occupied food contribution.";

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

            string resourceDeltaFailure = ValidateEconomyResourceDelta(runtime, ownerFactionId);
            if (!string.IsNullOrEmpty(resourceDeltaFailure)) return resourceDeltaFailure;

            return null;
        }

        private static string ValidateEconomyResourceDelta(SimulationRuntime runtime, string ownerFactionId)
        {
            if (runtime == null || runtime.state == null || runtime.context == null)
            {
                return "Cannot validate economy resource delta without runtime state.";
            }

            if (runtime.lastEconomyMoneyBeforeByFaction == null || runtime.lastEconomyMoneyAfterByFaction == null ||
                runtime.lastEconomyFoodBeforeByFaction == null || runtime.lastEconomyFoodAfterByFaction == null)
            {
                return "Economy resource snapshots are missing.";
            }

            int moneyBefore;
            int moneyAfter;
            int foodBefore;
            int foodAfter;
            if (!runtime.lastEconomyMoneyBeforeByFaction.TryGetValue(ownerFactionId, out moneyBefore) ||
                !runtime.lastEconomyMoneyAfterByFaction.TryGetValue(ownerFactionId, out moneyAfter) ||
                !runtime.lastEconomyFoodBeforeByFaction.TryGetValue(ownerFactionId, out foodBefore) ||
                !runtime.lastEconomyFoodAfterByFaction.TryGetValue(ownerFactionId, out foodAfter))
            {
                return "Capturing faction economy resource snapshots are missing.";
            }

            FactionState faction = runtime.state.FindFaction(ownerFactionId);
            if (faction == null) return "Capturing faction is missing for economy resource delta validation.";

            NumericContext numericContext = NumericModifierFactory.ForFaction(faction);
            int expectedTax;
            int expectedFood;
            CalculateExpectedFactionOutput(runtime, faction, numericContext, out expectedTax, out expectedFood);

            int expectedMoneyUpkeep = CalculateExpectedArmyUpkeep(runtime.context, faction, numericContext, false) +
                NumericFormulas.CalculateGovernanceUpkeep(numericContext, false) +
                NumericFormulas.CalculateTreasuryReserveDrag(faction, false);
            int expectedFoodUpkeep = CalculateExpectedArmyUpkeep(runtime.context, faction, numericContext, true) +
                NumericFormulas.CalculateGovernanceUpkeep(numericContext, true) +
                NumericFormulas.CalculateTreasuryReserveDrag(faction, true);

            int expectedMoneyDelta = expectedTax - expectedMoneyUpkeep;
            int expectedFoodDelta = expectedFood - expectedFoodUpkeep;
            int actualMoneyDelta = moneyAfter - moneyBefore;
            int actualFoodDelta = foodAfter - foodBefore;

            if (actualMoneyDelta != expectedMoneyDelta)
            {
                return "Capturing faction money delta mismatch. expected=" + expectedMoneyDelta + " actual=" + actualMoneyDelta;
            }

            if (actualFoodDelta != expectedFoodDelta)
            {
                return "Capturing faction food delta mismatch. expected=" + expectedFoodDelta + " actual=" + actualFoodDelta;
            }

            return null;
        }

        private static void AddEconomyDeltaAssertions(HeadlessSimulationResult result, SimulationRuntime runtime, string ownerFactionId)
        {
            if (runtime == null || runtime.state == null || runtime.context == null) return;
            FactionState faction = runtime.state.FindFaction(ownerFactionId);
            if (faction == null) return;

            int moneyBefore = GetSnapshotValue(runtime.lastEconomyMoneyBeforeByFaction, ownerFactionId);
            int moneyAfter = GetSnapshotValue(runtime.lastEconomyMoneyAfterByFaction, ownerFactionId);
            int foodBefore = GetSnapshotValue(runtime.lastEconomyFoodBeforeByFaction, ownerFactionId);
            int foodAfter = GetSnapshotValue(runtime.lastEconomyFoodAfterByFaction, ownerFactionId);

            NumericContext numericContext = NumericModifierFactory.ForFaction(faction);
            int expectedTax;
            int expectedFood;
            CalculateExpectedFactionOutput(runtime, faction, numericContext, out expectedTax, out expectedFood);

            int expectedMoneyUpkeep = CalculateExpectedArmyUpkeep(runtime.context, faction, numericContext, false) +
                NumericFormulas.CalculateGovernanceUpkeep(numericContext, false) +
                NumericFormulas.CalculateTreasuryReserveDrag(faction, false);
            int expectedFoodUpkeep = CalculateExpectedArmyUpkeep(runtime.context, faction, numericContext, true) +
                NumericFormulas.CalculateGovernanceUpkeep(numericContext, true) +
                NumericFormulas.CalculateTreasuryReserveDrag(faction, true);

            int expectedMoneyAfter = moneyBefore + expectedTax - expectedMoneyUpkeep;
            int expectedFoodAfter = foodBefore + expectedFood - expectedFoodUpkeep;

            AddAssertion(result, "economy.money_delta_matches_runtime_contribution", "economy",
                moneyAfter == expectedMoneyAfter,
                expectedMoneyAfter, moneyAfter,
                "moneyAfter equals moneyBefore + effectiveTax - moneyUpkeep");
            AddAssertion(result, "economy.food_delta_matches_runtime_contribution", "economy",
                foodAfter == expectedFoodAfter,
                expectedFoodAfter, foodAfter,
                "foodAfter equals foodBefore + effectiveFood - foodUpkeep");

            AddKeyDelta(result, "faction.money", ownerFactionId, moneyBefore, moneyAfter,
                "金钱变化按 runtime 地区贡献率折算收入，再扣除军费和治理成本。");
            AddKeyDelta(result, "faction.food", ownerFactionId, foodBefore, foodAfter,
                "粮食变化按 runtime 地区贡献率折算收入，再扣除军粮和治理成本。");
            AddExplanation(result, "economy",
                "money " + moneyBefore + " + " + expectedTax + " - " + expectedMoneyUpkeep + " = " + moneyAfter +
                "; food " + foodBefore + " + " + expectedFood + " - " + expectedFoodUpkeep + " = " + foodAfter + "。");
        }

        private static bool HasAssertionPassed(HeadlessSimulationResult result, string assertionId)
        {
            if (result == null || result.report == null || result.report.assertions == null) return false;
            for (int i = 0; i < result.report.assertions.Count; i++)
            {
                HeadlessAssertionResult assertion = result.report.assertions[i];
                if (assertion != null && assertion.id == assertionId) return assertion.passed;
            }
            return false;
        }

        private static int GetSnapshotValue(Dictionary<string, int> snapshot, string factionId)
        {
            if (snapshot == null || string.IsNullOrEmpty(factionId)) return 0;
            int value;
            return snapshot.TryGetValue(factionId, out value) ? value : 0;
        }

        private static int CalculateReportSidePower(GameContext context, MapState mapState, List<string> armyIds, bool attacking)
        {
            int totalPower = 0;
            if (context == null || mapState == null || armyIds == null) return totalPower;

            for (int i = 0; i < armyIds.Count; i++)
            {
                ArmyRuntimeState runtimeArmy;
                if (!mapState.TryGetArmy(armyIds[i], out runtimeArmy)) continue;

                UnitDefinition unit;
                if (!context.Data.Units.TryGetValue(runtimeArmy.unitId, out unit)) continue;

                ArmyState legacyArmy = FindLegacyArmy(context, runtimeArmy.id);
                if (legacyArmy == null) continue;

                FactionState faction = context.State.FindFaction(runtimeArmy.ownerFactionId);
                totalPower += NumericFormulas.CalculateBattlePower(legacyArmy, unit, faction, new EquipmentBonus(), attacking);
            }

            return totalPower;
        }

        private static void CalculateExpectedFactionOutput(
            SimulationRuntime runtime,
            FactionState faction,
            NumericContext numericContext,
            out int tax,
            out int food)
        {
            tax = 0;
            food = 0;

            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = runtime.state.FindRegion(faction.regionIds[i]);
                if (region == null) continue;

                int regionTax = NumericFormulas.CalculateRegionalTax(region, numericContext);
                int regionFood = NumericFormulas.CalculateRegionalFood(region, numericContext);

                RegionRuntimeState runtimeRegion;
                if (runtime.worldState != null && runtime.worldState.Map != null &&
                    runtime.worldState.Map.TryGetRegion(region.id, out runtimeRegion))
                {
                    regionTax = ApplyContributionPercent(regionTax, runtimeRegion.taxContributionPercent);
                    regionFood = ApplyContributionPercent(regionFood, runtimeRegion.foodContributionPercent);
                }

                tax += regionTax;
                food += regionFood;
            }
        }

        private static int CalculateExpectedArmyUpkeep(GameContext context, FactionState faction, NumericContext numericContext, bool food)
        {
            int upkeep = 0;
            if (context == null || context.State == null || context.Data == null) return upkeep;

            for (int i = 0; i < context.State.armies.Count; i++)
            {
                ArmyState army = context.State.armies[i];
                if (army.ownerFactionId != faction.id) continue;
                UnitDefinition unit = context.Data.GetUnit(army.unitId);
                upkeep += NumericFormulas.CalculateArmyUpkeep(army, unit, numericContext, food);
            }

            return upkeep;
        }

        private static int ApplyContributionPercent(int value, int contributionPercent)
        {
            return DomainMath.RoundToInt(value * DomainMath.Clamp(contributionPercent, 0, 100) / 100f);
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
            if (result.report != null)
            {
                result.report.passed = true;
            }
            return result;
        }

        private static HeadlessSimulationResult Fail(HeadlessSimulationResult result, GameState state, WorldState worldState, string reason)
        {
            result.passed = false;
            result.failureReason = reason;
            result.state = state;
            result.worldState = worldState;
            if (result.report != null)
            {
                result.report.passed = false;
                result.report.failureReason = reason;
                if (string.IsNullOrEmpty(result.report.failureStage))
                {
                    result.report.failureStage = InferFailureStage(reason);
                }
            }
            return result;
        }

        private static string InferFailureStage(string reason)
        {
            if (string.IsNullOrEmpty(reason)) return "unknown";
            string lower = reason.ToLowerInvariant();
            if (lower.Contains("command") || lower.Contains("route")) return "command";
            if (lower.Contains("arriv") || lower.Contains("move") || lower.Contains("retreat")) return "movement";
            if (lower.Contains("engagement") || lower.Contains("contact") || lower.Contains("membership")) return "engagement";
            if (lower.Contains("battle") || lower.Contains("victory")) return "battle";
            if (lower.Contains("occup") || lower.Contains("owner") || lower.Contains("region")) return "outcome";
            if (lower.Contains("governance") || lower.Contains("integration") || lower.Contains("contribution")) return "governance";
            if (lower.Contains("economy") || lower.Contains("money") || lower.Contains("food")) return "economy";
            return "unknown";
        }

        private static void FinalizeScenarioReport(HeadlessSimulationResult result)
        {
            if (result == null || result.report == null) return;
            result.report.name = result.scenarioName;
            result.report.passed = result.passed;
            result.report.turnsExecuted = result.turnsExecuted;
            result.report.failureReason = result.failureReason;
            if (!result.passed && string.IsNullOrEmpty(result.report.failureStage))
            {
                result.report.failureStage = InferFailureStage(result.failureReason);
            }
            result.report.logs = CopyLogs(result.state);
            EnsurePhase(result, "command", "skip", "该场景未记录命令阶段。");
            EnsurePhase(result, "movement", "skip", "该场景未记录行军阶段。");
            EnsurePhase(result, "engagement", "skip", "该场景未记录接敌阶段。");
            EnsurePhase(result, "battle", "skip", "该场景未记录战斗阶段。");
            EnsurePhase(result, "outcome", "skip", "该场景未记录结果阶段。");
            EnsurePhase(result, "governance", "skip", "该场景不涉及治理折损。");
            EnsurePhase(result, "economy", "skip", "该场景不涉及经济结算。");
        }

        private static List<string> CopyLogs(GameState state)
        {
            List<string> logs = new List<string>();
            if (state == null || state.turnLog == null) return logs;
            for (int i = 0; i < state.turnLog.Count; i++)
            {
                TurnLogEntry entry = state.turnLog[i];
                if (entry == null) continue;
                logs.Add("[" + entry.turn + "][" + entry.category + "] " + entry.message);
            }
            return logs;
        }

        private static void AddPhase(
            HeadlessSimulationResult result,
            string phase,
            string status,
            Dictionary<string, object> before,
            Dictionary<string, object> after,
            string explanation)
        {
            if (result == null || result.report == null) return;
            HeadlessPhaseResult existing = FindPhase(result, phase);
            HeadlessPhaseResult phaseResult = existing != null ? existing : new HeadlessPhaseResult { phase = phase };
            phaseResult.status = status;
            phaseResult.before = before != null ? before : new Dictionary<string, object>();
            phaseResult.after = after != null ? after : new Dictionary<string, object>();
            phaseResult.explanation = explanation;
            if (existing == null) result.report.phaseResults.Add(phaseResult);
        }

        private static void EnsurePhase(HeadlessSimulationResult result, string phase, string status, string explanation)
        {
            if (FindPhase(result, phase) != null) return;
            AddPhase(result, phase, status, null, null, explanation);
        }

        private static HeadlessPhaseResult FindPhase(HeadlessSimulationResult result, string phase)
        {
            if (result == null || result.report == null || result.report.phaseResults == null) return null;
            for (int i = 0; i < result.report.phaseResults.Count; i++)
            {
                if (result.report.phaseResults[i].phase == phase) return result.report.phaseResults[i];
            }
            return null;
        }

        private static bool AddAssertion(
            HeadlessSimulationResult result,
            string id,
            string phase,
            bool passed,
            object expected,
            object actual,
            string message)
        {
            if (result != null && result.report != null)
            {
                result.report.assertions.Add(new HeadlessAssertionResult
                {
                    id = id,
                    phase = phase,
                    passed = passed,
                    expected = expected,
                    actual = actual,
                    message = message
                });
                if (!passed && string.IsNullOrEmpty(result.report.failureStage))
                {
                    result.report.failureStage = phase;
                    result.report.failureReason = message;
                }
            }
            return passed;
        }

        private static void AddKeyDelta(HeadlessSimulationResult result, string field, string entityId, object before, object after, string impact)
        {
            if (result == null || result.report == null) return;
            result.report.keyDeltas.Add(new HeadlessKeyDelta
            {
                field = field,
                entityId = entityId,
                before = before,
                after = after,
                impact = impact
            });
        }

        private static void AddExplanation(HeadlessSimulationResult result, string phase, string message)
        {
            if (result == null || result.report == null) return;
            result.report.explanations.Add(new HeadlessExplanation
            {
                phase = phase,
                message = message
            });
        }

        private static Dictionary<string, object> Fields(params object[] values)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (values == null) return result;
            for (int i = 0; i + 1 < values.Length; i += 2)
            {
                string key = values[i] as string;
                if (string.IsNullOrEmpty(key)) continue;
                result[key] = values[i + 1];
            }
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

        private static void RunEconomyWithSnapshot(SimulationRuntime runtime)
        {
            runtime.lastEconomyMoneyBeforeByFaction = CaptureFactionMoney(runtime.state);
            runtime.lastEconomyFoodBeforeByFaction = CaptureFactionFood(runtime.state);
            runtime.economySystem.ExecuteTurn(runtime.context);
            runtime.lastEconomyMoneyAfterByFaction = CaptureFactionMoney(runtime.state);
            runtime.lastEconomyFoodAfterByFaction = CaptureFactionFood(runtime.state);
        }

        private static Dictionary<string, int> CaptureFactionMoney(GameState state)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            if (state == null || state.factions == null) return result;

            for (int i = 0; i < state.factions.Count; i++)
            {
                FactionState faction = state.factions[i];
                if (faction != null) result[faction.id] = faction.money;
            }

            return result;
        }

        private static Dictionary<string, int> CaptureFactionFood(GameState state)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            if (state == null || state.factions == null) return result;

            for (int i = 0; i < state.factions.Count; i++)
            {
                FactionState faction = state.factions[i];
                if (faction != null) result[faction.id] = faction.food;
            }

            return result;
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
            public Dictionary<string, int> lastEconomyMoneyBeforeByFaction;
            public Dictionary<string, int> lastEconomyMoneyAfterByFaction;
            public Dictionary<string, int> lastEconomyFoodBeforeByFaction;
            public Dictionary<string, int> lastEconomyFoodAfterByFaction;
        }
    }
}
