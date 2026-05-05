using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class EconomySystem : MonoBehaviour, IGameSystem
    {
        [SerializeField] private GameManager gameManager;

        private DomainEconomySystem domain;
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

            if (gameManager == null) return;
            if (domain != null && boundWorldState == gameManager.World) return;

            domain = new DomainEconomySystem(gameManager.World);
            boundWorldState = gameManager.World;
        }
    }
}
