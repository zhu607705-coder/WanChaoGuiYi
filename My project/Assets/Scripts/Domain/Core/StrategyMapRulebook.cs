using System;
using System.Collections.Generic;

namespace WanChaoGuiYi
{
    [Serializable]
    public sealed class GovernanceActionForecast
    {
        public GovernanceActionKind action;
        public string actionId;
        public string label;
        public string reason;
        public string sourceReference;
        public bool canApply;
        public string disabledReason;
        public int moneyDelta;
        public int foodDelta;
        public int legitimacyDelta;
        public int populationDelta;
        public int manpowerDelta;
        public int integrationDelta;
        public int rebellionRiskDelta;
        public int localPowerDelta;
        public int annexationPressureDelta;
        public int localAcceptanceDelta;
        public int taxContributionPercentDelta;
        public int foodContributionPercentDelta;
        public ControlStage nextControlStage;

        public string FormatCompact()
        {
            return label + " | money " + FormatDelta(moneyDelta) +
                   " food " + FormatDelta(foodDelta) +
                   " legitimacy " + FormatDelta(legitimacyDelta) +
                   " integration " + FormatDelta(integrationDelta) +
                   " acceptance " + FormatDelta(localAcceptanceDelta) +
                   " rebellion " + FormatDelta(rebellionRiskDelta) +
                   " localPower " + FormatDelta(localPowerDelta);
        }

        private static string FormatDelta(int value)
        {
            return value > 0 ? "+" + value : value.ToString();
        }
    }

    [Serializable]
    public sealed class CampaignRouteForecast
    {
        public bool canDispatch;
        public string disabledReason;
        public string[] route;
        public int routeSteps;
        public int firstTurnSupplyCost;
        public int fullRouteSupplyCost;
        public int supplyAfterFirstMove;
        public int supplyAtContact;
        public int supplyPowerPercent;
        public int contactRisk;
        public int interceptionRisk;
        public VisibilityState visibilityState;
        public bool hasUnknownRisk;
        public string sourceReference;

        public string FormatCompact()
        {
            string state = canDispatch ? "dispatch_ready" : "dispatch_blocked";
            string routeLabel = route != null && route.Length > 0 ? string.Join(">", route) : "none";
            return state +
                   " | route " + routeLabel +
                   " | supply " + supplyAtContact + "/" + supplyPowerPercent + "%" +
                   " | visibility " + visibilityState +
                   " | contactRisk " + contactRisk +
                   " | interceptionRisk " + interceptionRisk +
                   (hasUnknownRisk ? " | unknown enemy detail" : "") +
                   (string.IsNullOrEmpty(disabledReason) ? "" : " | reason " + disabledReason);
        }
    }

    [Serializable]
    public sealed class StrategyOutlinerEntry
    {
        public string category;
        public string groupId;
        public string groupLabel;
        public string targetId;
        public string label;
        public int groupPriority;
        public int priority;
    }

    public static class StrategyMapRulebook
    {
        public const string GovernanceSource = "Historical inspiration: Shiji, Hanshu, Zizhi Tongjian; numbers are playable abstractions.";
        public const string WarSource = "Historical inspiration: Sunzi, Wujing Zongyao, Zizhi Tongjian; route numbers are playable abstractions.";
        public const string FoodSource = "Historical inspiration: Hanshu Shihuo Zhi, Ming Shi Shihuo Zhi; food pressure is playable abstraction.";
        public const int ControlledContributionPercent = 70;

        public const int PacifyMoneyCost = 50;
        public const int PacifyFoodCost = 30;
        public const int PacifyIntegrationGain = 10;
        public const int PacifyRebellionReduction = 12;
        public const int PacifyLocalPowerReduction = 5;
        public const int PacifyAcceptanceGain = 8;

        public const int MilitaryGovernMoneyCost = 20;
        public const int MilitaryGovernFoodCost = 25;
        public const int MilitaryGovernLegitimacyCost = 1;
        public const int MilitaryGovernRebellionReduction = 18;
        public const int MilitaryGovernAcceptanceCost = 4;

        public const int RegisterMoneyCost = 35;
        public const int RegisterFoodCost = 10;
        public const int RegisterIntegrationGain = 18;
        public const int RegisterAcceptanceGain = 6;
        public const int RegisterContributionGain = 25;

        public const int ReliefFoodCost = 60;
        public const int ReliefRebellionReduction = 10;
        public const int ReliefAcceptanceGain = 12;

        public const int TaxPressureMoneyGain = 60;
        public const int TaxPressureRebellionIncrease = 8;
        public const int TaxPressureAcceptanceCost = 10;
        public const int TaxPressureLegitimacyCost = 2;

        public static void ApplyRegionDefaults(RegionDefinition definition, RegionState state, IDataRepository data)
        {
            if (definition == null || state == null) return;

            RegionSpecialization specialization = ResolveSpecialization(definition, state, data);
            state.regionSpecialization = specialization;
            state.controlStage = ResolveControlStage(state);
            if (state.localAcceptance <= 0)
            {
                state.localAcceptance = ResolveInitialAcceptance(definition, state);
            }
            state.visibilityState = ResolveInitialVisibility(definition, state);
            state.supplyNode = ResolveSupplyNode(definition, state, data);
        }

