namespace WanChaoGuiYi
{
    public sealed class HeadlessSimulationResult
    {
        public bool passed;
        public string failureReason;
        public GameState state;
        public WorldState worldState;
        public int turnsExecuted;
    }

    public sealed class HeadlessSimulationRunner
    {
        private const int MaxTurns = 24;

        public HeadlessSimulationResult RunSingleLaneWar(IDataRepository data, string playerFactionId)
        {
            HeadlessSimulationResult result = new HeadlessSimulationResult();
            if (data == null)
            {
                result.failureReason = "IDataRepository is required.";
                return result;
            }

            GameState state = GameStateFactory.CreateDefault(data, playerFactionId);
            WorldState worldState = WorldStateFactory.Create(state, data);
            EventBus events = new EventBus();
            GameContext context = new GameContext(state, data, events);
            MapGraphData graph = new MapGraphData(data);
            MapQueryService queries = new MapQueryService(worldState.Map, graph);
            MapCommandService commands = new MapCommandService(worldState, queries, context);

            ArmyRuntimeState playerArmy;
            ArmyRuntimeState enemyArmy;
            if (!worldState.Map.TryGetArmy("army_player_1", out playerArmy) || !worldState.Map.TryGetArmy("army_enemy_1", out enemyArmy))
            {
                return Fail(result, state, worldState, "Initial smoke armies are missing.");
            }

            string targetRegionId = enemyArmy.locationRegionId;
            string originalOwnerFactionId = enemyArmy.ownerFactionId;
            bool issued = commands.MoveArmy(playerArmy.id, targetRegionId);
            if (!issued)
            {
                return Fail(result, state, worldState, "MapCommandService.MoveArmy failed for the smoke route.");
            }

            DomainArmyMovementSystem movementSystem = new DomainArmyMovementSystem(worldState, commands, new DomainEngagementDetector());
            DomainMapWarResolutionSystem warResolutionSystem = new DomainMapWarResolutionSystem(
                worldState,
                new DomainEngagementDetector(),
                new DomainBattleSimulationSystem(),
                new DomainOccupationSystem(new DomainGovernanceImpactSystem()));
            DomainEconomySystem economySystem = new DomainEconomySystem(worldState);

            bool battleFinished = false;
            for (int turn = 0; turn < MaxTurns; turn++)
            {
                movementSystem.ExecuteTurn(context);
                warResolutionSystem.ExecuteTurn(context);
                economySystem.ExecuteTurn(context);
                result.turnsExecuted = turn + 1;

                if (HasLogContaining(state, "战斗结束"))
                {
                    battleFinished = true;
                    break;
                }
            }

            if (!battleFinished)
            {
                return Fail(result, state, worldState, "Battle did not finish within " + MaxTurns + " turns.");
            }

            RegionRuntimeState targetRegion;
            bool regionCaptured = worldState.Map.TryGetRegion(targetRegionId, out targetRegion) && targetRegion.ownerFactionId != originalOwnerFactionId;
            bool regionDefended = worldState.Map.TryGetRegion(targetRegionId, out targetRegion) && targetRegion.ownerFactionId == originalOwnerFactionId;

            string missingLog = FindMissingRequiredLog(state, regionCaptured, regionDefended);
            if (!string.IsNullOrEmpty(missingLog))
            {
                return Fail(result, state, worldState, "Missing required log: " + missingLog);
            }

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

        private static string FindMissingRequiredLog(GameState state, bool regionCaptured, bool regionDefended)
        {
            if (!HasLogContaining(state, "行军")) return "行军";
            if (!HasLogContaining(state, "抵达")) return "抵达";
            if (!HasLogContaining(state, "接敌")) return "接敌";
            if (!HasLogContaining(state, "战斗结束")) return "战斗结束";
            if (!regionCaptured && !regionDefended) return "占领 / 防守";
            if (regionCaptured && !HasLogContaining(state, "占领")) return "占领";
            if (regionDefended && !HasLogContaining(state, "进攻方获胜") && !HasLogContaining(state, "防守方获胜")) return "防守";
            if (regionCaptured && !HasLogContaining(state, "新占领")) return "治理折损";
            if (!HasLogContaining(state, "收入 金钱")) return "经济结算";
            return null;
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
