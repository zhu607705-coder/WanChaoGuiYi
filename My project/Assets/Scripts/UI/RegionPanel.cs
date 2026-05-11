using UnityEngine;
using UnityEngine.UI;

namespace WanChaoGuiYi
{
    public sealed class RegionPanel : MonoBehaviour
    {
        private const int PacifyMoneyCost = 50;
        private const int PacifyFoodCost = 30;
        private const int PacifyIntegrationGain = 10;
        private const int PacifyRebellionReduction = 12;
        private const int PacifyLocalPowerReduction = 5;

        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text regionNameText;
        [SerializeField] private Text terrainText;
        [SerializeField] private Text populationText;
        [SerializeField] private Text foodText;
        [SerializeField] private Text taxText;
        [SerializeField] private Text manpowerText;
        [SerializeField] private Text ownerText;
        [SerializeField] private Text integrationText;
        [SerializeField] private Text rebellionText;
        [SerializeField] private Text annexationText;
        [SerializeField] private Text localPowerText;
        [SerializeField] private Text neighborsText;
        [SerializeField] private Text landStructureText;
        [SerializeField] private Text customsText;
        [SerializeField] private Text governanceOverviewText;
        [SerializeField] private Text governanceSourceText;
        [SerializeField] private Text modeText;
        [SerializeField] private GameObject collapsedRoot;
        [SerializeField] private Text collapsedTabText;
        [SerializeField] private Button pacifyButton;
        [SerializeField] private Button buildButton;
        [SerializeField] private Button collapseButton;
        [SerializeField] private Button expandButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text pacifyActionHintText;
        [SerializeField] private Text buildActionHintText;
        [SerializeField] private Text governanceStageBadgeText;
        [SerializeField] private Text governanceRiskBadgeText;
        [SerializeField] private Text governanceIntegrationBadgeText;
        [SerializeField] private Text governanceContributionBadgeText;
        [SerializeField] private Image governanceIntegrationBarFill;
        [SerializeField] private Image governanceFoodBarFill;
        [SerializeField] private Image governanceTaxBarFill;
        [SerializeField] private Image governanceRebellionBarFill;
        [SerializeField] private Image pacifyActionIcon;
        [SerializeField] private Image buildActionIcon;

        private GameContext context;
        private FactionState faction;
        private RegionDefinition regionDefinition;
        private RegionState regionState;
        private BuildingSystem buildingSystem;
        private MapInteractionMode currentMode = MapInteractionMode.Governance;
        private string currentModeReason;
        private string currentWarForecast;
        private string currentRegionName;
        private bool currentSelectionIsFriendly = true;
        private bool isCollapsed;

        public string CurrentRegionId { get; private set; }

        public void Bind(GameObject root, Text regionName, Text terrain, Text population, Text food, Text tax, Text manpower, Text owner, Text integration, Text rebellion, Text annexation, Text localPower, Text neighbors, Text landStructure, Text customs, Text governanceOverview, Text governanceSource, Button pacify, Button build, Button close, Button collapse, GameObject collapsed, Button expand, Text collapsedText, Text mode)
        {
            panelRoot = root;
            regionNameText = regionName;
            terrainText = terrain;
            populationText = population;
            foodText = food;
            taxText = tax;
            manpowerText = manpower;
            ownerText = owner;
            integrationText = integration;
            rebellionText = rebellion;
            annexationText = annexation;
            localPowerText = localPower;
            neighborsText = neighbors;
            landStructureText = landStructure;
            customsText = customs;
            governanceOverviewText = governanceOverview;
            governanceSourceText = governanceSource;
            pacifyButton = pacify;
            buildButton = build;
            closeButton = close;
            collapseButton = collapse;
            collapsedRoot = collapsed;
            expandButton = expand;
            collapsedTabText = collapsedText;
            modeText = mode;
            BindGovernanceVisuals();
            BindButtons();
            Hide();
        }

        private void Awake()
        {
            UIPanelVisibility.Hide(panelRoot);
            UIPanelVisibility.Hide(collapsedRoot);
            BindButtons();
        }

        private void BindButtons()
        {
            if (pacifyButton != null)
            {
                pacifyButton.onClick.RemoveListener(PacifyRegion);
                pacifyButton.onClick.AddListener(PacifyRegion);
            }

            if (buildButton != null)
            {
                buildButton.onClick.RemoveListener(BuildFirstAvailableBuilding);
                buildButton.onClick.AddListener(BuildFirstAvailableBuilding);
            }

            if (collapseButton != null)
            {
                collapseButton.onClick.RemoveListener(CollapseToTab);
                collapseButton.onClick.AddListener(CollapseToTab);
            }

            if (expandButton != null)
            {
                expandButton.onClick.RemoveListener(ExpandFromTab);
                expandButton.onClick.AddListener(ExpandFromTab);
            }

            if (closeButton == null) return;

            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(Hide);
        }

        public void Show(RegionDefinition definition, RegionState state)
        {
            Show(definition, state, null, null, null);
        }

        public void Show(RegionDefinition definition, RegionState state, GameContext gameContext, FactionState ownerFaction, BuildingSystem buildings)
        {
            if (definition == null || state == null) return;

            context = gameContext;
            faction = ownerFaction;
            regionDefinition = definition;
            regionState = state;
            buildingSystem = buildings;

            CurrentRegionId = definition.id;
            currentRegionName = definition.name;
            ApplyVisibilityState();

            SetText(regionNameText, definition.name);
            SetText(terrainText, "地形：" + FormatTerrain(definition.terrain));
            SetText(populationText, "人口：" + FormatPopulation(state.population));
            SetText(foodText, "粮食产出：" + state.foodOutput);
            SetText(taxText, "税收产出：" + state.taxOutput);
            SetText(manpowerText, "兵源：" + state.manpower);
            SetText(integrationText, "整合度：" + state.integration + "%");
            SetText(rebellionText, "民变风险：" + state.rebellionRisk + "%");
            SetText(annexationText, "土地兼并：" + state.annexationPressure + "%");
            SetText(localPowerText, "地方势力：" + state.localPower);
            SetText(neighborsText, "相邻：" + FormatNeighbors(definition.neighbors));
            SetText(landStructureText, FormatLandStructure(state.landStructure));
            SetText(customsText, FormatCustoms(state.customs, state.customStability) + "\n" + FormatBuildings(state));
            RefreshRegionTextBlocks();
            UpdateCollapsedTab();
            ApplyModeActionState();
            UpdateGovernanceVisuals();
        }

        public void SetMode(MapInteractionMode mode, bool isFriendlySelection, string modeEntryReason, string warForecast)
        {
            currentMode = mode;
            currentSelectionIsFriendly = isFriendlySelection;
            currentModeReason = modeEntryReason;
            currentWarForecast = warForecast;
            SetText(modeText, FormatModeHeader());
            RefreshRegionTextBlocks();
            UpdateCollapsedTab();
            ApplyModeActionState();
            UpdateGovernanceVisuals();
        }

        public void SetOwner(string ownerName)
        {
            SetText(ownerText, "归属：" + ownerName);
        }

