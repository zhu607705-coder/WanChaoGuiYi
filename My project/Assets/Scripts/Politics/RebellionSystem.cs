using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class RebellionSystem : MonoBehaviour, IGameSystem
    {
        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            for (int i = 0; i < context.State.regions.Count; i++)
            {
                RegionState region = context.State.regions[i];
                if (region.rebellionRisk < 80) continue;

                FactionState owner = context.State.FindFaction(region.ownerFactionId);
                if (owner != null)
                {
                    owner.legitimacy = Mathf.Max(0, owner.legitimacy - 5);
                    context.State.AddLog("rebellion", region.id + "民变风险爆发，" + owner.name + "合法性下降。");
                }

                region.rebellionRisk = 55;
            }
        }
    }
}
