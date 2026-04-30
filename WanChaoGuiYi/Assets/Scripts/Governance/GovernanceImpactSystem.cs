using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class GovernanceImpactSystem : MonoBehaviour
    {
        private readonly DomainGovernanceImpactSystem domain = new DomainGovernanceImpactSystem();

        public GovernanceImpactPayload ApplyOccupationImpact(GameContext context, MapState mapState, string regionId)
        {
            return domain.ApplyOccupationImpact(context, mapState, regionId);
        }
    }
}