        public void Hide()
        {
            UIPanelVisibility.Hide(panelRoot);
            UIPanelVisibility.Hide(collapsedRoot);
            CurrentRegionId = null;
            context = null;
            faction = null;
            regionDefinition = null;
            regionState = null;
            buildingSystem = null;
            currentRegionName = null;
            currentMode = MapInteractionMode.Governance;
            currentModeReason = null;
            currentWarForecast = null;
            currentSelectionIsFriendly = true;
            isCollapsed = false;
            SetGovernanceVisualsVisible(false);
            SetActionHintsVisible(false);
        }

        private void CollapseToTab()
        {
            if (string.IsNullOrEmpty(CurrentRegionId)) return;

            isCollapsed = true;
            ApplyVisibilityState();
        }

        private void ExpandFromTab()
        {
            if (string.IsNullOrEmpty(CurrentRegionId)) return;

            isCollapsed = false;
            ApplyVisibilityState();
        }

        private void ApplyVisibilityState()
        {
            if (isCollapsed)
            {
                UIPanelVisibility.Hide(panelRoot);
                UIPanelVisibility.Show(collapsedRoot);
            }
            else
            {
                UIPanelVisibility.Show(panelRoot);
                UIPanelVisibility.Hide(collapsedRoot);
            }

            UpdateCollapsedTab();
        }

        private void ApplyModeActionState()
        {
            GovernanceActionForecast forecast = ResolvePrimaryGovernanceActionForecast(regionState);
            if (pacifyButton != null)
            {
                pacifyButton.interactable = CanPacifyCurrentRegion();
                Text label = FindButtonLabel(pacifyButton);
                if (label != null && forecast != null && forecast.action != GovernanceActionKind.Hold)
                {
                    label.text = FormatGovernanceActionName(forecast.action);
                }
            }
            if (buildButton != null)
            {
                buildButton.interactable = CanBuildCurrentRegion();
                Text label = FindButtonLabel(buildButton);
                if (label != null) label.text = "建造";
            }

            UpdateActionHints(forecast);
        }

        private void UpdateCollapsedTab()
        {
            if (collapsedTabText == null) return;

            string regionLabel = string.IsNullOrEmpty(currentRegionName) ? "未选区" : currentRegionName;
            collapsedTabText.text = FormatModeShortName(currentMode) + "\n" + regionLabel;
        }

        private string FormatModeHeader()
        {
            string header = FormatModeShortName(currentMode);
            switch (currentMode)
            {
                case MapInteractionMode.Governance:
                    header += currentSelectionIsFriendly ? " | 国内政务" : " | 非本国地区";
                    break;
                case MapInteractionMode.War:
                    header += " | 行军与接敌";
                    break;
                case MapInteractionMode.Diplomacy:
                    header += " | 邻国外交";
                    break;
            }

            if (!string.IsNullOrEmpty(currentModeReason))
            {
                header += " | " + FormatModeReason(currentModeReason);
            }

            return header;
        }

        private static string FormatModeShortName(MapInteractionMode mode)
        {
            switch (mode)
            {
                case MapInteractionMode.Governance: return "治理模式";
                case MapInteractionMode.War: return "战争模式";
                case MapInteractionMode.Diplomacy: return "外交过渡";
                default: return mode.ToString();
            }
        }

        private static string FormatModeReason(string reason)
        {
            switch (reason)
            {
                case "friendly_region": return "己方地区";
                case "war_neighbor_target": return "邻接目标";
                case "war_foreign_target": return "远征目标";
                case "neighbor_region": return "邻国接壤";
                case "distant_foreign_region": return "远邻地区";
                case "missing_state": return "状态缺失";
                case "region_not_found": return "地区缺失";
                default: return reason;
            }
        }

        private void PacifyRegion()
        {
            if (context == null || faction == null || regionState == null) return;

            GovernanceActionForecast forecast = ResolvePrimaryGovernanceActionForecast(regionState);
            if (forecast == null || forecast.action == GovernanceActionKind.Hold || !forecast.canApply)
            {
                context.State.AddLog("governance", "recommended governance rejected: " + (forecast != null ? forecast.disabledReason : "missing_forecast"));
                RefreshCurrentRegion();
                return;
            }

            forecast = StrategyMapRulebook.ApplyGovernanceAction(context, regionDefinition, regionState, faction, forecast.action);
            if (forecast == null || !forecast.canApply)
            {
                context.State.AddLog("governance", "recommended governance rejected: " + (forecast != null ? forecast.disabledReason : "missing_forecast"));
                RefreshCurrentRegion();
                return;
            }

            SyncRuntimeRegionFromState();
            RefreshCurrentRegion();
            if (forecast.action != GovernanceActionKind.Hold)
            {
                context.State.AddLog("governance", faction.name + " " + forecast.label + " " + regionState.id + " | " + forecast.reason);
                return;
            }

            if (faction.money < PacifyMoneyCost || faction.food < PacifyFoodCost)
            {
                context.State.AddLog("governance", "安抚地区资源不足：需要金钱" + PacifyMoneyCost + "、粮食" + PacifyFoodCost + "。");
                RefreshCurrentRegion();
                return;
            }

            faction.money -= PacifyMoneyCost;
            faction.food -= PacifyFoodCost;
            regionState.integration = Mathf.Min(100, regionState.integration + PacifyIntegrationGain);
            regionState.rebellionRisk = Mathf.Max(0, regionState.rebellionRisk - PacifyRebellionReduction);
            regionState.localPower = Mathf.Max(0, regionState.localPower - PacifyLocalPowerReduction);
            context.State.AddLog("governance", faction.name + "安抚" + regionState.id + "，整合提升、民变风险下降。");
            RefreshCurrentRegion();
        }

        private void BuildFirstAvailableBuilding()
        {
            if (context == null || faction == null || regionState == null || buildingSystem == null) return;

            BuildingDefinition building = FindFirstBuildableBuilding();
            if (building == null)
            {
                context.State.AddLog("building", "当前地区没有可建造建筑。");
                RefreshCurrentRegion();
                return;
            }

            bool built = buildingSystem.Build(context, faction, regionState, building.id);
            if (!built)
            {
                context.State.AddLog("building", "建筑建造失败：" + building.name);
            }

            RefreshCurrentRegion();
        }

        private BuildingDefinition FindFirstBuildableBuilding()
        {
            if (context == null || faction == null || regionState == null || buildingSystem == null) return null;

            foreach (BuildingDefinition building in context.Data.Buildings.Values)
            {
                if (building == null) continue;
                if (buildingSystem.CanBuild(context, faction, regionState, building.id)) return building;
            }

            return null;
        }

        private void RefreshCurrentRegion()
        {
            if (context == null || regionState == null || string.IsNullOrEmpty(CurrentRegionId)) return;

            RegionDefinition definition = context.Data.GetRegion(CurrentRegionId);
            Show(definition, regionState, context, faction, buildingSystem);
        }

        private string FormatBuildings(RegionState state)
        {
            if (state == null || state.buildings == null || state.buildings.Count == 0) return "建筑：无";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("建筑：");
            for (int i = 0; i < state.buildings.Count; i++)
            {
                if (i > 0) sb.Append("、");
                BuildingDefinition building = null;
                if (context != null) context.Data.Buildings.TryGetValue(state.buildings[i], out building);
                sb.Append(building != null ? building.name : state.buildings[i]);
            }

            return sb.ToString();
        }