        public static void ApplyRuntimeDefaults(RegionDefinition definition, RegionState legacy, RegionRuntimeState runtime, IDataRepository data)
        {
            if (runtime == null) return;

            if (legacy != null)
            {
                runtime.regionSpecialization = legacy.regionSpecialization;
                runtime.controlStage = legacy.controlStage;
                runtime.localAcceptance = legacy.localAcceptance;
                runtime.visibilityState = legacy.visibilityState;
                runtime.supplyNode = legacy.supplyNode;
            }

            if (definition != null && legacy != null)
            {
                if (runtime.localAcceptance <= 0) runtime.localAcceptance = ResolveInitialAcceptance(definition, legacy);
                runtime.regionSpecialization = runtime.regionSpecialization == RegionSpecialization.None
                    ? ResolveSpecialization(definition, legacy, data)
                    : runtime.regionSpecialization;
                runtime.controlStage = ResolveControlStage(runtime.occupationStatus, runtime.integration, runtime.taxContributionPercent, runtime.controlStage);
                runtime.visibilityState = runtime.visibilityState == VisibilityState.Hidden
                    ? ResolveInitialVisibility(definition, legacy)
                    : runtime.visibilityState;
                runtime.supplyNode = runtime.supplyNode || ResolveSupplyNode(definition, legacy, data);
            }
        }

        public static RegionSpecialization ResolveSpecialization(RegionDefinition definition, RegionState state, IDataRepository data)
        {
            if (definition == null) return RegionSpecialization.Culture;

            int grain = definition.foodOutput;
            int tax = definition.taxOutput;
            int military = definition.manpower;
            int border = 0;
            int legitimacy = 0;
            int culture = 0;
            int capital = 0;

            AddTerrainScores(definition.terrain, ref grain, ref tax, ref military, ref border, ref culture);
            AddMemoryScores(definition.legitimacyMemory, ref military, ref border, ref legitimacy, ref culture, ref capital);
            AddEraScores(definition.eraProfile, ref grain, ref tax, ref military, ref border, ref legitimacy, ref culture, ref capital);
            AddHistoricalLayerScores(FindHistoricalLayer(data, definition.id), ref grain, ref tax, ref military, ref border, ref legitimacy, ref culture, ref capital);

            if (definition.population >= 700000)
            {
                grain += 8;
                tax += 10;
                culture += 4;
            }

            if (definition.landStructure != null)
            {
                if (definition.landStructure.stateLand >= 0.15f) legitimacy += 6;
                if (definition.landStructure.localElites >= 0.35f) culture += 5;
                if (definition.landStructure.smallFarmers >= 0.58f) grain += 7;
            }

            RegionSpecialization strongHistoricalIdentity = ResolveStrongHistoricalIdentity(definition.id);
            if (strongHistoricalIdentity != RegionSpecialization.None)
            {
                return strongHistoricalIdentity;
            }

            RegionSpecialization best = RegionSpecialization.Grain;
            int bestScore = grain;
            ChooseBetter(RegionSpecialization.Military, military, ref best, ref bestScore);
            ChooseBetter(RegionSpecialization.Tax, tax, ref best, ref bestScore);
            ChooseBetter(RegionSpecialization.Border, border, ref best, ref bestScore);
            ChooseBetter(RegionSpecialization.Legitimacy, legitimacy, ref best, ref bestScore);
            ChooseBetter(RegionSpecialization.Culture, culture, ref best, ref bestScore);
            ChooseBetter(RegionSpecialization.Capital, capital, ref best, ref bestScore);
            return best;
        }

        private static RegionSpecialization ResolveStrongHistoricalIdentity(string regionId)
        {
            string id = regionId ?? "";
            if (ContainsAnyToken(id, "hexi", "xiyu", "liaodong", "liangzhou", "longxi", "yongzhou"))
            {
                return RegionSpecialization.Border;
            }
            if (ContainsAnyToken(id, "guanzhong", "chang_an", "xianyang", "luoyang"))
            {
                return RegionSpecialization.Capital;
            }
            if (ContainsAnyToken(id, "jiangnan", "jiangdong", "bashu", "huguang", "shu", "minyue"))
            {
                return RegionSpecialization.Grain;
            }
            if (ContainsAnyToken(id, "zhongyuan", "hedong", "hebei", "huainan", "huaibei"))
            {
                return RegionSpecialization.Tax;
            }
            if (ContainsAnyToken(id, "qilu", "jingzhou", "wuyue", "lingnan"))
            {
                return RegionSpecialization.Culture;
            }
            return RegionSpecialization.None;
        }

        public static bool ResolveSupplyNode(RegionDefinition definition, RegionState state, IDataRepository data)
        {
            RegionSpecialization specialization = ResolveSpecialization(definition, state, data);
            if (specialization == RegionSpecialization.Capital ||
                specialization == RegionSpecialization.Military ||
                specialization == RegionSpecialization.Border)
            {
                return true;
            }

            HistoricalLayerDefinition layer = definition != null ? FindHistoricalLayer(data, definition.id) : null;
            return ContainsAny(layer != null ? layer.geographyTags : null, "corridor", "pass", "frontier") ||
                   ContainsAny(layer != null ? layer.strategicResources : null, "horses", "iron", "labor_pool");
        }

