using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WanChaoGuiYi
{
    public sealed class UISetup : MonoBehaviour
    {
        private static readonly string[] GeneratedRootNames =
        {
            "HUDBar",
            "RegionPanel",
            "CollapsedRegionTab",
            "EmperorPanel",
            "CourtPanel",
            "EventPanel",
            "BattleReportPanel",
            "TechPanel",
            "WeatherPanel",
            "MechanismPanel",
            "StrategyLensBar",
            "StrategyOutlinerPanel",
            "StrategyOutlinerCollapsed",
            "LogisticsQueuePanel"
        };

        [SerializeField] private GameManager gameManager;

        private MainMapUI mainMapUI;
        private RegionPanel regionPanel;
        private EmperorPanel emperorPanel;
        private CourtPanel courtPanel;
        private EventPanel eventPanel;
        private BattleReportPanel battleReportPanel;
        private TechPanel techPanel;
        private WeatherPanel weatherPanel;
        private MechanismPanel mechanismPanel;
        private bool uiBuilt;

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }
        }

        private void Start()
        {
            BuildUI();
        }

        public void Bind(GameManager manager)
        {
            gameManager = manager;
            BuildUI();
        }

        private void BuildUI()
        {
            if (uiBuilt)
            {
                if (mainMapUI != null) mainMapUI.ActivateBindings();
                return;
            }

            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
            }

            EnsureEventSystem();
            Canvas canvas = EnsureCanvas();
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            if (canvas.GetComponent<GraphicRaycaster>() == null) canvas.gameObject.AddComponent<GraphicRaycaster>();
            ClearGeneratedUi(canvas.transform);

            HudBindings hud = BuildHUDBar(canvas.transform);
            RegionPanelBindings region = BuildRegionPanel(canvas.transform);
            EmperorPanelBindings emperor = BuildEmperorPanel(canvas.transform);
            CourtPanelBindings court = BuildCourtPanel(canvas.transform);
            EventPanelBindings events = BuildEventPanel(canvas.transform);
            BattleReportPanelBindings battle = BuildBattleReportPanel(canvas.transform);
            TechPanelBindings tech = BuildTechPanel(canvas.transform);
            WeatherPanelBindings weather = BuildWeatherPanel(canvas.transform);
            MechanismPanelBindings mechanism = BuildMechanismPanel(canvas.transform);
            BuildStrategyControls(canvas.transform);

            mainMapUI = GetOrAdd<MainMapUI>(gameObject);
            regionPanel = GetOrAdd<RegionPanel>(region.root);
            emperorPanel = GetOrAdd<EmperorPanel>(emperor.root);
            courtPanel = GetOrAdd<CourtPanel>(court.root);
            eventPanel = GetOrAdd<EventPanel>(events.root);
            battleReportPanel = GetOrAdd<BattleReportPanel>(battle.root);
            techPanel = GetOrAdd<TechPanel>(tech.root);
            weatherPanel = GetOrAdd<WeatherPanel>(weather.root);
            mechanismPanel = GetOrAdd<MechanismPanel>(mechanism.root);

            regionPanel.Bind(region.root, region.regionNameText, region.terrainText, region.populationText, region.foodText, region.taxText, region.manpowerText, region.ownerText, region.integrationText, region.rebellionText, region.annexationText, region.localPowerText, region.neighborsText, region.landStructureText, region.customsText, region.governanceOverviewText, region.governanceSourceText, region.pacifyButton, region.buildButton, region.closeButton, region.collapseButton, region.collapsedRoot, region.expandButton, region.collapsedTabText, region.modeText);
            emperorPanel.Bind(emperor.root, emperor.emperorNameText, emperor.titleText, emperor.mechanicNameText, emperor.mechanicDescText, emperor.statsText, emperor.burdensText, emperor.legitimacyText, emperor.successionText, emperor.heirText, emperor.stableSuccessionsText, emperor.skillText, emperor.resolveSuccessionButton, emperor.useSkillButton, emperor.closeButton);
            courtPanel.Bind(court.root, court.factionNameText, court.emperorNameText, court.moneyText, court.foodText, court.legitimacyText, court.successionText, court.courtPressureText, court.regionCountText, court.talentCountText, court.turnLogText, court.scrollRect, court.generalPortraitGridContent, court.recruitArmyButton, court.equipArmyButton, court.closeButton);
            eventPanel.Bind(events.root, events.eventNameText, events.categoryText, events.choiceText, events.choiceButtons, events.closeButton);
            battleReportPanel.Bind(battle.root, battle.attackerText, battle.defenderText, battle.resultText, battle.detailsText, battle.focusButton, battle.closeButton);
            techPanel.Bind(tech.root, tech.titleText, tech.currentResearchText, tech.progressText, tech.availableTechsText, tech.completedTechsText, tech.setResearchButton, tech.closeButton);
            weatherPanel.Bind(weather.root, weather.weatherNameText, weather.weatherEffectText, weather.celestialEventText, weather.resilienceText, weather.closeButton);
            mechanismPanel.Bind(mechanism.root, mechanism.titleText, mechanism.detailsText, mechanism.applyPolicyButton, mechanism.diplomacyButton, mechanism.borderButton, mechanism.enterWarModeButton, mechanism.espionageButton, mechanism.closeButton);
            mainMapUI.Bind(gameManager, regionPanel, emperorPanel, courtPanel, eventPanel, battleReportPanel, techPanel, weatherPanel, mechanismPanel, hud.turnText, hud.resourceText, hud.selectionContextText, hud.modeStateText, hud.overlayBudgetText, hud.governanceModeButton, hud.warModeButton, hud.nextTurnButton, hud.courtButton, hud.emperorButton, hud.attackButton, hud.prepareFrontlineButton, hud.techButton, hud.weatherButton, hud.eventButton, hud.mechanismButton);
            mainMapUI.ActivateBindings();
            uiBuilt = true;
        }

        private Canvas EnsureCanvas()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("GameCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            return canvas;
        }

        private static void EnsureEventSystem()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                eventSystem = FindObjectOfType<EventSystem>();
            }

            GameObject eventSystemObject;
            if (eventSystem == null)
            {
                eventSystemObject = new GameObject("EventSystem");
                eventSystem = eventSystemObject.AddComponent<EventSystem>();
            }
            else
            {
                eventSystemObject = eventSystem.gameObject;
            }

            if (eventSystemObject.GetComponent<BaseInputModule>() == null)
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }
        }

        private static void ClearGeneratedUi(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (!IsGeneratedRoot(child.name)) continue;

                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
        }

        private static bool IsGeneratedRoot(string objectName)
        {
            for (int i = 0; i < GeneratedRootNames.Length; i++)
            {
                if (GeneratedRootNames[i] == objectName) return true;
            }

            return false;
        }

        private HudBindings BuildHUDBar(Transform parent)
        {
            GameObject bar = CreatePanel(parent, "HUDBar", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -25), new Vector2(0, 50));
            Image barImage = bar.GetComponent<Image>();
            if (barImage != null) barImage.color = UITheme.HudBackgroundAlpha;
            HudBindings bindings = new HudBindings();
            bindings.root = bar;
            bindings.turnText = CreateText(bar.transform, "TurnText", "回合 1 | 1年春", new Vector2(20, 0), new Vector2(0, 0.5f), new Vector2(220, 40));
            bindings.resourceText = CreateText(bar.transform, "ResourceText", "金钱：300 | 粮食：400 | 合法性：60", new Vector2(220, 0), new Vector2(0, 0.5f), new Vector2(390, 40));
            bindings.governanceModeButton = CreateButton(bar.transform, "GovernanceModeButton", "治理", new Vector2(653, 0), new Vector2(0, 0.5f), new Vector2(66, 36));
            bindings.warModeButton = CreateButton(bar.transform, "WarModeButton", "战争", new Vector2(727, 0), new Vector2(0, 0.5f), new Vector2(66, 36));
            bindings.modeStateText = CreateText(bar.transform, "ModeStateText", "当前：治理模式", new Vector2(770, 0), new Vector2(0, 0.5f), new Vector2(125, 40));
            bindings.selectionContextText = CreateText(bar.transform, "SelectionContextText", "治理 | 未选区", new Vector2(902, 0), new Vector2(0, 0.5f), new Vector2(145, 40));
            bindings.overlayBudgetText = CreateText(bar.transform, "OverlayBudgetText", "避让 Z/B/O 0/0/0 | 标 0/0 | 脉 0/0 额0", new Vector2(1026, -48), new Vector2(0, 0.5f), new Vector2(330, 28));
            bindings.overlayBudgetText.color = UITheme.TextSecondary;
            bindings.overlayBudgetText.horizontalOverflow = HorizontalWrapMode.Overflow;
            bindings.attackButton = CreateButton(bar.transform, "AttackButton", "进攻选区", new Vector2(1110, 0), new Vector2(0, 0.5f), new Vector2(110, 36));
            bindings.prepareFrontlineButton = CreateButton(bar.transform, "PrepareFrontlineButton", "整备前线", new Vector2(1110, -48), new Vector2(0, 0.5f), new Vector2(92, 28));
            bindings.techButton = CreateButton(bar.transform, "TechButton", "科技", new Vector2(1238, 0), new Vector2(0, 0.5f), new Vector2(80, 36));
            bindings.weatherButton = CreateButton(bar.transform, "WeatherButton", "天象", new Vector2(1328, 0), new Vector2(0, 0.5f), new Vector2(80, 36));
            bindings.eventButton = CreateButton(bar.transform, "EventButton", "事件", new Vector2(1418, 0), new Vector2(0, 0.5f), new Vector2(80, 36));
            bindings.mechanismButton = CreateButton(bar.transform, "MechanismButton", "政策胜利", new Vector2(1520, 0), new Vector2(0, 0.5f), new Vector2(100, 36));
            bindings.emperorButton = CreateButton(bar.transform, "EmperorButton", "帝皇", new Vector2(1630, 0), new Vector2(0, 0.5f), new Vector2(80, 36));
            bindings.courtButton = CreateButton(bar.transform, "CourtButton", "朝廷", new Vector2(1720, 0), new Vector2(0, 0.5f), new Vector2(80, 36));
            bindings.nextTurnButton = CreateButton(bar.transform, "NextTurnButton", "下一回合", new Vector2(1840, 0), new Vector2(0, 0.5f), new Vector2(120, 36));
            ApplyCompactHudLayout(bindings);
            return bindings;
        }

        private static void ApplyCompactHudLayout(HudBindings bindings)
        {
            SetRect(bindings.root, new Vector2(0, -28), new Vector2(0, 56));
            SetRect(bindings.turnText, new Vector2(14, 0), new Vector2(106, 34));
            SetRect(bindings.resourceText, new Vector2(128, 0), new Vector2(254, 34));
            SetRect(bindings.governanceModeButton, new Vector2(394, 0), new Vector2(72, 44));
            SetRect(bindings.warModeButton, new Vector2(472, 0), new Vector2(72, 44));
            SetRect(bindings.modeStateText, new Vector2(550, 0), new Vector2(106, 34));
            SetRect(bindings.selectionContextText, new Vector2(662, 0), new Vector2(154, 34));
            SetRect(bindings.attackButton, new Vector2(824, 0), new Vector2(82, 44));
            SetRect(bindings.prepareFrontlineButton, new Vector2(824, -48), new Vector2(82, 28));
            SetRect(bindings.nextTurnButton, new Vector2(914, 0), new Vector2(92, 44));
            SetRect(bindings.techButton, new Vector2(18, -48), new Vector2(48, 28));
            SetRect(bindings.weatherButton, new Vector2(72, -48), new Vector2(48, 28));
            SetRect(bindings.eventButton, new Vector2(126, -48), new Vector2(48, 28));
            SetRect(bindings.mechanismButton, new Vector2(186, -48), new Vector2(72, 28));
            SetRect(bindings.emperorButton, new Vector2(264, -48), new Vector2(48, 28));
            SetRect(bindings.courtButton, new Vector2(318, -48), new Vector2(48, 28));
            SetRect(bindings.overlayBudgetText, new Vector2(380, -48), new Vector2(300, 28));
            SetFontSize(bindings.turnText, 11);
            SetFontSize(bindings.resourceText, 11);
            SetFontSize(bindings.modeStateText, 11);
            SetFontSize(bindings.selectionContextText, 10);
            SetFontSize(bindings.overlayBudgetText, 10);
            SetButtonFontSize(bindings.governanceModeButton, 12);
            SetButtonFontSize(bindings.warModeButton, 12);
            SetButtonFontSize(bindings.attackButton, 11);
            SetButtonFontSize(bindings.prepareFrontlineButton, 10);
            SetButtonFontSize(bindings.nextTurnButton, 11);
            SetButtonFontSize(bindings.techButton, 10);
            SetButtonFontSize(bindings.weatherButton, 10);
            SetButtonFontSize(bindings.eventButton, 10);
            SetButtonFontSize(bindings.mechanismButton, 10);
            SetButtonFontSize(bindings.emperorButton, 10);
            SetButtonFontSize(bindings.courtButton, 10);
        }

        private void BuildStrategyControls(Transform parent)
        {
            GameObject lensBar = CreatePanel(parent, "StrategyLensBar", StrategyLayoutSpec.TopLeftAnchor, StrategyLayoutSpec.TopLeftAnchor, StrategyLayoutSpec.LensBarPosition, StrategyLayoutSpec.LensBarSize);
            CreateText(lensBar.transform, "StrategyLensStateText", "图层:治理", new Vector2(10, -2), new Vector2(0, 1), new Vector2(78, 28));
            CreateButton(lensBar.transform, "LensGovernanceButton", "治理", new Vector2(StrategyLayoutSpec.LensButtonStartX, -16), new Vector2(0, 1), new Vector2(44, 24));
            CreateButton(lensBar.transform, "LensRiskButton", "风险", new Vector2(StrategyLayoutSpec.LensButtonStartX + StrategyLayoutSpec.LensButtonStepX, -16), new Vector2(0, 1), new Vector2(44, 24));
            CreateButton(lensBar.transform, "LensEconomyButton", "财税", new Vector2(StrategyLayoutSpec.LensButtonStartX + StrategyLayoutSpec.LensButtonStepX * 2, -16), new Vector2(0, 1), new Vector2(44, 24));
            CreateButton(lensBar.transform, "LensLegitimacyButton", "法统", new Vector2(StrategyLayoutSpec.LensButtonStartX + StrategyLayoutSpec.LensButtonStepX * 3, -16), new Vector2(0, 1), new Vector2(44, 24));
            CreateButton(lensBar.transform, "LensWarButton", "战事", new Vector2(StrategyLayoutSpec.LensButtonStartX + StrategyLayoutSpec.LensButtonStepX * 4, -16), new Vector2(0, 1), new Vector2(44, 24));
            CreateButton(lensBar.transform, "LensTerrainButton", "地形", new Vector2(StrategyLayoutSpec.LensButtonStartX + StrategyLayoutSpec.LensButtonStepX * 5, -16), new Vector2(0, 1), new Vector2(44, 24));

            GameObject outliner = CreatePanel(parent, "StrategyOutlinerPanel", StrategyLayoutSpec.TopRightAnchor, StrategyLayoutSpec.TopRightAnchor, StrategyLayoutSpec.OutlinerDockedPosition, StrategyLayoutSpec.OutlinerSize);
            CreateText(outliner.transform, "StrategyOutlinerTitle", "军政待办", new Vector2(12, -10), new Vector2(0, 1), new Vector2(168, 26));
            CreateButton(outliner.transform, "StrategyOutlinerCollapseButton", "-", new Vector2(268, -12), new Vector2(1, 1), new Vector2(24, 24));
            Text outlinerText = CreateText(outliner.transform, "StrategyOutlinerText", "", new Vector2(14, -42), new Vector2(0, 1), new Vector2(266, 58));
            outlinerText.verticalOverflow = VerticalWrapMode.Truncate;

            GameObject collapsed = CreatePanel(parent, "StrategyOutlinerCollapsed", StrategyLayoutSpec.TopRightAnchor, StrategyLayoutSpec.TopRightAnchor, StrategyLayoutSpec.OutlinerDockedPosition, StrategyLayoutSpec.OutlinerCollapsedSize);
            CreateButton(collapsed.transform, "StrategyOutlinerExpandButton", "待办", new Vector2(37, -18), new Vector2(0.5f, 1), new Vector2(60, 26));

            GameObject logistics = CreatePanel(parent, "LogisticsQueuePanel", StrategyLayoutSpec.TopRightAnchor, StrategyLayoutSpec.TopRightAnchor, StrategyLayoutSpec.LogisticsQueuePosition, StrategyLayoutSpec.LogisticsQueueSize);
            CreateText(logistics.transform, "LogisticsQueueTitle", "后勤队列", new Vector2(12, -8), new Vector2(0, 1), new Vector2(90, 22));
            Text logisticsText = CreateText(logistics.transform, "LogisticsQueueText", "暂无前线运输队", new Vector2(12, -30), new Vector2(0, 1), new Vector2(270, 42));
            logisticsText.verticalOverflow = VerticalWrapMode.Truncate;
            CreateButton(logistics.transform, "LogisticsPriorityUpButton", "加急", new Vector2(42, -92), new Vector2(0, 1), new Vector2(54, 24));
            CreateButton(logistics.transform, "LogisticsPriorityDownButton", "后置", new Vector2(102, -92), new Vector2(0, 1), new Vector2(54, 24));
            CreateButton(logistics.transform, "LogisticsPauseButton", "暂停", new Vector2(162, -92), new Vector2(0, 1), new Vector2(54, 24));
            CreateButton(logistics.transform, "LogisticsCancelButton", "取消", new Vector2(222, -92), new Vector2(0, 1), new Vector2(54, 24));
        }

        private static void SetRect(GameObject target, Vector2 anchoredPosition, Vector2 size)
        {
            if (target == null) return;
            RectTransform rt = target.GetComponent<RectTransform>();
            if (rt == null) return;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = size;
        }

        private static void SetRect(Graphic graphic, Vector2 anchoredPosition, Vector2 size)
        {
            if (graphic == null) return;
            RectTransform rt = graphic.rectTransform;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = size;
        }

        private static void SetRect(Selectable selectable, Vector2 anchoredPosition, Vector2 size)
        {
            if (selectable == null) return;
            RectTransform rt = selectable.GetComponent<RectTransform>();
            if (rt == null) return;
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = size;

            Text label = selectable.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.rectTransform.sizeDelta = size;
            }
        }

        private static void SetFontSize(Text text, int size)
        {
            if (text != null) text.fontSize = size;
        }

        private static void SetButtonFontSize(Selectable selectable, int size)
        {
            if (selectable == null) return;
            Text label = selectable.GetComponentInChildren<Text>();
            if (label != null) label.fontSize = size;
        }

        private RegionPanelBindings BuildRegionPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "RegionPanel", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-216, -8), new Vector2(408, 620));
            RegionPanelBindings bindings = new RegionPanelBindings();
            bindings.root = panel;
            Text title = CreateText(panel.transform, "Title", "地区治理", new Vector2(-24, -10), new Vector2(0.5f, 1), new Vector2(282, 34));
            title.fontSize = Mathf.RoundToInt(UITheme.TextSizeHeader);
            title.color = UITheme.TextAccent;
            bindings.collapseButton = CreateButton(panel.transform, "CollapseRegionPanelButton", "收起", new Vector2(-38, -14), new Vector2(1, 1), new Vector2(54, 26));
            bindings.modeText = CreateText(panel.transform, "RegionPanelModeText", "治理模式 | 国内政务", new Vector2(16, -42), new Vector2(0, 1), new Vector2(374, 24));
            bindings.modeText.color = UITheme.TextAccent;
            float y = -72;
            bindings.regionNameText = CreateText(panel.transform, "RegionNameText", "名称：", new Vector2(16, y), new Vector2(0, 1), new Vector2(178, 24));
            bindings.terrainText = CreateText(panel.transform, "TerrainText", "地形：", new Vector2(204, y), new Vector2(0, 1), new Vector2(186, 24)); y -= 25;
            bindings.ownerText = CreateText(panel.transform, "OwnerText", "归属：", new Vector2(16, y), new Vector2(0, 1), new Vector2(178, 24));
            bindings.integrationText = CreateText(panel.transform, "IntegrationText", "整合度：", new Vector2(204, y), new Vector2(0, 1), new Vector2(186, 24)); y -= 25;
            bindings.populationText = CreateText(panel.transform, "PopulationText", "人口：", new Vector2(16, y), new Vector2(0, 1), new Vector2(178, 24));
            bindings.manpowerText = CreateText(panel.transform, "ManpowerText", "兵源：", new Vector2(204, y), new Vector2(0, 1), new Vector2(186, 24)); y -= 25;
            bindings.foodText = CreateText(panel.transform, "FoodText", "粮食：", new Vector2(16, y), new Vector2(0, 1), new Vector2(178, 24));
            bindings.taxText = CreateText(panel.transform, "TaxText", "税收：", new Vector2(204, y), new Vector2(0, 1), new Vector2(186, 24)); y -= 25;
            bindings.rebellionText = CreateText(panel.transform, "RebellionText", "民变风险：", new Vector2(16, y), new Vector2(0, 1), new Vector2(178, 24));
            bindings.localPowerText = CreateText(panel.transform, "LocalPowerText", "地方势力：", new Vector2(204, y), new Vector2(0, 1), new Vector2(186, 24)); y -= 25;
            bindings.annexationText = CreateText(panel.transform, "AnnexationText", "土地兼并：", new Vector2(16, y), new Vector2(0, 1), new Vector2(178, 24));
            bindings.neighborsText = CreateText(panel.transform, "NeighborsText", "相邻：", new Vector2(204, y), new Vector2(0, 1), new Vector2(186, 30)); y -= 34;
            CreateBand(panel.transform, "RegionMetricsBand", new Vector2(16, y + 88), new Vector2(374, 166), UITheme.SectionBackground);
            CreateMetricBar(panel.transform, "GovernanceIntegrationUiBar", new Vector2(278, -116), new Vector2(98, 5));
            CreateMetricBar(panel.transform, "GovernanceFoodUiBar", new Vector2(90, -166), new Vector2(98, 5));
            CreateMetricBar(panel.transform, "GovernanceTaxUiBar", new Vector2(278, -166), new Vector2(98, 5));
            CreateMetricBar(panel.transform, "GovernanceRebellionUiBar", new Vector2(90, -191), new Vector2(98, 5));
            bindings.landStructureText = CreateText(panel.transform, "LandStructureText", "土地结构：", new Vector2(16, y), new Vector2(0, 1), new Vector2(374, 28)); y -= 30;
            bindings.customsText = CreateText(panel.transform, "CustomsText", "风俗：", new Vector2(16, y), new Vector2(0, 1), new Vector2(374, 34)); y -= 40;
            CreateBand(panel.transform, "RegionDecisionBand", new Vector2(16, y + 6), new Vector2(374, 142), UITheme.DecisionBandBackground);
            CreateGovernanceBadge(panel.transform, "GovernanceStageBadge", new Vector2(20, y - 2), new Vector2(86, 18));
            CreateGovernanceBadge(panel.transform, "GovernanceRiskBadge", new Vector2(110, y - 2), new Vector2(86, 18));
            CreateGovernanceBadge(panel.transform, "GovernanceIntegrationBadge", new Vector2(200, y - 2), new Vector2(86, 18));
            CreateGovernanceBadge(panel.transform, "GovernanceContributionBadge", new Vector2(290, y - 2), new Vector2(96, 18));
            bindings.governanceOverviewText = CreateText(panel.transform, "GovernanceOverviewText", "", new Vector2(16, y - 24), new Vector2(0, 1), new Vector2(374, 114));
            bindings.governanceOverviewText.fontSize = 12;
            bindings.governanceOverviewText.verticalOverflow = VerticalWrapMode.Truncate;
            y -= 148;
            CreateBand(panel.transform, "RegionSourceBand", new Vector2(16, y + 6), new Vector2(374, 82), UITheme.SourceBandBackground);
            bindings.governanceSourceText = CreateText(panel.transform, "GovernanceSourceText", "", new Vector2(16, y), new Vector2(0, 1), new Vector2(374, 78));
            bindings.governanceSourceText.fontSize = 10;
            bindings.governanceSourceText.color = UITheme.TextSecondary;
            bindings.governanceSourceText.verticalOverflow = VerticalWrapMode.Truncate;
            bindings.pacifyButton = CreateButton(panel.transform, "PacifyRegionButton", "安抚", new Vector2(-124, 24), new Vector2(0.5f, 0), new Vector2(118, 42));
            CreateButtonActionIcon(bindings.pacifyButton, "GovernancePacifyActionIcon", "政", UITheme.MeterGood);
            CreateButtonActionHint(bindings.pacifyButton, "GovernancePacifyActionHintText");
            bindings.buildButton = CreateButton(panel.transform, "BuildRegionBuildingButton", "建造", new Vector2(0, 24), new Vector2(0.5f, 0), new Vector2(118, 42));
            CreateButtonActionIcon(bindings.buildButton, "GovernanceBuildActionIcon", "营", UITheme.MoneyColor);
            CreateButtonActionHint(bindings.buildButton, "GovernanceBuildActionHintText");
            bindings.closeButton = CreateButton(panel.transform, "CloseButton", "关闭", new Vector2(128, 20), new Vector2(0.5f, 0), new Vector2(68, 28));
            ApplyRegionPanelTextHierarchy(bindings);

            GameObject collapsed = CreatePanel(parent, "CollapsedRegionTab", new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-58, 0), new Vector2(104, 168));
            bindings.collapsedRoot = collapsed;
            bindings.collapsedTabText = CreateText(collapsed.transform, "CollapsedRegionTabText", "治理模式\n未选区", new Vector2(8, -12), new Vector2(0, 1), new Vector2(88, 84));
            bindings.collapsedTabText.alignment = TextAnchor.UpperCenter;
            bindings.collapsedTabText.color = UITheme.TextAccent;
            bindings.expandButton = CreateButton(collapsed.transform, "CollapsedRegionTabButton", "展开", new Vector2(0, 20), new Vector2(0.5f, 0), new Vector2(76, 30));
            return bindings;
        }

        private static void ApplyRegionPanelTextHierarchy(RegionPanelBindings bindings)
        {
            SetTextStyle(bindings.regionNameText, 15, UITheme.TextAccent);
            SetTextStyle(bindings.modeText, 11, UITheme.TextAccent);
            SetTextStyle(bindings.terrainText, 11, UITheme.TextSecondary);
            SetTextStyle(bindings.ownerText, 11, UITheme.TextSecondary);
            SetTextStyle(bindings.integrationText, 11, UITheme.TextSecondary);
            SetTextStyle(bindings.populationText, 11, UITheme.TextSecondary);
            SetTextStyle(bindings.manpowerText, 11, UITheme.TextSecondary);
            SetTextStyle(bindings.foodText, 11, UITheme.TextSecondary);
            SetTextStyle(bindings.taxText, 11, UITheme.TextSecondary);
            SetTextStyle(bindings.rebellionText, 11, UITheme.TextSecondary);
            SetTextStyle(bindings.localPowerText, 11, UITheme.TextSecondary);
            SetTextStyle(bindings.annexationText, 11, UITheme.TextSecondary);
            SetTextStyle(bindings.neighborsText, 11, UITheme.TextSecondary);
            SetTextStyle(bindings.landStructureText, 11, UITheme.TextSecondary);
            SetTextStyle(bindings.customsText, 11, UITheme.TextSecondary);
            SetTextStyle(bindings.governanceOverviewText, 11, UITheme.TextPrimary);
            bindings.governanceOverviewText.lineSpacing = 1.12f;
            bindings.governanceOverviewText.horizontalOverflow = HorizontalWrapMode.Overflow;
            SetTextStyle(bindings.governanceSourceText, 9, UITheme.TextSecondary);
            bindings.governanceSourceText.lineSpacing = 1.04f;
            bindings.governanceSourceText.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        private static void SetTextStyle(Text text, int size, Color color)
        {
            if (text == null) return;
            text.fontSize = size;
            text.color = color;
        }

        private EmperorPanelBindings BuildEmperorPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "EmperorPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(500, 600));
            EmperorPanelBindings bindings = new EmperorPanelBindings();
            bindings.root = panel;
            CreateText(panel.transform, "Title", "帝皇信息", new Vector2(0, -10), new Vector2(0.5f, 1), new Vector2(480, 36));
            float y = -50;
            bindings.emperorNameText = CreateText(panel.transform, "EmperorNameText", "帝皇：", new Vector2(10, y), new Vector2(0, 1), new Vector2(480, 28)); y -= 32;
            bindings.titleText = CreateText(panel.transform, "TitleText", "称号：", new Vector2(10, y), new Vector2(0, 1), new Vector2(480, 24)); y -= 28;
            bindings.mechanicNameText = CreateText(panel.transform, "MechanicNameText", "机制：", new Vector2(10, y), new Vector2(0, 1), new Vector2(480, 24)); y -= 28;
            bindings.mechanicDescText = CreateText(panel.transform, "MechanicDescText", "", new Vector2(10, y), new Vector2(0, 1), new Vector2(480, 40)); y -= 44;
            bindings.statsText = CreateText(panel.transform, "StatsText", "属性：", new Vector2(10, y), new Vector2(0, 1), new Vector2(480, 24)); y -= 28;
            bindings.burdensText = CreateText(panel.transform, "BurdensText", "负担：", new Vector2(10, y), new Vector2(0, 1), new Vector2(480, 24)); y -= 28;
            bindings.legitimacyText = CreateText(panel.transform, "LegitimacyText", "合法性：", new Vector2(10, y), new Vector2(0, 1), new Vector2(240, 24));
            bindings.successionText = CreateText(panel.transform, "SuccessionText", "继承风险：", new Vector2(250, y), new Vector2(0, 1), new Vector2(240, 24)); y -= 28;
            bindings.heirText = CreateText(panel.transform, "HeirText", "继承人：", new Vector2(10, y), new Vector2(0, 1), new Vector2(480, 24)); y -= 28;
            bindings.stableSuccessionsText = CreateText(panel.transform, "StableSuccessionsText", "平稳继承：", new Vector2(10, y), new Vector2(0, 1), new Vector2(480, 24)); y -= 32;
            bindings.skillText = CreateText(panel.transform, "EmperorSkillText", "主动技能：", new Vector2(10, y), new Vector2(0, 1), new Vector2(480, 150));
            bindings.resolveSuccessionButton = CreateButton(panel.transform, "ResolveSuccessionButton", "处理继承", new Vector2(-170, 10), new Vector2(0.5f, 0), new Vector2(110, 28));
            bindings.useSkillButton = CreateButton(panel.transform, "UseEmperorSkillButton", "发动首个技能", new Vector2(-40, 10), new Vector2(0.5f, 0), new Vector2(140, 28));
            bindings.closeButton = CreateButton(panel.transform, "CloseButton", "关闭", new Vector2(120, 10), new Vector2(0.5f, 0), new Vector2(80, 28));
            return bindings;
        }

        private CourtPanelBindings BuildCourtPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "CourtPanel", new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(10, 0), new Vector2(350, 600));
            CourtPanelBindings bindings = new CourtPanelBindings();
            bindings.root = panel;
            CreateText(panel.transform, "Title", "朝廷总览", new Vector2(0, -10), new Vector2(0.5f, 1), new Vector2(330, 36));
            float y = -50;
            bindings.factionNameText = CreateText(panel.transform, "FactionNameText", "势力：", new Vector2(10, y), new Vector2(0, 1), new Vector2(330, 24)); y -= 28;
            bindings.emperorNameText = CreateText(panel.transform, "EmperorNameText", "帝皇：", new Vector2(10, y), new Vector2(0, 1), new Vector2(330, 24)); y -= 28;
            bindings.moneyText = CreateText(panel.transform, "MoneyText", "金钱：", new Vector2(10, y), new Vector2(0, 1), new Vector2(160, 24));
            bindings.foodText = CreateText(panel.transform, "FoodText", "粮食：", new Vector2(170, y), new Vector2(0, 1), new Vector2(160, 24)); y -= 28;
            bindings.legitimacyText = CreateText(panel.transform, "LegitimacyText", "合法性：", new Vector2(10, y), new Vector2(0, 1), new Vector2(160, 24));
            bindings.successionText = CreateText(panel.transform, "SuccessionText", "继承风险：", new Vector2(170, y), new Vector2(0, 1), new Vector2(160, 24)); y -= 28;
            bindings.courtPressureText = CreateText(panel.transform, "CourtPressureText", "朝局压力：", new Vector2(10, y), new Vector2(0, 1), new Vector2(160, 24));
            bindings.regionCountText = CreateText(panel.transform, "RegionCountText", "领地：", new Vector2(170, y), new Vector2(0, 1), new Vector2(160, 24)); y -= 28;
            bindings.talentCountText = CreateText(panel.transform, "TalentCountText", "人才：", new Vector2(10, y), new Vector2(0, 1), new Vector2(330, 24)); y -= 36;
            CreateText(panel.transform, "LogTitle", "回合日志", new Vector2(10, y), new Vector2(0, 1), new Vector2(330, 24)); y -= 28;
            GameObject portraitViewport = CreatePanel(panel.transform, "GeneralPortraitGridViewport", new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -245), new Vector2(-20, 132));
            ScrollRect portraitScroll = portraitViewport.AddComponent<ScrollRect>();
            portraitScroll.horizontal = true;
            portraitScroll.vertical = false;
            GameObject portraitContent = new GameObject("GeneralPortraitGridContent");
            portraitContent.transform.SetParent(portraitViewport.transform, false);
            RectTransform portraitContentRect = portraitContent.AddComponent<RectTransform>();
            portraitContentRect.anchorMin = new Vector2(0, 0.5f);
            portraitContentRect.anchorMax = new Vector2(0, 0.5f);
            portraitContentRect.pivot = new Vector2(0, 0.5f);
            portraitContentRect.anchoredPosition = new Vector2(8, 0);
            portraitContentRect.sizeDelta = new Vector2(980, 110);
            HorizontalLayoutGroup portraitLayout = portraitContent.AddComponent<HorizontalLayoutGroup>();
            portraitLayout.padding = new RectOffset(4, 4, 6, 6);
            portraitLayout.spacing = 8;
            portraitLayout.childForceExpandWidth = false;
            portraitLayout.childForceExpandHeight = false;
            portraitScroll.content = portraitContentRect;
            bindings.generalPortraitGridContent = portraitContentRect;
            GameObject scrollObj = CreatePanel(panel.transform, "LogScroll", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 120), new Vector2(-20, 240));
            bindings.scrollRect = scrollObj.AddComponent<ScrollRect>();
            bindings.turnLogText = CreateText(scrollObj.transform, "TurnLogText", "", new Vector2(10, -10), new Vector2(0, 1), new Vector2(310, 2000));
            bindings.scrollRect.content = bindings.turnLogText.rectTransform;
            bindings.recruitArmyButton = CreateButton(panel.transform, "RecruitArmyButton", "募兵", new Vector2(-115, 10), new Vector2(0.5f, 0), new Vector2(80, 28));
            bindings.equipArmyButton = CreateButton(panel.transform, "EquipArmyButton", "整备军队", new Vector2(-20, 10), new Vector2(0.5f, 0), new Vector2(100, 28));
            bindings.closeButton = CreateButton(panel.transform, "CloseButton", "关闭", new Vector2(90, 10), new Vector2(0.5f, 0), new Vector2(80, 28));
            return bindings;
        }

        private EventPanelBindings BuildEventPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "EventPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 50), new Vector2(450, 350));
            EventPanelBindings bindings = new EventPanelBindings();
            bindings.root = panel;
            CreateText(panel.transform, "Title", "事件", new Vector2(0, -10), new Vector2(0.5f, 1), new Vector2(430, 36));
            bindings.eventNameText = CreateText(panel.transform, "EventNameText", "", new Vector2(10, -50), new Vector2(0, 1), new Vector2(430, 28));
            bindings.categoryText = CreateText(panel.transform, "CategoryText", "", new Vector2(10, -80), new Vector2(0, 1), new Vector2(430, 24));
            bindings.choiceText = CreateText(panel.transform, "ChoiceText", "", new Vector2(10, -110), new Vector2(0, 1), new Vector2(430, 108));
            bindings.choiceButtons = new Button[3];
            bindings.choiceButtons[0] = CreateButton(panel.transform, "EventChoiceButton0", "Choice 1", new Vector2(0, -226), new Vector2(0.5f, 1), new Vector2(390, 30));
            bindings.choiceButtons[1] = CreateButton(panel.transform, "EventChoiceButton1", "Choice 2", new Vector2(0, -262), new Vector2(0.5f, 1), new Vector2(390, 30));
            bindings.choiceButtons[2] = CreateButton(panel.transform, "EventChoiceButton2", "Choice 3", new Vector2(0, -298), new Vector2(0.5f, 1), new Vector2(390, 30));
            bindings.closeButton = CreateButton(panel.transform, "CloseEventPanelButton", "关闭", new Vector2(0, 18), new Vector2(0.5f, 0), new Vector2(92, 28));
            return bindings;
        }

        private BattleReportPanelBindings BuildBattleReportPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "BattleReportPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 44), new Vector2(500, 392));
            BattleReportPanelBindings bindings = new BattleReportPanelBindings();
            bindings.root = panel;
            CreateText(panel.transform, "Title", "战报", new Vector2(0, -10), new Vector2(0.5f, 1), new Vector2(480, 34));
            CreateBattleRibbon(panel.transform, "BattleOutcomeRibbon", new Vector2(10, -46), new Vector2(480, 28));
            bindings.attackerText = CreateText(panel.transform, "AttackerText", "", new Vector2(10, -82), new Vector2(0, 1), new Vector2(220, 22));
            bindings.defenderText = CreateText(panel.transform, "DefenderText", "", new Vector2(260, -82), new Vector2(0, 1), new Vector2(220, 22));
            bindings.resultText = CreateText(panel.transform, "ResultText", "", new Vector2(12, -110), new Vector2(0, 1), new Vector2(220, 20));
            bindings.resultText.fontSize = 10;
            bindings.resultText.color = UITheme.TextSecondary;
            CreateMetricBar(panel.transform, "BattleAttackerPowerUiBar", new Vector2(92, -136), new Vector2(136, 8));
            CreateMetricBar(panel.transform, "BattleDefenderPowerUiBar", new Vector2(342, -136), new Vector2(136, 8));
            CreateText(panel.transform, "BattleAttackerPowerLabel", "攻方战力", new Vector2(10, -145), new Vector2(0, 1), new Vector2(78, 20)).fontSize = 10;
            CreateText(panel.transform, "BattleDefenderPowerLabel", "守方战力", new Vector2(260, -145), new Vector2(0, 1), new Vector2(78, 20)).fontSize = 10;
            CreateBattleBadge(panel.transform, "BattleAttackerSupplyBadge", new Vector2(10, -164), new Vector2(220, 20));
            CreateBattleBadge(panel.transform, "BattleDefenderSupplyBadge", new Vector2(260, -164), new Vector2(220, 20));
            bindings.detailsText = CreateText(panel.transform, "DetailsText", "", new Vector2(10, -194), new Vector2(0, 1), new Vector2(480, 128));
            bindings.detailsText.fontSize = 11;
            bindings.focusButton = CreateButton(panel.transform, "FocusBattleReportRegionButton", "定位战场", new Vector2(-58, 18), new Vector2(0.5f, 0), new Vector2(96, 28));
            bindings.closeButton = CreateButton(panel.transform, "CloseButton", "关闭", new Vector2(54, 18), new Vector2(0.5f, 0), new Vector2(80, 28));
            return bindings;
        }

        private TechPanelBindings BuildTechPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "TechPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-260, 0), new Vector2(500, 620));
            TechPanelBindings bindings = new TechPanelBindings();
            bindings.root = panel;
            bindings.titleText = CreateText(panel.transform, "Title", "科技研究", new Vector2(0, -10), new Vector2(0.5f, 1), new Vector2(480, 36));
            bindings.currentResearchText = CreateText(panel.transform, "CurrentResearchText", "", new Vector2(10, -50), new Vector2(0, 1), new Vector2(480, 28));
            bindings.progressText = CreateText(panel.transform, "ProgressText", "", new Vector2(10, -82), new Vector2(0, 1), new Vector2(480, 28));
            bindings.availableTechsText = CreateText(panel.transform, "AvailableTechsText", "", new Vector2(10, -120), new Vector2(0, 1), new Vector2(480, 260));
            bindings.completedTechsText = CreateText(panel.transform, "CompletedTechsText", "", new Vector2(10, -390), new Vector2(0, 1), new Vector2(480, 160));
            bindings.setResearchButton = CreateButton(panel.transform, "SetResearchButton", "选择首项研究", new Vector2(-75, 10), new Vector2(0.5f, 0), new Vector2(130, 28));
            bindings.closeButton = CreateButton(panel.transform, "CloseButton", "关闭", new Vector2(80, 10), new Vector2(0.5f, 0), new Vector2(80, 28));
            return bindings;
        }

        private WeatherPanelBindings BuildWeatherPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "WeatherPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(260, 0), new Vector2(460, 360));
            WeatherPanelBindings bindings = new WeatherPanelBindings();
            bindings.root = panel;
            CreateText(panel.transform, "Title", "天气与天象", new Vector2(0, -10), new Vector2(0.5f, 1), new Vector2(440, 36));
            bindings.weatherNameText = CreateText(panel.transform, "WeatherNameText", "", new Vector2(10, -55), new Vector2(0, 1), new Vector2(440, 28));
            bindings.weatherEffectText = CreateText(panel.transform, "WeatherEffectText", "", new Vector2(10, -92), new Vector2(0, 1), new Vector2(440, 60));
            bindings.celestialEventText = CreateText(panel.transform, "CelestialEventText", "", new Vector2(10, -165), new Vector2(0, 1), new Vector2(440, 90));
            bindings.resilienceText = CreateText(panel.transform, "ResilienceText", "", new Vector2(10, -270), new Vector2(0, 1), new Vector2(440, 28));
            bindings.closeButton = CreateButton(panel.transform, "CloseButton", "关闭", new Vector2(0, 10), new Vector2(0.5f, 0), new Vector2(80, 28));
            return bindings;
        }

        private MechanismPanelBindings BuildMechanismPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "MechanismPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(520, 560));
            MechanismPanelBindings bindings = new MechanismPanelBindings();
            bindings.root = panel;
            bindings.titleText = CreateText(panel.transform, "Title", "政策、外交、谍报与胜利", new Vector2(0, -10), new Vector2(0.5f, 1), new Vector2(500, 36));
            bindings.detailsText = CreateText(panel.transform, "DetailsText", "", new Vector2(10, -55), new Vector2(0, 1), new Vector2(500, 360));
            bindings.applyPolicyButton = CreateButton(panel.transform, "ApplyPolicyButton", "执行政策", new Vector2(-195, 44), new Vector2(0.5f, 0), new Vector2(110, 28));
            bindings.diplomacyButton = CreateButton(panel.transform, "DiplomacyActionButton", "外交行动", new Vector2(-70, 44), new Vector2(0.5f, 0), new Vector2(110, 28));
            bindings.borderButton = CreateButton(panel.transform, "BorderControlButton", "边境管控", new Vector2(55, 44), new Vector2(0.5f, 0), new Vector2(110, 28));
            bindings.espionageButton = CreateButton(panel.transform, "EspionageActionButton", "刺探情报", new Vector2(180, 44), new Vector2(0.5f, 0), new Vector2(110, 28));
            bindings.enterWarModeButton = CreateButton(panel.transform, "EnterWarModeButton", "进入战争", new Vector2(-55, 14), new Vector2(0.5f, 0), new Vector2(110, 28));
            CreateButtonLeadingIcon(bindings.applyPolicyButton, "PolicyActionIcon", "策", UITheme.MeterGood);
            CreateButtonLeadingIcon(bindings.diplomacyButton, "DiplomacyActionIcon", "交", UITheme.ResearchColor);
            CreateButtonLeadingIcon(bindings.borderButton, "BorderActionIcon", "关", UITheme.MeterWarning);
            CreateButtonLeadingIcon(bindings.espionageButton, "EspionageActionIcon", "谍", UITheme.LegitimacyColor);
            CreateButtonLeadingIcon(bindings.enterWarModeButton, "EnterWarModeActionIcon", "战", UITheme.DangerColor);
            bindings.closeButton = CreateButton(panel.transform, "CloseButton", "关闭", new Vector2(70, 14), new Vector2(0.5f, 0), new Vector2(80, 28));
            return bindings;
        }

        private GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            Image img = obj.AddComponent<Image>();
            img.color = UITheme.PanelBackgroundAlpha;
            Shadow shadow = obj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.48f);
            shadow.effectDistance = new Vector2(4f, -4f);
            Outline outline = obj.AddComponent<Outline>();
            outline.effectColor = UITheme.ButtonBorder;
            outline.effectDistance = new Vector2(1f, -1f);
            return obj;
        }

        private Text CreateText(Transform parent, string name, string content, Vector2 anchoredPos, Vector2 anchor, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            Text text = obj.AddComponent<Text>();
            text.text = content;
            text.fontSize = Mathf.RoundToInt(UITheme.TextSizeBody);
            text.color = UITheme.TextPrimary;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.lineSpacing = 1.03f;
            text.supportRichText = true;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return text;
        }

        private static GameObject CreateBand(Transform parent, string name, Vector2 anchoredPos, Vector2 size, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            Image image = obj.AddComponent<Image>();
            image.color = color;
            return obj;
        }

        private static void CreateMetricBar(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
        {
            GameObject track = new GameObject(name);
            track.transform.SetParent(parent, false);
            RectTransform rt = track.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            Image trackImage = track.AddComponent<Image>();
            trackImage.color = UITheme.MeterTrack;

            GameObject fill = new GameObject(name + "Fill");
            fill.transform.SetParent(track.transform, false);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = new Vector2(size.x, 0);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = UITheme.MeterGood;
        }

        private Text CreateGovernanceBadge(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
        {
            GameObject badge = new GameObject(name);
            badge.transform.SetParent(parent, false);
            RectTransform rt = badge.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            Image image = badge.AddComponent<Image>();
            image.color = UITheme.BadgeNormal;

            Text label = CreateText(badge.transform, name + "Text", "", Vector2.zero, new Vector2(0.5f, 0.5f), size);
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = 9;
            label.color = UITheme.ButtonText;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            return label;
        }

        private Text CreateBattleRibbon(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
        {
            GameObject ribbon = new GameObject(name);
            ribbon.transform.SetParent(parent, false);
            RectTransform rt = ribbon.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            Image image = ribbon.AddComponent<Image>();
            image.color = UITheme.BattleLoss;

            Text label = CreateText(ribbon.transform, name + "Text", "", Vector2.zero, new Vector2(0.5f, 0.5f), size);
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = 14;
            label.color = UITheme.ButtonText;
            return label;
        }

        private Text CreateBattleBadge(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
        {
            GameObject badge = new GameObject(name);
            badge.transform.SetParent(parent, false);
            RectTransform rt = badge.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            Image image = badge.AddComponent<Image>();
            image.color = UITheme.BadgeNormal;

            Text label = CreateText(badge.transform, name + "Text", "", Vector2.zero, new Vector2(0.5f, 0.5f), size);
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = 10;
            label.color = UITheme.ButtonText;
            return label;
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 anchor, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            Image img = obj.AddComponent<Image>();
            img.color = UITheme.ButtonNormal;
            Shadow shadow = obj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.36f);
            shadow.effectDistance = new Vector2(2f, -2f);
            Outline outline = obj.AddComponent<Outline>();
            outline.effectColor = UITheme.ButtonBorder;
            outline.effectDistance = new Vector2(1f, -1f);
            Button btn = obj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = UITheme.ButtonNormal;
            colors.highlightedColor = UITheme.ButtonHover;
            colors.pressedColor = UITheme.ButtonPressed;
            btn.colors = colors;
            Text text = CreateText(obj.transform, "Label", label, Vector2.zero, new Vector2(0.5f, 0.5f), size);
            text.alignment = TextAnchor.MiddleCenter;
            text.color = UITheme.ButtonText;
            return btn;
        }

        private Text CreateButtonActionIcon(Button button, string iconName, string glyph, Color color)
        {
            if (button == null) return null;

            RectTransform buttonRect = button.GetComponent<RectTransform>();
            Vector2 buttonSize = buttonRect != null ? buttonRect.sizeDelta : new Vector2(118, 42);
            GameObject icon = new GameObject(iconName);
            icon.transform.SetParent(button.transform, false);
            RectTransform rt = icon.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(-buttonSize.x * 0.5f + 18f, 10f);
            rt.sizeDelta = new Vector2(22, 22);

            Image image = icon.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            Outline outline = icon.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.42f);
            outline.effectDistance = new Vector2(1f, -1f);

            Text text = CreateText(icon.transform, iconName + "Glyph", glyph, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(22, 22));
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 11;
            text.color = UITheme.ButtonText;
            text.raycastTarget = false;
            return text;
        }

        private Text CreateButtonActionHint(Button button, string hintName)
        {
            if (button == null) return null;

            RectTransform buttonRect = button.GetComponent<RectTransform>();
            Vector2 buttonSize = buttonRect != null ? buttonRect.sizeDelta : new Vector2(118, 42);
            Text label = button.transform.Find("Label") != null ? button.transform.Find("Label").GetComponent<Text>() : null;
            if (label != null)
            {
                RectTransform labelRect = label.rectTransform;
                labelRect.anchorMin = new Vector2(0.5f, 0.5f);
                labelRect.anchorMax = new Vector2(0.5f, 0.5f);
                labelRect.pivot = new Vector2(0.5f, 0.5f);
                labelRect.anchoredPosition = new Vector2(11, 8);
                labelRect.sizeDelta = new Vector2(buttonSize.x - 42, 18);
                label.fontSize = 12;
                label.alignment = TextAnchor.MiddleCenter;
                label.verticalOverflow = VerticalWrapMode.Truncate;
            }

            Text hint = CreateText(button.transform, hintName, "", new Vector2(0, 4), new Vector2(0.5f, 0), new Vector2(buttonSize.x - 10, 16));
            hint.alignment = TextAnchor.MiddleCenter;
            hint.fontSize = 9;
            hint.color = UITheme.TextSecondary;
            hint.horizontalOverflow = HorizontalWrapMode.Overflow;
            hint.verticalOverflow = VerticalWrapMode.Truncate;
            return hint;
        }

        private Text CreateButtonLeadingIcon(Button button, string iconName, string glyph, Color color)
        {
            if (button == null) return null;

            RectTransform buttonRect = button.GetComponent<RectTransform>();
            Vector2 buttonSize = buttonRect != null ? buttonRect.sizeDelta : new Vector2(110, 28);
            const float iconSize = 18f;
            GameObject icon = new GameObject(iconName);
            icon.transform.SetParent(button.transform, false);
            RectTransform rt = icon.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(-buttonSize.x * 0.5f + 14f, 0f);
            rt.sizeDelta = new Vector2(iconSize, iconSize);

            Image image = icon.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            Outline outline = icon.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.42f);
            outline.effectDistance = new Vector2(1f, -1f);

            Text text = CreateText(icon.transform, iconName + "Glyph", glyph, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(iconSize, iconSize));
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 10;
            text.color = UITheme.ButtonText;
            text.raycastTarget = false;

            Transform labelTransform = button.transform.Find("Label");
            Text label = labelTransform != null ? labelTransform.GetComponent<Text>() : null;
            if (label != null)
            {
                RectTransform labelRect = label.rectTransform;
                labelRect.anchorMin = new Vector2(0.5f, 0.5f);
                labelRect.anchorMax = new Vector2(0.5f, 0.5f);
                labelRect.pivot = new Vector2(0.5f, 0.5f);
                labelRect.anchoredPosition = new Vector2(10f, 0f);
                labelRect.sizeDelta = new Vector2(buttonSize.x - 34f, buttonSize.y);
                label.fontSize = Mathf.Min(label.fontSize, 11);
                label.alignment = TextAnchor.MiddleCenter;
                label.verticalOverflow = VerticalWrapMode.Truncate;
            }

            return text;
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private sealed class HudBindings
        {
            public GameObject root;
            public Text turnText;
            public Text resourceText;
            public Text selectionContextText;
            public Text modeStateText;
            public Text overlayBudgetText;
            public Button governanceModeButton;
            public Button warModeButton;
            public Button nextTurnButton;
            public Button courtButton;
            public Button emperorButton;
            public Button attackButton;
            public Button prepareFrontlineButton;
            public Button techButton;
            public Button weatherButton;
            public Button eventButton;
            public Button mechanismButton;
        }

        private sealed class RegionPanelBindings
        {
            public GameObject root;
            public GameObject collapsedRoot;
            public Text modeText;
            public Text regionNameText;
            public Text terrainText;
            public Text populationText;
            public Text foodText;
            public Text taxText;
            public Text manpowerText;
            public Text ownerText;
            public Text integrationText;
            public Text rebellionText;
            public Text annexationText;
            public Text localPowerText;
            public Text neighborsText;
            public Text landStructureText;
            public Text customsText;
            public Text governanceOverviewText;
            public Text governanceSourceText;
            public Text collapsedTabText;
            public Button pacifyButton;
            public Button buildButton;
            public Button collapseButton;
            public Button expandButton;
            public Button closeButton;
        }

        private sealed class EmperorPanelBindings
        {
            public GameObject root;
            public Text emperorNameText;
            public Text titleText;
            public Text mechanicNameText;
            public Text mechanicDescText;
            public Text statsText;
            public Text burdensText;
            public Text legitimacyText;
            public Text successionText;
            public Text heirText;
            public Text stableSuccessionsText;
            public Text skillText;
            public Button resolveSuccessionButton;
            public Button useSkillButton;
            public Button closeButton;
        }

        private sealed class CourtPanelBindings
        {
            public GameObject root;
            public Text factionNameText;
            public Text emperorNameText;
            public Text moneyText;
            public Text foodText;
            public Text legitimacyText;
            public Text successionText;
            public Text courtPressureText;
            public Text regionCountText;
            public Text talentCountText;
            public Text turnLogText;
            public ScrollRect scrollRect;
            public RectTransform generalPortraitGridContent;
            public Button recruitArmyButton;
            public Button equipArmyButton;
            public Button closeButton;
        }

        private sealed class EventPanelBindings
        {
            public GameObject root;
            public Text eventNameText;
            public Text categoryText;
            public Text choiceText;
            public Button[] choiceButtons;
            public Button closeButton;
        }

        private sealed class BattleReportPanelBindings
        {
            public GameObject root;
            public Text attackerText;
            public Text defenderText;
            public Text resultText;
            public Text detailsText;
            public Button focusButton;
            public Button closeButton;
        }

        private sealed class TechPanelBindings
        {
            public GameObject root;
            public Text titleText;
            public Text currentResearchText;
            public Text progressText;
            public Text availableTechsText;
            public Text completedTechsText;
            public Button setResearchButton;
            public Button closeButton;
        }

        private sealed class WeatherPanelBindings
        {
            public GameObject root;
            public Text weatherNameText;
            public Text weatherEffectText;
            public Text celestialEventText;
            public Text resilienceText;
            public Button closeButton;
        }

        private sealed class MechanismPanelBindings
        {
            public GameObject root;
            public Text titleText;
            public Text detailsText;
            public Button applyPolicyButton;
            public Button diplomacyButton;
            public Button borderButton;
            public Button enterWarModeButton;
            public Button espionageButton;
            public Button closeButton;
        }
    }
}
