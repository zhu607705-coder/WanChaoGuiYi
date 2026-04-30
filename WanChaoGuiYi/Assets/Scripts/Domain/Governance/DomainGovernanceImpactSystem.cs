using System;

namespace WanChaoGuiYi
{
    public sealed class DomainGovernanceImpactSystem
    {
        private const int OccupiedIntegration = 25;
        private const int OccupiedContributionPercent = 35;
        private const int RebellionRiskIncrease = 12;
        private const int LocalPowerIncrease = 8;
        private const int AnnexationPressureIncrease = 10;

        public GovernanceImpactPayload ApplyOccupationImpact(GameContext context, MapState mapState, string regionId)
        {
            if (context == null || mapState == null || string.IsNullOrEmpty(regionId)) return null;

            RegionRuntimeState runtimeRegion;
            if (!mapState.TryGetRegion(regionId, out runtimeRegion)) return null;

            RegionState legacyRegion = context.State.FindRegion(regionId);
            if (legacyRegion == null) return null;

            runtimeRegion.occupationStatus = OccupationStatus.Occupied;
            runtimeRegion.integration = Math.Min(runtimeRegion.integration, OccupiedIntegration);
            runtimeRegion.taxContributionPercent = Math.Min(runtimeRegion.taxContributionPercent, OccupiedContributionPercent);
            runtimeRegion.foodContributionPercent = Math.Min(runtimeRegion.foodContributionPercent, OccupiedContributionPercent);
            runtimeRegion.rebellionRisk = DomainMath.Min(100, runtimeRegion.rebellionRisk + RebellionRiskIncrease);
            runtimeRegion.localPower = DomainMath.Min(100, runtimeRegion.localPower + LocalPowerIncrease);
            runtimeRegion.annexationPressure = DomainMath.Min(100, runtimeRegion.annexationPressure + AnnexationPressureIncrease);

            legacyRegion.integration = runtimeRegion.integration;
            legacyRegion.occupationStatus = runtimeRegion.occupationStatus;
            legacyRegion.taxContributionPercent = runtimeRegion.taxContributionPercent;
            legacyRegion.foodContributionPercent = runtimeRegion.foodContributionPercent;
            legacyRegion.rebellionRisk = runtimeRegion.rebellionRisk;
            legacyRegion.localPower = runtimeRegion.localPower;
            legacyRegion.annexationPressure = runtimeRegion.annexationPressure;

            GovernanceImpactPayload payload = new GovernanceImpactPayload
            {
                regionId = regionId,
                integration = runtimeRegion.integration,
                taxContributionPercent = runtimeRegion.taxContributionPercent,
                foodContributionPercent = runtimeRegion.foodContributionPercent,
                rebellionRisk = runtimeRegion.rebellionRisk,
                localPower = runtimeRegion.localPower,
                annexationPressure = runtimeRegion.annexationPressure
            };

            context.State.AddLog("governance", regionId + "新占领：整合度降至" + runtimeRegion.integration + "，税粮贡献暂为" + OccupiedContributionPercent + "% ，民变与地方势力上升。");
            context.Events.Publish(new GameEvent(GameEventType.GovernanceImpactApplied, regionId, payload));
            return payload;
        }
    }
}