        public static ControlStage ResolveControlStage(RegionState state)
        {
            if (state == null) return ControlStage.Controlled;
            return ResolveControlStage(state.occupationStatus, state.integration, state.taxContributionPercent, state.controlStage);
        }

        public static ControlStage ResolveControlStage(OccupationStatus occupationStatus, int integration, int contribution, ControlStage explicitStage)
        {
            if (explicitStage != ControlStage.Controlled) return explicitStage;
            if (occupationStatus != OccupationStatus.Occupied) return ControlStage.Controlled;
            if (integration <= StrategyCausalRules.OccupiedIntegration && contribution <= StrategyCausalRules.OccupiedContributionPercent)
            {
                return ControlStage.NewlyAttached;
            }
            return ControlStage.MilitaryGoverned;
        }

        public static VisibilityState ResolveInitialVisibility(RegionDefinition definition, RegionState state)
        {
            if (state == null) return VisibilityState.Hidden;
            return VisibilityState.Hidden;
        }

        public static VisibilityState ResolveInitialVisibility(RegionDefinition definition, RegionState state, GameState gameState, string observingFactionId)
        {
            if (state == null || string.IsNullOrEmpty(state.ownerFactionId) || string.IsNullOrEmpty(observingFactionId))
            {
                return VisibilityState.Hidden;
            }

            if (state.ownerFactionId == observingFactionId)
            {
                return VisibilityState.Known;
            }

            if (definition != null && IsAdjacentToFaction(definition, gameState, observingFactionId))
            {
                return VisibilityState.Known;
            }

            return VisibilityState.Hidden;
        }

        private static bool IsAdjacentToFaction(RegionDefinition definition, GameState gameState, string factionId)
        {
            if (definition == null || definition.neighbors == null || gameState == null || string.IsNullOrEmpty(factionId))
            {
                return false;
            }

            for (int i = 0; i < definition.neighbors.Length; i++)
            {
                RegionState neighbor = gameState.FindRegion(definition.neighbors[i]);
                if (neighbor != null && neighbor.ownerFactionId == factionId)
                {
                    return true;
                }
            }

            return false;
        }

        public static int ResolveInitialAcceptance(RegionDefinition definition, RegionState state)
        {
            if (state == null) return 50;
            int value = state.integration - state.rebellionRisk / 2 - state.localPower / 5;
            if (definition != null && ContainsAny(definition.legitimacyMemory, "mandate", "bureaucracy", "ritual"))
            {
                value += 8;
            }
            return DomainMath.Clamp(value, 15, 90);
        }