        private void RefreshRegionTextBlocks()
        {
            if (regionDefinition == null || regionState == null) return;

            SetText(governanceOverviewText, FormatRegionOverview(regionDefinition, regionState));
            SetText(governanceSourceText, FormatRegionSourceDetails(regionDefinition, regionState));
            UpdateGovernanceVisuals();
        }

        private void BindGovernanceVisuals()
        {
            if (panelRoot == null) return;

            governanceStageBadgeText = FindPanelComponent<Text>("GovernanceStageBadgeText");
            governanceRiskBadgeText = FindPanelComponent<Text>("GovernanceRiskBadgeText");
            governanceIntegrationBadgeText = FindPanelComponent<Text>("GovernanceIntegrationBadgeText");
            governanceContributionBadgeText = FindPanelComponent<Text>("GovernanceContributionBadgeText");
            governanceIntegrationBarFill = FindPanelComponent<Image>("GovernanceIntegrationUiBarFill");
            governanceFoodBarFill = FindPanelComponent<Image>("GovernanceFoodUiBarFill");
            governanceTaxBarFill = FindPanelComponent<Image>("GovernanceTaxUiBarFill");
            governanceRebellionBarFill = FindPanelComponent<Image>("GovernanceRebellionUiBarFill");
            pacifyActionHintText = FindPanelComponent<Text>("GovernancePacifyActionHintText");
            buildActionHintText = FindPanelComponent<Text>("GovernanceBuildActionHintText");
            pacifyActionIcon = FindPanelComponent<Image>("GovernancePacifyActionIcon");
            buildActionIcon = FindPanelComponent<Image>("GovernanceBuildActionIcon");
        }

        private void UpdateGovernanceVisuals()
        {
            bool visible = currentMode == MapInteractionMode.Governance && regionState != null;
            SetGovernanceVisualsVisible(visible);
            if (!visible) return;

            SetBadge(governanceStageBadgeText, FormatControlStageName(StrategyMapRulebook.ResolveControlStage(regionState)), UITheme.BadgeNormal);
            SetBadge(governanceRiskBadgeText, regionState.rebellionRisk >= 65 ? "高民变" : regionState.rebellionRisk >= 35 ? "民变关注" : "民心稳", ResolveBadgeColor(regionState.rebellionRisk, true));
            SetBadge(governanceIntegrationBadgeText, regionState.integration < 45 ? "低整合" : regionState.integration < 70 ? "整合中" : "整合稳", ResolveBadgeColor(regionState.integration, false));
            bool contributionReduced = regionState.taxContributionPercent < StrategyMapRulebook.ControlledContributionPercent || regionState.foodContributionPercent < StrategyMapRulebook.ControlledContributionPercent;
            SetBadge(governanceContributionBadgeText, contributionReduced ? "贡献折损" : "贡献正常", contributionReduced ? UITheme.BadgeWarning : UITheme.BadgeNormal);
            SetBar(governanceIntegrationBarFill, regionState.integration, ResolveMeterColor(regionState.integration, false));
            SetBar(governanceFoodBarFill, regionState.foodContributionPercent, UITheme.MeterFood);
            SetBar(governanceTaxBarFill, regionState.taxContributionPercent, UITheme.MeterTax);
            SetBar(governanceRebellionBarFill, regionState.rebellionRisk, ResolveMeterColor(regionState.rebellionRisk, true));
        }

        private void SetGovernanceVisualsVisible(bool visible)
        {
            SetVisualVisible(governanceStageBadgeText, visible);
            SetVisualVisible(governanceRiskBadgeText, visible);
            SetVisualVisible(governanceIntegrationBadgeText, visible);
            SetVisualVisible(governanceContributionBadgeText, visible);
            SetVisualVisible(governanceIntegrationBarFill, visible);
            SetVisualVisible(governanceFoodBarFill, visible);
            SetVisualVisible(governanceTaxBarFill, visible);
            SetVisualVisible(governanceRebellionBarFill, visible);
        }

        private void UpdateActionHints(GovernanceActionForecast forecast)
        {
            bool visible = regionState != null;
            SetActionHintsVisible(visible);
            if (!visible) return;

            SetActionHint(pacifyActionHintText, FormatGovernanceActionButtonHint(forecast), CanPacifyCurrentRegion());
            SetActionHint(buildActionHintText, FormatBuildActionButtonHint(), CanBuildCurrentRegion());
            SetActionIcon(pacifyActionIcon, CanPacifyCurrentRegion(), UITheme.MeterGood);
            SetActionIcon(buildActionIcon, CanBuildCurrentRegion(), UITheme.MoneyColor);
        }

        private void SetActionHintsVisible(bool visible)
        {
            if (pacifyActionHintText != null) pacifyActionHintText.gameObject.SetActive(visible);
            if (buildActionHintText != null) buildActionHintText.gameObject.SetActive(visible);
            if (pacifyActionIcon != null) pacifyActionIcon.gameObject.SetActive(visible);
            if (buildActionIcon != null) buildActionIcon.gameObject.SetActive(visible);
        }

        private static void SetActionHint(Text text, string value, bool isReady)
        {
            if (text == null) return;
            text.text = value;
            text.color = isReady ? UITheme.TextAccent : UITheme.TextSecondary;
        }

        private static void SetActionIcon(Image image, bool isReady, Color readyColor)
        {
            if (image == null) return;
            image.color = isReady ? readyColor : new Color(UITheme.TextSecondary.r, UITheme.TextSecondary.g, UITheme.TextSecondary.b, 0.46f);
        }

        private static Text FindButtonLabel(Button button)
        {
            if (button == null) return null;
            Transform label = FindChildRecursive(button.transform, "Label");
            return label != null ? label.GetComponent<Text>() : button.GetComponentInChildren<Text>();
        }

        private static void SetVisualVisible(Component component, bool visible)
        {
            if (component == null || component.transform == null) return;
            Transform root = component.transform.parent != null ? component.transform.parent : component.transform;
            root.gameObject.SetActive(visible);
        }

        private static void SetBadge(Text text, string label, Color background)
        {
            if (text == null) return;
            text.text = label;
            text.color = UITheme.ButtonText;
            Image image = text.transform.parent != null ? text.transform.parent.GetComponent<Image>() : null;
            if (image != null) image.color = background;
        }

        private static void SetBar(Image fill, int value, Color color)
        {
            if (fill == null) return;

            RectTransform parent = fill.transform.parent != null ? fill.transform.parent.GetComponent<RectTransform>() : null;
            RectTransform fillRect = fill.rectTransform;
            float maxWidth = parent != null ? parent.sizeDelta.x : fillRect.sizeDelta.x;
            float width = Mathf.Max(1f, maxWidth * Mathf.Clamp01(value / 100f));
            fillRect.sizeDelta = new Vector2(width, fillRect.sizeDelta.y);
            fill.color = color;
        }

