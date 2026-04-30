using System;
using System.Collections.Generic;

namespace WanChaoGuiYi
{
    public sealed class DomainEngagementDetector
    {
        public EngagementRuntimeState DetectRegion(GameContext context, MapState mapState, string regionId)
        {
            return DetectRegion(context, mapState, regionId, null);
        }

        public EngagementRuntimeState DetectRegion(GameContext context, MapState mapState, string regionId, string initiatingArmyId)
        {
            if (context == null || mapState == null || string.IsNullOrEmpty(regionId)) return null;

            List<ArmyRuntimeState> armies = mapState.GetArmiesInRegion(regionId);
            if (armies.Count < 2) return null;

            ArmyRuntimeState initiatingArmy = FindInitiatingArmy(armies, initiatingArmyId);
            ArmyRuntimeState attackerSeed = initiatingArmy != null ? initiatingArmy : FindAttackerSeed(armies, regionId);
            if (attackerSeed == null) return null;

            List<string> attackerArmyIds = new List<string>();
            List<string> defenderArmyIds = new List<string>();

            for (int i = 0; i < armies.Count; i++)
            {
                if (armies[i].ownerFactionId == attackerSeed.ownerFactionId)
                {
                    AddUnique(attackerArmyIds, armies[i].id);
                }
                else
                {
                    AddUnique(defenderArmyIds, armies[i].id);
                }
            }

            if (attackerArmyIds.Count == 0 || defenderArmyIds.Count == 0) return null;

            EngagementRuntimeState engagement;
            if (!mapState.TryGetEngagementInRegion(regionId, out engagement))
            {
                engagement = new EngagementRuntimeState
                {
                    id = "engagement_" + regionId + "_" + Guid.NewGuid().ToString("N"),
                    regionId = regionId,
                    phase = EngagementPhase.Forming,
                    initiatingArmyId = attackerSeed.id,
                    initiatingFactionId = attackerSeed.ownerFactionId
                };
                mapState.AddEngagement(engagement);
            }

            bool hasNewMembership = HasNewSideMembership(engagement, attackerArmyIds, defenderArmyIds);
            MergeArmyIds(engagement.attackerArmyIds, attackerArmyIds);
            MergeArmyIds(engagement.defenderArmyIds, defenderArmyIds);
            MarkArmiesEngaged(mapState, engagement);
            MarkRegionContested(mapState, regionId);

            if (engagement.phase != EngagementPhase.Forming) return engagement;
            if (!hasNewMembership) return engagement;

            EngagementPayload payload = CreatePayload(engagement);
            context.State.AddLog("war", regionId + "发生接敌：" + engagement.attackerArmyIds.Count + " 支部队对 " + engagement.defenderArmyIds.Count + " 支部队。");
            context.Events.Publish(new GameEvent(GameEventType.ContactDetected, engagement.id, payload));
            context.Events.Publish(new GameEvent(GameEventType.EngagementStarted, engagement.id, payload));
            return engagement;
        }

        public void DetectAll(GameContext context, MapState mapState)
        {
            if (mapState == null) return;
            foreach (RegionRuntimeState region in mapState.RegionsById.Values)
            {
                DetectRegion(context, mapState, region.id);
            }
        }

        private static void MarkArmiesEngaged(MapState mapState, EngagementRuntimeState engagement)
        {
            for (int i = 0; i < engagement.attackerArmyIds.Count; i++)
            {
                MarkArmyEngaged(mapState, engagement.attackerArmyIds[i], engagement.id);
            }

            for (int i = 0; i < engagement.defenderArmyIds.Count; i++)
            {
                MarkArmyEngaged(mapState, engagement.defenderArmyIds[i], engagement.id);
            }
        }

        private static void MarkArmyEngaged(MapState mapState, string armyId, string engagementId)
        {
            ArmyRuntimeState army;
            if (mapState.TryGetArmy(armyId, out army))
            {
                army.engagementId = engagementId;
                army.task = ArmyTask.Attack;
            }
        }

        private static void MarkRegionContested(MapState mapState, string regionId)
        {
            RegionRuntimeState region;
            if (mapState.TryGetRegion(regionId, out region))
            {
                region.occupationStatus = OccupationStatus.Contested;
            }
        }

        private static EngagementPayload CreatePayload(EngagementRuntimeState engagement)
        {
            return new EngagementPayload
            {
                engagementId = engagement.id,
                regionId = engagement.regionId,
                attackerArmyIds = engagement.attackerArmyIds.ToArray(),
                defenderArmyIds = engagement.defenderArmyIds.ToArray()
            };
        }

        private static void MergeArmyIds(List<string> target, List<string> source)
        {
            for (int i = 0; i < source.Count; i++)
            {
                AddUnique(target, source[i]);
            }
        }

        private static bool HasNewSideMembership(EngagementRuntimeState engagement, List<string> attackerArmyIds, List<string> defenderArmyIds)
        {
            return ContainsNewArmy(engagement.attackerArmyIds, attackerArmyIds) || ContainsNewArmy(engagement.defenderArmyIds, defenderArmyIds);
        }

        private static bool ContainsNewArmy(List<string> existingArmyIds, List<string> candidateArmyIds)
        {
            for (int i = 0; i < candidateArmyIds.Count; i++)
            {
                if (!existingArmyIds.Contains(candidateArmyIds[i])) return true;
            }

            return false;
        }

        private static ArmyRuntimeState FindInitiatingArmy(List<ArmyRuntimeState> armies, string initiatingArmyId)
        {
            if (string.IsNullOrEmpty(initiatingArmyId)) return null;

            for (int i = 0; i < armies.Count; i++)
            {
                if (armies[i].id == initiatingArmyId) return armies[i];
            }

            return null;
        }

        private static ArmyRuntimeState FindAttackerSeed(List<ArmyRuntimeState> armies, string regionId)
        {
            for (int i = 0; i < armies.Count; i++)
            {
                if (armies[i].targetRegionId == regionId && armies[i].task != ArmyTask.Idle) return armies[i];
            }

            return armies.Count > 0 ? armies[0] : null;
        }

        private static void AddUnique(List<string> target, string value)
        {
            if (!target.Contains(value))
            {
                target.Add(value);
            }
        }
    }
}