        public static GovernanceActionForecast BuildGovernanceForecast(GameContext context, RegionDefinition definition, RegionState state, FactionState faction, GovernanceActionKind action)
        {
            GovernanceActionForecast forecast = new GovernanceActionForecast
            {
                action = action,
                actionId = action.ToString(),
                label = FormatActionLabel(action),
                sourceReference = ResolveActionSource(action),
                nextControlStage = state != null ? ResolveControlStage(state) : ControlStage.Controlled
            };

            if (state == null)
            {
                forecast.canApply = false;
                forecast.disabledReason = "missing_region_state";
                return forecast;
            }

            switch (action)
            {
                case GovernanceActionKind.Pacify:
                    forecast.moneyDelta = -PacifyMoneyCost;
                    forecast.foodDelta = -PacifyFoodCost;
                    forecast.integrationDelta = PacifyIntegrationGain;
                    forecast.rebellionRiskDelta = -PacifyRebellionReduction;
                    forecast.localPowerDelta = -PacifyLocalPowerReduction;
                    forecast.localAcceptanceDelta = PacifyAcceptanceGain;
                    forecast.nextControlStage = AdvanceControlStage(state, ControlStage.Pacified);
                    forecast.reason = "reduces unrest before extracting more tax or soldiers";
                    break;
                case GovernanceActionKind.MilitaryGovern:
                    forecast.moneyDelta = -MilitaryGovernMoneyCost;
                    forecast.foodDelta = -MilitaryGovernFoodCost;
                    forecast.legitimacyDelta = -MilitaryGovernLegitimacyCost;
                    forecast.rebellionRiskDelta = -MilitaryGovernRebellionReduction;
                    forecast.localPowerDelta = -3;
                    forecast.localAcceptanceDelta = -MilitaryGovernAcceptanceCost;
                    forecast.nextControlStage = AdvanceControlStage(state, ControlStage.MilitaryGoverned);
                    forecast.reason = "suppression buys time but hurts recognition";
                    break;
                case GovernanceActionKind.RegisterHouseholds:
                    forecast.moneyDelta = -RegisterMoneyCost;
                    forecast.foodDelta = -RegisterFoodCost;
                    forecast.integrationDelta = RegisterIntegrationGain;
                    forecast.localPowerDelta = -7;
                    forecast.localAcceptanceDelta = RegisterAcceptanceGain;
                    forecast.taxContributionPercentDelta = RegisterContributionGain;
                    forecast.foodContributionPercentDelta = RegisterContributionGain;
                    forecast.nextControlStage = AdvanceControlStage(state, ControlStage.Registered);
                    int projectedIntegration = DomainMath.Clamp(state.integration + forecast.integrationDelta, 0, 100);
                    int projectedAcceptance = DomainMath.Clamp(state.localAcceptance + forecast.localAcceptanceDelta, 0, 100);
                    if (forecast.nextControlStage == ControlStage.Registered &&
                        projectedIntegration >= 55 &&
                        projectedAcceptance >= 45)
                    {
                        int projectedTaxContribution = DomainMath.Clamp(state.taxContributionPercent + forecast.taxContributionPercentDelta, 0, 100);
                        int projectedFoodContribution = DomainMath.Clamp(state.foodContributionPercent + forecast.foodContributionPercentDelta, 0, 100);
                        forecast.nextControlStage = ControlStage.Controlled;
                        forecast.taxContributionPercentDelta = DomainMath.Max(projectedTaxContribution, ControlledContributionPercent) - state.taxContributionPercent;
                        forecast.foodContributionPercentDelta = DomainMath.Max(projectedFoodContribution, ControlledContributionPercent) - state.foodContributionPercent;
                    }
                    forecast.reason = "registration turns nominal conquest into taxable households";
                    break;
                case GovernanceActionKind.Relief:
                    forecast.foodDelta = -ReliefFoodCost;
                    forecast.rebellionRiskDelta = -ReliefRebellionReduction;
                    forecast.localAcceptanceDelta = ReliefAcceptanceGain;
                    forecast.reason = "grain relief trades reserves for order and acceptance";
                    break;
                case GovernanceActionKind.TaxPressure:
                    forecast.moneyDelta = TaxPressureMoneyGain;
                    forecast.legitimacyDelta = -TaxPressureLegitimacyCost;
                    forecast.rebellionRiskDelta = TaxPressureRebellionIncrease;
                    forecast.localAcceptanceDelta = -TaxPressureAcceptanceCost;
                    forecast.reason = "extra extraction raises money but damages acceptance";
                    break;
                case GovernanceActionKind.Conscription:
                    int manpowerSpent = DomainMath.Min(state.manpower, StrategyCausalRules.ConscriptionManpowerCost);
                    forecast.populationDelta = -(manpowerSpent * StrategyCausalRules.ConscriptionPopulationCostPerManpower);
                    forecast.manpowerDelta = -manpowerSpent;
                    forecast.rebellionRiskDelta = StrategyCausalRules.ConscriptionRebellionIncrease;
                    forecast.localAcceptanceDelta = -5;
                    forecast.reason = "drafting converts households into troops with demographic cost";
                    break;
                default:
                    forecast.reason = "hold position and avoid new pressure";
                    break;
            }

            forecast.canApply = CanApplyForecast(faction, state, forecast);
            forecast.disabledReason = forecast.canApply ? null : ResolveDisabledReason(faction, state, forecast);
            return forecast;
        }

        public static GovernanceActionForecast BuildRecommendedGovernanceForecast(GameContext context, RegionDefinition definition, RegionState state, FactionState faction)
        {
            GovernanceActionKind action = ResolveRecommendedGovernanceAction(definition, state, faction);
            return BuildGovernanceForecast(context, definition, state, faction, action);
        }

        public static GovernanceActionKind ResolveRecommendedGovernanceAction(RegionDefinition definition, RegionState state, FactionState faction)
        {
            if (state == null) return GovernanceActionKind.Hold;

            ControlStage stage = ResolveControlStage(state);
            if (stage == ControlStage.NewlyAttached) return GovernanceActionKind.MilitaryGovern;
            if (stage == ControlStage.MilitaryGoverned) return GovernanceActionKind.Pacify;
            if (stage == ControlStage.Pacified && state.integration >= 35 && state.rebellionRisk <= 55) return GovernanceActionKind.RegisterHouseholds;
            if (state.rebellionRisk >= 50 || state.integration < 55) return GovernanceActionKind.Pacify;
            if (faction != null && faction.food >= ReliefFoodCost && state.localAcceptance < 45) return GovernanceActionKind.Relief;
            return GovernanceActionKind.Hold;
        }

