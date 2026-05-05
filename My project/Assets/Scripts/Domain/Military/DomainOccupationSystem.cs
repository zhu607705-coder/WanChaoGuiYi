using System;

namespace WanChaoGuiYi
{
    public sealed class DomainOccupationSystem
    {
        private readonly DomainGovernanceImpactSystem governanceImpactSystem;

        public DomainOccupationSystem(DomainGovernanceImpactSystem governanceImpactSystem)
        {
            this.governanceImpactSystem = governanceImpactSystem;
        }

        public RegionOccupiedPayload ApplyBattleOccupation(GameContext context, MapState mapState, string engagementId)
        {
            if (context == null || mapState == null || string.IsNullOrEmpty(engagementId)) return null;

            EngagementRuntimeState engagement;
            if (!mapState.EngagementsById.TryGetValue(engagementId, out engagement)) return null;
            if (engagement.phase != EngagementPhase.Resolved || engagement.result == null) return null;

            string winningArmyId = ResolveWinningArmyId(engagement);
            if (string.IsNullOrEmpty(winningArmyId)) return null;

            ArmyRuntimeState winningArmy;
            if (!mapState.TryGetArmy(winningArmyId, out winningArmy)) return null;

            RegionRuntimeState runtimeRegion;
            if (!mapState.TryGetRegion(engagement.regionId, out runtimeRegion)) return null;

            string previousOwnerFactionId = runtimeRegion.ownerFactionId;
            string newOwnerFactionId = winningArmy.ownerFactionId;
            if (previousOwnerFactionId == newOwnerFactionId) return null;

            RegionOwnerChangedPayload ownerChanged = context.ChangeRegionOwner(engagement.regionId, newOwnerFactionId);
            if (ownerChanged == null) return null;

            runtimeRegion.ownerFactionId = newOwnerFactionId;
            runtimeRegion.occupationStatus = OccupationStatus.Occupied;

            RegionState legacyRegion = context.State.FindRegion(engagement.regionId);
            if (legacyRegion != null)
            {
                legacyRegion.occupationStatus = runtimeRegion.occupationStatus;
                runtimeRegion.integration = legacyRegion.integration;
                runtimeRegion.rebellionRisk = legacyRegion.rebellionRisk;
            }

            RegionOccupiedPayload payload = new RegionOccupiedPayload
            {
                regionId = engagement.regionId,
                previousOwnerFactionId = previousOwnerFactionId,
                newOwnerFactionId = newOwnerFactionId,
                engagementId = engagement.id
            };

            context.State.AddLog("war", engagement.regionId + "被" + newOwnerFactionId + "占领。原因：胜利方与原地区归属不同。影响：地区归属改变，并进入新占领治理折损。");
            context.Events.Publish(new GameEvent(GameEventType.RegionOccupied, engagement.regionId, payload));

            if (governanceImpactSystem != null)
            {
                governanceImpactSystem.ApplyOccupationImpact(context, mapState, engagement.regionId);
            }

            return payload;
        }

        private static string ResolveWinningArmyId(EngagementRuntimeState engagement)
        {
            if (engagement.result.attackerWon)
            {
                return engagement.attackerArmyIds.Count > 0 ? engagement.attackerArmyIds[0] : null;
            }

            return engagement.defenderArmyIds.Count > 0 ? engagement.defenderArmyIds[0] : null;
        }
    }
}
