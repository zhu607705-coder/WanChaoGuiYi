using System.Collections.Generic;

namespace WanChaoGuiYi
{
    public sealed class MapCommandService
    {
        private readonly MapQueryService queries;
        private readonly GameContext context;

        public MapCommandService(MapQueryService queries, GameContext context)
        {
            this.queries = queries;
            this.context = context;
        }

        public bool MoveArmy(string armyId, string targetRegionId)
        {
            return IssueRouteCommand(armyId, targetRegionId, ArmyTask.Move, "行军");
        }

        public bool StopArmy(string armyId)
        {
            ArmyRuntimeState army = queries != null ? queries.GetArmy(armyId) : null;
            if (army == null) return false;
            if (army.engagementId != null) return false;

            army.targetRegionId = null;
            army.route.Clear();
            army.task = ArmyTask.Idle;
            AddLog("war", army.id + "停止行动，驻扎于" + army.locationRegionId + "。原因：玩家取消当前命令。影响：该部队不再推进路线。");
            return true;
        }

        public bool RetreatArmy(string armyId, string targetRegionId)
        {
            ArmyRuntimeState army = queries != null ? queries.GetArmy(armyId) : null;
            if (army == null || string.IsNullOrEmpty(army.engagementId)) return false;

            bool issued = IssueRouteCommand(armyId, targetRegionId, ArmyTask.Retreat, "撤退");
            if (issued) AddLog("war", armyId + "尝试脱离接敌，撤退会在行军阶段生效。原因：已接敌军队只允许主动撤退。影响：成功抵达后会移出接敌成员。");
            return issued;
        }

        public bool ReinforceArmy(string armyId, string targetRegionId)
        {
            bool issued = IssueRouteCommand(armyId, targetRegionId, ArmyTask.Reinforce, "增援");
            if (issued) AddLog("war", armyId + "正在前往" + targetRegionId + "增援，抵达后会加入当地接敌。原因：目标地区已有战斗接触。影响：抵达后会改变该方参战成员和战力。");
            return issued;
        }

        public bool SiegeRegion(string armyId, string targetRegionId)
        {
            return IssueRouteCommand(armyId, targetRegionId, ArmyTask.Siege, "围攻");
        }

        private bool IssueRouteCommand(string armyId, string targetRegionId, ArmyTask task, string actionLabel)
        {
            if (queries == null || context == null) return false;

            ArmyRuntimeState army = queries.GetArmy(armyId);
            if (army == null || string.IsNullOrEmpty(targetRegionId)) return false;
            if (army.engagementId != null && task != ArmyTask.Retreat) return false;

            List<string> route = queries.FindRoute(army.locationRegionId, targetRegionId);
            if (route.Count < 2)
            {
                return false;
            }

            army.targetRegionId = targetRegionId;
            army.route.Clear();
            army.route.AddRange(route);
            army.task = task;

            MapArmyMovementPayload payload = new MapArmyMovementPayload
            {
                armyId = army.id,
                ownerFactionId = army.ownerFactionId,
                fromRegionId = army.locationRegionId,
                toRegionId = targetRegionId,
                route = route.ToArray(),
                task = task.ToString()
            };

            AddLog("war", army.id + actionLabel + "：" + army.locationRegionId + " → " + targetRegionId + "。原因：命令入口创建路线。影响：部队将在行军阶段向目标推进。");
            context.Events.Publish(new GameEvent(GameEventType.ArmyMoveStarted, army.id, payload));
            return true;
        }

        private void AddLog(string category, string message)
        {
            if (context != null && context.State != null)
            {
                context.State.AddLog(category, message);
            }
        }
    }
}
