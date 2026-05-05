using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WanChaoGuiYi
{
    public sealed class MainMapUI : MonoBehaviour
    {
        [Header("Core References")]
        [SerializeField] private GameManager gameManager;

        [Header("Panels")]
        [SerializeField] private RegionPanel regionPanel;
        [SerializeField] private EmperorPanel emperorPanel;
        [SerializeField] private CourtPanel courtPanel;
        [SerializeField] private EventPanel eventPanel;
        [SerializeField] private BattleReportPanel battleReportPanel;
        [SerializeField] private TechPanel techPanel;
        [SerializeField] private WeatherPanel weatherPanel;
        [SerializeField] private MechanismPanel mechanismPanel;

        [Header("HUD")]
        [SerializeField] private Text turnText;
        [SerializeField] private Text resourceText;
        [SerializeField] private Text selectionContextText;
        [SerializeField] private Text modeStateText;
        [SerializeField] private Button governanceModeButton;
        [SerializeField] private Button warModeButton;
        [SerializeField] private Button nextTurnButton;
        [SerializeField] private Button courtButton;
        [SerializeField] private Button emperorButton;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button techButton;
        [SerializeField] private Button weatherButton;
        [SerializeField] private Button eventButton;
        [SerializeField] private Button mechanismButton;

        private string selectedRegionId;
        private SelectionContext selectionContext = new SelectionContext();
        private MapInteractionMode currentMode = MapInteractionMode.Governance;
        private MapLensMode currentLens = MapLensMode.Governance;
        private static readonly Vector2 StrategyOutlinerDockedPosition = new Vector2(-12, -108);
        private static readonly Vector2 StrategyOutlinerRegionPanelAvoidPosition = new Vector2(-600, -108);
        private GameObject strategyLensBar;
        private GameObject strategyOutlinerRoot;
        private GameObject strategyOutlinerCollapsedRoot;
        private Text strategyLensStateText;
        private Text strategyOutlinerText;
        private readonly List<GameObject> strategyOutlinerEntryRoots = new List<GameObject>();
        private bool strategyOutlinerCollapsed;
        private bool subscribed;

        public MapInteractionMode CurrentMode { get { return currentMode; } }
        public MapLensMode CurrentLens { get { return currentLens; } }
        public SelectionContext CurrentSelectionContext { get { return selectionContext; } }
        public string SelectedRegionId { get { return selectedRegionId; } }

        public void Bind(GameManager manager, RegionPanel region, EmperorPanel emperor, CourtPanel court, EventPanel eventsPanel, BattleReportPanel battlePanel, TechPanel tech, WeatherPanel weather, MechanismPanel mechanism, Text turn, Text resources, Text selectionText, Text modeText, Button governanceModeButtonRef, Button warModeButtonRef, Button nextTurn, Button courtButtonRef, Button emperorButtonRef, Button attackButtonRef, Button techButtonRef, Button weatherButtonRef, Button eventButtonRef, Button mechanismButtonRef)
        {
            UnsubscribeBindings();

            gameManager = manager;
            regionPanel = region;
            emperorPanel = emperor;
            courtPanel = court;
            eventPanel = eventsPanel;
            battleReportPanel = battlePanel;
            techPanel = tech;
            weatherPanel = weather;
            mechanismPanel = mechanism;
            turnText = turn;
            resourceText = resources;
            selectionContextText = selectionText;
            modeStateText = modeText;
            governanceModeButton = governanceModeButtonRef;
            warModeButton = warModeButtonRef;
            nextTurnButton = nextTurn;
            courtButton = courtButtonRef;
            emperorButton = emperorButtonRef;
            attackButton = attackButtonRef;
            techButton = techButtonRef;
            weatherButton = weatherButtonRef;
            eventButton = eventButtonRef;
            mechanismButton = mechanismButtonRef;

            if (eventPanel != null && gameManager != null)
            {
                eventPanel.Initialize(gameManager.Context);
            }

            selectedRegionId = null;
            currentMode = MapInteractionMode.Governance;
            selectionContext = CreateEmptySelectionContext();

            if (isActiveAndEnabled)
            {
                SubscribeBindings();
            }
        }

        public void ActivateBindings()
        {
            SubscribeBindings();
            EnsureStrategyControls();
            RefreshHUD();
        }

        private void OnEnable()
        {
            SubscribeBindings();
        }

        private void OnDisable()
        {
            UnsubscribeBindings();
        }

        private void SubscribeBindings()
        {
            if (subscribed || gameManager == null || gameManager.Events == null) return;

            gameManager.Events.Subscribe(GameEventType.RegionSelected, OnRegionSelected);
            gameManager.Events.Subscribe(GameEventType.TurnEnded, OnTurnEnded);
            gameManager.Events.Subscribe(GameEventType.BattleResolved, OnBattleResolved);
            gameManager.Events.Subscribe(GameEventType.RegionOccupied, OnRegionOccupied);
            gameManager.Events.Subscribe(GameEventType.GovernanceImpactApplied, OnGovernanceImpactApplied);
            gameManager.Events.Subscribe(GameEventType.EventTriggered, OnEventTriggered);
            gameManager.Events.Subscribe(GameEventType.SuccessionResolved, OnSuccessionResolved);
            gameManager.Events.Subscribe(GameEventType.TechResearched, OnMechanismChanged);
            gameManager.Events.Subscribe(GameEventType.WeatherChanged, OnMechanismChanged);
            gameManager.Events.Subscribe(GameEventType.CelestialEventOccurred, OnMechanismChanged);
            gameManager.Events.Subscribe(GameEventType.PolicyApplied, OnMechanismChanged);
            gameManager.Events.Subscribe(GameEventType.EmperorSkillUsed, OnMechanismChanged);
            gameManager.Events.Subscribe(GameEventType.DiplomacyWarDeclared, OnMechanismChanged);
            gameManager.Events.Subscribe(GameEventType.DiplomacyProposalAccepted, OnMechanismChanged);
            gameManager.Events.Subscribe(GameEventType.DiplomacyProposalRejected, OnMechanismChanged);
            gameManager.Events.Subscribe(GameEventType.DiplomacyTreatyBroken, OnMechanismChanged);
            gameManager.Events.Subscribe(GameEventType.EspionageOperationStarted, OnMechanismChanged);
            gameManager.Events.Subscribe(GameEventType.EspionageOperationCompleted, OnMechanismChanged);
            gameManager.Events.Subscribe(GameEventType.EspionageAgentCaught, OnMechanismChanged);
            gameManager.Events.Subscribe(GameEventType.VictoryChecked, OnMechanismChanged);

            if (nextTurnButton != null) nextTurnButton.onClick.AddListener(AdvanceTurn);
            if (courtButton != null) courtButton.onClick.AddListener(OpenCourtPanel);
            if (emperorButton != null) emperorButton.onClick.AddListener(OpenEmperorPanel);
            if (governanceModeButton != null) governanceModeButton.onClick.AddListener(EnterGovernanceMode);
            if (warModeButton != null) warModeButton.onClick.AddListener(EnterWarModeButtonPressed);
            if (attackButton != null) attackButton.onClick.AddListener(AttackSelectedRegion);
            if (techButton != null) techButton.onClick.AddListener(OpenTechPanel);
            if (weatherButton != null) weatherButton.onClick.AddListener(OpenWeatherPanel);
            if (eventButton != null) eventButton.onClick.AddListener(OpenLatestEventPanel);
            if (mechanismButton != null) mechanismButton.onClick.AddListener(OpenMechanismPanel);
            subscribed = true;
        }

        private void UnsubscribeBindings()
        {
            if (!subscribed) return;

            if (gameManager != null && gameManager.Events != null)
            {
                gameManager.Events.Unsubscribe(GameEventType.RegionSelected, OnRegionSelected);
                gameManager.Events.Unsubscribe(GameEventType.TurnEnded, OnTurnEnded);
                gameManager.Events.Unsubscribe(GameEventType.BattleResolved, OnBattleResolved);
                gameManager.Events.Unsubscribe(GameEventType.RegionOccupied, OnRegionOccupied);
                gameManager.Events.Unsubscribe(GameEventType.GovernanceImpactApplied, OnGovernanceImpactApplied);
                gameManager.Events.Unsubscribe(GameEventType.EventTriggered, OnEventTriggered);
                gameManager.Events.Unsubscribe(GameEventType.SuccessionResolved, OnSuccessionResolved);
                gameManager.Events.Unsubscribe(GameEventType.TechResearched, OnMechanismChanged);
                gameManager.Events.Unsubscribe(GameEventType.WeatherChanged, OnMechanismChanged);
                gameManager.Events.Unsubscribe(GameEventType.CelestialEventOccurred, OnMechanismChanged);
                gameManager.Events.Unsubscribe(GameEventType.PolicyApplied, OnMechanismChanged);
                gameManager.Events.Unsubscribe(GameEventType.EmperorSkillUsed, OnMechanismChanged);
                gameManager.Events.Unsubscribe(GameEventType.DiplomacyWarDeclared, OnMechanismChanged);
                gameManager.Events.Unsubscribe(GameEventType.DiplomacyProposalAccepted, OnMechanismChanged);
                gameManager.Events.Unsubscribe(GameEventType.DiplomacyProposalRejected, OnMechanismChanged);
                gameManager.Events.Unsubscribe(GameEventType.DiplomacyTreatyBroken, OnMechanismChanged);
                gameManager.Events.Unsubscribe(GameEventType.EspionageOperationStarted, OnMechanismChanged);
                gameManager.Events.Unsubscribe(GameEventType.EspionageOperationCompleted, OnMechanismChanged);
                gameManager.Events.Unsubscribe(GameEventType.EspionageAgentCaught, OnMechanismChanged);
                gameManager.Events.Unsubscribe(GameEventType.VictoryChecked, OnMechanismChanged);
            }

            if (nextTurnButton != null) nextTurnButton.onClick.RemoveListener(AdvanceTurn);
            if (courtButton != null) courtButton.onClick.RemoveListener(OpenCourtPanel);
            if (emperorButton != null) emperorButton.onClick.RemoveListener(OpenEmperorPanel);
            if (governanceModeButton != null) governanceModeButton.onClick.RemoveListener(EnterGovernanceMode);
            if (warModeButton != null) warModeButton.onClick.RemoveListener(EnterWarModeButtonPressed);
            if (attackButton != null) attackButton.onClick.RemoveListener(AttackSelectedRegion);
            if (techButton != null) techButton.onClick.RemoveListener(OpenTechPanel);
            if (weatherButton != null) weatherButton.onClick.RemoveListener(OpenWeatherPanel);
            if (eventButton != null) eventButton.onClick.RemoveListener(OpenLatestEventPanel);
            if (mechanismButton != null) mechanismButton.onClick.RemoveListener(OpenMechanismPanel);
            subscribed = false;
        }

        private void Start()
        {
            RefreshHUD();
        }

        public void AdvanceTurn()
        {
            if (gameManager == null) return;

            // 先关闭可能打开的面板
            if (regionPanel != null) regionPanel.Hide();

            gameManager.NextTurn();
            RefreshHUD();
        }

        private void OnRegionSelected(GameEvent gameEvent)
        {
            if (regionPanel == null || gameManager == null || gameManager.State == null) return;

            string regionId = gameEvent.EntityId;
            ApplySelectionContext(regionId);
            RegionDefinition definition = gameManager.Data.GetRegion(regionId);
            RegionState state = gameManager.State.FindRegion(regionId);
            FactionState owner = null;
            FactionState buildFaction = null;
            if (state != null)
            {
                owner = gameManager.State.FindFaction(state.ownerFactionId);
                if (state.ownerFactionId == gameManager.State.playerFactionId)
                {
                    buildFaction = owner;
                }
            }

            regionPanel.Show(definition, state, gameManager.Context, buildFaction, gameManager.GetComponent<BuildingSystem>());
            regionPanel.SetMode(selectionContext.mode, selectionContext.isFriendly, selectionContext.modeEntryReason, BuildWarPreDecisionForecast());

            // 设置归属名称
            if (state != null)
            {
                regionPanel.SetOwner(owner != null ? owner.name : "未知");
            }

            RefreshHUD();
        }

        public void SetInteractionMode(MapInteractionMode mode)
        {
            RebuildSelectionContextForMode(mode);
        }

        private void EnterGovernanceMode()
        {
            RebuildSelectionContextForMode(MapInteractionMode.Governance);
        }

        private void EnterWarModeButtonPressed()
        {
            RebuildSelectionContextForMode(MapInteractionMode.War);
        }

        private SelectionContext RebuildSelectionContextForMode(MapInteractionMode mode)
        {
            currentMode = mode;
            if (!string.IsNullOrEmpty(selectedRegionId))
            {
                selectionContext = BuildSelectionContext(selectedRegionId);
                currentMode = selectionContext.mode;
            }
            else
            {
                selectionContext = CreateEmptySelectionContext();
                selectionContext.mode = mode;
                currentMode = mode;
            }

            UpdateMechanismPanelSelectionContext();
            UpdateRegionPanelMode();
            RefreshHUD();
            return selectionContext;
        }

        private void ApplySelectionContext(string regionId)
        {
            selectedRegionId = regionId;
            selectionContext = BuildSelectionContext(regionId);
            currentMode = selectionContext.mode;
            UpdateMechanismPanelSelectionContext();
            UpdateRegionPanelMode();
        }

        private SelectionContext BuildSelectionContext(string regionId)
        {
            SelectionContext context = CreateEmptySelectionContext();
            context.selectedRegionId = regionId;
            context.mode = currentMode;

            if (gameManager == null || gameManager.State == null)
            {
                context.modeEntryReason = "missing_state";
                context.disabledReasons = new[] { "missing_state" };
                return context;
            }

            RegionState state = gameManager.State.FindRegion(regionId);
            if (state == null)
            {
                context.modeEntryReason = "region_not_found";
                context.disabledReasons = new[] { "region_not_found" };
                return context;
            }

            FactionState owner = gameManager.State.FindFaction(state.ownerFactionId);
            context.ownerFactionId = owner != null ? owner.id : state.ownerFactionId;
            context.targetFactionId = context.ownerFactionId;
            context.isFriendly = context.ownerFactionId == gameManager.State.playerFactionId;
            context.isNeighbor = IsNeighborToPlayerTerritory(regionId);
            context.isHostile = !context.isFriendly;

            if (context.isFriendly)
            {
                context.mode = MapInteractionMode.Governance;
                context.modeEntryReason = "friendly_region";
                context.availableActions = new[] { "open_governance", "inspect_policies", "inspect_buildings" };
                context.disabledReasons = new[] { "attack_requires_war_mode" };
                return context;
            }

            if (currentMode == MapInteractionMode.War)
            {
                context.mode = MapInteractionMode.War;
                context.modeEntryReason = context.isNeighbor ? "war_neighbor_target" : "war_foreign_target";
                context.selectedArmyId = ResolvePlayerOperableArmyId(regionId, !context.isNeighbor);
                bool canDispatch = !string.IsNullOrEmpty(context.selectedArmyId);
                context.availableActions = canDispatch
                    ? new[] { "dispatch_attack", "inspect_diplomacy", "inspect_border" }
                    : new[] { "inspect_diplomacy", "inspect_border" };
                context.disabledReasons = canDispatch
                    ? new[] { "dispatch_requires_route_review" }
                    : ResolveWarDisabledReasons(context.isNeighbor, regionId);
                return context;
            }

            context.mode = MapInteractionMode.Diplomacy;
            context.modeEntryReason = context.isNeighbor ? "neighbor_region" : "distant_foreign_region";
            context.availableActions = context.isNeighbor
                ? new[] { "open_diplomacy", "enter_war_mode", "inspect_border" }
                : new[] { "open_diplomacy", "inspect_border" };
            context.disabledReasons = context.isNeighbor
                ? new[] { "attack_requires_explicit_war_mode" }
                : new[] { "dispatch_requires_adjacent_target", "attack_requires_explicit_war_mode" };
            return context;
        }

        private static SelectionContext CreateEmptySelectionContext()
        {
            return new SelectionContext
            {
                mode = MapInteractionMode.Governance,
                availableActions = new string[0],
                disabledReasons = new string[0]
            };
        }

        private bool IsNeighborToPlayerTerritory(string regionId)
        {
            if (gameManager == null || gameManager.State == null || gameManager.Data == null || string.IsNullOrEmpty(regionId))
            {
                return false;
            }

            RegionState region = gameManager.State.FindRegion(regionId);
            if (region == null) return false;

            RegionDefinition definition = gameManager.Data.GetRegion(regionId);
            if (definition == null || definition.neighbors == null) return false;

            for (int i = 0; i < definition.neighbors.Length; i++)
            {
                RegionState neighbor = gameManager.State.FindRegion(definition.neighbors[i]);
                if (neighbor != null && neighbor.ownerFactionId == gameManager.State.playerFactionId)
                {
                    return true;
                }
            }

            return false;
        }

        private string ResolvePlayerOperableArmyId(string targetRegionId)
        {
            return ResolvePlayerOperableArmyId(targetRegionId, false);
        }

        private string ResolvePlayerOperableArmyId(string targetRegionId, bool requireScoutedSupplyProjection)
        {
            if (gameManager == null || gameManager.World == null || gameManager.World.Map == null || gameManager.MapQueries == null || gameManager.State == null)
            {
                return null;
            }

            foreach (ArmyRuntimeState army in gameManager.World.Map.ArmiesById.Values)
            {
                if (army == null || army.ownerFactionId != gameManager.State.playerFactionId || army.task != ArmyTask.Idle) continue;
                if (army.locationRegionId == targetRegionId) continue;
                if (!IsPlayerFrontlineAttackArmy(army, targetRegionId, requireScoutedSupplyProjection)) continue;
                if (!gameManager.MapQueries.HasRoute(army.locationRegionId, targetRegionId)) continue;
                return army.id;
            }

            return null;
        }

        private bool IsPlayerFrontlineAttackArmy(ArmyRuntimeState army, string targetRegionId)
        {
            return IsPlayerFrontlineAttackArmy(army, targetRegionId, false);
        }

        private bool IsPlayerFrontlineAttackArmy(ArmyRuntimeState army, string targetRegionId, bool requireScoutedSupplyProjection)
        {
            if (army == null || gameManager == null || gameManager.State == null || gameManager.MapQueries == null || string.IsNullOrEmpty(targetRegionId))
            {
                return false;
            }

            if (army.ownerFactionId != gameManager.State.playerFactionId) return false;

            RegionState stagingRegion = gameManager.State.FindRegion(army.locationRegionId);
            if (stagingRegion == null || stagingRegion.ownerFactionId != gameManager.State.playerFactionId) return false;
            if (gameManager.MapQueries.AreNeighbors(army.locationRegionId, targetRegionId)) return true;

            CampaignRouteForecast forecast = StrategyMapRulebook.BuildCampaignRouteForecast(gameManager.MapQueries, gameManager.Context, army, targetRegionId);
            if (requireScoutedSupplyProjection && forecast != null && forecast.canDispatch && forecast.visibilityState != VisibilityState.Scouted)
            {
                return false;
            }

            return forecast != null && forecast.canDispatch;
        }

        private string[] ResolveWarDisabledReasons(bool isNeighbor, string regionId)
        {
            if (isNeighbor)
            {
                return new[] { "dispatch_requires_operable_army", "attack_requires_war_mode" };
            }

            if (HasUnscoutedSupplyProjection(regionId))
            {
                return new[] { "dispatch_requires_adjacent_target", "dispatch_requires_scouted_supply_projection", "attack_requires_war_mode" };
            }

            return new[] { "dispatch_requires_adjacent_target", "attack_requires_war_mode" };
        }

        private bool HasUnscoutedSupplyProjection(string regionId)
        {
            if (gameManager == null || gameManager.World == null || gameManager.World.Map == null || gameManager.State == null || gameManager.MapQueries == null) return false;

            foreach (ArmyRuntimeState army in gameManager.World.Map.ArmiesById.Values)
            {
                if (army == null || army.ownerFactionId != gameManager.State.playerFactionId || army.task != ArmyTask.Idle) continue;
                if (army.locationRegionId == regionId) continue;
                CampaignRouteForecast forecast = StrategyMapRulebook.BuildCampaignRouteForecast(gameManager.MapQueries, gameManager.Context, army, regionId);
                if (forecast != null && forecast.canDispatch && forecast.visibilityState != VisibilityState.Scouted)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnTurnEnded(GameEvent gameEvent)
        {
            RefreshSelectionContextFromState();
            RefreshHUD();
        }

        private void OnBattleResolved(GameEvent gameEvent)
        {
            if (battleReportPanel == null || gameManager == null) return;

            BattleResult result = gameEvent.Payload as BattleResult;
            if (result == null) return;

            // 查找军队所属势力名称
            ArmyState attacker = FindArmy(result.attackerArmyId);
            ArmyState defender = FindArmy(result.defenderArmyId);
            string attackerName = GetFactionName(attacker != null ? attacker.ownerFactionId : null);
            string defenderName = GetFactionName(defender != null ? defender.ownerFactionId : null);
            string battleRegionId = result.battleRegionId;
            EngagementRuntimeState engagement = FindEngagement(gameEvent.EntityId);
            if (string.IsNullOrEmpty(battleRegionId) && engagement != null)
            {
                battleRegionId = engagement.regionId;
            }

            string regionName = !string.IsNullOrEmpty(battleRegionId)
                ? GetRegionName(battleRegionId)
                : (defender != null ? GetRegionName(defender.regionId) : "");

            battleReportPanel.Show(result, attackerName, defenderName, regionName);
        }

        private void OnRegionOccupied(GameEvent gameEvent)
        {
            if (battleReportPanel == null || gameManager == null) return;

            RegionOccupiedPayload payload = gameEvent.Payload as RegionOccupiedPayload;
            if (payload == null) return;

            battleReportPanel.AppendOccupation(
                payload,
                GetFactionName(payload.previousOwnerFactionId),
                GetFactionName(payload.newOwnerFactionId),
                GetRegionName(payload.regionId)
            );
            RefreshSelectionContextFromState();
            RefreshHUD();
        }

        private void OnGovernanceImpactApplied(GameEvent gameEvent)
        {
            if (battleReportPanel == null) return;

            GovernanceImpactPayload payload = gameEvent.Payload as GovernanceImpactPayload;
            if (payload == null) return;

            battleReportPanel.AppendGovernanceImpact(payload);
            RefreshSelectionContextFromState();
            RefreshHUD();
        }

        private void OnEventTriggered(GameEvent gameEvent)
        {
            if (eventPanel == null || gameManager == null || gameManager.State == null) return;

            EventDefinition eventDefinition = gameEvent.Payload as EventDefinition;
            if (eventDefinition == null) return;

            eventPanel.Show(eventDefinition, gameManager.State.playerFactionId);
        }

        private void OnMechanismChanged(GameEvent gameEvent)
        {
            RefreshSelectionContextFromState();
            RefreshHUD();
        }

        private void OnSuccessionResolved(GameEvent gameEvent)
        {
            RefreshHUD();
            if (gameManager != null && gameManager.State != null)
            {
                FactionState playerFaction = gameManager.State.FindFaction(gameManager.State.playerFactionId);
                if (playerFaction != null)
                {
                    gameManager.State.AddLog("succession", playerFaction.name + "继承完成。");
                }
            }
        }

        private void OpenCourtPanel()
        {
            if (courtPanel == null || gameManager == null || gameManager.State == null) return;

            FactionState playerFaction = gameManager.State.FindFaction(gameManager.State.playerFactionId);
            if (playerFaction == null) return;

            EmperorDefinition emperor = gameManager.Data.GetEmperor(playerFaction.emperorId);
            string emperorName = emperor != null ? emperor.name : "未知";

            courtPanel.Show(gameManager.Context, playerFaction, emperorName, gameManager.State.turnLog, gameManager.GetComponent<EquipmentSystem>());
        }

        private void OpenEmperorPanel()
        {
            if (emperorPanel == null || gameManager == null || gameManager.State == null) return;

            FactionState playerFaction = gameManager.State.FindFaction(gameManager.State.playerFactionId);
            if (playerFaction == null) return;

            EmperorDefinition emperor = gameManager.Data.GetEmperor(playerFaction.emperorId);
            emperorPanel.Show(gameManager.Context, emperor, playerFaction, gameManager.GetComponent<EmperorSkillSystem>(), gameManager.GetComponent<SuccessionSystem>());
        }

        private void OpenTechPanel()
        {
            if (techPanel == null || gameManager == null || gameManager.Context == null || gameManager.State == null) return;

            FactionState playerFaction = gameManager.State.FindFaction(gameManager.State.playerFactionId);
            if (playerFaction == null) return;

            techPanel.Show(gameManager.Context, playerFaction, gameManager.GetComponent<TechSystem>());
        }

        private void OpenWeatherPanel()
        {
            if (weatherPanel == null || gameManager == null || gameManager.Context == null) return;

            weatherPanel.Show(gameManager.Context, gameManager.GetComponent<WeatherSystem>(), gameManager.GetComponent<CelestialEventSystem>());
        }

        private void OpenLatestEventPanel()
        {
            if (eventPanel == null || gameManager == null || gameManager.Context == null || gameManager.Data == null || gameManager.State == null) return;

            foreach (EventDefinition eventDefinition in gameManager.Data.Events.Values)
            {
                eventPanel.Show(eventDefinition, gameManager.State.playerFactionId);
                return;
            }
        }

        private void OpenMechanismPanel()
        {
            if (mechanismPanel == null || gameManager == null || gameManager.Context == null || gameManager.State == null) return;

            FactionState playerFaction = gameManager.State.FindFaction(gameManager.State.playerFactionId);
            if (playerFaction == null) return;

            mechanismPanel.Show(gameManager.Context, playerFaction, gameManager.GetComponent<ReformSystem>(), gameManager.GetComponent<VictorySystem>(), gameManager.GetComponent<DiplomacySystem>(), gameManager.GetComponent<EspionageSystem>(), selectionContext, EnterWarModeForCurrentSelection);
        }

        private SelectionContext EnterWarModeForCurrentSelection()
        {
            return RebuildSelectionContextForMode(MapInteractionMode.War);
        }

        private void AttackSelectedRegion()
        {
            if (gameManager == null || gameManager.State == null) return;
            if (string.IsNullOrEmpty(selectedRegionId))
            {
                gameManager.State.AddLog("war", "请先选择一个目标地区。");
                RefreshHUD();
                return;
            }

            if (selectionContext == null || selectionContext.mode != MapInteractionMode.War || !selectionContext.HasAvailableAction("dispatch_attack"))
            {
                gameManager.State.AddLog("war", "Map selection is not in explicit war dispatch mode: " + DescribeAttackDisabledReason());
                RefreshHUD();
                return;
            }

            bool issued = gameManager.StartPlayerAttack(selectionContext.selectedArmyId, selectedRegionId);
            if (issued)
            {
                RefreshSelectionContextFromState();
                gameManager.State.AddLog("war", "已向选中地区发起行军：" + GetRegionName(selectedRegionId));
            }
            else
            {
                gameManager.State.AddLog("war", "无法向选中地区发起行军：" + GetRegionName(selectedRegionId));
            }

            RefreshHUD();
        }

        private string DescribeAttackDisabledReason()
        {
            if (selectionContext == null || selectionContext.disabledReasons == null || selectionContext.disabledReasons.Length == 0)
            {
                return "missing_selection_context";
            }

            return string.Join(", ", selectionContext.disabledReasons);
        }

        private void RefreshHUD()
        {
            if (gameManager == null || gameManager.State == null) return;

            GameState state = gameManager.State;
            SetText(turnText, "回合 " + state.turn + " | " + state.year + "年" + (state.season == Season.Spring ? "春" : "秋"));

            FactionState playerFaction = state.FindFaction(state.playerFactionId);
            if (playerFaction != null)
            {
                SetText(resourceText,
                    "金钱：" + playerFaction.money +
                    " | 粮食：" + playerFaction.food +
                    " | 合法性：" + playerFaction.legitimacy +
                    " | 领地：" + playerFaction.regionIds.Count
                );
            }

            RefreshSelectionContextText();
            RefreshAttackButtonState();
            RefreshModeHUD();
            RefreshStrategyOutliner();
        }

        private void RefreshSelectionContextText()
        {
            if (selectionContextText == null) return;

            if (selectionContext == null || string.IsNullOrEmpty(selectionContext.selectedRegionId))
            {
                selectionContextText.text = "M:" + currentMode + " | R:none";
                return;
            }

            selectionContextText.text =
                "M:" + selectionContext.mode +
                " | R:" + selectionContext.selectedRegionId +
                " | F:" + Flag(selectionContext.isFriendly) +
                " N:" + Flag(selectionContext.isNeighbor) +
                " H:" + Flag(selectionContext.isHostile);
        }

        private void RefreshAttackButtonState()
        {
            if (attackButton == null) return;

            attackButton.interactable = selectionContext != null &&
                                       selectionContext.mode == MapInteractionMode.War &&
                                       selectionContext.HasAvailableAction("dispatch_attack");
        }

        private void RefreshModeHUD()
        {
            MapInteractionMode visibleMode = selectionContext != null ? selectionContext.mode : currentMode;
            if (modeStateText != null)
            {
                modeStateText.text = "当前：" + FormatModeName(visibleMode);
            }

            ApplyModeButtonState(governanceModeButton, visibleMode == MapInteractionMode.Governance);
            ApplyModeButtonState(warModeButton, visibleMode == MapInteractionMode.War);
        }

        private static void ApplyModeButtonState(Button button, bool selected)
        {
            if (button == null) return;

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = selected ? UITheme.PanelHeaderAccent : UITheme.ButtonNormal;
            }

            Text label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.color = selected ? UITheme.PanelBackground : UITheme.ButtonText;
            }
        }

        private static string FormatModeName(MapInteractionMode mode)
        {
            switch (mode)
            {
                case MapInteractionMode.Governance: return "治理模式";
                case MapInteractionMode.War: return "战争模式";
                case MapInteractionMode.Diplomacy: return "外交过渡";
                default: return mode.ToString();
            }
        }

        private void UpdateMechanismPanelSelectionContext()
        {
            if (mechanismPanel == null) return;
            mechanismPanel.UpdateSelectionContext(selectionContext);
        }

        private void UpdateRegionPanelMode()
        {
            if (regionPanel == null || selectionContext == null || string.IsNullOrEmpty(selectionContext.selectedRegionId)) return;
            RefreshRegionPanelSelectionDetails();
            regionPanel.SetMode(selectionContext.mode, selectionContext.isFriendly, selectionContext.modeEntryReason, BuildWarPreDecisionForecast());
        }

        private void RefreshRegionPanelSelectionDetails()
        {
            if (gameManager == null || gameManager.State == null || gameManager.Data == null) return;
            if (regionPanel == null || regionPanel.CurrentRegionId != selectionContext.selectedRegionId) return;

            RegionDefinition definition = gameManager.Data.GetRegion(selectionContext.selectedRegionId);
            RegionState state = gameManager.State.FindRegion(selectionContext.selectedRegionId);
            if (definition == null || state == null) return;

            FactionState owner = gameManager.State.FindFaction(state.ownerFactionId);
            FactionState buildFaction = state.ownerFactionId == gameManager.State.playerFactionId ? owner : null;
            regionPanel.Show(definition, state, gameManager.Context, buildFaction, gameManager.GetComponent<BuildingSystem>());
            regionPanel.SetOwner(owner != null ? owner.name : "未知");
        }

        private string BuildWarPreDecisionForecast()
        {
            if (selectionContext == null || selectionContext.mode != MapInteractionMode.War)
            {
                return null;
            }

            if (gameManager == null || gameManager.MapQueries == null || string.IsNullOrEmpty(selectionContext.selectedRegionId))
            {
                return "行军预告: 缺少地图查询，不能计算补给路线。";
            }

            if (string.IsNullOrEmpty(selectionContext.selectedArmyId))
            {
                string previewArmyId = ResolvePlayerOperableArmyId(selectionContext.selectedRegionId, false);
                ArmyRuntimeState previewArmy = !string.IsNullOrEmpty(previewArmyId) ? gameManager.MapQueries.GetArmy(previewArmyId) : null;
                CampaignRouteForecast previewForecast = previewArmy != null
                    ? StrategyMapRulebook.BuildCampaignRouteForecast(gameManager.MapQueries, gameManager.Context, previewArmy, selectionContext.selectedRegionId)
                    : null;
                if (previewForecast != null && previewForecast.canDispatch)
                {
                    return "Route forecast: " + previewForecast.FormatCompact() +
                           " | 补给-" + previewForecast.firstTurnSupplyCost + " -> " + previewForecast.supplyAfterFirstMove +
                           " | 全程补给-" + previewForecast.fullRouteSupplyCost +
                           " | 战力修正" + previewForecast.supplyPowerPercent + "%" +
                           " | 风险等级 " + ResolveForecastRiskGrade(previewForecast) +
                           " | 需侦察确认后投送";
                }

                return "行军预告: 无可用空闲军队；需先在邻接地区布置军队。";
            }

            ArmyRuntimeState army = gameManager.MapQueries.GetArmy(selectionContext.selectedArmyId);
            if (army == null)
            {
                return "行军预告: 选中军队不存在，不能计算补给。";
            }

            CampaignRouteForecast forecast = StrategyMapRulebook.BuildCampaignRouteForecast(gameManager.MapQueries, gameManager.Context, army, selectionContext.selectedRegionId);
            if (forecast != null)
            {
                return "Route forecast: " + forecast.FormatCompact() +
                       " | 补给-" + forecast.firstTurnSupplyCost + " -> " + forecast.supplyAfterFirstMove +
                       " | 全程补给-" + forecast.fullRouteSupplyCost +
                       " | 战力修正" + forecast.supplyPowerPercent + "%" +
                       " | 风险等级 " + ResolveForecastRiskGrade(forecast) +
                       " | source " + forecast.sourceReference;
            }

            List<string> route = gameManager.MapQueries.FindRoute(army.locationRegionId, selectionContext.selectedRegionId);
            int routeSteps = Mathf.Max(0, route.Count - 1);
            int firstTurnSupplyCost = StrategyCausalRules.WarMovementSupplyCost;
            int fullRouteSupplyCost = routeSteps * StrategyCausalRules.WarMovementSupplyCost;
            int supplyAfterFirstMove = Mathf.Max(0, army.supply - firstTurnSupplyCost);
            int supplyAtContact = Mathf.Max(0, army.supply - fullRouteSupplyCost);
            int supplyPowerPercent = StrategyCausalRules.CalculateBattleSupplyPowerPercentForSupply(supplyAtContact);
            string targetRisk = ResolveTargetWarRisk(selectionContext.selectedRegionId, supplyPowerPercent);

            return "行军预告: " + army.id +
                   " | 路径" + routeSteps + "段" +
                   " | 首回合补给-" + firstTurnSupplyCost + "→" + supplyAfterFirstMove +
                   " | 全程最多补给-" + fullRouteSupplyCost +
                   " | 接敌补给" + supplyAtContact +
                   " | 战力修正" + supplyPowerPercent + "%" +
                   " | 风险等级 " + targetRisk +
                    " | 目标接敌后自动结算";
        }

        private static string ResolveForecastRiskGrade(CampaignRouteForecast forecast)
        {
            if (forecast == null) return "未知";
            if (forecast.supplyPowerPercent <= StrategyCausalRules.DepletedSupplyBattlePowerPercent ||
                forecast.contactRisk >= 65 ||
                forecast.interceptionRisk >= 65)
            {
                return "高";
            }

            if (forecast.supplyPowerPercent < 100 ||
                forecast.contactRisk >= 35 ||
                forecast.interceptionRisk >= 35 ||
                forecast.hasUnknownRisk)
            {
                return "中";
            }

            return "低";
        }

        private string ResolveTargetWarRisk(string regionId, int supplyPowerPercent)
        {
            if (gameManager == null || gameManager.State == null || string.IsNullOrEmpty(regionId))
            {
                return "未知";
            }

            RegionState targetState = gameManager.State.FindRegion(regionId);
            if (targetState == null)
            {
                return "未知";
            }

            int resistancePressure = Mathf.RoundToInt((targetState.rebellionRisk + targetState.localPower + targetState.annexationPressure) / 3f);
            if (supplyPowerPercent <= StrategyCausalRules.DepletedSupplyBattlePowerPercent || resistancePressure >= 65)
            {
                return "高";
            }

            if (supplyPowerPercent < 100 || resistancePressure >= 40)
            {
                return "中";
            }

            return "低";
        }

        private void RefreshSelectionContextFromState()
        {
            if (string.IsNullOrEmpty(selectedRegionId))
            {
                selectionContext = CreateEmptySelectionContext();
                selectionContext.mode = currentMode;
            }
            else
            {
                selectionContext = BuildSelectionContext(selectedRegionId);
                currentMode = selectionContext.mode;
            }

            UpdateMechanismPanelSelectionContext();
            UpdateRegionPanelMode();
            RefreshAttackButtonState();
            RefreshSelectionContextText();
            RefreshModeHUD();
        }

        private ArmyState FindArmy(string armyId)
        {
            if (gameManager == null || gameManager.State == null || string.IsNullOrEmpty(armyId)) return null;

            string[] armyIds = armyId.Split(',');
            for (int i = 0; i < gameManager.State.armies.Count; i++)
            {
                for (int j = 0; j < armyIds.Length; j++)
                {
                    if (gameManager.State.armies[i].id == armyIds[j]) return gameManager.State.armies[i];
                }
            }

            return null;
        }

        private EngagementRuntimeState FindEngagement(string engagementId)
        {
            if (gameManager == null || gameManager.World == null || gameManager.World.Map == null || string.IsNullOrEmpty(engagementId))
            {
                return null;
            }

            EngagementRuntimeState engagement;
            return gameManager.World.Map.EngagementsById.TryGetValue(engagementId, out engagement) ? engagement : null;
        }

        private string GetFactionName(string factionId)
        {
            if (gameManager == null || gameManager.State == null || string.IsNullOrEmpty(factionId)) return "未知";

            FactionState faction = gameManager.State.FindFaction(factionId);
            return faction != null ? faction.name : "未知";
        }

        private string GetRegionName(string regionId)
        {
            if (gameManager == null || gameManager.Data == null || string.IsNullOrEmpty(regionId)) return "";

            RegionDefinition definition = gameManager.Data.GetRegion(regionId);
            return definition != null ? definition.name : regionId;
        }

        private void EnsureStrategyControls()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) return;

            if (strategyLensBar == null)
            {
                strategyLensBar = CreateRuntimePanel(canvas.transform, "StrategyLensBar", new Vector2(0, 1), new Vector2(0, 1), new Vector2(12, -108), new Vector2(500, 34));
                strategyLensStateText = CreateRuntimeText(strategyLensBar.transform, "StrategyLensStateText", "Lens Governance", new Vector2(8, 0), new Vector2(0, 0.5f), new Vector2(105, 28), 11);
                CreateLensButton("LensGovernanceButton", "Gov", MapLensMode.Governance, 116);
                CreateLensButton("LensRiskButton", "Risk", MapLensMode.Risk, 174);
                CreateLensButton("LensEconomyButton", "Eco", MapLensMode.Economy, 232);
                CreateLensButton("LensLegitimacyButton", "Legit", MapLensMode.Legitimacy, 290);
                CreateLensButton("LensWarButton", "War", MapLensMode.War, 348);
                CreateLensButton("LensTerrainButton", "Land", MapLensMode.Terrain, 406);
            }

            if (strategyOutlinerRoot == null)
            {
                strategyOutlinerRoot = CreateRuntimePanel(canvas.transform, "StrategyOutlinerPanel", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-12, -108), new Vector2(292, 330));
                CreateRuntimeText(strategyOutlinerRoot.transform, "StrategyOutlinerTitle", "Outliner", new Vector2(10, -10), new Vector2(0, 1), new Vector2(170, 26), 13);
                Button collapse = CreateRuntimeButton(strategyOutlinerRoot.transform, "StrategyOutlinerCollapseButton", "-", new Vector2(264, -12), new Vector2(1, 1), new Vector2(24, 24));
                collapse.onClick.AddListener(CollapseStrategyOutliner);
                strategyOutlinerText = CreateRuntimeText(strategyOutlinerRoot.transform, "StrategyOutlinerText", "", new Vector2(10, -42), new Vector2(0, 1), new Vector2(272, 34), 10);
                strategyOutlinerText.verticalOverflow = VerticalWrapMode.Truncate;
            }

            if (strategyOutlinerCollapsedRoot == null)
            {
                strategyOutlinerCollapsedRoot = CreateRuntimePanel(canvas.transform, "StrategyOutlinerCollapsed", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-12, -108), new Vector2(84, 42));
                Button expand = CreateRuntimeButton(strategyOutlinerCollapsedRoot.transform, "StrategyOutlinerExpandButton", "Out", new Vector2(42, -20), new Vector2(0.5f, 1), new Vector2(70, 28));
                expand.onClick.AddListener(ExpandStrategyOutliner);
            }

            ApplyStrategyOutlinerVisibility();
            ApplyLensToMap();
        }

        private void CreateLensButton(string name, string label, MapLensMode lens, float x)
        {
            if (strategyLensBar == null) return;
            Button button = CreateRuntimeButton(strategyLensBar.transform, name, label, new Vector2(x, 0), new Vector2(0, 0.5f), new Vector2(52, 28));
            button.onClick.AddListener(delegate { SetMapLens(lens); });
        }

        private void SetMapLens(MapLensMode lens)
        {
            currentLens = lens;
            ApplyLensToMap();
            RefreshStrategyOutliner();
        }

        private void ApplyLensToMap()
        {
            if (strategyLensStateText != null) strategyLensStateText.text = "Lens " + currentLens;
            MapRenderer renderer = Object.FindObjectOfType<MapRenderer>();
            if (renderer != null)
            {
                renderer.SetLens(currentLens);
            }
        }

        private void RefreshStrategyOutliner()
        {
            if (strategyOutlinerText == null || gameManager == null) return;

            ClearStrategyOutlinerEntryButtons();
            List<StrategyOutlinerEntry> entries = StrategyMapRulebook.BuildOutliner(gameManager.State, gameManager.World);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Top actions | lens " + currentLens);
            int count = Mathf.Min(entries.Count, 8);
            for (int i = 0; i < count; i++)
            {
                StrategyOutlinerEntry entry = entries[i];
                CreateStrategyOutlinerEntryButton(i, entry);
            }
            if (count == 0)
            {
                sb.AppendLine("No critical items.");
            }
            strategyOutlinerText.text = sb.ToString();
            ApplyStrategyOutlinerVisibility();
        }

        private void CreateStrategyOutlinerEntryButton(int index, StrategyOutlinerEntry entry)
        {
            if (strategyOutlinerRoot == null || entry == null) return;

            string label = entry.category + " | " + entry.label;
            if (label.Length > 42)
            {
                label = label.Substring(0, 39) + "...";
            }

            Button button = CreateRuntimeButton(
                strategyOutlinerRoot.transform,
                "StrategyOutlinerEntryButton_" + index,
                label,
                new Vector2(146, -82 - index * 30),
                new Vector2(0.5f, 1),
                new Vector2(272, 26));
            int capturedIndex = index;
            string capturedCategory = entry.category;
            string capturedTargetId = entry.targetId;
            button.onClick.AddListener(delegate { SelectStrategyOutlinerEntry(capturedIndex, capturedCategory, capturedTargetId); });
            strategyOutlinerEntryRoots.Add(button.gameObject);
        }

        private void SelectStrategyOutlinerEntry(int index, string category, string targetId)
        {
            if (gameManager == null || gameManager.Events == null || string.IsNullOrEmpty(targetId)) return;

            string regionId = targetId;
            if (category == "marching_army" || category == "low_supply")
            {
                ArmyRuntimeState army = gameManager.MapQueries != null ? gameManager.MapQueries.GetArmy(targetId) : null;
                if (army != null)
                {
                    regionId = !string.IsNullOrEmpty(army.targetRegionId) ? army.targetRegionId : army.locationRegionId;
                }
            }

            if (string.IsNullOrEmpty(regionId)) return;

            gameManager.State.AddLog("ui", "outliner selected " + index + ": " + category + " -> " + regionId);
            gameManager.Events.Publish(new GameEvent(GameEventType.RegionSelected, regionId, null));
        }

        private void ClearStrategyOutlinerEntryButtons()
        {
            for (int i = strategyOutlinerEntryRoots.Count - 1; i >= 0; i--)
            {
                GameObject entry = strategyOutlinerEntryRoots[i];
                if (entry == null) continue;
                entry.SetActive(false);
                Destroy(entry);
            }

            strategyOutlinerEntryRoots.Clear();
        }

        private void CollapseStrategyOutliner()
        {
            strategyOutlinerCollapsed = true;
            ApplyStrategyOutlinerVisibility();
        }

        private void ExpandStrategyOutliner()
        {
            strategyOutlinerCollapsed = false;
            ApplyStrategyOutlinerVisibility();
        }

        private void ApplyStrategyOutlinerVisibility()
        {
            bool forceCollapsed = IsRegionPanelExpanded();
            bool showCollapsed = strategyOutlinerCollapsed || forceCollapsed;

            if (strategyOutlinerRoot != null)
            {
                if (showCollapsed) UIPanelVisibility.Hide(strategyOutlinerRoot);
                else UIPanelVisibility.Show(strategyOutlinerRoot);
            }

            if (strategyOutlinerCollapsedRoot != null)
            {
                RectTransform collapsedRect = strategyOutlinerCollapsedRoot.GetComponent<RectTransform>();
                if (collapsedRect != null)
                {
                    collapsedRect.anchoredPosition = forceCollapsed ? StrategyOutlinerRegionPanelAvoidPosition : StrategyOutlinerDockedPosition;
                }

                if (showCollapsed) UIPanelVisibility.Show(strategyOutlinerCollapsedRoot);
                else UIPanelVisibility.Hide(strategyOutlinerCollapsedRoot);
            }
        }

        private bool IsRegionPanelExpanded()
        {
            if (regionPanel == null || string.IsNullOrEmpty(regionPanel.CurrentRegionId)) return false;

            CanvasGroup group = regionPanel.GetComponent<CanvasGroup>();
            if (group != null)
            {
                return group.alpha > 0.5f && group.interactable;
            }

            return regionPanel.gameObject.activeInHierarchy;
        }

        private static GameObject CreateRuntimePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(anchorMin.x, anchorMax.y);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = panel.AddComponent<Image>();
            image.color = UITheme.PanelBackground;
            return panel;
        }

        private static Text CreateRuntimeText(Transform parent, string name, string value, Vector2 position, Vector2 anchor, Vector2 size, int fontSize)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Text text = obj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = UITheme.TextPrimary;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = value;
            return text;
        }

        private static Button CreateRuntimeButton(Transform parent, string name, string label, Vector2 position, Vector2 anchor, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = obj.AddComponent<Image>();
            image.color = UITheme.ButtonNormal;
            Button button = obj.AddComponent<Button>();

            Text text = CreateRuntimeText(obj.transform, name + "Text", label, Vector2.zero, new Vector2(0.5f, 0.5f), size, 10);
            text.alignment = TextAnchor.MiddleCenter;
            text.color = UITheme.ButtonText;
            text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            return button;
        }

        private static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }

        private static string Flag(bool value)
        {
            return value ? "1" : "0";
        }
    }
}