        public static GovernanceActionForecast ApplyGovernanceAction(GameContext context, RegionDefinition definition, RegionState state, FactionState faction, GovernanceActionKind action)
        {
            GovernanceActionForecast forecast = BuildGovernanceForecast(context, definition, state, faction, action);
            if (!forecast.canApply || state == null) return forecast;

            if (faction != null)
            {
                faction.money = DomainMath.Max(0, faction.money + forecast.moneyDelta);
                faction.food = DomainMath.Max(0, faction.food + forecast.foodDelta);
                faction.legitimacy = DomainMath.Clamp(faction.legitimacy + forecast.legitimacyDelta, 0, 100);
            }

            state.population = DomainMath.Max(0, state.population + forecast.populationDelta);
            state.manpower = DomainMath.Max(0, state.manpower + forecast.manpowerDelta);
            state.integration = DomainMath.Clamp(state.integration + forecast.integrationDelta, 0, 100);
            state.rebellionRisk = DomainMath.Clamp(state.rebellionRisk + forecast.rebellionRiskDelta, 0, 100);
            state.localPower = DomainMath.Clamp(state.localPower + forecast.localPowerDelta, 0, 100);
            state.annexationPressure = DomainMath.Clamp(state.annexationPressure + forecast.annexationPressureDelta, 0, 100);
            state.localAcceptance = DomainMath.Clamp(state.localAcceptance + forecast.localAcceptanceDelta, 0, 100);
            state.taxContributionPercent = DomainMath.Clamp(state.taxContributionPercent + forecast.taxContributionPercentDelta, 0, 100);
            state.foodContributionPercent = DomainMath.Clamp(state.foodContributionPercent + forecast.foodContributionPercentDelta, 0, 100);
            state.controlStage = forecast.nextControlStage;

            if ((state.controlStage == ControlStage.Registered || state.controlStage == ControlStage.Controlled) &&
                state.integration >= 55 &&
                state.localAcceptance >= 45)
            {
                state.controlStage = ControlStage.Controlled;
                state.occupationStatus = OccupationStatus.Controlled;
                state.taxContributionPercent = DomainMath.Max(state.taxContributionPercent, ControlledContributionPercent);
                state.foodContributionPercent = DomainMath.Max(state.foodContributionPercent, ControlledContributionPercent);
            }

            if (context != null && context.State != null)
            {
                context.State.AddLog("governance", state.id + " " + FormatLocalizedActionLabel(forecast.action) + " action " + forecast.actionId + ": " + forecast.FormatCompact());
            }

            return forecast;
        }

        public static CampaignRouteForecast BuildCampaignRouteForecast(MapQueryService queries, GameContext context, ArmyRuntimeState army, string targetRegionId)
        {
            CampaignRouteForecast forecast = new CampaignRouteForecast
            {
                canDispatch = false,
                disabledReason = "missing_route_context",
                route = new string[0],
                visibilityState = VisibilityState.Hidden,
                sourceReference = WarSource
            };

            if (queries == null || context == null || army == null || string.IsNullOrEmpty(targetRegionId))
            {
                return forecast;
            }

            List<string> route = queries.FindRoute(army.locationRegionId, targetRegionId);
            forecast.route = route.ToArray();
            if (route.Count < 2)
            {
                forecast.disabledReason = "no_route";
                return forecast;
            }

            forecast.routeSteps = route.Count - 1;
            forecast.firstTurnSupplyCost = StrategyCausalRules.WarMovementSupplyCost;
            forecast.fullRouteSupplyCost = forecast.routeSteps * StrategyCausalRules.WarMovementSupplyCost;
            forecast.supplyAfterFirstMove = DomainMath.Max(0, army.supply - forecast.firstTurnSupplyCost);
            forecast.supplyAtContact = DomainMath.Max(0, army.supply - forecast.fullRouteSupplyCost);
            forecast.supplyPowerPercent = StrategyCausalRules.CalculateBattleSupplyPowerPercentForSupply(forecast.supplyAtContact);

            RegionState targetState = context.State != null ? context.State.FindRegion(targetRegionId) : null;
            forecast.visibilityState = targetState != null ? targetState.visibilityState : VisibilityState.Hidden;
            forecast.hasUnknownRisk = forecast.visibilityState == VisibilityState.Hidden;
            forecast.contactRisk = CalculateContactRisk(queries, context, army, targetRegionId, forecast.visibilityState);
            forecast.interceptionRisk = CalculateInterceptionRisk(queries, context, army, route);

            bool adjacent = route.Count == 2;
            bool supplyProjection = CanProjectFromSupplyNode(context, queries, army, route);
            forecast.canDispatch = adjacent || supplyProjection;
            forecast.disabledReason = forecast.canDispatch ? null : "dispatch_requires_adjacent_or_supply_node";
            return forecast;
        }

        public static List<StrategyOutlinerEntry> BuildOutliner(GameState state, WorldState world)
        {
            List<StrategyOutlinerEntry> entries = new List<StrategyOutlinerEntry>();
            if (state == null) return entries;

            for (int i = 0; i < state.regions.Count; i++)
            {
                RegionState region = state.regions[i];
                if (region == null) continue;

                ControlStage stage = ResolveControlStage(region);
                if (region.rebellionRisk >= 55 || region.localPower >= 70)
                {
                    entries.Add(CreateOutlinerEntry("critical_region", "risk", "高风险地区", 100, region.id, region.id + " risk " + region.rebellionRisk + "/" + region.localPower, 90));
                }
                if (stage != ControlStage.Controlled)
                {
                    entries.Add(CreateOutlinerEntry("occupation_chain", "occupation", "新占治理", 90, region.id, region.id + " control " + stage, 80));
                }
                if (region.localAcceptance > 0 && region.localAcceptance < 45)
                {
                    entries.Add(CreateOutlinerEntry("acceptance", "risk", "高风险地区", 100, region.id, region.id + " acceptance " + region.localAcceptance, 70));
                }
            }

            if (world != null && world.Map != null)
            {
                foreach (ArmyRuntimeState army in world.Map.ArmiesById.Values)
                {
                    if (army == null) continue;
                    if (army.task != ArmyTask.Idle)
                    {
                        entries.Add(CreateOutlinerEntry("marching_army", "army", "行军军队", 80, army.id, army.id + " " + army.task + " -> " + army.targetRegionId, 75));
                    }
                    if (army.supply < StrategyCausalRules.LowSupplyBattleThreshold)
                    {
                        entries.Add(CreateOutlinerEntry("low_supply", "army", "行军军队", 80, army.id, army.id + " supply " + army.supply, 85));
                    }
                }
            }

            AddRecentReportEntries(entries, state);

            entries.Sort(CompareOutlinerEntries);
            return entries;
        }

