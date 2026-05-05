using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class EngagementDetector : MonoBehaviour
    {
        private readonly DomainEngagementDetector domain = new DomainEngagementDetector();

        public EngagementRuntimeState DetectRegion(GameContext context, MapState mapState, string regionId)
        {
            return domain.DetectRegion(context, mapState, regionId);
        }

        public void DetectAll(GameContext context, MapState mapState)
        {
            domain.DetectAll(context, mapState);
        }
    }
}
