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

        public bool PrepareFrontline(string armyId, string targetRegionId)
        {
            if (queries == null || context == null || context.State == null) return false;

            ArmyRuntimeState army = queries.GetArmy(armyId);
            if (army == null || string.IsNullOrEmpty(targetRegionId)) return false;
            if (army.task != ArmyTask.Idle || !string.IsNullOrEmpty(army.engagementId))
            {
                AddLog("war", army.id + "无法前线整备。原因：军队正在执行任务或已接敌。影响：必须先停军或结束接敌。");
                return false;
            }

            FactionState faction = context.State.FindFaction(army.ownerFactionId);
            if (faction == null)
            {
                AddLog("war", army.id + "无法前线整备。原因：缺少所属势力。影响：不能预留粮草。");
                return false;
            }

            FrontlineSupplyPlanForecast before = StrategyMapRulebook.BuildFrontlineSupplyPlanForecast(queries, context, army, targetRegionId);
            if (before == null || before.routeForecast == null || !CanPrepareFromRouteForecast(before.routeForecast))
            {
                AddLog("war", army.id + "前线整备失败。原因：" + (before != null && before.routeForecast != null ? before.routeForecast.disabledReason : "missing_route") + "。影响：仍需邻接推进、兵站或侦察后再整备。");
                return false;
            }

            if (before.routeForecast.routeSteps > 1)
            {
                return StartFrontlineLogisticsPlan(army, faction, targetRegionId, before);
            }

            int supplyBefore = army.supply;
            int reserveBefore = army.frontlinePreparedTargetRegionId == targetRegionId ? DomainMath.Max(0, army.frontlineReservedFood) : 0;
            int supplyNeed = DomainMath.Max(0, before.frontlineSupplyTarget - army.supply);
            int reserveNeed = DomainMath.Max(0, before.occupationAdministrationFoodCost - reserveBefore);
            int foodCost = supplyNeed + reserveNeed;
            if (foodCost <= 0)
            {
                AddLog("war", army.id + "前线整备已足。原因：补给与占后粮政已经覆盖" + targetRegionId + "。影响：可直接进入出征判断。");
                return true;
            }

            if (faction.food < foodCost)
            {
                AddLog("war", army.id + "前线整备失败。原因：粮食不足，需要" + foodCost + "，现有" + faction.food + "。影响：贸然出征会放大补给和占后治理压力。");
                return false;
            }

            int foodBefore = faction.food;
            faction.food -= foodCost;
            army.supply = DomainMath.Min(StrategyCausalRules.FrontlinePreparedSupplyCap, army.supply + supplyNeed);
            army.frontlinePreparedTargetRegionId = targetRegionId;
            army.frontlineReservedFood = reserveBefore + reserveNeed;
            army.frontlinePreparedTurn = context.State.turn;

            FrontlineSupplyPlanForecast after = StrategyMapRulebook.BuildFrontlineSupplyPlanForecast(queries, context, army, targetRegionId);
            AddLog("war", army.id + "前线整备：" + army.locationRegionId + " -> " + targetRegionId +
                          "。原因：预置粮道、兵站接续和占后军管口粮。" +
                          "影响：粮食" + foodBefore + "→" + faction.food +
                          "，补给" + supplyBefore + "→" + army.supply +
                          "，占后预留" + reserveBefore + "→" + army.frontlineReservedFood +
                          "。史据：" + StrategyMapRulebook.FoodSource);

            FrontlinePreparationPayload payload = new FrontlinePreparationPayload
            {
                armyId = army.id,
                targetRegionId = targetRegionId,
                foodBefore = foodBefore,
                foodAfter = faction.food,
                foodCost = foodCost,
                supplyBefore = supplyBefore,
                supplyAfter = army.supply,
                reserveFoodBefore = reserveBefore,
                reserveFoodAfter = army.frontlineReservedFood,
                readinessBefore = before.readinessScore,
                readinessAfter = after != null ? after.readinessScore : before.readinessScore,
                recommendedStep = after != null ? after.recommendedStep : before.recommendedStep
            };
            context.Events.Publish(new GameEvent(GameEventType.FrontlinePrepared, army.id, payload));
            return true;
        }

        public bool CancelFrontlineLogistics(string armyId)
        {
            ArmyRuntimeState army = queries != null ? queries.GetArmy(armyId) : null;
            if (!HasActiveLogistics(army)) return false;

            string convoyId = army.frontlineLogisticsConvoyId;
            string targetRegionId = army.frontlineLogisticsTargetRegionId;
            int priority = army.frontlineLogisticsPriority;
            bool paused = army.frontlineLogisticsPaused;
            int turnsRemaining = army.frontlineLogisticsTurnsRemaining;
            int foodPerTurn = army.frontlineLogisticsFoodPerTurn;
            AddLog("war", army.id + "取消后勤运输队" + FormatConvoySuffix(convoyId) + "。原因：玩家取消前线后勤队列。影响：已送达的补给和预留粮保留，未执行的运输不再消耗粮食。");
            ClearFrontlineLogistics(army);
            PublishLogisticsCommand(army, convoyId, targetRegionId, "cancel", priority, paused, turnsRemaining, foodPerTurn);
            return true;
        }

        public bool ToggleFrontlineLogisticsPause(string armyId)
        {
            ArmyRuntimeState army = queries != null ? queries.GetArmy(armyId) : null;
            if (!HasActiveLogistics(army)) return false;

            army.frontlineLogisticsPaused = !army.frontlineLogisticsPaused;
            AddLog("war", army.id + (army.frontlineLogisticsPaused ? "暂停" : "恢复") + "后勤运输队" + FormatConvoySuffix(army.frontlineLogisticsConvoyId) +
                          "。原因：玩家调整后勤队列节奏。影响：" + (army.frontlineLogisticsPaused ? "本队暂停分段转运。" : "下回合继续转运。"));
            PublishLogisticsCommand(army, army.frontlineLogisticsConvoyId, army.frontlineLogisticsTargetRegionId, army.frontlineLogisticsPaused ? "pause" : "resume");
            return true;
        }

        public bool AdjustFrontlineLogisticsPriority(string armyId, int delta)
        {
            ArmyRuntimeState army = queries != null ? queries.GetArmy(armyId) : null;
            if (!HasActiveLogistics(army) || delta == 0) return false;

            int before = army.frontlineLogisticsPriority;
            int after = DomainMath.Clamp(before + delta, 0, 2);
            if (before == after) return false;

            army.frontlineLogisticsPriority = after;
            RebalanceFrontlineLogisticsPace(army);
            AddLog("war", army.id + "重排后勤运输队" + FormatConvoySuffix(army.frontlineLogisticsConvoyId) +
                          "。原因：玩家调整粮队优先级。" +
                          "影响：优先级" + FormatLogisticsPriority(before) + "→" + FormatLogisticsPriority(after) +
                          "，每回合调粮约" + army.frontlineLogisticsFoodPerTurn +
                          "，预计剩余" + army.frontlineLogisticsTurnsRemaining + "回合。");
            PublishLogisticsCommand(army, army.frontlineLogisticsConvoyId, army.frontlineLogisticsTargetRegionId, "priority");
            return true;
        }

        public bool ApplyEnemyLogisticsRaid(MapState mapState, string raiderFactionId)
        {
            if (mapState == null || context == null || context.State == null || string.IsNullOrEmpty(raiderFactionId)) return false;
            ArmyRuntimeState target = SelectLogisticsRaidTarget(mapState, raiderFactionId);
            if (target == null) return false;

            int riskBefore = target.frontlineLogisticsInterceptionRisk;
            int raidLoss = DomainMath.Max(1, DomainMath.RoundToInt(target.frontlineLogisticsFoodPerTurn * StrategyCausalRules.FrontlineLogisticsAiRaidLossPercent / 100f));
            target.frontlineLogisticsLostFood += raidLoss;
            target.frontlineLogisticsFoodRemaining += raidLoss;
            target.frontlineLogisticsLastRaidTurn = context.State.turn;
            target.frontlineLogisticsLastRaidFactionId = raiderFactionId;
            target.frontlineLogisticsLastRaidLoss = raidLoss;
            target.frontlineLogisticsRaidPressure = DomainMath.Clamp(target.frontlineLogisticsRaidPressure + 25, 0, 100);
            target.frontlineLogisticsInterceptionRisk = DomainMath.Clamp(target.frontlineLogisticsInterceptionRisk + StrategyCausalRules.FrontlineLogisticsAiRaidRiskIncrease, 0, 95);
            RebalanceFrontlineLogisticsPace(target);

            FactionState raider = context.State.FindFaction(raiderFactionId);
            string raiderName = raider != null ? raider.name : raiderFactionId;
            AddLog("war", raiderName + "主动截粮：" + target.id + FormatConvoySuffix(target.frontlineLogisticsConvoyId) +
                          "。原因：敌方识别到前线运输队暴露在粮道压力下。" +
                          "影响：损粮" + raidLoss +
                          "，截粮风险" + riskBefore + "→" + target.frontlineLogisticsInterceptionRisk +
                          "，预计剩余" + target.frontlineLogisticsTurnsRemaining + "回合。");
            context.Events.Publish(new GameEvent(GameEventType.FrontlineLogisticsRaided, target.id, new FrontlineLogisticsRaidPayload
            {
                armyId = target.id,
                convoyId = target.frontlineLogisticsConvoyId,
                raiderFactionId = raiderFactionId,
                targetRegionId = target.frontlineLogisticsTargetRegionId,
                supplyNodeRegionId = target.frontlineLogisticsSupplyNodeRegionId,
                foodLost = raidLoss,
                riskBefore = riskBefore,
                riskAfter = target.frontlineLogisticsInterceptionRisk,
                turnsRemaining = target.frontlineLogisticsTurnsRemaining,
                raidPressure = target.frontlineLogisticsRaidPressure
            }));
            return true;
        }

        public void ExecuteLogisticsTurn(MapState mapState)
        {
            if (mapState == null || context == null || context.State == null) return;

            AdvanceFrontlineLogistics(mapState);
            AdvanceOccupationPacificationQueues(mapState);
        }

        private bool StartFrontlineLogisticsPlan(ArmyRuntimeState army, FactionState faction, string targetRegionId, FrontlineSupplyPlanForecast plan)
        {
            if (army == null || faction == null || plan == null || plan.routeForecast == null) return false;

            int reserveBefore = army.frontlinePreparedTargetRegionId == targetRegionId ? DomainMath.Max(0, army.frontlineReservedFood) : 0;
            int supplyNeed = DomainMath.Max(0, plan.frontlineSupplyTarget - army.supply);
            int reserveNeed = DomainMath.Max(0, plan.occupationAdministrationFoodCost - reserveBefore);
            int nodeBuildNeed = string.IsNullOrEmpty(plan.supplyNodeBuildRegionId) ? 0 : StrategyCausalRules.FrontlineSupplyNodeBuildFoodCost;
            int baseFoodNeed = supplyNeed + reserveNeed + nodeBuildNeed;
            int lossBuffer = CalculateLogisticsLoss(baseFoodNeed, plan.interceptionLossRisk);
            int totalFoodNeed = baseFoodNeed + lossBuffer;
            if (totalFoodNeed <= 0)
            {
                AddLog("war", army.id + "前线后勤排程已足。原因：补给、兵站和占后粮政均已覆盖" + targetRegionId + "。影响：可直接进入出征判断。");
                return true;
            }

            int turns = DomainMath.Max(2, plan.logisticsTurns);
            int foodPerTurn = DomainMath.Max(1, (totalFoodNeed + turns - 1) / turns);
            if (faction.food < foodPerTurn)
            {
                AddLog("war", army.id + "前线后勤排程失败。原因：首段粮食不足，需要" + foodPerTurn + "，现有" + faction.food + "。影响：必须先积粮或缩短路线。");
                return false;
            }

            army.frontlinePreparedTargetRegionId = targetRegionId;
            army.frontlineLogisticsTargetRegionId = targetRegionId;
            army.frontlineLogisticsSupplyNodeRegionId = plan.supplyNodeBuildRegionId;
            army.frontlineLogisticsTotalTurns = turns;
            army.frontlineLogisticsTurnsRemaining = turns;
            army.frontlineLogisticsFoodRemaining = totalFoodNeed;
            army.frontlineLogisticsFoodPerTurn = foodPerTurn;
            army.frontlineLogisticsSupplyNeedRemaining = supplyNeed;
            army.frontlineLogisticsReserveNeedRemaining = reserveNeed;
            army.frontlineLogisticsSupplyNodeBuildFoodRemaining = nodeBuildNeed;
            army.frontlineLogisticsCompletedSegments = 0;
            army.frontlineLogisticsInterceptionRisk = plan.interceptionLossRisk;
            army.frontlineLogisticsLostFood = 0;
            army.frontlineLogisticsConvoyId = "convoy_" + army.id + "_" + context.State.turn;
            army.frontlineLogisticsPriority = 1;
            army.frontlineLogisticsPaused = false;
            army.frontlineLogisticsRaidPressure = 0;
            army.frontlineLogisticsLastRaidTurn = -1;
            army.frontlineLogisticsLastRaidFactionId = null;
            army.frontlineLogisticsLastRaidLoss = 0;
            army.frontlinePreparedTurn = context.State.turn;

            AddLog("war", army.id + "前线后勤排程：" + army.locationRegionId + " -> " + targetRegionId +
                          "。原因：长线出征需要分段转运、兵站施工和占后安抚队列。" +
                          "影响：计划" + turns + "回合，每回合约调粮" + foodPerTurn +
                          "，运输队" + army.frontlineLogisticsConvoyId +
                          "，兵站" + (string.IsNullOrEmpty(plan.supplyNodeBuildRegionId) ? "既有" : plan.supplyNodeBuildRegionId) +
                          "，截粮风险" + plan.interceptionLossRisk + "%。史据：" + StrategyMapRulebook.FoodSource);
            context.Events.Publish(new GameEvent(GameEventType.FrontlinePrepared, army.id, new FrontlinePreparationPayload
            {
                armyId = army.id,
                targetRegionId = targetRegionId,
                foodBefore = faction.food,
                foodAfter = faction.food,
                foodCost = 0,
                supplyBefore = army.supply,
                supplyAfter = army.supply,
                reserveFoodBefore = reserveBefore,
                reserveFoodAfter = army.frontlineReservedFood,
                readinessBefore = plan.readinessScore,
                readinessAfter = plan.readinessScore,
                recommendedStep = "后勤排程执行中"
            }));
            PublishLogisticsCommand(army, army.frontlineLogisticsConvoyId, targetRegionId, "schedule");
            return true;
        }

        private void AdvanceFrontlineLogistics(MapState mapState)
        {
            List<ArmyRuntimeState> logisticsQueue = BuildActiveLogisticsQueue(mapState);
            foreach (ArmyRuntimeState army in logisticsQueue)
            {
                if (army.frontlineLogisticsTurnsRemaining <= 0) continue;
                if (army.frontlineLogisticsPaused)
                {
                    AddLog("war", army.id + "后勤运输队暂停" + FormatConvoySuffix(army.frontlineLogisticsConvoyId) + "。原因：玩家将此粮队后置。影响：本回合不消耗粮食，也不推进兵站或预留粮。");
                    continue;
                }
                if (!string.IsNullOrEmpty(army.engagementId))
                {
                    AddLog("war", army.id + "前线后勤暂停。原因：军队已接敌。影响：转运粮队等待战斗结果。");
                    continue;
                }

                FactionState faction = context.State.FindFaction(army.ownerFactionId);
                if (faction == null) continue;

                int requested = DomainMath.Min(ResolveLogisticsFoodRequest(army), army.frontlineLogisticsFoodRemaining);
                if (requested <= 0)
                {
                    FinishFrontlineLogisticsIfReady(army);
                    continue;
                }

                if (faction.food <= 0)
                {
                    AddLog("war", army.id + "前线后勤停滞。原因：所属势力粮食见底。影响：补给分段和占后安抚队列无法继续。");
                    continue;
                }

                int foodBefore = faction.food;
                int spent = DomainMath.Min(requested, faction.food);
                faction.food -= spent;
                army.frontlineLogisticsFoodRemaining = DomainMath.Max(0, army.frontlineLogisticsFoodRemaining - spent);

                int lost = CalculateLogisticsLoss(spent, army.frontlineLogisticsInterceptionRisk);
                int delivered = DomainMath.Max(0, spent - lost);
                army.frontlineLogisticsLostFood += lost;

                int buildUsed = DomainMath.Min(delivered, army.frontlineLogisticsSupplyNodeBuildFoodRemaining);
                if (buildUsed > 0)
                {
                    army.frontlineLogisticsSupplyNodeBuildFoodRemaining -= buildUsed;
                    delivered -= buildUsed;
                    if (army.frontlineLogisticsSupplyNodeBuildFoodRemaining <= 0)
                    {
                        MarkSupplyNodeBuilt(mapState, army.frontlineLogisticsSupplyNodeRegionId);
                    }
                }

                int supplyUsed = DomainMath.Min(delivered, army.frontlineLogisticsSupplyNeedRemaining);
                if (supplyUsed > 0)
                {
                    army.frontlineLogisticsSupplyNeedRemaining -= supplyUsed;
                    army.supply = DomainMath.Min(StrategyCausalRules.FrontlinePreparedSupplyCap, army.supply + supplyUsed);
                    delivered -= supplyUsed;
                }

                int reserveUsed = DomainMath.Min(delivered, army.frontlineLogisticsReserveNeedRemaining);
                if (reserveUsed > 0)
                {
                    army.frontlineLogisticsReserveNeedRemaining -= reserveUsed;
                    army.frontlineReservedFood += reserveUsed;
                    army.frontlinePreparedTargetRegionId = army.frontlineLogisticsTargetRegionId;
                    delivered -= reserveUsed;
                }

                army.frontlineLogisticsCompletedSegments++;
                army.frontlineLogisticsTurnsRemaining = DomainMath.Max(0, army.frontlineLogisticsTurnsRemaining - 1);

                int remainingNeed = army.frontlineLogisticsSupplyNeedRemaining + army.frontlineLogisticsReserveNeedRemaining + army.frontlineLogisticsSupplyNodeBuildFoodRemaining;
                if (army.frontlineLogisticsTurnsRemaining <= 0 && remainingNeed > 0)
                {
                    int extraBuffer = CalculateLogisticsLoss(remainingNeed, army.frontlineLogisticsInterceptionRisk);
                    army.frontlineLogisticsFoodRemaining += remainingNeed + extraBuffer;
                    army.frontlineLogisticsTurnsRemaining = 1;
                    army.frontlineLogisticsTotalTurns++;
                    army.frontlineLogisticsFoodPerTurn = DomainMath.Max(1, remainingNeed + extraBuffer);
                    AddLog("war", army.id + "前线后勤延长一回合。原因：截粮或兵站消耗导致计划缺口仍有" + remainingNeed + "。影响：出征前仍需等待补齐。");
                }

                AddLog("war", army.id + "前线后勤推进：" + army.frontlineLogisticsCompletedSegments + "/" + army.frontlineLogisticsTotalTurns +
                              "。原因：分段转运粮草与军需。" +
                              "影响：粮食" + foodBefore + "→" + faction.food +
                              "，送达" + (spent - lost) +
                              "，截粮损耗" + lost +
                              "，补给" + army.supply +
                              "，占后预留" + army.frontlineReservedFood + "。");
                context.Events.Publish(new GameEvent(GameEventType.FrontlineLogisticsAdvanced, army.id, new FrontlineLogisticsPayload
                {
                    armyId = army.id,
                    convoyId = army.frontlineLogisticsConvoyId,
                    targetRegionId = army.frontlineLogisticsTargetRegionId,
                    supplyNodeRegionId = army.frontlineLogisticsSupplyNodeRegionId,
                    foodSpent = spent,
                    foodDelivered = spent - lost,
                    foodLost = lost,
                    supplyAfter = army.supply,
                    reserveFoodAfter = army.frontlineReservedFood,
                    turnsRemaining = army.frontlineLogisticsTurnsRemaining,
                    completedSegments = army.frontlineLogisticsCompletedSegments,
                    priority = army.frontlineLogisticsPriority,
                    paused = army.frontlineLogisticsPaused
                }));

                FinishFrontlineLogisticsIfReady(army);
            }
        }

        private void AdvanceOccupationPacificationQueues(MapState mapState)
        {
            foreach (RegionRuntimeState runtimeRegion in mapState.RegionsById.Values)
            {
                if (runtimeRegion == null || runtimeRegion.occupationPacificationQueueTurnsRemaining <= 0) continue;
                RegionState region = context.State.FindRegion(runtimeRegion.id);
                FactionState faction = region != null ? context.State.FindFaction(region.ownerFactionId) : null;
                if (region == null || faction == null) continue;

                GovernanceActionKind action = ResolveOccupationQueueAction(region.occupationPacificationQueueStep);
                RegionDefinition definition = context.Data != null ? context.Data.GetRegion(region.id) : null;
                GovernanceActionForecast forecast = StrategyMapRulebook.ApplyGovernanceAction(context, definition, region, faction, action);
                if (forecast == null || !forecast.canApply)
                {
                    AddLog("governance", region.id + "占后安抚队列暂停。原因：" + (forecast != null ? forecast.disabledReason : "missing_forecast") + "。影响：预留粮仍保留，控制链暂不推进。");
                    continue;
                }

                int stepBefore = runtimeRegion.occupationPacificationQueueStep;
                region.occupationPacificationQueueStep++;
                region.occupationPacificationQueueTurnsRemaining = DomainMath.Max(0, region.occupationPacificationQueueTurnsRemaining - 1);
                if (region.occupationPacificationQueueStep > 3 || region.controlStage == ControlStage.Controlled)
                {
                    region.occupationPacificationQueueStep = 0;
                    region.occupationPacificationQueueTurnsRemaining = 0;
                }

                SyncRuntimeRegionFromLegacy(runtimeRegion, region);
                AddLog("governance", region.id + "占后安抚队列推进：" + action +
                                      "。原因：前线预留粮进入分回合治理。" +
                                      "影响：控制阶段" + region.controlStage +
                                      "，剩余预留粮" + region.occupationReservedFood +
                                      "，剩余队列" + region.occupationPacificationQueueTurnsRemaining + "。");
                context.Events.Publish(new GameEvent(GameEventType.OccupationPacificationQueueAdvanced, region.id, new OccupationPacificationQueuePayload
                {
                    regionId = region.id,
                    actionId = action.ToString(),
                    queueStepBefore = stepBefore,
                    queueStepAfter = region.occupationPacificationQueueStep,
                    turnsRemainingAfter = region.occupationPacificationQueueTurnsRemaining,
                    reservedFoodAfter = region.occupationReservedFood,
                    controlStageAfter = region.controlStage
                }));
            }
        }

        private static bool CanPrepareFromRouteForecast(CampaignRouteForecast forecast)
        {
            if (forecast == null || forecast.route == null || forecast.route.Length < 2) return false;
            return forecast.canDispatch || forecast.disabledReason == "dispatch_requires_adjacent_or_supply_node";
        }

        private static int CalculateLogisticsLoss(int foodAmount, int interceptionRisk)
        {
            if (foodAmount <= 0) return 0;
            if (interceptionRisk >= 65) return DomainMath.Max(1, DomainMath.RoundToInt(foodAmount * StrategyCausalRules.FrontlineLogisticsHighRiskLossPercent / 100f));
            if (interceptionRisk >= 35) return DomainMath.RoundToInt(foodAmount * StrategyCausalRules.FrontlineLogisticsMediumRiskLossPercent / 100f);
            return 0;
        }

        private static List<ArmyRuntimeState> BuildActiveLogisticsQueue(MapState mapState)
        {
            List<ArmyRuntimeState> queue = new List<ArmyRuntimeState>();
            if (mapState == null) return queue;

            foreach (ArmyRuntimeState army in mapState.ArmiesById.Values)
            {
                if (HasActiveLogistics(army))
                {
                    queue.Add(army);
                }
            }

            queue.Sort(CompareLogisticsQueue);
            return queue;
        }

        private static int CompareLogisticsQueue(ArmyRuntimeState left, ArmyRuntimeState right)
        {
            if (left == null && right == null) return 0;
            if (left == null) return 1;
            if (right == null) return -1;

            int priority = right.frontlineLogisticsPriority.CompareTo(left.frontlineLogisticsPriority);
            if (priority != 0) return priority;

            int paused = left.frontlineLogisticsPaused.CompareTo(right.frontlineLogisticsPaused);
            if (paused != 0) return paused;

            int risk = right.frontlineLogisticsInterceptionRisk.CompareTo(left.frontlineLogisticsInterceptionRisk);
            if (risk != 0) return risk;

            return string.Compare(left.id, right.id, System.StringComparison.Ordinal);
        }

        private static int ResolveLogisticsFoodRequest(ArmyRuntimeState army)
        {
            if (army == null) return 0;
            if (army.frontlineLogisticsFoodPerTurn <= 0)
            {
                RebalanceFrontlineLogisticsPace(army);
            }

            return DomainMath.Max(1, army.frontlineLogisticsFoodPerTurn);
        }

        private static void RebalanceFrontlineLogisticsPace(ArmyRuntimeState army)
        {
            if (army == null || string.IsNullOrEmpty(army.frontlineLogisticsTargetRegionId)) return;

            int remainingFood = DomainMath.Max(0, army.frontlineLogisticsFoodRemaining);
            int remainingTurns = DomainMath.Max(1, army.frontlineLogisticsTurnsRemaining);
            int basePerTurn = DomainMath.Max(1, DomainMath.CeilToInt((float)remainingFood / remainingTurns));
            int priorityPercent = ResolvePriorityPercent(army.frontlineLogisticsPriority);
            army.frontlineLogisticsFoodPerTurn = DomainMath.Max(1, DomainMath.CeilToInt(basePerTurn * priorityPercent / 100f));
            army.frontlineLogisticsTurnsRemaining = remainingFood > 0
                ? DomainMath.Max(1, DomainMath.CeilToInt((float)remainingFood / army.frontlineLogisticsFoodPerTurn))
                : 0;
            army.frontlineLogisticsTotalTurns = DomainMath.Max(army.frontlineLogisticsTotalTurns, army.frontlineLogisticsCompletedSegments + army.frontlineLogisticsTurnsRemaining);
        }

        private static int ResolvePriorityPercent(int priority)
        {
            if (priority >= 2) return StrategyCausalRules.FrontlineLogisticsUrgentPriorityPercent;
            if (priority <= 0) return StrategyCausalRules.FrontlineLogisticsDelayedPriorityPercent;
            return StrategyCausalRules.FrontlineLogisticsNormalPriorityPercent;
        }

        private ArmyRuntimeState SelectLogisticsRaidTarget(MapState mapState, string raiderFactionId)
        {
            ArmyRuntimeState best = null;
            int bestScore = int.MinValue;
            foreach (ArmyRuntimeState army in mapState.ArmiesById.Values)
            {
                if (!CanRaidLogistics(army, raiderFactionId)) continue;

                int score = ScoreLogisticsRaidTarget(army, raiderFactionId);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = army;
                }
            }

            return bestScore >= 35 ? best : null;
        }

        private bool CanRaidLogistics(ArmyRuntimeState army, string raiderFactionId)
        {
            if (!HasActiveLogistics(army) || army.frontlineLogisticsPaused) return false;
            if (army.ownerFactionId == raiderFactionId) return false;
            if (context == null || context.State == null) return false;
            if (army.frontlineLogisticsLastRaidTurn == context.State.turn) return false;
            if (!IsHostilePair(raiderFactionId, army.ownerFactionId) && !OwnsRegion(raiderFactionId, army.frontlineLogisticsTargetRegionId)) return false;

            return ThreatensLogisticsRegion(raiderFactionId, army.frontlineLogisticsTargetRegionId) ||
                   ThreatensLogisticsRegion(raiderFactionId, army.frontlineLogisticsSupplyNodeRegionId) ||
                   ThreatensLogisticsRegion(raiderFactionId, army.locationRegionId);
        }

        private int ScoreLogisticsRaidTarget(ArmyRuntimeState army, string raiderFactionId)
        {
            int score = 25 + army.frontlineLogisticsPriority * 10;
            score += army.frontlineLogisticsInterceptionRisk / 2;
            score += army.frontlineLogisticsFoodRemaining / 8;
            score += army.frontlineLogisticsReserveNeedRemaining / 3;
            if (OwnsRegion(raiderFactionId, army.frontlineLogisticsTargetRegionId)) score += 20;
            if (ThreatensLogisticsRegion(raiderFactionId, army.frontlineLogisticsSupplyNodeRegionId)) score += 12;
            return score;
        }

        private bool ThreatensLogisticsRegion(string factionId, string regionId)
        {
            if (string.IsNullOrEmpty(factionId) || string.IsNullOrEmpty(regionId) || queries == null || context == null || context.State == null) return false;
            if (OwnsRegion(factionId, regionId)) return true;

            foreach (string neighborId in queries.GetNeighborRegions(regionId))
            {
                if (OwnsRegion(factionId, neighborId)) return true;
                List<ArmyRuntimeState> armies = queries.GetArmiesInRegion(neighborId);
                for (int i = 0; i < armies.Count; i++)
                {
                    if (armies[i] != null && armies[i].ownerFactionId == factionId) return true;
                }
            }

            List<ArmyRuntimeState> localArmies = queries.GetArmiesInRegion(regionId);
            for (int i = 0; i < localArmies.Count; i++)
            {
                if (localArmies[i] != null && localArmies[i].ownerFactionId == factionId) return true;
            }

            return false;
        }

        private bool OwnsRegion(string factionId, string regionId)
        {
            if (context == null || context.State == null || string.IsNullOrEmpty(factionId) || string.IsNullOrEmpty(regionId)) return false;
            RegionState region = context.State.FindRegion(regionId);
            return region != null && region.ownerFactionId == factionId;
        }

        private bool IsHostilePair(string factionA, string factionB)
        {
            if (context == null || context.State == null || string.IsNullOrEmpty(factionA) || string.IsNullOrEmpty(factionB)) return false;
            if (factionA == factionB) return false;

            for (int i = 0; i < context.State.diplomaticRelations.Count; i++)
            {
                DiplomaticRelation relation = context.State.diplomaticRelations[i];
                if (relation.status != DiplomacyStatus.AtWar) continue;
                if ((relation.factionA == factionA && relation.factionB == factionB) ||
                    (relation.factionB == factionA && relation.factionA == factionB))
                {
                    return true;
                }
            }

            return false;
        }

        private void MarkSupplyNodeBuilt(MapState mapState, string regionId)
        {
            if (string.IsNullOrEmpty(regionId)) return;
            RegionState legacyRegion = context.State.FindRegion(regionId);
            if (legacyRegion != null)
            {
                legacyRegion.supplyNode = true;
            }

            RegionRuntimeState runtimeRegion;
            if (mapState != null && mapState.TryGetRegion(regionId, out runtimeRegion) && runtimeRegion != null)
            {
                runtimeRegion.supplyNode = true;
            }

            AddLog("war", regionId + "兵站建成。原因：前线后勤排程完成本段兵站施工。影响：后续长线出征可从该补给节点投送。");
        }

        private static void FinishFrontlineLogisticsIfReady(ArmyRuntimeState army)
        {
            if (army == null || string.IsNullOrEmpty(army.frontlineLogisticsTargetRegionId)) return;
            int remainingNeed = army.frontlineLogisticsSupplyNeedRemaining + army.frontlineLogisticsReserveNeedRemaining + army.frontlineLogisticsSupplyNodeBuildFoodRemaining;
            if (remainingNeed > 0 || (army.frontlineLogisticsTurnsRemaining > 0 && army.frontlineLogisticsFoodRemaining > 0)) return;

            ClearFrontlineLogistics(army);
        }

        private static bool HasActiveLogistics(ArmyRuntimeState army)
        {
            return army != null && !string.IsNullOrEmpty(army.frontlineLogisticsTargetRegionId);
        }

        private static void ClearFrontlineLogistics(ArmyRuntimeState army)
        {
            if (army == null) return;

            army.frontlineLogisticsTargetRegionId = null;
            army.frontlineLogisticsSupplyNodeRegionId = null;
            army.frontlineLogisticsFoodRemaining = 0;
            army.frontlineLogisticsFoodPerTurn = 0;
            army.frontlineLogisticsSupplyNeedRemaining = 0;
            army.frontlineLogisticsReserveNeedRemaining = 0;
            army.frontlineLogisticsSupplyNodeBuildFoodRemaining = 0;
            army.frontlineLogisticsInterceptionRisk = 0;
            army.frontlineLogisticsConvoyId = null;
            army.frontlineLogisticsPriority = 0;
            army.frontlineLogisticsPaused = false;
            army.frontlineLogisticsRaidPressure = 0;
            army.frontlineLogisticsLastRaidTurn = -1;
            army.frontlineLogisticsLastRaidFactionId = null;
            army.frontlineLogisticsLastRaidLoss = 0;
        }

        private void PublishLogisticsCommand(ArmyRuntimeState army, string convoyId, string targetRegionId, string command)
        {
            PublishLogisticsCommand(
                army,
                convoyId,
                targetRegionId,
                command,
                army != null ? army.frontlineLogisticsPriority : 0,
                army != null && army.frontlineLogisticsPaused,
                army != null ? army.frontlineLogisticsTurnsRemaining : 0,
                army != null ? army.frontlineLogisticsFoodPerTurn : 0);
        }

        private void PublishLogisticsCommand(ArmyRuntimeState army, string convoyId, string targetRegionId, string command, int priority, bool paused, int turnsRemaining, int foodPerTurn)
        {
            if (context == null || context.Events == null || army == null) return;
            context.Events.Publish(new GameEvent(GameEventType.FrontlineLogisticsCommanded, army.id, new FrontlineLogisticsCommandPayload
            {
                armyId = army.id,
                convoyId = convoyId,
                targetRegionId = targetRegionId,
                command = command,
                priority = priority,
                paused = paused,
                turnsRemaining = turnsRemaining,
                foodPerTurn = foodPerTurn
            }));
        }

        private static string FormatLogisticsPriority(int priority)
        {
            if (priority >= 2) return "加急";
            if (priority <= 0) return "后置";
            return "常规";
        }

        private static string FormatConvoySuffix(string convoyId)
        {
            return string.IsNullOrEmpty(convoyId) ? "" : "（" + convoyId + "）";
        }

        private static GovernanceActionKind ResolveOccupationQueueAction(int queueStep)
        {
            if (queueStep <= 1) return GovernanceActionKind.MilitaryGovern;
            if (queueStep == 2) return GovernanceActionKind.Pacify;
            return GovernanceActionKind.RegisterHouseholds;
        }

        private static void SyncRuntimeRegionFromLegacy(RegionRuntimeState runtime, RegionState legacy)
        {
            if (runtime == null || legacy == null) return;
            runtime.ownerFactionId = legacy.ownerFactionId;
            runtime.occupationStatus = legacy.occupationStatus;
            runtime.integration = legacy.integration;
            runtime.taxContributionPercent = legacy.taxContributionPercent;
            runtime.foodContributionPercent = legacy.foodContributionPercent;
            runtime.rebellionRisk = legacy.rebellionRisk;
            runtime.localPower = legacy.localPower;
            runtime.annexationPressure = legacy.annexationPressure;
            runtime.regionSpecialization = legacy.regionSpecialization;
            runtime.controlStage = legacy.controlStage;
            runtime.occupationReservedFood = legacy.occupationReservedFood;
            runtime.occupationPacificationQueueStep = legacy.occupationPacificationQueueStep;
            runtime.occupationPacificationQueueTurnsRemaining = legacy.occupationPacificationQueueTurnsRemaining;
            runtime.localAcceptance = legacy.localAcceptance;
            runtime.visibilityState = legacy.visibilityState;
            runtime.supplyNode = legacy.supplyNode;
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

            if (task != ArmyTask.Retreat)
            {
                CampaignRouteForecast forecast = StrategyMapRulebook.BuildCampaignRouteForecast(queries, context, army, targetRegionId);
                if (forecast == null || !forecast.canDispatch)
                {
                    AddLog("war", army.id + " route rejected: " + (forecast != null ? forecast.FormatCompact() : "missing forecast"));
                    return false;
                }
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
