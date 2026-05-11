using System.Collections.Generic;

namespace WanChaoGuiYi
{
    public static class DomainEngagementCleanup
    {
        public static void RemoveArmyFromEngagement(MapState mapState, string engagementId, string armyId)
        {
            if (mapState == null || string.IsNullOrEmpty(engagementId) || string.IsNullOrEmpty(armyId)) return;

            EngagementRuntimeState engagement;
            if (!mapState.EngagementsById.TryGetValue(engagementId, out engagement) || engagement == null) return;

            engagement.attackerArmyIds.Remove(armyId);
            engagement.defenderArmyIds.Remove(armyId);
        }

        public static void ClearEngagementIfSideEmpty(MapState mapState, string engagementId)
        {
            if (mapState == null || string.IsNullOrEmpty(engagementId)) return;

            EngagementRuntimeState engagement;
            if (!mapState.EngagementsById.TryGetValue(engagementId, out engagement) || engagement == null) return;
            if (engagement.attackerArmyIds.Count > 0 && engagement.defenderArmyIds.Count > 0) return;

            ClearArmyEngagements(mapState, engagement.attackerArmyIds, false);
            ClearArmyEngagements(mapState, engagement.defenderArmyIds, false);
            mapState.RemoveEngagement(engagementId);
            RestoreContestedRegion(mapState, engagement.regionId);
        }

        public static void ClearResolvedEngagement(MapState mapState, EngagementRuntimeState engagement)
        {
            if (mapState == null || engagement == null) return;

            ClearArmyEngagements(mapState, engagement.attackerArmyIds, true);
            ClearArmyEngagements(mapState, engagement.defenderArmyIds, true);
            RestoreContestedRegion(mapState, engagement.regionId);
        }

        public static void RestoreContestedRegion(MapState mapState, string regionId)
        {
            RegionRuntimeState region;
            if (mapState != null && mapState.TryGetRegion(regionId, out region) && region.occupationStatus == OccupationStatus.Contested)
            {
                region.occupationStatus = OccupationStatus.Controlled;
            }
        }

        private static void ClearArmyEngagements(MapState mapState, List<string> armyIds, bool clearOrders)
        {
            if (mapState == null || armyIds == null) return;

            for (int i = 0; i < armyIds.Count; i++)
            {
                ArmyRuntimeState army;
                if (!mapState.TryGetArmy(armyIds[i], out army)) continue;

                army.engagementId = null;
                if (!clearOrders) continue;

                army.targetRegionId = null;
                army.route.Clear();
                army.task = ArmyTask.Idle;
            }
        }
    }
}
