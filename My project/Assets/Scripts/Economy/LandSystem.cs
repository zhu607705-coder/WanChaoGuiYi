using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class LandSystem : MonoBehaviour, IGameSystem
    {
        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            for (int i = 0; i < context.State.regions.Count; i++)
            {
                RegionState region = context.State.regions[i];
                int drift = region.localPower > 55 ? 2 : 1;
                if (region.integration < 50) drift += 1;

                region.annexationPressure = ClampPercent(region.annexationPressure + drift);
                region.rebellionRisk = ClampPercent(region.rebellionRisk + (region.annexationPressure > 60 ? 1 : 0));

                if (region.landStructure != null)
                {
                    region.landStructure.localElites = Mathf.Clamp01(region.landStructure.localElites + drift * 0.002f);
                    region.landStructure.smallFarmers = Mathf.Clamp01(region.landStructure.smallFarmers - drift * 0.002f);
                }
            }
        }

        private static int ClampPercent(int value)
        {
            if (value < 0) return 0;
            if (value > 100) return 100;
            return value;
        }
    }
}
