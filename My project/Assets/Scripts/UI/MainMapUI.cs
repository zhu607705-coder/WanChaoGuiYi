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
        [SerializeField] private Text overlayBudgetText;
        [SerializeField] private Button governanceModeButton;
        [SerializeField] private Button warModeButton;
        [SerializeField] private Button nextTurnButton;
        [SerializeField] private Button courtButton;
        [SerializeField] private Button emperorButton;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button prepareFrontlineButton;
        [SerializeField] private Button techButton;
        [SerializeField] private Button weatherButton;
        [SerializeField] private Button eventButton;
        [SerializeField] private Button mechanismButton;

        private string selectedRegionId;
        private SelectionContext selectionContext = new SelectionContext();
        private MapInteractionMode currentMode = MapInteractionMode.Governance;
        private MapLensMode currentLens = MapLensMode.Governance;
        private GameObject strategyLensBar;
        private GameObject strategyOutlinerRoot;
        private GameObject strategyOutlinerCollapsedRoot;
        private Text strategyLensStateText;
        private Text strategyOutlinerText;
        private readonly List<GameObject> strategyOutlinerEntryRoots = new List<GameObject>();
        private GameObject logisticsQueuePanel;
        private Text logisticsQueueText;
        private Button logisticsPriorityUpButton;
        private Button logisticsPriorityDownButton;
        private Button logisticsPauseButton;
        private Button logisticsCancelButton;
        private string selectedLogisticsArmyId;
        private DemoEntityVisualSpawner entityVisualSpawner;
        private bool strategyOutlinerCollapsed;
        private bool subscribed;

        public MapInteractionMode CurrentMode { get { return currentMode; } }
        public MapLensMode CurrentLens { get { return currentLens; } }
        public SelectionContext CurrentSelectionContext { get { return selectionContext; } }
        public string SelectedRegionId { get { return selectedRegionId; } }

        public void Bind(GameManager manager, RegionPanel region, EmperorPanel emperor, CourtPanel court, EventPanel eventsPanel, BattleReportPanel battlePanel, TechPanel tech, WeatherPanel weather, MechanismPanel mechanism, Text turn, Text resources, Text selectionText, Text modeText, Text overlayBudget, Button governanceModeButtonRef, Button warModeButtonRef, Button nextTurn, Button courtButtonRef, Button emperorButtonRef, Button attackButtonRef, Button prepareFrontlineButtonRef, Button techButtonRef, Button weatherButtonRef, Button eventButtonRef, Button mechanismButtonRef)
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
            overlayBudgetText = overlayBudget;
            governanceModeButton = governanceModeButtonRef;
            warModeButton = warModeButtonRef;
            nextTurnButton = nextTurn;
            courtButton = courtButtonRef;
            emperorButton = emperorButtonRef;
            attackButton = attackButtonRef;
            prepareFrontlineButton = prepareFrontlineButtonRef;
            techButton = techButtonRef;
            weatherButton = weatherButtonRef;
            eventButton = eventButtonRef;
            mechanismButton = mechanismButtonRef;

            if (eventPanel != null && gameManager != null)
            {
                eventPanel.Initialize(gameManager.Context);
            }
            if (battleReportPanel != null)
            {
                battleReportPanel.SetFocusHandler(FocusBattleReportRegion);
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
            gameManager.Events.Subscribe(GameEventType.FrontlinePrepared, OnMechanismChanged);
            gameManager.Events.Subscribe(GameEventType.FrontlineLogisticsAdvanced, OnMechanismChanged);
            gameManager.Events.Subscribe(GameEventType.FrontlineLogisticsCommanded, OnMechanismChanged);
            gameManager.Events.Subscribe(GameEventType.FrontlineLogisticsRaided, OnMechanismChanged);
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
            if (prepareFrontlineButton != null) prepareFrontlineButton.onClick.AddListener(PrepareFrontlineSelectedRegion);
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
                gameManager.Events.Unsubscribe(GameEventType.FrontlinePrepared, OnMechanismChanged);
                gameManager.Events.Unsubscribe(GameEventType.FrontlineLogisticsAdvanced, OnMechanismChanged);
                gameManager.Events.Unsubscribe(GameEventType.FrontlineLogisticsCommanded, OnMechanismChanged);
                gameManager.Events.Unsubscribe(GameEventType.FrontlineLogisticsRaided, OnMechanismChanged);
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
            if (prepareFrontlineButton != null) prepareFrontlineButton.onClick.RemoveListener(PrepareFrontlineSelectedRegion);
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

        private void LateUpdate()
        {
            RefreshOverlayBudgetText();
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
                    ? new[] { "dispatch_attack", "prepare_frontline", "inspect_diplomacy", "inspect_border" }
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

            battleReportPanel.Show(result, attackerName, defenderName, regionName, battleRegionId);
        }

        private void FocusBattleReportRegion(string regionId)
        {
            SelectAndFocusMapRegion(regionId, "battle report focus");
        }

        private static bool TryResolveRegionWorldCenter(string regionId, out Vector2 center)
        {
            center = Vector2.zero;
            if (string.IsNullOrEmpty(regionId)) return false;

            GameObject regionObject = GameObject.Find("RegionSurface_" + regionId);
            if (regionObject == null) return false;

            MeshFilter meshFilter = regionObject.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                center = regionObject.transform.position;
                return true;
            }

            Vector3 world = regionObject.transform.TransformPoint(meshFilter.sharedMesh.bounds.center);
            center = new Vector2(world.x, world.y);
            return true;
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

        private void PrepareFrontlineSelectedRegion()
        {
            if (gameManager == null || gameManager.State == null) return;
            if (string.IsNullOrEmpty(selectedRegionId))
            {
                gameManager.State.AddLog("war", "请先选择一个前线目标地区。");
                RefreshHUD();
                return;
            }

            if (selectionContext == null || selectionContext.mode != MapInteractionMode.War || !selectionContext.HasAvailableAction("prepare_frontline"))
            {
                gameManager.State.AddLog("war", "当前选择不能前线整备：" + DescribeAttackDisabledReason());
                RefreshHUD();
                return;
            }

            bool prepared = gameManager.PrepareFrontline(selectionContext.selectedArmyId, selectedRegionId);
            if (prepared)
            {
                RefreshSelectionContextFromState();
                gameManager.State.AddLog("war", "已完成前线整备：" + GetRegionName(selectedRegionId));
            }
            else
            {
                gameManager.State.AddLog("war", "前线整备未能执行：" + GetRegionName(selectedRegionId));
            }

            RefreshHUD();
        }

        private string DescribeAttackDisabledReason()
        {
            if (selectionContext == null || selectionContext.disabledReasons == null || selectionContext.disabledReasons.Length == 0)
            {
                return "缺少当前选择";
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < selectionContext.disabledReasons.Length; i++)
            {
                if (i > 0) sb.Append("、");
                sb.Append(FormatDisabledReason(selectionContext.disabledReasons[i]));
            }

            return sb.ToString();
        }

        private static string FormatDisabledReason(string reason)
        {
            switch (reason)
            {
                case "missing_state": return "局势尚未初始化";
                case "region_not_found": return "目标地区不存在";
                case "attack_requires_war_mode": return "需要先进入战争模式";
                case "attack_requires_explicit_war_mode": return "需要从外交面板确认战争模式";
                case "dispatch_requires_route_review": return "需要先确认路线和补给";
                case "dispatch_requires_operable_army": return "缺少可调度军队";
                case "dispatch_requires_adjacent_target": return "需要邻接控制区或可用前线";
                case "dispatch_requires_scouted_supply_projection": return "需要侦察后才能评估补给";
                default: return string.IsNullOrEmpty(reason) ? "原因未明" : reason;
            }
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
            RefreshPrepareFrontlineButtonState();
            RefreshModeHUD();
            RefreshOverlayBudgetText();
            RefreshStrategyOutliner();
            RefreshLogisticsQueuePanel();
        }

        private void RefreshSelectionContextText()
        {
            if (selectionContextText == null) return;

            if (selectionContext == null || string.IsNullOrEmpty(selectionContext.selectedRegionId))
            {
                selectionContextText.text = FormatCompactModeName(currentMode) + " | 未选区";
                return;
            }

            selectionContextText.text =
                FormatCompactModeName(selectionContext.mode) +
                " | " + GetRegionName(selectionContext.selectedRegionId) +
                " | " + FormatCompactRelationTag(selectionContext);
        }

        private void RefreshAttackButtonState()
        {
            if (attackButton == null) return;

            attackButton.interactable = selectionContext != null &&
                                       selectionContext.mode == MapInteractionMode.War &&
                                       selectionContext.HasAvailableAction("dispatch_attack");
        }

        private void RefreshPrepareFrontlineButtonState()
        {
            if (prepareFrontlineButton == null) return;

            prepareFrontlineButton.interactable = selectionContext != null &&
                                                  selectionContext.mode == MapInteractionMode.War &&
                                                  selectionContext.HasAvailableAction("prepare_frontline");
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

        private void RefreshOverlayBudgetText()
        {
            if (overlayBudgetText == null) return;

            if (entityVisualSpawner == null)
            {
                entityVisualSpawner = FindObjectOfType<DemoEntityVisualSpawner>();
            }

            if (entityVisualSpawner == null)
            {
                SetText(overlayBudgetText, "避让 缩放/预算/重叠 0/0/0 | 标签 0/0 隐0 | 脉冲 0/0 额0");
                return;
            }

            int totalPulses = entityVisualSpawner.LastActivePulseCount + entityVisualSpawner.LastInactivePulseCount;
            SetText(overlayBudgetText,
                "避让 缩放/预算/重叠 " +
                entityVisualSpawner.LastHiddenByZoomCount + "/" +
                entityVisualSpawner.LastHiddenByBudgetCount + "/" +
                entityVisualSpawner.LastHiddenByOverlapCount +
                " | 标签 " + entityVisualSpawner.LastVisibleLabelCount + "/" +
                entityVisualSpawner.LastWarLabelObjectCount +
                " 隐" + entityVisualSpawner.LastHiddenLabelCount +
                " | 脉冲 " + entityVisualSpawner.LastActivePulseCount + "/" +
                totalPulses +
                " 额" + entityVisualSpawner.LastPulseBudget);
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

        private static string FormatRelationTag(SelectionContext context)
        {
            if (context == null) return "未判定";
            if (context.isFriendly) return "己方";
            if (context.isHostile) return "敌对";
            if (context.isNeighbor) return "邻接";
            return "远邻";
        }

        private static string FormatCompactModeName(MapInteractionMode mode)
        {
            switch (mode)
            {
                case MapInteractionMode.War: return "战争";
                case MapInteractionMode.Diplomacy: return "外交";
                default: return "治理";
            }
        }

        private static string FormatCompactRelationTag(SelectionContext context)
        {
            if (context == null) return "未判定";
            if (context.isFriendly) return "己方";
            if (context.isHostile && context.isNeighbor) return "敌对邻接";
            if (context.isHostile) return "敌对远邻";
            if (context.isNeighbor) return "邻接";
            return "远邻";
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
                    FrontlineSupplyPlanForecast previewPlan = previewArmy != null
                        ? StrategyMapRulebook.BuildFrontlineSupplyPlanForecast(gameManager.MapQueries, gameManager.Context, previewArmy, selectionContext.selectedRegionId)
                        : null;
                    return FormatCampaignRouteForecast(previewForecast, "预备军", previewPlan);
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
                FrontlineSupplyPlanForecast plan = StrategyMapRulebook.BuildFrontlineSupplyPlanForecast(gameManager.MapQueries, gameManager.Context, army, selectionContext.selectedRegionId);
                return FormatCampaignRouteForecast(forecast, army.id, plan);
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

        private static string FormatCampaignRouteForecast(CampaignRouteForecast forecast, string armyLabel, FrontlineSupplyPlanForecast plan)
        {
            if (forecast == null) return "行军预告: 暂无路线。";
            string planLine = plan != null ? "\n" + plan.FormatCompact() : "";
            return "行军预告: " + armyLabel + " | 路径" + forecast.routeSteps + "段 | 风险等级 " + ResolveForecastRiskGrade(forecast) +
                   "\n补给压力: 首回合补给-" + forecast.firstTurnSupplyCost + "→" + forecast.supplyAfterFirstMove +
                   " | 全程补给-" + forecast.fullRouteSupplyCost +
                   " | 战力修正" + forecast.supplyPowerPercent + "%" +
                   "\n接敌判断: 路线可见 " + FormatVisibilityState(forecast.visibilityState) +
                   " | 接敌" + forecast.contactRisk + "%" +
                   " | 拦截" + forecast.interceptionRisk + "%" +
                   (forecast.hasUnknownRisk ? " | 情报不足" : " | 情报可读") +
                   planLine;
        }

        private static string FormatVisibilityState(VisibilityState state)
        {
            switch (state)
            {
                case VisibilityState.Scouted: return "已侦察";
                case VisibilityState.Known: return "邻接可见";
                case VisibilityState.Hidden: return "未知";
                default: return state.ToString();
            }
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
            RefreshPrepareFrontlineButtonState();
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

            try
            {
                RegionDefinition definition = gameManager.Data.GetRegion(regionId);
                return definition != null ? definition.name : regionId;
            }
            catch (System.Exception)
            {
                return regionId;
            }
        }

        private void EnsureStrategyControls()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) return;

            if (strategyLensBar == null)
            {
                strategyLensBar = GameObject.Find("StrategyLensBar");
            }
            if (strategyLensBar == null)
            {
                strategyLensBar = CreateRuntimePanel(canvas.transform, "StrategyLensBar", StrategyLayoutSpec.TopLeftAnchor, StrategyLayoutSpec.TopLeftAnchor, StrategyLayoutSpec.LensBarPosition, StrategyLayoutSpec.LensBarSize);
            }
            strategyLensStateText = FindChildComponent<Text>(strategyLensBar.transform, "StrategyLensStateText");
            if (strategyLensStateText == null)
            {
                strategyLensStateText = CreateRuntimeText(strategyLensBar.transform, "StrategyLensStateText", "图层:治理", new Vector2(10, -2), new Vector2(0, 1), new Vector2(78, 28), 10);
            }
            ConfigureLensButton("LensGovernanceButton", "治理", MapLensMode.Governance, 0);
            ConfigureLensButton("LensRiskButton", "风险", MapLensMode.Risk, 1);
            ConfigureLensButton("LensEconomyButton", "财税", MapLensMode.Economy, 2);
            ConfigureLensButton("LensLegitimacyButton", "法统", MapLensMode.Legitimacy, 3);
            ConfigureLensButton("LensWarButton", "战事", MapLensMode.War, 4);
            ConfigureLensButton("LensTerrainButton", "地形", MapLensMode.Terrain, 5);

            if (strategyOutlinerRoot == null)
            {
                strategyOutlinerRoot = GameObject.Find("StrategyOutlinerPanel");
            }
            if (strategyOutlinerRoot == null)
            {
                strategyOutlinerRoot = CreateRuntimePanel(canvas.transform, "StrategyOutlinerPanel", StrategyLayoutSpec.TopRightAnchor, StrategyLayoutSpec.TopRightAnchor, StrategyLayoutSpec.OutlinerDockedPosition, StrategyLayoutSpec.OutlinerSize);
                CreateRuntimeText(strategyOutlinerRoot.transform, "StrategyOutlinerTitle", "军政待办", new Vector2(12, -10), new Vector2(0, 1), new Vector2(168, 26), 13);
            }
            Button collapse = ResolveButton(strategyOutlinerRoot.transform, "StrategyOutlinerCollapseButton", "-", new Vector2(268, -12), new Vector2(1, 1), new Vector2(24, 24));
            if (collapse != null)
            {
                collapse.onClick.RemoveAllListeners();
                collapse.onClick.AddListener(CollapseStrategyOutliner);
            }
            strategyOutlinerText = FindChildComponent<Text>(strategyOutlinerRoot.transform, "StrategyOutlinerText");
            if (strategyOutlinerText == null)
            {
                strategyOutlinerText = CreateRuntimeText(strategyOutlinerRoot.transform, "StrategyOutlinerText", "", new Vector2(14, -42), new Vector2(0, 1), new Vector2(266, 58), 10);
            }
            strategyOutlinerText.verticalOverflow = VerticalWrapMode.Truncate;

            if (strategyOutlinerCollapsedRoot == null)
            {
                strategyOutlinerCollapsedRoot = GameObject.Find("StrategyOutlinerCollapsed");
            }
            if (strategyOutlinerCollapsedRoot == null)
            {
                strategyOutlinerCollapsedRoot = CreateRuntimePanel(canvas.transform, "StrategyOutlinerCollapsed", StrategyLayoutSpec.TopRightAnchor, StrategyLayoutSpec.TopRightAnchor, StrategyLayoutSpec.OutlinerDockedPosition, StrategyLayoutSpec.OutlinerCollapsedSize);
            }
            Button expand = ResolveButton(strategyOutlinerCollapsedRoot.transform, "StrategyOutlinerExpandButton", "待办", new Vector2(37, -18), new Vector2(0.5f, 1), new Vector2(60, 26));
            if (expand != null)
            {
                expand.onClick.RemoveAllListeners();
                expand.onClick.AddListener(ExpandStrategyOutliner);
            }

            if (logisticsQueuePanel == null)
            {
                logisticsQueuePanel = GameObject.Find("LogisticsQueuePanel");
            }
            if (logisticsQueuePanel == null)
            {
                logisticsQueuePanel = CreateRuntimePanel(canvas.transform, "LogisticsQueuePanel", StrategyLayoutSpec.TopRightAnchor, StrategyLayoutSpec.TopRightAnchor, StrategyLayoutSpec.LogisticsQueuePosition, StrategyLayoutSpec.LogisticsQueueSize);
                CreateRuntimeText(logisticsQueuePanel.transform, "LogisticsQueueTitle", "后勤队列", new Vector2(12, -8), new Vector2(0, 1), new Vector2(90, 22), 12);
            }
            logisticsQueueText = FindChildComponent<Text>(logisticsQueuePanel.transform, "LogisticsQueueText");
            if (logisticsQueueText == null)
            {
                logisticsQueueText = CreateRuntimeText(logisticsQueuePanel.transform, "LogisticsQueueText", "暂无前线运输队", new Vector2(12, -30), new Vector2(0, 1), new Vector2(270, 42), 10);
            }
            logisticsQueueText.verticalOverflow = VerticalWrapMode.Truncate;
            logisticsPriorityUpButton = ResolveButton(logisticsQueuePanel.transform, "LogisticsPriorityUpButton", "加急", new Vector2(42, -92), new Vector2(0, 1), new Vector2(54, 24));
            logisticsPriorityDownButton = ResolveButton(logisticsQueuePanel.transform, "LogisticsPriorityDownButton", "后置", new Vector2(102, -92), new Vector2(0, 1), new Vector2(54, 24));
            logisticsPauseButton = ResolveButton(logisticsQueuePanel.transform, "LogisticsPauseButton", "暂停", new Vector2(162, -92), new Vector2(0, 1), new Vector2(54, 24));
            logisticsCancelButton = ResolveButton(logisticsQueuePanel.transform, "LogisticsCancelButton", "取消", new Vector2(222, -92), new Vector2(0, 1), new Vector2(54, 24));
            BindButton(logisticsPriorityUpButton, delegate { AdjustSelectedLogisticsPriority(1); });
            BindButton(logisticsPriorityDownButton, delegate { AdjustSelectedLogisticsPriority(-1); });
            BindButton(logisticsPauseButton, ToggleSelectedLogisticsPause);
            BindButton(logisticsCancelButton, CancelSelectedLogistics);

            ApplyStrategyOutlinerVisibility();
            ApplyLensToMap();
            RefreshLogisticsQueuePanel();
        }

        private void ConfigureLensButton(string name, string label, MapLensMode lens, int index)
        {
            if (strategyLensBar == null) return;
            float x = StrategyLayoutSpec.LensButtonStartX + StrategyLayoutSpec.LensButtonStepX * index;
            Button button = ResolveButton(strategyLensBar.transform, name, label, new Vector2(x, -16), new Vector2(0, 1), new Vector2(44, 24));
            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate { SetMapLens(lens); });
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null || action == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private static Button ResolveButton(Transform parent, string name, string label, Vector2 position, Vector2 anchor, Vector2 size)
        {
            Button button = FindChildComponent<Button>(parent, name);
            if (button != null) return button;
            return CreateRuntimeButton(parent, name, label, position, anchor, size);
        }

        private static T FindChildComponent<T>(Transform root, string objectName) where T : Component
        {
            Transform child = FindChildRecursive(root, objectName);
            return child != null ? child.GetComponent<T>() : null;
        }

        private static Transform FindChildRecursive(Transform root, string objectName)
        {
            if (root == null || string.IsNullOrEmpty(objectName)) return null;
            if (root.name == objectName) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChildRecursive(root.GetChild(i), objectName);
                if (found != null) return found;
            }

            return null;
        }

        private void SetMapLens(MapLensMode lens)
        {
            currentLens = lens;
            ApplyLensToMap();
            RefreshStrategyOutliner();
        }

        private void ApplyLensToMap()
        {
            if (strategyLensStateText != null) strategyLensStateText.text = "图层:" + FormatLensName(currentLens);
            MapRenderer renderer = Object.FindObjectOfType<MapRenderer>();
            if (renderer != null)
            {
                renderer.SetLens(currentLens);
            }
        }

        private static string FormatLensName(MapLensMode lens)
        {
            switch (lens)
            {
                case MapLensMode.Risk: return "风险";
                case MapLensMode.Economy: return "财税";
                case MapLensMode.Legitimacy: return "法统";
                case MapLensMode.War: return "战事";
                case MapLensMode.Terrain: return "地形";
                default: return "治理";
            }
        }

        private void RefreshStrategyOutliner()
        {
            if (strategyOutlinerText == null || gameManager == null) return;

            ClearStrategyOutlinerEntryButtons();
            List<StrategyOutlinerEntry> entries = StrategyMapRulebook.BuildOutliner(gameManager.State, gameManager.World);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            AppendOutlinerSummary(sb, entries);
            int count = Mathf.Min(entries.Count, 7);
            for (int i = 0; i < count; i++)
            {
                StrategyOutlinerEntry entry = entries[i];
                CreateStrategyOutlinerEntryButton(i, entry);
            }
            if (count == 0)
            {
                sb.AppendLine("暂无急件");
            }
            strategyOutlinerText.text = sb.ToString();
            ApplyStrategyOutlinerVisibility();
        }

        private void RefreshLogisticsQueuePanel()
        {
            if (logisticsQueuePanel == null || logisticsQueueText == null || gameManager == null || gameManager.State == null || gameManager.World == null || gameManager.World.Map == null) return;

            List<ArmyRuntimeState> queue = CollectPlayerLogisticsQueue();
            string managedArmyId = ResolveManagedLogisticsArmyId(queue);
            ArmyRuntimeState managedArmy = !string.IsNullOrEmpty(managedArmyId) && gameManager.MapQueries != null
                ? gameManager.MapQueries.GetArmy(managedArmyId)
                : null;

            if (queue.Count == 0)
            {
                logisticsQueueText.text = "暂无前线运输队\n整备长路线后会在此排程。";
            }
            else
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("队列 ");
                sb.Append(queue.Count);
                sb.Append(" | 管理 ");
                sb.Append(managedArmy != null ? managedArmy.id : queue[0].id);
                sb.AppendLine();
                int count = Mathf.Min(queue.Count, 2);
                for (int i = 0; i < count; i++)
                {
                    ArmyRuntimeState army = queue[i];
                    sb.Append(army.id);
                    sb.Append(" ");
                    sb.Append(FormatLogisticsPriority(army.frontlineLogisticsPriority));
                    sb.Append(army.frontlineLogisticsPaused ? " 暂停" : " 执行");
                    sb.Append(" ");
                    sb.Append(army.frontlineLogisticsCompletedSegments);
                    sb.Append("/");
                    sb.Append(army.frontlineLogisticsTotalTurns);
                    sb.Append(" -> ");
                    sb.Append(GetRegionName(army.frontlineLogisticsTargetRegionId));
                    sb.Append(" 截");
                    sb.Append(army.frontlineLogisticsRaidPressure);
                    if (i + 1 < count) sb.AppendLine();
                }

                logisticsQueueText.text = sb.ToString();
            }

            bool canManage = managedArmy != null && HasActiveLogistics(managedArmy);
            SetButtonInteractable(logisticsPriorityUpButton, canManage && managedArmy.frontlineLogisticsPriority < 2);
            SetButtonInteractable(logisticsPriorityDownButton, canManage && managedArmy.frontlineLogisticsPriority > 0);
            SetButtonInteractable(logisticsPauseButton, canManage);
            SetButtonInteractable(logisticsCancelButton, canManage);
            SetButtonLabel(logisticsPauseButton, canManage && managedArmy.frontlineLogisticsPaused ? "继续" : "暂停");
        }

        private List<ArmyRuntimeState> CollectPlayerLogisticsQueue()
        {
            List<ArmyRuntimeState> queue = new List<ArmyRuntimeState>();
            if (gameManager == null || gameManager.State == null || gameManager.World == null || gameManager.World.Map == null) return queue;

            foreach (ArmyRuntimeState army in gameManager.World.Map.ArmiesById.Values)
            {
                if (army == null || army.ownerFactionId != gameManager.State.playerFactionId || !HasActiveLogistics(army)) continue;
                queue.Add(army);
            }

            queue.Sort(CompareLogisticsQueueForUi);
            return queue;
        }

        private static int CompareLogisticsQueueForUi(ArmyRuntimeState left, ArmyRuntimeState right)
        {
            if (left == null && right == null) return 0;
            if (left == null) return 1;
            if (right == null) return -1;
            int priority = right.frontlineLogisticsPriority.CompareTo(left.frontlineLogisticsPriority);
            if (priority != 0) return priority;
            int paused = left.frontlineLogisticsPaused.CompareTo(right.frontlineLogisticsPaused);
            if (paused != 0) return paused;
            return string.Compare(left.id, right.id, System.StringComparison.Ordinal);
        }

        private string ResolveManagedLogisticsArmyId(List<ArmyRuntimeState> queue)
        {
            if (!string.IsNullOrEmpty(selectedLogisticsArmyId) && gameManager != null && gameManager.MapQueries != null)
            {
                ArmyRuntimeState selected = gameManager.MapQueries.GetArmy(selectedLogisticsArmyId);
                if (HasActiveLogistics(selected)) return selectedLogisticsArmyId;
                selectedLogisticsArmyId = null;
            }

            if (selectionContext != null && !string.IsNullOrEmpty(selectionContext.selectedArmyId) && gameManager != null && gameManager.MapQueries != null)
            {
                ArmyRuntimeState selectedArmy = gameManager.MapQueries.GetArmy(selectionContext.selectedArmyId);
                if (HasActiveLogistics(selectedArmy))
                {
                    selectedLogisticsArmyId = selectedArmy.id;
                    return selectedArmy.id;
                }
            }

            if (queue != null && queue.Count > 0)
            {
                selectedLogisticsArmyId = queue[0].id;
                return queue[0].id;
            }

            return null;
        }

        private void AdjustSelectedLogisticsPriority(int delta)
        {
            string armyId = ResolveManagedLogisticsArmyId(CollectPlayerLogisticsQueue());
            if (string.IsNullOrEmpty(armyId) || gameManager == null) return;

            gameManager.AdjustFrontlineLogisticsPriority(armyId, delta);
            selectedLogisticsArmyId = armyId;
            RefreshHUD();
        }

        private void ToggleSelectedLogisticsPause()
        {
            string armyId = ResolveManagedLogisticsArmyId(CollectPlayerLogisticsQueue());
            if (string.IsNullOrEmpty(armyId) || gameManager == null) return;

            gameManager.ToggleFrontlineLogisticsPause(armyId);
            selectedLogisticsArmyId = armyId;
            RefreshHUD();
        }

        private void CancelSelectedLogistics()
        {
            string armyId = ResolveManagedLogisticsArmyId(CollectPlayerLogisticsQueue());
            if (string.IsNullOrEmpty(armyId) || gameManager == null) return;

            gameManager.CancelFrontlineLogistics(armyId);
            if (selectedLogisticsArmyId == armyId) selectedLogisticsArmyId = null;
            RefreshHUD();
        }

        private static bool HasActiveLogistics(ArmyRuntimeState army)
        {
            return army != null && !string.IsNullOrEmpty(army.frontlineLogisticsTargetRegionId);
        }

        private static string FormatLogisticsPriority(int priority)
        {
            if (priority >= 2) return "加急";
            if (priority <= 0) return "后置";
            return "常规";
        }

        private static void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null) button.interactable = interactable;
        }

        private static void SetButtonLabel(Button button, string label)
        {
            if (button == null) return;
            Text text = button.GetComponentInChildren<Text>();
            if (text != null) text.text = label;
        }

        private void CreateStrategyOutlinerEntryButton(int index, StrategyOutlinerEntry entry)
        {
            if (strategyOutlinerRoot == null || entry == null) return;

            string label = ResolveOutlinerButtonLabel(entry);
            if (label.Length > 30)
            {
                label = label.Substring(0, 28) + "..";
            }

            Button button = CreateRuntimeButton(
                strategyOutlinerRoot.transform,
                "StrategyOutlinerEntryButton_" + index,
                label,
                new Vector2(148, StrategyLayoutSpec.OutlinerEntryStartY - index * StrategyLayoutSpec.OutlinerEntryStepY),
                new Vector2(0.5f, 1),
                new Vector2(266, 24));
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
            if (category == "frontline_logistics")
            {
                selectedLogisticsArmyId = targetId;
            }
            if (IsArmyOutlinerCategory(category))
            {
                ArmyRuntimeState army = gameManager.MapQueries != null ? gameManager.MapQueries.GetArmy(targetId) : null;
                if (army != null)
                {
                    regionId = !string.IsNullOrEmpty(army.targetRegionId) ? army.targetRegionId : army.locationRegionId;
                }
            }

            if (string.IsNullOrEmpty(regionId)) return;

            gameManager.State.AddLog("ui", "待办选中 " + index + ": " + ResolveOutlinerCategoryLogLabel(category) + " -> " + GetRegionName(regionId));
            SelectAndFocusMapRegion(regionId, "待办定位");
            RefreshLogisticsQueuePanel();
        }

        private void SelectAndFocusMapRegion(string regionId, string logPrefix)
        {
            if (gameManager == null || gameManager.Events == null || string.IsNullOrEmpty(regionId)) return;

            if (gameManager.State != null)
            {
                gameManager.State.AddLog("ui", logPrefix + " -> " + regionId);
            }

            gameManager.Events.Publish(new GameEvent(GameEventType.RegionSelected, regionId, null));
            MapRenderer renderer = Object.FindObjectOfType<MapRenderer>();
            if (renderer != null)
            {
                renderer.PulseRegionFocus(regionId);
            }

            Vector2 center;
            if (!TryResolveRegionWorldCenter(regionId, out center)) return;

            CameraController controller = Object.FindObjectOfType<CameraController>();
            if (controller != null)
            {
                controller.SmoothCenterOnRegion(center);
            }
        }

        private void AppendOutlinerSummary(System.Text.StringBuilder sb, List<StrategyOutlinerEntry> entries)
        {
            if (sb == null) return;
            sb.AppendLine("图层 " + FormatLensName(currentLens) + " | 待办 " + (entries != null ? entries.Count : 0));
            if (entries == null || entries.Count == 0) return;

            List<string> groups = new List<string>();
            List<int> counts = new List<int>();
            for (int i = 0; i < entries.Count; i++)
            {
                StrategyOutlinerEntry entry = entries[i];
                string label = ResolveOutlinerGroupLabel(entry);
                int index = groups.IndexOf(label);
                if (index < 0)
                {
                    groups.Add(label);
                    counts.Add(1);
                }
                else
                {
                    counts[index] = counts[index] + 1;
                }
            }

            int visibleGroups = Mathf.Min(groups.Count, 5);
            for (int i = 0; i < visibleGroups; i++)
            {
                if (i > 0) sb.Append("  ");
                sb.Append(groups[i]);
                sb.Append(" ");
                sb.Append(counts[i]);
            }
        }

        private string ResolveOutlinerButtonLabel(StrategyOutlinerEntry entry)
        {
            if (entry == null) return "";
            string targetName = IsArmyOutlinerCategory(entry.category) || entry.category == "frontline_prepared" || entry.category == "frontline_logistics"
                ? entry.label
                : GetRegionName(entry.targetId);
            if (string.IsNullOrEmpty(targetName) || targetName == entry.targetId)
            {
                targetName = entry.targetId;
            }

            return ResolveOutlinerGroupLabel(entry) + " · " + targetName;
        }

        private static bool IsArmyOutlinerCategory(string category)
        {
            return category == "marching_army" || category == "low_supply" || category == "frontline_logistics";
        }

        private static string ResolveOutlinerCategoryLogLabel(string category)
        {
            switch (category)
            {
                case "critical_region": return "高风险地区";
                case "acceptance": return "高风险地区";
                case "occupation_chain": return "新占治理";
                case "occupation_queue": return "新占治理";
                case "marching_army": return "行军军队";
                case "low_supply": return "低补给军队";
                case "frontline_prepared": return "前线整备";
                case "frontline_logistics": return "前线补给";
                case "latest_report": return "最新战报";
                default: return string.IsNullOrEmpty(category) ? "未分类" : category;
            }
        }

        private static string ResolveOutlinerGroupLabel(StrategyOutlinerEntry entry)
        {
            if (entry == null) return "其他";
            if (!string.IsNullOrEmpty(entry.groupLabel)) return entry.groupLabel;
            if (entry.category == "critical_region" || entry.category == "acceptance") return "高风险地区";
            if (entry.category == "occupation_chain" || entry.category == "occupation_queue") return "新占治理";
            if (entry.category == "marching_army") return "行军军队";
            if (entry.category == "low_supply" || entry.category == "frontline_prepared" || entry.category == "frontline_logistics") return "前线补给";
            if (entry.category == "latest_report") return "最新战报";
            return "其他";
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
                    collapsedRect.anchoredPosition = forceCollapsed ? ResolveCollapsedOutlinerAvoidPosition(collapsedRect) : StrategyLayoutSpec.OutlinerDockedPosition;
                }

                if (showCollapsed) UIPanelVisibility.Show(strategyOutlinerCollapsedRoot);
                else UIPanelVisibility.Hide(strategyOutlinerCollapsedRoot);
            }

            if (logisticsQueuePanel != null)
            {
                if (forceCollapsed) UIPanelVisibility.Hide(logisticsQueuePanel);
                else UIPanelVisibility.Show(logisticsQueuePanel);
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

        private Vector2 ResolveCollapsedOutlinerAvoidPosition(RectTransform collapsedRect)
        {
            RectTransform parentRect = collapsedRect != null ? collapsedRect.parent as RectTransform : null;
            RectTransform regionRect = regionPanel != null ? regionPanel.GetComponent<RectTransform>() : null;
            if (parentRect == null || regionRect == null)
            {
                return StrategyLayoutSpec.OutlinerRegionPanelAvoidPosition;
            }

            Rect regionLocalRect = ResolveLocalRect(parentRect, regionRect);
            Rect parentLocalRect = parentRect.rect;
            Vector2 size = collapsedRect.sizeDelta;
            const float gap = 12f;

            float chipRight = Mathf.Clamp(
                regionLocalRect.xMin - gap,
                parentLocalRect.xMin + size.x + gap,
                parentLocalRect.xMax - gap);
            float chipTop = Mathf.Clamp(
                parentLocalRect.yMax + StrategyLayoutSpec.OutlinerDockedPosition.y,
                parentLocalRect.yMin + size.y + gap,
                parentLocalRect.yMax - gap);

            return new Vector2(chipRight - parentLocalRect.xMax, chipTop - parentLocalRect.yMax);
        }

        private static Rect ResolveLocalRect(RectTransform parent, RectTransform child)
        {
            Vector3[] corners = new Vector3[4];
            child.GetWorldCorners(corners);
            Vector3 bottomLeft = parent.InverseTransformPoint(corners[0]);
            Vector3 topRight = parent.InverseTransformPoint(corners[2]);
            return Rect.MinMaxRect(bottomLeft.x, bottomLeft.y, topRight.x, topRight.y);
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
            image.color = UITheme.PanelBackgroundAlpha;
            Shadow shadow = panel.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.48f);
            shadow.effectDistance = new Vector2(4f, -4f);
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = UITheme.ButtonBorder;
            outline.effectDistance = new Vector2(1f, -1f);
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
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.lineSpacing = 1.05f;
            text.supportRichText = true;
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
            Shadow shadow = obj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.36f);
            shadow.effectDistance = new Vector2(2f, -2f);
            Outline outline = obj.AddComponent<Outline>();
            outline.effectColor = UITheme.ButtonBorder;
            outline.effectDistance = new Vector2(1f, -1f);
            Button button = obj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = UITheme.ButtonNormal;
            colors.highlightedColor = UITheme.ButtonHover;
            colors.pressedColor = UITheme.ButtonPressed;
            button.colors = colors;

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
