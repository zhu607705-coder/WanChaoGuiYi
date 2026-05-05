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
            SetText(neighborsText, "相邻区域：" + FormatNeighbors(definition.neighbors));
            SetText(landStructureText, FormatLandStructure(state.landStructure));
            SetText(customsText, FormatCustoms(state.customs, state.customStability) + "\n" + FormatBuildings(state));
            RefreshRegionTextBlocks();
            UpdateCollapsedTab();
            ApplyModeActionState();
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
            if (pacifyButton != null)
            {
                pacifyButton.interactable = CanPacifyCurrentRegion();
                Text label = pacifyButton.GetComponentInChildren<Text>();
                GovernanceActionForecast forecast = ResolvePrimaryGovernanceActionForecast(regionState);
                if (label != null && forecast != null && forecast.action != GovernanceActionKind.Hold)
                {
                    label.text = FormatGovernanceActionName(forecast.action);
                }
            }
            if (buildButton != null) buildButton.interactable = CanBuildCurrentRegion();
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
                header += " | " + currentModeReason;
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
            sb.AppendLine("Governance 治理模式 | 本回合摘要 | " + FormatSpecializationLine(definition, state));
            sb.AppendLine(FormatPrimaryDecisionLine(state));
            sb.AppendLine("Recommended 最优行动: " + ResolveBestGovernanceAction(state) + " | 可执行政务");
            sb.AppendLine(FormatExpectedGovernanceEffect(state));
            sb.AppendLine(FormatGovernanceActionState());
            sb.AppendLine("Politics 政治: legitimacy " + ResolveLegitimacy() + " | integration " + state.integration + "% | Civic 民生: customs " + state.customStability + "%");
            sb.AppendLine("Grain 粮食: output " + state.foodOutput + " / contribution " + state.foodContributionPercent + "% | Population 人口: " + FormatPopulation(state.population) + " / manpower " + state.manpower);
            sb.AppendLine("Building 建筑: " + FormatBuildingDecision() + " | Policy 政策: " + FormatPolicyDecision());
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
            sb.AppendLine(FormatGovernanceCausalLine(state));
            AppendCausalFeedback(sb, state);
            AppendSourceNotes(sb, definition);
            return sb.ToString();
        }

        private string FormatWarOverview(RegionDefinition definition, RegionState state)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("War 战争模式 | 战前压力摘要");
            sb.AppendLine("目标地区: " + SafeRegionName(definition) + " | 归属 " + state.ownerFactionId);
            if (!string.IsNullOrEmpty(currentWarForecast))
            {
                sb.AppendLine(currentWarForecast);
            }
            else
            {
                sb.AppendLine("行军判断: 需邻接、需可用军队、需路线复核");
            }
            sb.AppendLine("接敌风险: 民变 " + state.rebellionRisk + "% | 地方势力 " + state.localPower + " | 占领后治理折损");
            sb.AppendLine("攻占预告: 合法性 -" + StrategyCausalRules.OccupationLegitimacyCost + " | 税粮贡献降至" + StrategyCausalRules.OccupiedContributionPercent + "% | 整合≤" + StrategyCausalRules.OccupiedIntegration + "%");
            sb.AppendLine("占后风险: 民变+" + StrategyCausalRules.OccupationRebellionRiskIncrease + " | 地方势力+" + StrategyCausalRules.OccupationLocalPowerIncrease + " | 土地兼并+" + StrategyCausalRules.OccupationAnnexationPressureIncrease);
            sb.AppendLine("历史来源: 《汉书·高帝纪》《明史·食货志》");
            return sb.ToString();
        }

        private static string FormatWarSourceDetails()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Causal 因果: 行军消耗补给，低补给会降低接敌战力。");
            sb.AppendLine("War Source: 《孙子》《六韬》；数值为可玩性抽象。");
            sb.AppendLine("Occupation Source: 《汉书·高帝纪》《明史·食货志》");
            return sb.ToString();
        }

        private string FormatDiplomacyOverview(RegionDefinition definition, RegionState state)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Diplomacy 外交过渡 | 邻国判断");
            sb.AppendLine("目标地区: " + SafeRegionName(definition) + " | 归属 " + state.ownerFactionId);
            sb.AppendLine("可选方向: 外交、封关、刺探；邻接后可显式转入战争模式。");
            sb.AppendLine("风险提示: 宣战会触发行军、接敌和占领治理代价。");
            sb.AppendLine("历史来源: 战争与占领规则保留史料说明，数值为可玩性抽象。");
            return sb.ToString();
        }

        private static string FormatDiplomacySourceDetails()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("Causal 因果: 外交先定义封关、刺探、开战后果，再转入战争模式。");
            sb.AppendLine("Diplomacy Source: 《史记》《资治通鉴》；数值为可玩性抽象。");
            sb.AppendLine("Occupation Source: 战争与占领规则保留史料说明。");
            return sb.ToString();
        }

        private int ResolveLegitimacy()
        {
            return faction != null ? faction.legitimacy : 0;
        }

        private static string FormatPrimaryDecisionLine(RegionState state)
        {
            return "Decision 决策 | Risk 风险: 最大风险 " + ResolvePrimaryRisk(state) + " | 治理压力 " + ResolveGovernancePressure(state);
        }

        private string FormatExpectedGovernanceEffect(RegionState state)
        {
            GovernanceActionForecast recommendedForecast = ResolveRecommendedGovernanceForecast(state);
            if (recommendedForecast != null && recommendedForecast.action == GovernanceActionKind.Pacify && ShouldPrioritizePacify(state))
            {
                return "Expected 预计效果: 安抚 金钱-" + PacifyMoneyCost + " 粮食-" + PacifyFoodCost +
                       " | 整合 +" + PacifyIntegrationGain + "至" + Mathf.Min(100, state.integration + PacifyIntegrationGain) + "%" +
                       " | 民变 -" + PacifyRebellionReduction + "至" + Mathf.Max(0, state.rebellionRisk - PacifyRebellionReduction) + "%";
            }

            GovernanceActionForecast forecast = recommendedForecast;
            if (forecast != null && forecast.action != GovernanceActionKind.Hold)
            {
                if (forecast.action == GovernanceActionKind.Pacify)
                {
                    return "Expected 预计效果: 安抚 金钱" + forecast.moneyDelta + " 粮食" + forecast.foodDelta +
                           " | 整合 " + FormatSignedDelta(forecast.integrationDelta) + "至" + Mathf.Min(100, state.integration + forecast.integrationDelta) + "%" +
                           " | 民变 " + FormatSignedDelta(forecast.rebellionRiskDelta) + "至" + Mathf.Max(0, state.rebellionRisk + forecast.rebellionRiskDelta) + "%";
                }

                return "Expected 预计效果: " + FormatForecastDeltas(forecast) + " | nextControl " + forecast.nextControlStage;
            }

            BuildingDefinition building = FindFirstBuildableBuilding();
            if (building != null)
            {
                return "Expected 预计效果: 建造 " + building.name + " 金钱-" + building.cost + " | " + FormatEffectSummary(building.effects);
            }

            return "Expected 预计效果: 维持税粮与民生；不新增税役负担，观察风险。";
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
            return "Action State 按钮: " + FormatPacifyButtonState() + " | 建造 " + FormatBuildButtonState();
        }

        private string FormatPacifyButtonState()
        {
            GovernanceActionForecast forecast = ResolvePrimaryGovernanceActionForecast(regionState);
            string actionName = forecast != null ? FormatGovernanceActionName(forecast.action) : "政务";
            if (!CanUseGovernanceActions()) return actionName + " 不可用(非国内治理)";
            if (forecast == null) return actionName + " 不可用(缺少预告)";
            if (forecast.action == GovernanceActionKind.Hold) return actionName + " 观察";
            if (!forecast.canApply) return actionName + " 不可用(" + forecast.disabledReason + ")";
            return actionName + " 可用(钱" + FormatSignedDelta(forecast.moneyDelta) + " 粮" + FormatSignedDelta(forecast.foodDelta) + ")";
        }

        private string FormatBuildButtonState()
        {
            if (!CanUseGovernanceActions()) return "不可用(非国内治理)";
            if (faction == null || buildingSystem == null) return "不可用(缺少系统)";

            BuildingDefinition building = FindFirstBuildableBuilding();
            if (building == null) return "不可用(无可建/前置不足)";
            return "可用(" + building.name + " 钱-" + building.cost + ")";
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
                return "Causal 因果: 缺少地区状态，暂不能推导治理变化。";
            }

            if (state.controlStage != ControlStage.Controlled)
            {
                return "Causal: control " + state.controlStage + " keeps conquest below full tax/food until pacify and household registration.";
            }

            if (state.occupationStatus == OccupationStatus.Occupied ||
                state.taxContributionPercent < 70 ||
                state.foodContributionPercent < 70)
            {
                return "Causal 因果: 新占领/低整合会压低税粮贡献，先稳民心再取税粮。";
            }

            if (state.rebellionRisk >= 50)
            {
                return "Causal 因果: 高民变会伤产出与合法性，安抚优先于继续加压。";
            }

            return "Causal 因果: 稳定地区适合补建筑或维持税粮，不额外推高民变。";
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
            if (state == null) return "Specialization unknown";
            RegionSpecialization specialization = StrategyMapRulebook.ResolveSpecialization(definition, state, context != null ? context.Data : null);
            ControlStage stage = StrategyMapRulebook.ResolveControlStage(state);
            return "Specialization " + specialization +
                   " | Control " + stage +
                   " | Acceptance " + state.localAcceptance +
                   " | SupplyNode " + (state.supplyNode ? "yes" : "no");
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
            if (policy == null) return "none available";
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
                sb.AppendLine("Building 建筑: " + building.name + " | Source: " + SafeSource(building.sourceReference));
                return;
            }

            sb.AppendLine("Building 建筑: " + FormatBuildings(state));
        }

        private void AppendPolicyOverview(System.Text.StringBuilder sb)
        {
            PolicyDefinition policy = FindFirstAvailablePolicy();
            if (policy == null)
            {
                sb.AppendLine("Policy 政策: none available");
                return;
            }

            sb.AppendLine("Policy 政策: " + policy.name + " | Source: " + SafeSource(policy.sourceReference));
        }

        private void AppendCausalFeedback(System.Text.StringBuilder sb, RegionState state)
        {
            if (state == null) return;

            bool appended = false;
            if (state.occupationStatus == OccupationStatus.Occupied ||
                state.taxContributionPercent < 70 ||
                state.foodContributionPercent < 70)
            {
                sb.Append("Negative 负反馈: occupied/low integration limits tax-food contribution");
                appended = true;
            }

            if (state.rebellionRisk >= 50)
            {
                if (!appended)
                {
                    sb.Append("Negative 负反馈: ");
                }
                else
                {
                    sb.Append("; ");
                }

                sb.Append("high rebellion risk lowers output and legitimacy");
                appended = true;
            }

            if (appended) sb.AppendLine(".");
        }

        private void AppendSourceNotes(System.Text.StringBuilder sb, RegionDefinition definition)
        {
            sb.AppendLine("Governance Source: " + FormatGovernanceSource(definition));
            sb.AppendLine("Occupation Source: 《汉书·高帝纪》《明史·食货志》");
            sb.AppendLine("Building Source: " + FormatBuildingSourceNotes());
            sb.AppendLine("Policy Source: " + FormatPolicySourceNotes());
        }

        private string FormatGovernanceSource(RegionDefinition definition)
        {
            if (definition == null || definition.legitimacyMemory == null || definition.legitimacyMemory.Length == 0)
            {
                return "regional legitimacy memory missing";
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
                return "building data missing";
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

            return "building source missing";
        }

        private string FormatPolicySourceNotes()
        {
            PolicyDefinition policy = FindFirstAvailablePolicy();
            if (policy == null) return "policy source missing";
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

        private static string FormatNeighbors(string[] neighbors)
        {
            if (neighbors == null || neighbors.Length == 0) return "无";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < neighbors.Length; i++)
            {
                if (i > 0) sb.Append("、");
                sb.Append(neighbors[i]);
            }

            return sb.ToString();
        }

        private static string FormatLandStructure(LandStructure ls)
        {
            if (ls == null) return "";

            return "土地结构：小农 " + Mathf.RoundToInt(ls.smallFarmers * 100) +
                   "% | 地方精英 " + Mathf.RoundToInt(ls.localElites * 100) +
                   "% | 国有 " + Mathf.RoundToInt(ls.stateLand * 100) +
                   "% | 寺院 " + Mathf.RoundToInt(ls.religiousLand * 100) + "%";
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

            sb.Append(" | 稳定度：" + stability + "%");
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
