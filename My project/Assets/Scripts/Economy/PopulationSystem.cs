using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class PopulationSystem : MonoBehaviour, IGameSystem
    {
        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            for (int i = 0; i < context.State.regions.Count; i++)
            {
                RegionState region = context.State.regions[i];
                float riskPenalty = Mathf.Clamp01(region.rebellionRisk / 200f);
                int growth = Mathf.RoundToInt(region.population * (0.005f - riskPenalty * 0.003f));
                region.population = Mathf.Max(0, region.population + growth);
            }
        }
    }
}