        private static Color ResolveBadgeColor(int value, bool inverseGood)
        {
            if (inverseGood)
            {
                if (value >= 65) return UITheme.BadgeDanger;
                if (value >= 35) return UITheme.BadgeWarning;
                return UITheme.BadgeNormal;
            }

            if (value < 45) return UITheme.BadgeDanger;
            if (value < 70) return UITheme.BadgeWarning;
            return UITheme.BadgeNormal;
        }

        private static Color ResolveMeterColor(int value, bool inverseGood)
        {
            if (inverseGood)
            {
                if (value >= 65) return UITheme.MeterDanger;
                if (value >= 35) return UITheme.MeterWarning;
                return UITheme.MeterGood;
            }

            if (value < 45) return UITheme.MeterDanger;
            if (value < 70) return UITheme.MeterWarning;
            return UITheme.MeterGood;
        }

        private T FindPanelComponent<T>(string objectName) where T : Component
        {
            if (panelRoot == null || string.IsNullOrEmpty(objectName)) return null;
            Transform child = FindChildRecursive(panelRoot.transform, objectName);
            return child != null ? child.GetComponent<T>() : null;
        }

        private static Transform FindChildRecursive(Transform root, string objectName)
        {
            if (root == null) return null;
            if (root.name == objectName) return root;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChildRecursive(root.GetChild(i), objectName);
                if (found != null) return found;
            }

            return null;
        }

        private string FormatRegionOverview(RegionDefinition definition, RegionState state)
        {
            if (currentMode == MapInteractionMode.War)
            {
                return FormatWarOverview(definition, state);
            }

            if (currentMode == MapInteractionMode.Diplomacy)
            {
                return FormatDiplomacyOverview(definition, state);
            }

            return FormatGovernanceOverview(definition, state);
        }

