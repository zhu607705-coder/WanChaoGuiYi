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

        public bool RunSingleLaneWarSmokeTest()
        {
            if (State == null || worldState == null || mapCommandService == null) return false;

            ArmyRuntimeState playerArmy;
            ArmyRuntimeState enemyArmy;
            if (!worldState.Map.TryGetArmy("army_player_1", out playerArmy) || !worldState.Map.TryGetArmy("army_enemy_1", out enemyArmy))
            {
                State.AddLog("war", "单路战争验收失败：未找到初始敌我军队。");
                return false;
            }

            bool issued = mapCommandService.MoveArmy(playerArmy.id, enemyArmy.locationRegionId);
            State.AddLog("war", "单路战争验收：" + playerArmy.locationRegionId + " → " + enemyArmy.locationRegionId + "，命令" + (issued ? "已发出" : "失败") + "。");
            return issued;
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
