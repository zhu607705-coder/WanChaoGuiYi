using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private string playerFactionId = "faction_qin_shi_huang";
        [SerializeField] private bool startNewGameOnAwake = true;

        private DataRepository dataRepository;
        private TurnManager turnManager;
        private MapGraph mapGraph;
        private GameContext context;
        private WorldState worldState;
        private MapQueryService mapQueryService;
        private MapCommandService mapCommandService;

        public GameState State { get; private set; }
        public WorldState World { get { return worldState; } }
        public MapQueryService MapQueries { get { return mapQueryService; } }
        public MapCommandService MapCommands { get { return mapCommandService; } }
        public EventBus Events { get; private set; }
        public DataRepository Data { get { return dataRepository; } }
        public GameContext Context { get { return context; } }

        private void Awake()
        {
            Events = new EventBus();
            dataRepository = EnsureComponent<DataRepository>();
            turnManager = EnsureComponent<TurnManager>();
            mapGraph = EnsureComponent<MapGraph>();
            EnsureBattleServices();

            if (startNewGameOnAwake)
            {
                StartNewGame();
            }
        }

        public void StartNewGame()
        {
            dataRepository.Load();
            mapGraph.Build(dataRepository);
            State = GameStateFactory.CreateDefault(dataRepository, playerFactionId);
            worldState = WorldStateFactory.Create(State, dataRepository);
            mapQueryService = new MapQueryService(worldState.Map, mapGraph);
            context = new GameContext(State, dataRepository, Events);
            mapCommandService = new MapCommandService(mapQueryService, context);
            turnManager.Configure(context, CollectSystems());
            Events.Publish(new GameEvent(GameEventType.GameStarted, playerFactionId, State));
        }

        public void NextTurn()
        {
            turnManager.AdvanceTurn();
        }

        public bool MoveArmy(string armyId, string targetRegionId)
        {
            return mapCommandService != null && mapCommandService.MoveArmy(armyId, targetRegionId);
        }

        public bool StopArmy(string armyId)
        {
            return mapCommandService != null && mapCommandService.StopArmy(armyId);
        }

        public bool RetreatArmy(string armyId, string targetRegionId)
        {
            return mapCommandService != null && mapCommandService.RetreatArmy(armyId, targetRegionId);
        }

        public bool ReinforceArmy(string armyId, string targetRegionId)
        {
            return mapCommandService != null && mapCommandService.ReinforceArmy(armyId, targetRegionId);
        }

        public bool SiegeRegion(string armyId, string targetRegionId)
        {
            return mapCommandService != null && mapCommandService.SiegeRegion(armyId, targetRegionId);
        }

        public bool StartPlayerAttack(string targetRegionId)
        {
            if (State == null || worldState == null || string.IsNullOrEmpty(targetRegionId)) return false;

            ArmyRuntimeState army = FindPlayerIdleArmyForAttack(targetRegionId);
            if (army == null)
            {
                State.AddLog("war", "没有可向" + targetRegionId + "出征的空闲军队。");
                return false;
            }

            return StartPlayerAttack(army.id, targetRegionId);
        }

        public bool StartPlayerAttack(string armyId, string targetRegionId)
        {
            if (State == null || worldState == null || worldState.Map == null || string.IsNullOrEmpty(armyId) || string.IsNullOrEmpty(targetRegionId)) return false;

            ArmyRuntimeState army;
            if (!worldState.Map.TryGetArmy(armyId, out army) || army == null)
            {
                State.AddLog("war", "选中军队不存在：" + armyId);
                return false;
            }

            if (army.ownerFactionId != State.playerFactionId || army.task != ArmyTask.Idle || army.locationRegionId == targetRegionId)
            {
                State.AddLog("war", "选中军队不可出征：" + armyId);
                return false;
            }

            if (!IsPlayerFrontlineAttackArmy(army, targetRegionId))
            {
                State.AddLog("war", "Selected army is not staged in a player-owned frontline neighbor: " + armyId + " -> " + targetRegionId);
                return false;
            }

            if (mapQueryService == null || !mapQueryService.HasRoute(army.locationRegionId, targetRegionId))
            {
                State.AddLog("war", "选中军队没有通往目标的路线：" + armyId + " -> " + targetRegionId);
                return false;
            }

            return MoveArmy(army.id, targetRegionId);
        }

        private ArmyRuntimeState FindPlayerIdleArmyForAttack(string targetRegionId)
        {
            if (worldState == null || worldState.Map == null || State == null) return null;

            foreach (ArmyRuntimeState army in worldState.Map.ArmiesById.Values)
            {
                if (army == null || army.ownerFactionId != State.playerFactionId || army.task != ArmyTask.Idle) continue;
                if (army.locationRegionId == targetRegionId) continue;
                if (!IsPlayerFrontlineAttackArmy(army, targetRegionId)) continue;
                if (mapQueryService == null || !mapQueryService.HasRoute(army.locationRegionId, targetRegionId)) continue;
                return army;
            }

            return null;
        }

        private bool IsPlayerFrontlineAttackArmy(ArmyRuntimeState army, string targetRegionId)
        {
            if (army == null || State == null || mapQueryService == null || string.IsNullOrEmpty(targetRegionId)) return false;
            if (army.ownerFactionId != State.playerFactionId) return false;
            RegionState stagingRegion = State.FindRegion(army.locationRegionId);
            if (stagingRegion == null || stagingRegion.ownerFactionId != State.playerFactionId) return false;
            if (mapQueryService.AreNeighbors(army.locationRegionId, targetRegionId)) return true;

            CampaignRouteForecast forecast = StrategyMapRulebook.BuildCampaignRouteForecast(mapQueryService, context, army, targetRegionId);
            return forecast != null && forecast.canDispatch;
        }

        private IEnumerable<IGameSystem> CollectSystems()
        {
            return new IGameSystem[]
            {
                // 天气和天文在回合开始时触发
                EnsureComponent<WeatherSystem>(),
                EnsureComponent<CelestialEventSystem>(),
                // 核心政治
                EnsureComponent<PopulationSystem>(),
                EnsureComponent<LandSystem>(),
                EnsureComponent<BuildingSystem>(),
                EnsureComponent<EmperorMechanicSystem>(),
                EnsureComponent<LegitimacySystem>(),
                EnsureComponent<FactionSystem>(),
                EnsureComponent<ReformSystem>(),
                EnsureComponent<RebellionSystem>(),
                // 军事
                EnsureComponent<ArmyMovementSystem>(),
                EnsureComponent<MapWarResolutionSystem>(),
                EnsureComponent<SiegeSystem>(),
                // 战争和治理结果进入本回合经济结算
                EnsureComponent<EconomySystem>(),
                // 科技、风俗、装备
                EnsureComponent<TechSystem>(),
                EnsureComponent<CultureSystem>(),
                EnsureComponent<EquipmentSystem>(),
                // 继承和人才
                EnsureComponent<SuccessionSystem>(),
                EnsureComponent<TalentSystem>(),
                // 帝皇技能
                EnsureComponent<EmperorSkillSystem>(),
                // 外交和谍报
                EnsureComponent<DiplomacySystem>(),
                EnsureComponent<EspionageSystem>(),
                // 事件评估
                EnsureComponent<EventEvaluationSystem>(),
                // AI 和胜利
                EnsureComponent<StrategicAI>(),
                EnsureComponent<VictorySystem>()
            };
        }

        private void EnsureBattleServices()
        {
            EnsureComponent<BattleSetupSystem>();
            EnsureComponent<BattleExecutionSystem>();
            EnsureComponent<BattleResolver>();
            EnsureComponent<BattleConfigSystem>();
            EnsureComponent<BattleSessionSystem>();
            EnsureComponent<BattleDisplaySystem>();
        }

        private T EnsureComponent<T>() where T : Component
        {
            T component = GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }
    }
}
