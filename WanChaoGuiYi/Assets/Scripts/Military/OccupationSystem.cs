using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class OccupationSystem : MonoBehaviour
    {
        private DomainOccupationSystem domain;

        private void Awake()
        {
            EnsureDomain();
        }

        public RegionOccupiedPayload ApplyBattleOccupation(GameContext context, MapState mapState, string engagementId)
        {
            EnsureDomain();
            return domain.ApplyBattleOccupation(context, mapState, engagementId);
        }

        private void EnsureDomain()
        {
            if (domain != null) return;
            domain = new DomainOccupationSystem(new DomainGovernanceImpactSystem());
        }
    }
}
