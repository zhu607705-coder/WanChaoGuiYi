using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class ArmyMovementSystem : MonoBehaviour, IGameSystem
    {
        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            for (int i = 0; i < context.State.armies.Count; i++)
            {
                ArmyState army = context.State.armies[i];
                if (army.movementProgress > 0)
                {
                    army.movementProgress = Mathf.Max(0, army.movementProgress - 50);
                }
            }
        }

        public bool MoveArmy(GameContext context, string armyId, string targetRegionId, MapGraph mapGraph)
        {
            ArmyState army = FindArmy(context, armyId);
            if (army == null || mapGraph == null) return false;
            if (!mapGraph.AreNeighbors(army.regionId, targetRegionId)) return false;

            army.regionId = targetRegionId;
            army.movementProgress = 100;
            return true;
        }

        private static ArmyState FindArmy(GameContext context, string armyId)
        {
            for (int i = 0; i < context.State.armies.Count; i++)
            {
                if (context.State.armies[i].id == armyId) return context.State.armies[i];
            }

            return null;
        }
    }
}
