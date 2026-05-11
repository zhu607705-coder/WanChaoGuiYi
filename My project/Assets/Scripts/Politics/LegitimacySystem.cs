using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class LegitimacySystem : MonoBehaviour, IGameSystem
    {
        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            for (int i = 0; i < context.State.factions.Count; i++)
            {
                FactionState faction = context.State.factions[i];
                int lowIntegrationCount = CountLowIntegrationRegions(context, faction);
                faction.legitimacy = ClampPercent(faction.legitimacy - lowIntegrationCount);
            }
        }

        private static int CountLowIntegrationRegions(GameContext context, FactionState faction)
        {
            int count = 0;
            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = context.State.FindRegion(faction.regionIds[i]);
                if (region != null && region.integration < 40)
                {
                    count++;
                }
            }

            return count;
        }

        private static int ClampPercent(int value)
        {
            if (value < 0) return 0;
            if (value > 100) return 100;
            return value;
        }
    }
}
