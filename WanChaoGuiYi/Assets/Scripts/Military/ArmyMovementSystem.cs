using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class ArmyMovementSystem : MonoBehaviour, IGameSystem
    {
        [SerializeField] private GameManager gameManager;

        private DomainArmyMovementSystem domain;
        private WorldState boundWorldState;
        private MapCommandService boundMapCommandService;

        public void Initialize(GameContext context)
        {
            EnsureDomain();
            if (domain != null) domain.Initialize(context);
        }

        public void OnTurnStart(GameContext context)
        {
            EnsureDomain();
            if (domain != null) domain.OnTurnStart(context);
        }

        public void ExecuteTurn(GameContext context)
        {
            EnsureDomain();
            if (domain != null) domain.ExecuteTurn(context);
        }

        public void OnTurnEnd(GameContext context)
        {
            EnsureDomain();
            if (domain != null) domain.OnTurnEnd(context);
        }

        public bool MoveArmy(GameContext context, string armyId, string targetRegionId, MapGraph mapGraph)
        {
            EnsureDomain();
            return domain != null && domain.MoveArmy(context, armyId, targetRegionId, mapGraph);
        }

        private void EnsureDomain()
        {
            if (gameManager == null)
            {
                gameManager = GetComponent<GameManager>();
            }

            WorldState worldState = gameManager != null ? gameManager.World : null;
            MapCommandService mapCommandService = gameManager != null ? gameManager.MapCommands : null;
            if (domain != null && boundWorldState == worldState && boundMapCommandService == mapCommandService) return;

            domain = new DomainArmyMovementSystem(worldState, mapCommandService, new DomainEngagementDetector());
            boundWorldState = worldState;
            boundMapCommandService = mapCommandService;
        }
    }
}
