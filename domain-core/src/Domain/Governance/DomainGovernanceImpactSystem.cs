using System;

namespace WanChaoGuiYi
{
    public sealed class DomainGovernanceImpactSystem
    {
        public GovernanceImpactPayload ApplyOccupationImpact(GameContext context, MapState mapState, string regionId, int occupationReservedFoodTransferred = 0)
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
            runtimeRegion.occupationReservedFood = DomainMath.Max(0, occupationReservedFoodTransferred);
            runtimeRegion.occupationPacificationQueueStep = runtimeRegion.occupationReservedFood > 0 ? 1 : 0;
            runtimeRegion.occupationPacificationQueueTurnsRemaining = runtimeRegion.occupationReservedFood > 0 ? 3 : 0;
            runtimeRegion.integration = Math.Min(runtimeRegion.integration, StrategyCausalRules.OccupiedIntegration);
            runtimeRegion.taxContributionPercent = Math.Min(runtimeRegion.taxContributionPercent, StrategyCausalRules.OccupiedContributionPercent);
            runtimeRegion.foodContributionPercent = Math.Min(runtimeRegion.foodContributionPercent, StrategyCausalRules.OccupiedContributionPercent);
            runtimeRegion.rebellionRisk = DomainMath.Min(100, runtimeRegion.rebellionRisk + StrategyCausalRules.OccupationRebellionRiskIncrease);
            runtimeRegion.localPower = DomainMath.Min(100, runtimeRegion.localPower + StrategyCausalRules.OccupationLocalPowerIncrease);
            runtimeRegion.annexationPressure = DomainMath.Min(100, runtimeRegion.annexationPressure + StrategyCausalRules.OccupationAnnexationPressureIncrease);
            runtimeRegion.localAcceptance = StrategyCausalRules.ApplyOccupationAcceptanceShock(runtimeRegion.localAcceptance);

            legacyRegion.integration = runtimeRegion.integration;
            legacyRegion.occupationStatus = runtimeRegion.occupationStatus;
            legacyRegion.controlStage = runtimeRegion.controlStage;
            legacyRegion.occupationReservedFood = runtimeRegion.occupationReservedFood;
            legacyRegion.occupationPacificationQueueStep = runtimeRegion.occupationPacificationQueueStep;
            legacyRegion.occupationPacificationQueueTurnsRemaining = runtimeRegion.occupationPacificationQueueTurnsRemaining;
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
                legitimacyDelta = legitimacyAfter - legitimacyBefore,
                occupationReservedFoodTransferred = DomainMath.Max(0, occupationReservedFoodTransferred),
                occupationReservedFoodAvailable = runtimeRegion.occupationReservedFood
            };

            string reserveText = runtimeRegion.occupationReservedFood > 0
                ? "，前线预留粮转入占后治理" + runtimeRegion.occupationReservedFood + "，将优先抵扣军管、安抚、编户粮耗"
                : "";
            context.State.AddLog("governance", regionId + "新占领：整合度降至" + runtimeRegion.integration + "，税粮贡献暂为" + StrategyCausalRules.OccupiedContributionPercent + "% ，民变与地方势力上升，合法性" + legitimacyBefore + "→" + legitimacyAfter + reserveText + "。");
            context.Events.Publish(new GameEvent(GameEventType.GovernanceImpactApplied, regionId, payload));
            return payload;
        }
    }
}
