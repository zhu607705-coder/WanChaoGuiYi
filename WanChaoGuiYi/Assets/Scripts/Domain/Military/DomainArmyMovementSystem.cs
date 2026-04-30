namespace WanChaoGuiYi
{
    public sealed class DomainArmyMovementSystem : IGameSystem
    {
        private readonly WorldState worldState;
        private readonly MapCommandService mapCommandService;
        private readonly DomainEngagementDetector engagementDetector;

        public DomainArmyMovementSystem(
            WorldState worldState,
            MapCommandService mapCommandService,
            DomainEngagementDetector engagementDetector)
        {
            this.worldState = worldState;
            this.mapCommandService = mapCommandService;
            this.engagementDetector = engagementDetector;
        }

        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            if (worldState != null && worldState.Map != null)
            {
                AdvanceMapLedArmies(context, worldState.Map);
                return;
            }

            AdvanceLegacyMovement(context);
        }

        public bool MoveArmy(GameContext context, string armyId, string targetRegionId, IMapGraphData mapGraph)
        {
            if (mapCommandService != null)
            {
                return mapCommandService.MoveArmy(armyId, targetRegionId);
            }

            ArmyState army = FindArmy(context, armyId);
            if (army == null || mapGraph == null) return false;
            if (!mapGraph.AreNeighbors(army.regionId, targetRegionId)) return false;

            army.regionId = targetRegionId;
            army.movementProgress = 100;
            return true;
        }

        private void AdvanceMapLedArmies(GameContext context, MapState mapState)
        {
            foreach (ArmyRuntimeState army in mapState.ArmiesById.Values)
            {
                if (army.task == ArmyTask.Idle || army.route == null || army.route.Count < 2) continue;
                if (army.engagementId != null && army.task != ArmyTask.Retreat) continue;

                string fromRegionId = army.locationRegionId;
                string nextRegionId = army.route[1];
                mapState.MoveArmyToRegion(army.id, nextRegionId);
                SyncLegacyArmyRegion(context, army.id, nextRegionId);
                army.route.RemoveAt(0);

                MapArmyMovementPayload payload = new MapArmyMovementPayload
                {
                    armyId = army.id,
                    ownerFactionId = army.ownerFactionId,
                    fromRegionId = fromRegionId,
                    toRegionId = nextRegionId,
                    route = army.route.ToArray(),
                    task = army.task.ToString()
                };

                context.State.AddLog("war", army.id + "抵达" + nextRegionId + "。原因：行军路线推进到下一节点。影响：系统会检查当地是否存在敌对部队并触发接敌。");
                context.Events.Publish(new GameEvent(GameEventType.ArmyArrived, army.id, payload));

                if (army.task == ArmyTask.Retreat && army.engagementId != null)
                {
                    string retreatEngagementId = army.engagementId;
                    DomainEngagementCleanup.RemoveArmyFromEngagement(mapState, retreatEngagementId, army.id);
                    DomainEngagementCleanup.ClearEngagementIfSideEmpty(mapState, retreatEngagementId);
                    army.engagementId = null;
                    context.State.AddLog("war", army.id + "脱离接敌并撤退至" + nextRegionId + "。原因：撤退命令在行军阶段生效。影响：原接敌若缺少一方会被清理。");
                }

                if (engagementDetector != null)
                {
                    engagementDetector.DetectRegion(context, mapState, nextRegionId, army.id);
                }

                if (army.engagementId != null)
                {
                    continue;
                }

                if (army.locationRegionId == army.targetRegionId || army.route.Count < 2)
                {
                    army.targetRegionId = null;
                    army.route.Clear();
                    army.task = ArmyTask.Idle;
                }
            }
        }

        private static void AdvanceLegacyMovement(GameContext context)
        {
            if (context == null || context.State == null) return;

            for (int i = 0; i < context.State.armies.Count; i++)
            {
                ArmyState army = context.State.armies[i];
                if (army.movementProgress > 0)
                {
                    army.movementProgress = DomainMath.Max(0, army.movementProgress - 50);
                }
            }
        }

        private static void SyncLegacyArmyRegion(GameContext context, string armyId, string regionId)
        {
            ArmyState legacyArmy = FindArmy(context, armyId);
            if (legacyArmy != null)
            {
                legacyArmy.regionId = regionId;
                legacyArmy.movementProgress = 100;
            }
        }

        private static ArmyState FindArmy(GameContext context, string armyId)
        {
            if (context == null || context.State == null) return null;

            for (int i = 0; i < context.State.armies.Count; i++)
            {
                if (context.State.armies[i].id == armyId) return context.State.armies[i];
            }

            return null;
        }
    }
}
