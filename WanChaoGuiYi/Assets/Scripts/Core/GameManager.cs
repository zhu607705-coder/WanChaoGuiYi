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

        public GameState State { get; private set; }
        public EventBus Events { get; private set; }
        public DataRepository Data { get { return dataRepository; } }

        private void Awake()
        {
            Events = new EventBus();
            dataRepository = EnsureComponent<DataRepository>();
            turnManager = EnsureComponent<TurnManager>();
            mapGraph = EnsureComponent<MapGraph>();

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
            context = new GameContext(State, dataRepository, Events);
            turnManager.Configure(context, CollectSystems());
            Events.Publish(new GameEvent(GameEventType.GameStarted, playerFactionId, State));
        }

        public void NextTurn()
        {
            turnManager.AdvanceTurn();
        }

        private IEnumerable<IGameSystem> CollectSystems()
        {
            return new IGameSystem[]
            {
                // 天气和天文在回合开始时触发
                EnsureComponent<WeatherSystem>(),
                EnsureComponent<CelestialEventSystem>(),
                // 核心经济和政治
                EnsureComponent<EconomySystem>(),
                EnsureComponent<PopulationSystem>(),
                EnsureComponent<LandSystem>(),
                EnsureComponent<EmperorMechanicSystem>(),
                EnsureComponent<LegitimacySystem>(),
                EnsureComponent<FactionSystem>(),
                EnsureComponent<ReformSystem>(),
                EnsureComponent<RebellionSystem>(),
                // 军事
                EnsureComponent<ArmyMovementSystem>(),
                EnsureComponent<SiegeSystem>(),
                // 科技、风俗、装备
                EnsureComponent<TechSystem>(),
                EnsureComponent<CultureSystem>(),
                EnsureComponent<EquipmentSystem>(),
                // 继承和人才
                EnsureComponent<SuccessionSystem>(),
                EnsureComponent<TalentSystem>(),
                // AI 和胜利
                EnsureComponent<StrategicAI>(),
                EnsureComponent<VictorySystem>()
            };
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