        public static int CalculateLensScore(MapLensMode lens, RegionDefinition definition, RegionState state)
        {
            if (state == null) return 0;
            switch (lens)
            {
                case MapLensMode.Risk:
                    return DomainMath.Clamp((state.rebellionRisk + state.localPower + state.annexationPressure) / 3, 0, 100);
                case MapLensMode.Economy:
                    return DomainMath.Clamp((state.foodOutput + state.taxOutput + state.manpower) / 3, 0, 160);
                case MapLensMode.Legitimacy:
                    return DomainMath.Clamp(state.localAcceptance > 0 ? state.localAcceptance : state.integration, 0, 100);
                case MapLensMode.War:
                    return state.supplyNode ? 85 : (state.ownerFactionId != null ? 45 : 20);
                case MapLensMode.Terrain:
                    return ResolveSpecialization(definition, state, null) == RegionSpecialization.Border ? 85 : 45;
                case MapLensMode.Governance:
                default:
                    return ResolveControlStage(state) == ControlStage.Controlled ? state.integration : 100 - state.integration;
            }
        }

        private static int ResolveInitialAcceptance(RegionRuntimeState runtime)
        {
            if (runtime == null) return 50;
            return DomainMath.Clamp(runtime.integration - runtime.rebellionRisk / 2 - runtime.localPower / 5, 15, 90);
        }

        private static ControlStage AdvanceControlStage(RegionState state, ControlStage target)
        {
            ControlStage current = ResolveControlStage(state);
            if (current == ControlStage.Controlled) return target == ControlStage.Registered ? ControlStage.Controlled : target;
            if (target == ControlStage.MilitaryGoverned && current == ControlStage.NewlyAttached) return ControlStage.MilitaryGoverned;
            if (target == ControlStage.Pacified && (current == ControlStage.NewlyAttached || current == ControlStage.MilitaryGoverned)) return ControlStage.Pacified;
            if (target == ControlStage.Registered && (current == ControlStage.Pacified || current == ControlStage.MilitaryGoverned)) return ControlStage.Registered;
            return target;
        }

        private static bool CanApplyForecast(FactionState faction, RegionState state, GovernanceActionForecast forecast)
        {
            if (forecast == null || state == null) return false;
            if (faction == null && (forecast.moneyDelta < 0 || forecast.foodDelta < 0 || forecast.legitimacyDelta < 0)) return false;
            if (faction != null)
            {
                if (forecast.moneyDelta < 0 && faction.money < -forecast.moneyDelta) return false;
                if (forecast.foodDelta < 0 && faction.food < -forecast.foodDelta) return false;
                if (forecast.legitimacyDelta < 0 && faction.legitimacy < -forecast.legitimacyDelta) return false;
            }
            if (forecast.action == GovernanceActionKind.RegisterHouseholds)
            {
                ControlStage stage = ResolveControlStage(state);
                return stage == ControlStage.Pacified || stage == ControlStage.MilitaryGoverned || stage == ControlStage.Registered;
            }
            return true;
        }

        private static string ResolveDisabledReason(FactionState faction, RegionState state, GovernanceActionForecast forecast)
        {
            if (forecast == null) return "missing_forecast";
            if (faction == null) return "missing_faction";
            if (forecast.moneyDelta < 0 && faction.money < -forecast.moneyDelta) return "not_enough_money";
            if (forecast.foodDelta < 0 && faction.food < -forecast.foodDelta) return "not_enough_food";
            if (forecast.legitimacyDelta < 0 && faction.legitimacy < -forecast.legitimacyDelta) return "not_enough_legitimacy";
            if (forecast.action == GovernanceActionKind.RegisterHouseholds) return "control_chain_not_ready";
            return "not_available";
        }

        private static string FormatActionLabel(GovernanceActionKind action)
        {
            switch (action)
            {
                case GovernanceActionKind.Pacify: return "Pacify";
                case GovernanceActionKind.MilitaryGovern: return "Military govern";
                case GovernanceActionKind.RegisterHouseholds: return "Register households";
                case GovernanceActionKind.Relief: return "Grain relief";
                case GovernanceActionKind.TaxPressure: return "Emergency tax";
                case GovernanceActionKind.Conscription: return "Conscription";
                default: return "Hold";
            }
        }

