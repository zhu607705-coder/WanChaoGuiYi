using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class MapWarResolutionSystem : MonoBehaviour, IGameSystem
    {
        [SerializeField] private GameManager gameManager;

        private DomainMapWarResolutionSystem domain;
        private WorldState boundWorldState;

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

        private void EnsureDomain()
        {
            if (gameManager == null)
            {
                gameManager = GetComponent<GameManager>();
            }

            if (gameManager == null || gameManager.World == null) return;
            if (domain != null && boundWorldState == gameManager.World) return;

            domain = new DomainMapWarResolutionSystem(
                gameManager.World,
                new DomainEngagementDetector(),
                new DomainBattleSimulationSystem(),
                new DomainOccupationSystem(new DomainGovernanceImpactSystem()));
            boundWorldState = gameManager.World;
        }
    }
}
