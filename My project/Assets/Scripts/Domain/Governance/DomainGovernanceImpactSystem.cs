using System;

namespace WanChaoGuiYi
{
    public sealed class DomainGovernanceImpactSystem
    {
        public GovernanceImpactPayload ApplyOccupationImpact(GameContext context, MapState mapState, string regionId)
        {
            if (context == null || mapState == null || string.IsNullOrEmpty(regionId)) return null;

            RegionRuntimeState runtimeRegion;
            if (!mapState.TryGetRegion(regionId, out runtimeRegion)) return null;

            RegionState legacyRegion = context.State.FindRegion(regionId);
            if (legacyRegion == null) return null;

            FactionState ownerFaction = context.State.FindFaction(runtimeRegion.ownerFactionId);
            int legitimacyBefore = ownerFaction != null ? ownerFaction.legitimacy : 0;
            StrategyCausalRules.ApplyOccupationLegitimacyCost(ownerFaction);
            int legitimacyAfter = ownerFaction != null ? ownerFaction.legitimacy : legitimacyBefore;

            runtimeRegion.occupationStatus = OccupationStatus.Occupied;
            runtimeRegion.controlStage = ControlStage.NewlyAttached;
            runtimeRegion.integration = Math.Min(runtimeRegion.integration, StrategyCausalRules.OccupiedIntegration);
            runtimeRegion.taxContributionPercent = Math.Min(runtimeRegion.taxContributionPercent, StrategyCausalRules.OccupiedContributionPercent);
            runtimeRegion.foodContributionPercent = Math.Min(runtimeRegion.foodContributionPercent, StrategyCausalRules.OccupiedContributionPercent);
            runtimeRegion.rebellionRisk = DomainMath.Min(100, runtimeRegion.rebellionRisk + StrategyCausalRules.OccupationRebellionRiskIncrease);
            runtimeRegion.localPower = DomainMath.Min(100, runtimeRegion.localPower + StrategyCausalRules.OccupationLocalPowerIncrease);
            runtimeRegion.annexationPressure = DomainMath.Min(100, runtimeRegion.annexationPressure + StrategyCausalRules.OccupationAnnexationPressureIncrease);
            runtimeRegion.localAcceptance = DomainMath.Max(0, runtimeRegion.localAcceptance - 18);

            legacyRegion.integration = runtimeRegion.integration;
            legacyRegion.occupationStatus = runtimeRegion.occupationStatus;
            legacyRegion.controlStage = runtimeRegion.controlStage;
            legacyRegion.taxContributionPercent = runtimeRegion.taxContributionPercent;
            legacyRegion.foodContributionPercent = runtimeRegion.foodContributionPercent;
            legacyRegion.rebellionRisk = runtimeRegion.rebellionRisk;
            legacyRegion.localPower = runtimeRegion.localPower;
            legacyRegion.annexationPressure = runtimeRegion.annexationPressure;
            legacyRegion.localAcceptance = runtimeRegion.localAcceptance;

            GovernanceImpactPayload payload = new GovernanceImpactPayload
            {
                regionId = regionId,
                integration = runtimeRegion.integration,
                taxContributionPercent = runtimeRegion.taxContributionPercent,
                foodContributionPercent = runtimeRegion.foodContributionPercent,
                rebellionRisk = runtimeRegion.rebellionRisk,
                localPower = runtimeRegion.localPower,
                annexationPressure = runtimeRegion.annexationPressure,
                legitimacyBefore = legitimacyBefore,
                legitimacyAfter = legitimacyAfter,
                legitimacyDelta = legitimacyAfter - legitimacyBefore
            };

            context.State.AddLog("governance", regionId + "新占领：整合度降至" + runtimeRegion.integration + "，税粮贡献暂为" + StrategyCausalRules.OccupiedContributionPercent + "% ，民变与地方势力上升，合法性" + legitimacyBefore + "→" + legitimacyAfter + "。");
            context.Events.Publish(new GameEvent(GameEventType.GovernanceImpactApplied, regionId, payload));
            return payload;
        }
    }
}
