using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class BattleSimulationSystem : MonoBehaviour
    {
        private readonly DomainBattleSimulationSystem domain = new DomainBattleSimulationSystem();

        public BattleResult ResolveEngagement(GameContext context, MapState mapState, string engagementId)
        {
            return domain.ResolveEngagement(context, mapState, engagementId);
        }

        public void ResolveAllReadyEngagements(GameContext context, MapState mapState)
        {
            domain.ResolveAllReadyEngagements(context, mapState);
        }
    }
}
