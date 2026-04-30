using System.Collections.Generic;

namespace WanChaoGuiYi
{
    public sealed class DomainMapWarResolutionSystem : IGameSystem
    {
        private const int RoutSoldierThreshold = 100;

        private readonly WorldState worldState;
        private readonly DomainEngagementDetector engagementDetector;
        private readonly DomainBattleSimulationSystem battleSimulationSystem;
        private readonly DomainOccupationSystem occupationSystem;

        public DomainMapWarResolutionSystem(
            WorldState worldState,
            DomainEngagementDetector engagementDetector,
            DomainBattleSimulationSystem battleSimulationSystem,
            DomainOccupationSystem occupationSystem)
        {
            this.worldState = worldState;
            this.engagementDetector = engagementDetector;
            this.battleSimulationSystem = battleSimulationSystem;
            this.occupationSystem = occupationSystem;
        }

        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            if (worldState == null || worldState.Map == null) return;
            if (engagementDetector == null || battleSimulationSystem == null || occupationSystem == null) return;

            MapState mapState = worldState.Map;
            engagementDetector.DetectAll(context, mapState);
            ResolveFormedEngagements(context, mapState);
        }

        private void ResolveFormedEngagements(GameContext context, MapState mapState)
        {
            List<string> engagementIds = new List<string>(mapState.EngagementsById.Keys);
            for (int i = 0; i < engagementIds.Count; i++)
            {
                EngagementRuntimeState engagement;
                if (!mapState.EngagementsById.TryGetValue(engagementIds[i], out engagement)) continue;
                if (engagement.phase == EngagementPhase.Resolved && engagement.result != null)
                {
                    continue;
                }

                BattleResult result = battleSimulationSystem.ResolveEngagement(context, mapState, engagement.id);
                if (result == null) continue;

                occupationSystem.ApplyBattleOccupation(context, mapState, engagement.id);
                ResolveLoserAfterBattle(context, mapState, engagement);
                ClearResolvedEngagementArmies(mapState, engagement);
                mapState.RemoveEngagement(engagement.id);
            }
        }

        private static void ResolveLoserAfterBattle(GameContext context, MapState mapState, EngagementRuntimeState engagement)
        {
            if (context == null || mapState == null || engagement == null || engagement.result == null) return;

            List<string> loserArmyIds = new List<string>(engagement.result.attackerWon ? engagement.defenderArmyIds : engagement.attackerArmyIds);
            for (int i = 0; i < loserArmyIds.Count; i++)
            {
                ResolveLoserArmy(context, mapState, engagement, engagement.regionId, loserArmyIds[i]);
            }
        }

        private static void ResolveLoserArmy(GameContext context, MapState mapState, EngagementRuntimeState engagement, string battleRegionId, string armyId)
        {
            ArmyRuntimeState army;
            if (!mapState.TryGetArmy(armyId, out army)) return;

            if (army.soldiers < RoutSoldierThreshold || army.morale <= 0)
            {
                RemoveArmy(context, mapState, engagement, army.id, battleRegionId, "溃散");
                return;
            }

            string retreatRegionId = FindOwnedNeighborRegion(context, battleRegionId, army.ownerFactionId);
            if (string.IsNullOrEmpty(retreatRegionId))
            {
                RemoveArmy(context, mapState, engagement, army.id, battleRegionId, "无路可退而溃散");
                return;
            }

            mapState.MoveArmyToRegion(army.id, retreatRegionId);
            RemoveArmyFromEngagement(engagement, army.id);
            army.targetRegionId = null;
            army.route.Clear();
            army.task = ArmyTask.Idle;
            army.engagementId = null;

            ArmyState legacyArmy = FindLegacyArmy(context, army.id);
            if (legacyArmy != null)
            {
                legacyArmy.regionId = retreatRegionId;
                legacyArmy.movementProgress = 0;
            }

            context.State.AddLog("war", army.id + "战败后撤退至" + retreatRegionId + "。");
        }

        private static string FindOwnedNeighborRegion(GameContext context, string regionId, string ownerFactionId)
        {
            RegionDefinition region = context.Data.GetRegion(regionId);
            if (region == null || region.neighbors == null) return null;

            for (int i = 0; i < region.neighbors.Length; i++)
            {
                RegionState neighbor = context.State.FindRegion(region.neighbors[i]);
                if (neighbor != null && neighbor.ownerFactionId == ownerFactionId)
                {
                    return neighbor.id;
                }
            }

            return null;
        }

        private static void RemoveArmy(GameContext context, MapState mapState, EngagementRuntimeState engagement, string armyId, string battleRegionId, string reason)
        {
            RemoveArmyFromEngagement(engagement, armyId);
            mapState.RemoveArmy(armyId);
            for (int i = context.State.armies.Count - 1; i >= 0; i--)
            {
                if (context.State.armies[i].id == armyId)
                {
                    context.State.armies.RemoveAt(i);
                    break;
                }
            }

            context.State.AddLog("war", armyId + "在" + battleRegionId + reason + "。");
        }

        private static void RemoveArmyFromEngagement(EngagementRuntimeState engagement, string armyId)
        {
            if (engagement == null || string.IsNullOrEmpty(armyId)) return;
            engagement.attackerArmyIds.Remove(armyId);
            engagement.defenderArmyIds.Remove(armyId);
        }

        private static ArmyState FindLegacyArmy(GameContext context, string armyId)
        {
            for (int i = 0; i < context.State.armies.Count; i++)
            {
                if (context.State.armies[i].id == armyId) return context.State.armies[i];
            }

            return null;
        }

        private static void ClearResolvedEngagementArmies(MapState mapState, EngagementRuntimeState engagement)
        {
            ClearArmyEngagements(mapState, engagement.attackerArmyIds);
            ClearArmyEngagements(mapState, engagement.defenderArmyIds);

            RegionRuntimeState region;
            if (mapState.TryGetRegion(engagement.regionId, out region) && region.occupationStatus == OccupationStatus.Contested)
            {
                region.occupationStatus = OccupationStatus.Controlled;
            }
        }

        private static void ClearArmyEngagements(MapState mapState, List<string> armyIds)
        {
            for (int i = 0; i < armyIds.Count; i++)
            {
                ArmyRuntimeState army;
                if (!mapState.TryGetArmy(armyIds[i], out army)) continue;
                army.engagementId = null;
                if (army.task == ArmyTask.Attack)
                {
                    army.task = ArmyTask.Idle;
                }
            }
        }
    }
}