        private string FormatGovernanceOverview(RegionDefinition definition, RegionState state)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            RegionSpecializationPlanForecast plan = StrategyMapRulebook.BuildSpecializationPlanForecast(context, definition, state, faction);
            sb.AppendLine("本回合摘要 | 政务 民生 粮政 | 状态徽标 治理条 产出条");
            sb.AppendLine("粮政 人口 法统 | " + FormatSpecializationLine(definition, state) + " | 专精路线 " + plan.routeStage + "/" + plan.buildingFocus);
            sb.AppendLine("风险 决策 推荐 | " + FormatPrimaryDecisionLine(state) + " | 路线收益 " + plan.expectedBenefit);
            sb.AppendLine("最优行动：" + ResolveBestGovernanceAction(state) + " | 可执行政务 | 预计效果: " + FormatCompactExpectedGovernanceEffect(state));
            sb.AppendLine("按钮状态: " + FormatGovernanceActionState() + " | 建设 " + FormatBuildingDecision() + " | 政策 " + FormatPolicyDecision() + " | 取舍 " + plan.tradeoff);
            return sb.ToString();
        }

        private static string FormatGovernanceBadges(RegionState state)
        {
            if (state == null) return "[状态缺失]";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            AppendBadge(sb, FormatControlStageName(StrategyMapRulebook.ResolveControlStage(state)));
            AppendBadge(sb, state.rebellionRisk >= 65 ? "高民变" : state.rebellionRisk >= 35 ? "民变关注" : "民心稳");
            AppendBadge(sb, state.integration < 45 ? "低整合" : state.integration < 70 ? "整合中" : "整合稳");
            AppendBadge(sb, state.taxContributionPercent < StrategyMapRulebook.ControlledContributionPercent || state.foodContributionPercent < StrategyMapRulebook.ControlledContributionPercent ? "贡献折损" : "贡献正常");
            return sb.ToString();
        }

        private static void AppendBadge(System.Text.StringBuilder sb, string label)
        {
            if (sb.Length > 0) sb.Append(" ");
            sb.Append("[");
            sb.Append(label);
            sb.Append("]");
        }

        private static string FormatMeter(int value, bool inverseGood)
        {
            int clamped = Mathf.Clamp(value, 0, 100);
            int filled = Mathf.Clamp(Mathf.RoundToInt(clamped / 20f), 0, 5);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("[");
            for (int i = 0; i < 5; i++)
            {
                sb.Append(i < filled ? "#" : "-");
            }
            sb.Append("]");
            sb.Append(clamped);
            sb.Append("%");
            if (inverseGood)
            {
                sb.Append(clamped >= 65 ? " 高" : clamped >= 35 ? " 中" : " 低");
            }
            else
            {
                sb.Append(clamped >= 70 ? " 稳" : clamped >= 45 ? " 中" : " 低");
            }

            return sb.ToString();
        }

        private string FormatRegionSourceDetails(RegionDefinition definition, RegionState state)
        {
            if (currentMode == MapInteractionMode.War)
            {
                return FormatWarSourceDetails();
            }

            if (currentMode == MapInteractionMode.Diplomacy)
            {
                return FormatDiplomacySourceDetails();
            }

            return FormatGovernanceSourceDetails(definition, state);
        }

        private string FormatGovernanceSourceDetails(RegionDefinition definition, RegionState state)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("因果: " + FormatGovernanceCausalLine(state));
            AppendCausalFeedback(sb, state);
            AppendSourceNotes(sb, definition);
            return sb.ToString();
        }

        private string FormatWarOverview(RegionDefinition definition, RegionState state)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("战争态势 | 战前压力");
            sb.AppendLine("目标地区: " + SafeRegionName(definition) + " | 归属 " + state.ownerFactionId);
            if (!string.IsNullOrEmpty(currentWarForecast))
            {
                sb.AppendLine(currentWarForecast);
            }
            else
            {
                sb.AppendLine("行军判断: 需邻接、需可用军队、需路线复核");
            }
            sb.AppendLine("目标阻力: 民变 " + state.rebellionRisk + "% | 地方势力 " + state.localPower + " | 占后先军管");
            sb.AppendLine("攻占代价: 合法性 -" + StrategyCausalRules.OccupationLegitimacyCost + " | 税粮贡献降至" + StrategyCausalRules.OccupiedContributionPercent + "% | 整合≤" + StrategyCausalRules.OccupiedIntegration + "%");
            sb.AppendLine("战后治理: 民变+" + StrategyCausalRules.OccupationRebellionRiskIncrease + " | 地方势力+" + StrategyCausalRules.OccupationLocalPowerIncrease + " | 土地兼并+" + StrategyCausalRules.OccupationAnnexationPressureIncrease);
            return sb.ToString();
        }

        private static string FormatWarSourceDetails()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("因果: 行军消耗补给，低补给会降低接敌战力。");
            sb.AppendLine("战争来源: 《孙子》《六韬》；数值为可玩性抽象。");
            sb.AppendLine("占领来源: 《汉书·高帝纪》《明史·食货志》");
            return sb.ToString();
        }

        private string FormatDiplomacyOverview(RegionDefinition definition, RegionState state)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("外交过渡 | 邻国判断");
            sb.AppendLine("目标地区: " + SafeRegionName(definition) + " | 归属 " + state.ownerFactionId);
            sb.AppendLine("可选方向: 外交、封关、刺探；邻接后可显式转入战争模式。");
            sb.AppendLine("风险提示: 宣战会触发行军、接敌和占领治理代价。");
            sb.AppendLine("历史来源: 战争与占领规则保留史料说明，数值为可玩性抽象。");
            return sb.ToString();
        }

        private static string FormatDiplomacySourceDetails()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("因果: 外交先定义封关、刺探、开战后果，再转入战争模式。");
            sb.AppendLine("外交: 《史记》《资治通鉴》；数值为可玩性抽象。");
            sb.AppendLine("占领: 战争与占领规则保留史料说明。");
            return sb.ToString();
        }

        private int ResolveLegitimacy()
        {
            return faction != null ? faction.legitimacy : 0;
        }

        private static string FormatPrimaryDecisionLine(RegionState state)
        {
            return "最大风险：" + ResolvePrimaryRisk(state) + " | 治理压力：" + ResolveGovernancePressure(state);
        }

        private string FormatExpectedGovernanceEffect(RegionState state)
        {
            GovernanceActionForecast recommendedForecast = ResolveRecommendedGovernanceForecast(state);
            if (recommendedForecast != null && recommendedForecast.action == GovernanceActionKind.Pacify && ShouldPrioritizePacify(state))
            {
                return "预计效果: 安抚 金钱-" + PacifyMoneyCost + " 粮食-" + PacifyFoodCost +
                       " | 整合 +" + PacifyIntegrationGain + "至" + Mathf.Min(100, state.integration + PacifyIntegrationGain) + "%" +
                       " | 民变 -" + PacifyRebellionReduction + "至" + Mathf.Max(0, state.rebellionRisk - PacifyRebellionReduction) + "%";
            }

            GovernanceActionForecast forecast = recommendedForecast;
            if (forecast != null && forecast.action != GovernanceActionKind.Hold)
            {
                if (forecast.action == GovernanceActionKind.Pacify)
                {
                    return "预计效果: 安抚 金钱" + forecast.moneyDelta + " 粮食" + forecast.foodDelta +
                           " | 整合 " + FormatSignedDelta(forecast.integrationDelta) + "至" + Mathf.Min(100, state.integration + forecast.integrationDelta) + "%" +
                           " | 民变 " + FormatSignedDelta(forecast.rebellionRiskDelta) + "至" + Mathf.Max(0, state.rebellionRisk + forecast.rebellionRiskDelta) + "%";
                }

                return "预计效果: " + FormatForecastDeltas(forecast) + " | 下一阶段 " + FormatControlStageName(forecast.nextControlStage) + FormatHiddenForecastStageToken(forecast);
            }

            BuildingDefinition building = FindFirstBuildableBuilding();
            if (building != null)
            {
                return "预计效果: 建造 " + building.name + " 金钱-" + building.cost + " | " + FormatEffectSummary(building.effects);
            }

            return "预计效果: 维持税粮与民生，不新增税役负担。";
        }

        private string FormatCompactExpectedGovernanceEffect(RegionState state)
        {
            GovernanceActionForecast forecast = ResolveRecommendedGovernanceForecast(state);
            if (forecast != null && forecast.action != GovernanceActionKind.Hold)
            {
                if (forecast.action == GovernanceActionKind.Pacify)
                {
                    int integrationDelta = forecast.integrationDelta != 0 ? forecast.integrationDelta : PacifyIntegrationGain;
                    int rebellionDelta = forecast.rebellionRiskDelta != 0 ? forecast.rebellionRiskDelta : -PacifyRebellionReduction;
                    return "安抚 金钱" + forecast.moneyDelta + " 粮食" + forecast.foodDelta +
                           " | 整合 " + FormatSignedDelta(integrationDelta) +
                           " | 民变 " + FormatSignedDelta(rebellionDelta);
                }

                return FormatGovernanceActionName(forecast.action) +
                       " | 整合 " + FormatSignedDelta(forecast.integrationDelta) +
                       " | 民变 " + FormatSignedDelta(forecast.rebellionRiskDelta) +
                       " | 税粮 " + FormatSignedDelta(forecast.taxContributionPercentDelta) + "/" + FormatSignedDelta(forecast.foodContributionPercentDelta) +
                       " | 下一阶段 " + FormatControlStageName(forecast.nextControlStage) +
                       FormatHiddenForecastStageToken(forecast);
            }

            BuildingDefinition building = FindFirstBuildableBuilding();
            if (building != null)
            {
                return "建造 " + building.name + " 金钱-" + building.cost + " | " + FormatEffectSummary(building.effects);
            }

            return "维持税粮与民生，不新增税役负担。";
        }

        private static string FormatHiddenForecastStageToken(GovernanceActionForecast forecast)
        {
            if (forecast == null) return string.Empty;
            return string.Empty;
        }

        private static string FormatForecastDeltas(GovernanceActionForecast forecast)
        {
            if (forecast == null) return "暂无可执行政务";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(FormatGovernanceActionName(forecast.action));
            AppendForecastDelta(sb, "金钱", forecast.moneyDelta);
            AppendForecastDelta(sb, "粮食", forecast.foodDelta);
            AppendForecastDelta(sb, "合法性", forecast.legitimacyDelta);
            AppendForecastDelta(sb, "人口", forecast.populationDelta);
            AppendForecastDelta(sb, "兵源", forecast.manpowerDelta);
            AppendForecastDelta(sb, "整合", forecast.integrationDelta);
            AppendForecastDelta(sb, "民变", forecast.rebellionRiskDelta);
            AppendForecastDelta(sb, "地方", forecast.localPowerDelta);
            AppendForecastDelta(sb, "接受", forecast.localAcceptanceDelta);
            AppendForecastDelta(sb, "税贡", forecast.taxContributionPercentDelta);
            AppendForecastDelta(sb, "粮贡", forecast.foodContributionPercentDelta);
            AppendForecastDelta(sb, "预留粮", forecast.occupationReservedFoodDelta);
            if (!forecast.canApply && !string.IsNullOrEmpty(forecast.disabledReason))
            {
                sb.Append(" | 不可用 ");
                sb.Append(forecast.disabledReason);
            }
            return sb.ToString();
        }

        private static string FormatGovernanceActionName(GovernanceActionKind action)
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

        private static void AppendForecastDelta(System.Text.StringBuilder sb, string label, int value)
        {
            if (value == 0) return;
            sb.Append(" | ");
            sb.Append(label);
            sb.Append(" ");
            sb.Append(FormatSignedDelta(value));
        }

        private static string FormatSignedDelta(int value)
        {
            return value > 0 ? "+" + value : value.ToString();
        }

        private string FormatGovernanceActionState()
        {
            return FormatPacifyButtonState() + "；" + FormatBuildButtonState();
        }

        private string FormatPacifyButtonState()
        {
            GovernanceActionForecast forecast = ResolvePrimaryGovernanceActionForecast(regionState);
            string actionName = forecast != null ? FormatGovernanceActionName(forecast.action) : "政务";
            if (!CanUseGovernanceActions()) return actionName + " 不可用";
            if (forecast == null) return actionName + " 不可用";
            if (forecast.action == GovernanceActionKind.Hold) return actionName + " 观察";
            if (!forecast.canApply) return actionName + " 不可用";
            return actionName + " 可用";
        }

        private string FormatBuildButtonState()
        {
            if (!CanUseGovernanceActions()) return "建造 不可用";
            if (faction == null || buildingSystem == null) return "建造 不可用";

            BuildingDefinition building = FindFirstBuildableBuilding();
            if (building == null) return "建造 不可用";
            return "建造 可用(" + building.name + ")";
        }

        private string FormatGovernanceActionButtonHint(GovernanceActionForecast forecast)
        {
            if (!CanUseGovernanceActions()) return FormatGovernanceBlockedReason();
            if (forecast == null) return "缺少政务预测";
            if (forecast.action == GovernanceActionKind.Hold) return "观望 不增税役";
            if (!forecast.canApply) return "缺口 " + FormatDisabledReason(forecast.disabledReason);
            return FormatCompactForecastHint(forecast);
        }

        private string FormatBuildActionButtonHint()
        {
            if (!CanUseGovernanceActions()) return FormatGovernanceBlockedReason();
            if (context == null || faction == null || buildingSystem == null) return "建筑未就绪";

            BuildingDefinition building = FindFirstBuildableBuilding();
            if (building != null)
            {
                return building.name + " 金" + building.cost + " " + FormatCompactEffectHint(building.effects, 2);
            }

            return ResolveBuildUnavailableReason();
        }

        private string FormatGovernanceBlockedReason()
        {
            if (regionState == null) return "未选地区";
            if (currentMode == MapInteractionMode.War) return "战争模式锁定";
            if (currentMode == MapInteractionMode.Diplomacy) return "外交模式锁定";
            if (!currentSelectionIsFriendly) return "非己方地区";
            return "暂不可用";
        }

        private static string FormatDisabledReason(string reason)
        {
            switch (reason)
            {
                case "missing_faction": return "缺势力";
                case "missing_forecast": return "缺预测";
                case "missing_region_state": return "缺地区";
                case "not_enough_money": return "缺金钱";
                case "not_enough_food": return "缺粮食";
                case "not_enough_legitimacy": return "缺合法性";
                case "control_chain_not_ready": return "控制链未到";
                default: return string.IsNullOrEmpty(reason) ? "暂不可用" : reason;
            }
        }

        private static string FormatCompactForecastHint(GovernanceActionForecast forecast)
        {
            if (forecast == null) return "缺少政务预测";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            AppendResourceHint(sb, "金", forecast.moneyDelta);
            AppendResourceHint(sb, "粮", forecast.foodDelta);
            AppendResourceHint(sb, "法", forecast.legitimacyDelta);
            AppendSignedHint(sb, "整", forecast.integrationDelta);
            AppendSignedHint(sb, "乱", forecast.rebellionRiskDelta);
            AppendSignedHint(sb, "地", forecast.localPowerDelta);
            AppendSignedHint(sb, "民", forecast.localAcceptanceDelta);
            AppendSignedHint(sb, "税", forecast.taxContributionPercentDelta);
            AppendSignedHint(sb, "贡粮", forecast.foodContributionPercentDelta);
            AppendSignedHint(sb, "预粮", forecast.occupationReservedFoodDelta);
            return sb.Length == 0 ? "维持秩序" : sb.ToString();
        }

        private static string FormatCompactEffectHint(EffectSet effects, int maxParts)
        {
            if (effects == null) return "表内结算";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int count = 0;
            AppendLimitedEffectHint(sb, "粮", effects.food, maxParts, ref count);
            AppendLimitedEffectHint(sb, "税效", effects.taxEfficiency, maxParts, ref count);
            AppendLimitedEffectHint(sb, "税基", effects.taxBase, maxParts, ref count);
            AppendLimitedEffectHint(sb, "整", effects.integrationSpeed, maxParts, ref count);
            AppendLimitedEffectHint(sb, "乱", effects.rebellionRisk, maxParts, ref count);
            AppendLimitedEffectHint(sb, "地", effects.localPower, maxParts, ref count);
            return sb.Length == 0 ? "表内结算" : sb.ToString();
        }

        private static void AppendLimitedEffectHint(System.Text.StringBuilder sb, string label, int value, int maxParts, ref int count)
        {
            if (value == 0 || count >= maxParts) return;
            AppendSignedHint(sb, label, value);
            count++;
        }

        private static void AppendResourceHint(System.Text.StringBuilder sb, string label, int delta)
        {
            if (delta == 0) return;
            if (sb.Length > 0) sb.Append(" ");
            sb.Append(label);
            if (delta < 0)
            {
                sb.Append(-delta);
                return;
            }

            sb.Append("+");
            sb.Append(delta);
        }

        private static void AppendSignedHint(System.Text.StringBuilder sb, string label, int delta)
        {
            if (delta == 0) return;
            if (sb.Length > 0) sb.Append(" ");
            sb.Append(label);
            sb.Append(delta > 0 ? "+" : "");
            sb.Append(delta);
        }

        private string ResolveBuildUnavailableReason()
        {
            if (context == null || context.Data == null || context.Data.Buildings == null || context.Data.Buildings.Count == 0)
            {
                return "无建筑表";
            }

            if (regionState != null && regionState.buildings != null && regionState.buildings.Count >= 3)
            {
                return "槽位已满";
            }

            foreach (BuildingDefinition building in context.Data.Buildings.Values)
            {
                if (building == null) continue;
                if (regionState != null && regionState.buildings != null && regionState.buildings.Contains(building.id)) continue;
                if (!string.IsNullOrEmpty(building.requiresTech) && (faction == null || !faction.completedTechIds.Contains(building.requiresTech)))
                {
                    return "需科技 " + ResolveTechnologyName(building.requiresTech);
                }

                if (faction != null && faction.money < building.cost)
                {
                    return "缺金" + building.cost;
                }
            }

            return "无可建";
        }

        private string ResolveTechnologyName(string techId)
        {
            if (context != null && context.Data != null && context.Data.Technologies != null)
            {
                TechnologyDefinition tech;
                if (context.Data.Technologies.TryGetValue(techId, out tech) && tech != null && !string.IsNullOrEmpty(tech.name))
                {
                    return tech.name;
                }
            }

            return techId;
        }

        private bool CanUseGovernanceActions()
        {
            return currentMode == MapInteractionMode.Governance && currentSelectionIsFriendly && regionState != null;
        }

        private bool CanPacifyCurrentRegion()
        {
            GovernanceActionForecast forecast = ResolvePrimaryGovernanceActionForecast(regionState);
            return CanUseGovernanceActions() &&
                   forecast != null &&
                   forecast.action != GovernanceActionKind.Hold &&
                   forecast.canApply;
        }

        private bool CanBuildCurrentRegion()
        {
            return CanUseGovernanceActions() &&
                   faction != null &&
                   buildingSystem != null &&
                   FindFirstBuildableBuilding() != null;
        }

        private static bool ShouldPrioritizePacify(RegionState state)
        {
            return state != null && (state.rebellionRisk >= 50 || state.integration < 55);
        }

        private static string FormatGovernanceCausalLine(RegionState state)
        {
            if (state == null)
            {
                return "因果: 缺少地区状态，暂不能推导治理变化。";
            }

            if (state.controlStage != ControlStage.Controlled)
            {
                return "因果: 当前处于" + FormatControlStageName(state.controlStage) + "，安抚与编户前不能取得完整税粮。";
            }

            if (state.occupationStatus == OccupationStatus.Occupied ||
                state.taxContributionPercent < 70 ||
                state.foodContributionPercent < 70)
            {
                return "因果: 新占领或低整合会压低税粮贡献，先稳民心再取税粮。";
            }

            if (state.rebellionRisk >= 50)
            {
                return "因果: 高民变会伤产出与合法性，安抚优先于继续加压。";
            }

            return "因果: 稳定地区适合补建筑或维持税粮，不额外推高民变。";
        }

        private static string FormatEffectSummary(EffectSet effects)
        {
            if (effects == null) return "效果由建筑表结算";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            AppendEffect(sb, "整合", effects.integrationSpeed);
            AppendEffect(sb, "税效", effects.taxEfficiency);
            AppendEffect(sb, "税基", effects.taxBase);
            AppendEffect(sb, "粮食", effects.food);
            AppendEffect(sb, "民变", effects.rebellionRisk);
            AppendEffect(sb, "地方", effects.localPower);

            return sb.Length == 0 ? "效果由建筑表结算" : sb.ToString();
        }

        private static void AppendEffect(System.Text.StringBuilder sb, string label, int value)
        {
            if (value == 0) return;
            if (sb.Length > 0) sb.Append(" ");
            sb.Append(label);
            sb.Append(value > 0 ? "+" : "");
            sb.Append(value);
        }

        private static string ResolvePrimaryRisk(RegionState state)
        {
            if (state == null) return "unknown";

            if (state.rebellionRisk >= state.annexationPressure && state.rebellionRisk >= state.localPower)
            {
                return "民变 " + state.rebellionRisk + "%";
            }

            if (state.annexationPressure >= state.localPower)
            {
                return "土地兼并 " + state.annexationPressure + "%";
            }

            return "地方势力 " + state.localPower;
        }

        private static string ResolveGovernancePressure(RegionState state)
        {
            if (state == null) return "unknown";

            int pressure = Mathf.RoundToInt((100 - state.integration + state.rebellionRisk + state.annexationPressure + state.localPower) / 4f);
            if (pressure >= 60) return "高 " + pressure;
            if (pressure >= 35) return "中 " + pressure;
            return "低 " + pressure;
        }

        private string ResolveBestGovernanceAction(RegionState state)
        {
            if (state == null) return "等待";

            GovernanceActionForecast forecast = ResolveRecommendedGovernanceForecast(state);
            if (forecast != null && forecast.action != GovernanceActionKind.Hold)
            {
                string actionName = FormatGovernanceActionName(forecast.action);
                return forecast.canApply ? actionName : actionName + "(" + forecast.disabledReason + ")";
            }

            if (ShouldPrioritizePacify(state))
            {
                return CanPacifyCurrentRegion() ? "安抚地区" : "安抚地区(先筹钱粮)";
            }

            if (FindFirstBuildableBuilding() != null)
            {
                return "建造建筑";
            }

            return "维持税粮与民生";
        }

        private GovernanceActionForecast ResolveRecommendedGovernanceForecast(RegionState state)
        {
            return StrategyMapRulebook.BuildRecommendedGovernanceForecast(context, regionDefinition, state, faction);
        }

        private GovernanceActionForecast ResolvePrimaryGovernanceActionForecast(RegionState state)
        {
            GovernanceActionForecast forecast = ResolveRecommendedGovernanceForecast(state);
            if (forecast != null && forecast.action != GovernanceActionKind.Hold)
            {
                return forecast;
            }

            return StrategyMapRulebook.BuildGovernanceForecast(context, regionDefinition, state, faction, GovernanceActionKind.Pacify);
        }

        private string FormatSpecializationLine(RegionDefinition definition, RegionState state)
        {
            if (state == null) return "定位未明";
            RegionSpecialization specialization = StrategyMapRulebook.ResolveSpecialization(definition, state, context != null ? context.Data : null);
            ControlStage stage = StrategyMapRulebook.ResolveControlStage(state);
            return FormatSpecializationName(specialization) +
                   " | " + FormatControlStageName(stage) +
                   " | 接受 " + state.localAcceptance +
                   " | 补给 " + FormatYesNo(state.supplyNode);
        }

        private static string FormatSpecializationName(RegionSpecialization specialization)
        {
            switch (specialization)
            {
                case RegionSpecialization.Grain: return "粮仓";
                case RegionSpecialization.Military: return "兵源";
                case RegionSpecialization.Tax: return "财税";
                case RegionSpecialization.Border: return "边防";
                case RegionSpecialization.Legitimacy: return "法统";
                case RegionSpecialization.Culture: return "文化";
                case RegionSpecialization.Capital: return "都城";
                default: return "未定";
            }
        }

        private static string FormatControlStageName(ControlStage stage)
        {
            switch (stage)
            {
                case ControlStage.NewlyAttached: return "新附";
                case ControlStage.MilitaryGoverned: return "军管";
                case ControlStage.Pacified: return "安抚";
                case ControlStage.Registered: return "编户";
                default: return "控制";
            }
        }

        private static string FormatYesNo(bool value)
        {
            return value ? "有" : "无";
        }

        private string FormatBuildingDecision()
        {
            BuildingDefinition building = FindFirstBuildableBuilding();
            if (building == null) return "暂无可建";
            return building.name;
        }

        private string FormatPolicyDecision()
        {
            PolicyDefinition policy = FindFirstAvailablePolicy();
            if (policy == null) return "暂无可用";
            return policy.name;
        }

        private void SyncRuntimeRegionFromState()
        {
            if (regionState == null) return;
            GameManager manager = Object.FindObjectOfType<GameManager>();
            if (manager == null || manager.World == null || manager.World.Map == null) return;

            RegionRuntimeState runtime;
            if (!manager.World.Map.TryGetRegion(regionState.id, out runtime) || runtime == null) return;

            runtime.integration = regionState.integration;
            runtime.occupationStatus = regionState.occupationStatus;
            runtime.controlStage = regionState.controlStage;
            runtime.occupationReservedFood = regionState.occupationReservedFood;
            runtime.occupationPacificationQueueStep = regionState.occupationPacificationQueueStep;
            runtime.occupationPacificationQueueTurnsRemaining = regionState.occupationPacificationQueueTurnsRemaining;
            runtime.taxContributionPercent = regionState.taxContributionPercent;
            runtime.foodContributionPercent = regionState.foodContributionPercent;
            runtime.rebellionRisk = regionState.rebellionRisk;
            runtime.localPower = regionState.localPower;
            runtime.annexationPressure = regionState.annexationPressure;
            runtime.localAcceptance = regionState.localAcceptance;
            runtime.supplyNode = regionState.supplyNode;
        }

        private static string SafeRegionName(RegionDefinition definition)
        {
            if (definition == null) return "unknown";
            return string.IsNullOrEmpty(definition.name) ? definition.id : definition.name;
        }

        private void AppendBuildingOverview(System.Text.StringBuilder sb, RegionState state)
        {
            BuildingDefinition building = FindFirstBuildableBuilding();
            if (building != null)
            {
                sb.AppendLine("建设: " + building.name + " | 来源: " + SafeSource(building.sourceReference));
                return;
            }

            sb.AppendLine("建设: " + FormatBuildings(state));
        }

        private void AppendPolicyOverview(System.Text.StringBuilder sb)
        {
            PolicyDefinition policy = FindFirstAvailablePolicy();
            if (policy == null)
            {
                sb.AppendLine("政策: 暂无可用");
                return;
            }

            sb.AppendLine("政策: " + policy.name + " | 来源: " + SafeSource(policy.sourceReference));
        }

        private void AppendCausalFeedback(System.Text.StringBuilder sb, RegionState state)
        {
            if (state == null) return;

            bool appended = false;
            if (state.occupationStatus == OccupationStatus.Occupied ||
                state.taxContributionPercent < 70 ||
                state.foodContributionPercent < 70)
            {
                sb.Append("负反馈: 新占或低整合会限制税粮贡献");
                appended = true;
            }

            if (state.rebellionRisk >= 50)
            {
                if (!appended)
                {
                    sb.Append("负反馈: ");
                }
                else
                {
                    sb.Append("; ");
                }

                sb.Append("高民变会压低产出与合法性");
                appended = true;
            }

            if (!appended)
            {
                sb.Append("负反馈: 当前无高危反噬。");
            }

            sb.AppendLine();
        }

        private void AppendSourceNotes(System.Text.StringBuilder sb, RegionDefinition definition)
        {
            sb.AppendLine("治理来源: " + FormatGovernanceSource(definition));
            sb.AppendLine("占领来源: 《汉书·高帝纪》《明史·食货志》");
            sb.AppendLine("建设来源: " + FormatBuildingSourceNotes());
            sb.AppendLine("政策来源: " + FormatPolicySourceNotes());
        }

        private string FormatGovernanceSource(RegionDefinition definition)
        {
            if (definition == null || definition.legitimacyMemory == null || definition.legitimacyMemory.Length == 0)
            {
                return "缺少法统记忆";
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int count = Mathf.Min(2, definition.legitimacyMemory.Length);
            for (int i = 0; i < count; i++)
            {
                if (i > 0) sb.Append("; ");
                sb.Append(definition.legitimacyMemory[i]);
            }

            return sb.ToString();
        }

        private string FormatBuildingSourceNotes()
        {
            if (context == null || context.Data == null || context.Data.Buildings == null || context.Data.Buildings.Count == 0)
            {
                return "缺少建筑数据";
            }

            BuildingDefinition buildable = FindFirstBuildableBuilding();
            if (buildable != null)
            {
                return buildable.name + " / " + SafeSource(buildable.sourceReference);
            }

            foreach (BuildingDefinition building in context.Data.Buildings.Values)
            {
                if (building != null)
                {
                    return building.name + " / " + SafeSource(building.sourceReference);
                }
            }

            return "缺少建筑史源";
        }

        private string FormatPolicySourceNotes()
        {
            PolicyDefinition policy = FindFirstAvailablePolicy();
            if (policy == null) return "缺少可用政策";
            return policy.name + " / " + SafeSource(policy.sourceReference);
        }

        private PolicyDefinition FindFirstAvailablePolicy()
        {
            if (context == null || faction == null) return null;

            PolicyDefinition standardization;
            if (context.Data.Policies.TryGetValue("standardization", out standardization) && IsAvailablePolicy(standardization))
            {
                return standardization;
            }

            foreach (PolicyDefinition policy in context.Data.Policies.Values)
            {
                if (IsAvailablePolicy(policy)) return policy;
            }

            return null;
        }

        private bool IsAvailablePolicy(PolicyDefinition policy)
        {
            return policy != null && !faction.completedReformIds.Contains(policy.id) && CanPayPolicy(policy.cost);
        }

        private bool CanPayPolicy(CostSet cost)
        {
            if (faction == null) return false;
            if (cost == null) return true;
            return faction.money >= cost.money && faction.food >= cost.food && faction.legitimacy >= cost.legitimacy;
        }

        private static string SafeSource(string sourceReference)
        {
            return string.IsNullOrEmpty(sourceReference) ? "missing" : sourceReference;
        }

        private static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }

        private static string FormatTerrain(string terrain)
        {
            switch (terrain)
            {
                case "plain": return "平原";
                case "hill": return "丘陵";
                case "mountain": return "山地";
                case "basin": return "盆地";
                case "river_plain": return "河谷平原";
                case "frontier_plain": return "边塞平原";
                case "plateau": return "高原";
                case "corridor": return "走廊";
                case "river_delta": return "三角洲";
                case "mountain_coast": return "山海";
                case "subtropical": return "亚热带";
                case "frontier_forest": return "边塞林地";
                default: return terrain;
            }
        }

        private static string FormatPopulation(int population)
        {
            if (population >= 10000)
            {
                return (population / 10000f).ToString("F1") + "万";
            }

            return population.ToString();
        }

        private string FormatNeighbors(string[] neighbors)
        {
            if (neighbors == null || neighbors.Length == 0) return "无";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int visible = Mathf.Min(3, neighbors.Length);
            for (int i = 0; i < visible; i++)
            {
                if (i > 0) sb.Append("、");
                sb.Append(ResolveRegionDisplayName(neighbors[i]));
            }

            if (neighbors.Length > visible)
            {
                sb.Append("等");
                sb.Append(neighbors.Length);
                sb.Append("区");
            }

            return sb.ToString();
        }

        private string ResolveRegionDisplayName(string regionId)
        {
            if (context != null && context.Data != null)
            {
                RegionDefinition definition = context.Data.GetRegion(regionId);
                if (definition != null && !string.IsNullOrEmpty(definition.name))
                {
                    return definition.name;
                }
            }

            return regionId;
        }

        private static string FormatLandStructure(LandStructure ls)
        {
            if (ls == null) return "";

            return "土地：小农" + Mathf.RoundToInt(ls.smallFarmers * 100) +
                   "%  豪强" + Mathf.RoundToInt(ls.localElites * 100) +
                   "%  官田" + Mathf.RoundToInt(ls.stateLand * 100) +
                   "%  寺院" + Mathf.RoundToInt(ls.religiousLand * 100) + "%";
        }

        private static string FormatCustoms(string[] customs, int stability)
        {
            if (customs == null || customs.Length == 0) return "风俗：无";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("风俗：");
            for (int i = 0; i < customs.Length; i++)
            {
                if (i > 0) sb.Append("、");
                sb.Append(FormatCustomName(customs[i]));
            }

            sb.Append(" | 稳定 " + stability + "%");
            return sb.ToString();
        }

        private static string FormatCustomName(string custom)
        {
            switch (custom)
            {
                case "martial": return "尚武";
                case "scholarly": return "崇文";
                case "mercantile": return "重商";
                case "agrarian": return "农耕";
                case "frontier": return "边塞";
                case "pluralistic": return "多元";
                default: return custom;
            }
        }
    }
}
