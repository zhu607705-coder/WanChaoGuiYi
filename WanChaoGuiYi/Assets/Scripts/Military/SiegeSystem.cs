using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class SiegeSystem : MonoBehaviour, IGameSystem
    {
        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            for (int i = 0; i < context.State.regions.Count; i++)
            {
                RegionState region = context.State.regions[i];
                if (HasEnemyArmyInRegion(context, region))
                {
                    region.integration = Mathf.Max(0, region.integration - 3);
                    region.rebellionRisk = Mathf.Min(100, region.rebellionRisk + 2);
                }
            }
        }

        private static bool HasEnemyArmyInRegion(GameContext context, RegionState region)
        {
            for (int i = 0; i < context.State.armies.Count; i++)
            {
                ArmyState army = context.State.armies[i];
                if (army.regionId == region.id && army.ownerFactionId != region.ownerFactionId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