        private static string FormatLocalizedActionLabel(GovernanceActionKind action)
        {
            switch (action)
            {
                case GovernanceActionKind.Pacify: return "安抚";
                case GovernanceActionKind.MilitaryGovern: return "军管";
                case GovernanceActionKind.RegisterHouseholds: return "编户";
                case GovernanceActionKind.Relief: return "赈济";
                case GovernanceActionKind.TaxPressure: return "急征";
                case GovernanceActionKind.Conscription: return "征兵";
                default: return "维持";
            }
        }

        private static string ResolveActionSource(GovernanceActionKind action)
        {
            if (action == GovernanceActionKind.Relief) return FoodSource;
            if (action == GovernanceActionKind.MilitaryGovern || action == GovernanceActionKind.RegisterHouseholds) return GovernanceSource + " Occupation chain: military rule -> pacification -> registration.";
            return GovernanceSource;
        }

        private static bool CanProjectFromSupplyNode(GameContext context, MapQueryService queries, ArmyRuntimeState army, List<string> route)
        {
            if (context == null || context.State == null || army == null || route == null || route.Count < 2) return false;
            if (route.Count > 4) return false;

            RegionState source = context.State.FindRegion(army.locationRegionId);
            if (source != null && source.ownerFactionId == army.ownerFactionId && source.supplyNode) return true;

            for (int i = 0; i < route.Count - 1; i++)
            {
                RegionState step = context.State.FindRegion(route[i]);
                if (step != null && step.ownerFactionId == army.ownerFactionId && step.supplyNode)
                {
                    return true;
                }
            }
            return false;
        }

        private static int CalculateContactRisk(MapQueryService queries, GameContext context, ArmyRuntimeState army, string targetRegionId, VisibilityState visibility)
        {
            if (visibility == VisibilityState.Hidden) return 35;
            List<ArmyRuntimeState> hostile = queries.GetHostileArmies(targetRegionId, army.ownerFactionId);
            int risk = hostile.Count * 30;
            RegionState target = context.State != null ? context.State.FindRegion(targetRegionId) : null;
            if (target != null)
            {
                risk += target.localPower / 3 + target.rebellionRisk / 4;
            }
            return DomainMath.Clamp(risk, 0, 100);
        }

        private static int CalculateInterceptionRisk(MapQueryService queries, GameContext context, ArmyRuntimeState army, List<string> route)
        {
            if (queries == null || context == null || context.State == null || army == null || route == null) return 0;
            int risk = 0;
            for (int i = 1; i < route.Count; i++)
            {
                foreach (string neighborId in queries.GetNeighborRegions(route[i]))
                {
                    RegionState neighbor = context.State.FindRegion(neighborId);
                    if (neighbor != null && neighbor.ownerFactionId != army.ownerFactionId)
                    {
                        risk += 8;
                    }
                    List<ArmyRuntimeState> hostile = queries.GetHostileArmies(neighborId, army.ownerFactionId);
                    risk += hostile.Count * 18;
                }
            }
            return DomainMath.Clamp(risk, 0, 100);
        }

        private static int CompareOutlinerEntries(StrategyOutlinerEntry a, StrategyOutlinerEntry b)
        {
            int ag = a != null ? a.groupPriority : 0;
            int bg = b != null ? b.groupPriority : 0;
            int groupCompare = bg.CompareTo(ag);
            if (groupCompare != 0) return groupCompare;

            int ap = a != null ? a.priority : 0;
            int bp = b != null ? b.priority : 0;
            int priorityCompare = bp.CompareTo(ap);
            if (priorityCompare != 0) return priorityCompare;

            string al = a != null ? a.label : "";
            string bl = b != null ? b.label : "";
            return string.Compare(al, bl, StringComparison.Ordinal);
        }

        private static StrategyOutlinerEntry CreateOutlinerEntry(string category, string groupId, string groupLabel, int groupPriority, string targetId, string label, int priority)
        {
            return new StrategyOutlinerEntry
            {
                category = category,
                groupId = groupId,
                groupLabel = groupLabel,
                groupPriority = groupPriority,
                targetId = targetId,
                label = label,
                priority = priority
            };
        }

        private static void AddRecentReportEntries(List<StrategyOutlinerEntry> entries, GameState state)
        {
            if (entries == null || state == null || state.turnLog == null || state.turnLog.Count == 0) return;

            int added = 0;
            for (int i = state.turnLog.Count - 1; i >= 0 && added < 2; i--)
            {
                TurnLogEntry log = state.turnLog[i];
                if (log == null || string.IsNullOrEmpty(log.message)) continue;
                if (!IsOutlinerReportCategory(log.category)) continue;

                string targetId = ResolveFirstRegionMention(state, log.message);
                if (string.IsNullOrEmpty(targetId)) continue;

                string label = "[" + log.category + "] " + CompactOutlinerLabel(log.message, 34);
                entries.Add(CreateOutlinerEntry("latest_report", "report", "最新战报", 60, targetId, label, 50 - added));
                added++;
            }
        }

        private static bool IsOutlinerReportCategory(string category)
        {
            return category == "war" || category == "rebellion" || category == "event" || category == "diplomacy";
        }

        private static string ResolveFirstRegionMention(GameState state, string message)
        {
            if (state == null || state.regions == null || string.IsNullOrEmpty(message)) return null;
            for (int i = 0; i < state.regions.Count; i++)
            {
                RegionState region = state.regions[i];
                if (region != null && !string.IsNullOrEmpty(region.id) && message.Contains(region.id))
                {
                    return region.id;
                }
            }

            return null;
        }

        private static string CompactOutlinerLabel(string label, int maxLength)
        {
            if (string.IsNullOrEmpty(label) || maxLength <= 0) return "";
            if (label.Length <= maxLength) return label;
            if (maxLength <= 3) return label.Substring(0, maxLength);
            return label.Substring(0, maxLength - 3) + "...";
        }

        private static void ChooseBetter(RegionSpecialization candidate, int score, ref RegionSpecialization best, ref int bestScore)
        {
            if (score <= bestScore) return;
            best = candidate;
            bestScore = score;
        }

        private static void AddTerrainScores(string terrain, ref int grain, ref int tax, ref int military, ref int border, ref int culture)
        {
            string value = terrain ?? "";
            if (value.Contains("plain") || value.Contains("delta")) grain += 18;
            if (value.Contains("river")) { grain += 10; tax += 6; }
            if (value.Contains("mountain") || value.Contains("pass") || value.Contains("hill")) { military += 10; border += 12; }
            if (value.Contains("frontier") || value.Contains("steppe")) { border += 18; military += 10; }
            if (value.Contains("coast")) { tax += 12; culture += 5; }
        }

        private static void AddMemoryScores(string[] memory, ref int military, ref int border, ref int legitimacy, ref int culture, ref int capital)
        {
            if (memory == null) return;
            for (int i = 0; i < memory.Length; i++)
            {
                string value = memory[i] ?? "";
                if (value.Contains("military") || value.Contains("law")) military += 12;
                if (value.Contains("frontier")) border += 15;
                if (value.Contains("mandate") || value.Contains("bureaucracy")) legitimacy += 14;
                if (value.Contains("culture") || value.Contains("ritual")) culture += 12;
                if (value.Contains("capital") || value.Contains("mandate")) capital += 10;
            }
        }

        private static void AddEraScores(EraProfile era, ref int grain, ref int tax, ref int military, ref int border, ref int legitimacy, ref int culture, ref int capital)
        {
            if (era == null) return;
            string text = (era.classical ?? "") + " " + (era.medieval ?? "") + " " + (era.early_modern ?? "");
            if (text.Contains("grain")) grain += 14;
            if (text.Contains("tax") || text.Contains("trade")) tax += 14;
            if (text.Contains("frontier") || text.Contains("gate")) border += 16;
            if (text.Contains("military") || text.Contains("strategic")) military += 10;
            if (text.Contains("imperial") || text.Contains("core")) { legitimacy += 10; capital += 12; }
            if (text.Contains("culture")) culture += 10;
        }

        private static void AddHistoricalLayerScores(HistoricalLayerDefinition layer, ref int grain, ref int tax, ref int military, ref int border, ref int legitimacy, ref int culture, ref int capital)
        {
            if (layer == null) return;
            if (layer.yieldModifiers != null)
            {
                grain += layer.yieldModifiers.food;
                tax += layer.yieldModifiers.tax;
                military += layer.yieldModifiers.manpower;
                border += layer.yieldModifiers.mobility;
                legitimacy += layer.yieldModifiers.legitimacy;
            }
            if (ContainsAny(layer.geographyTags, "capital", "corridor")) capital += 10;
            if (ContainsAny(layer.geographyTags, "frontier", "pass", "corridor")) border += 14;
            if (ContainsAny(layer.strategicResources, "grain", "rice", "labor_pool")) grain += 12;
            if (ContainsAny(layer.strategicResources, "horses", "iron", "bronze")) military += 12;
            if (ContainsAny(layer.customTags, "ritual", "scholar", "confucian")) culture += 10;
        }

        private static HistoricalLayerDefinition FindHistoricalLayer(IDataRepository data, string regionId)
        {
            if (data == null || string.IsNullOrEmpty(regionId)) return null;
            foreach (HistoricalLayerDefinition layer in data.HistoricalLayers.Values)
            {
                if (layer != null && layer.regionId == regionId) return layer;
            }
            return null;
        }

        private static bool ContainsAny(string[] values, params string[] tokens)
        {
            if (values == null || tokens == null) return false;
            for (int i = 0; i < values.Length; i++)
            {
                string value = values[i] ?? "";
                for (int j = 0; j < tokens.Length; j++)
                {
                    if (value.Contains(tokens[j])) return true;
                }
            }
            return false;
        }

        private static bool ContainsAnyToken(string value, params string[] tokens)
        {
            if (string.IsNullOrEmpty(value) || tokens == null) return false;
            for (int i = 0; i < tokens.Length; i++)
            {
                if (!string.IsNullOrEmpty(tokens[i]) && value.Contains(tokens[i])) return true;
            }
            return false;
        }
    }
}
