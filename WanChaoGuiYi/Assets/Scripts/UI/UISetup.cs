using UnityEngine;
using UnityEngine.UI;

namespace WanChaoGuiYi
{
    public sealed class UISetup : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        private MainMapUI mainMapUI;
        private RegionPanel regionPanel;
        private EmperorPanel emperorPanel;
        private CourtPanel courtPanel;
        private EventPanel eventPanel;
        private BattleReportPanel battleReportPanel;

        private void Awake()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            Canvas canvas = EnsureCanvas();
            CanvasScaler scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvas.gameObject.AddComponent<GraphicRaycaster>();

            // HUD 顶部栏
            GameObject hudBar = BuildHUDBar(canvas.transform);

            // 地区面板（右侧）
            GameObject regionPanelObj = BuildRegionPanel(canvas.transform);

            // 帝皇面板（左上）
            GameObject emperorPanelObj = BuildEmperorPanel(canvas.transform);

            // 朝廷面板（左下）
            GameObject courtPanelObj = BuildCourtPanel(canvas.transform);

            // 事件面板（居中弹窗）
            GameObject eventPanelObj = BuildEventPanel(canvas.transform);

            // 战报面板（居中弹窗）
            GameObject battlePanelObj = BuildBattleReportPanel(canvas.transform);

            // 连接 MainMapUI
            mainMapUI = gameObject.AddComponent<MainMapUI>();
            // 通过 SendMessage 或直接引用连接（需在 Inspector 中配置）
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

        private GameObject BuildHUDBar(Transform parent)
        {
            GameObject bar = CreatePanel(parent, "HUDBar", new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(0, 1080 - 50), new Vector2(1920, 1080));

            // 回合信息
            CreateText(bar.transform, "TurnText", "回合 1 | 1年春",
                new Vector2(20, 0), new Vector2(0, 0.5f), new Vector2(200, 40));

            // 资源信息
            CreateText(bar.transform, "ResourceText", "金钱：300 | 粮食：400 | 合法性：60",
                new Vector2(250, 0), new Vector2(0, 0.5f), new Vector2(600, 40));

            // 天气信息
            CreateText(bar.transform, "WeatherText", "天气：正常",
                new Vector2(900, 0), new Vector2(0, 0.5f), new Vector2(200, 40));

            // 下一回合按钮
            CreateButton(bar.transform, "NextTurnButton", "下一回合",
                new Vector2(-100, 0), new Vector2(1, 0.5f), new Vector2(120, 36));

            // 朝廷按钮
            CreateButton(bar.transform, "CourtButton", "朝廷",
                new Vector2(-240, 0), new Vector2(1, 0.5f), new Vector2(80, 36));

            // 帝皇按钮
            CreateButton(bar.transform, "EmperorButton", "帝皇",
                new Vector2(-340, 0), new Vector2(1, 0.5f), new Vector2(80, 36));

            return bar;
        }

        private GameObject BuildRegionPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "RegionPanel", new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-320, 0), new Vector2(300, 600));

            // 标题
            CreateText(panel.transform, "Title", "地区信息",
                new Vector2(0, -10), new Vector2(0.5f, 1), new Vector2(280, 36));

            // 内容区
            float y = -50;
            CreateText(panel.transform, "RegionNameText", "名称：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(280, 24)); y -= 28;
            CreateText(panel.transform, "TerrainText", "地形：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(280, 24)); y -= 28;
            CreateText(panel.transform, "PopulationText", "人口：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(280, 24)); y -= 28;
            CreateText(panel.transform, "FoodText", "粮食：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(280, 24)); y -= 28;
            CreateText(panel.transform, "TaxText", "税收：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(280, 24)); y -= 28;
            CreateText(panel.transform, "ManpowerText", "兵源：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(280, 24)); y -= 28;
            CreateText(panel.transform, "OwnerText", "归属：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(280, 24)); y -= 28;
            CreateText(panel.transform, "IntegrationText", "整合度：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(280, 24)); y -= 28;
            CreateText(panel.transform, "RebellionText", "民变风险：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(280, 24)); y -= 28;
            CreateText(panel.transform, "AnnexationText", "土地兼并：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(280, 24)); y -= 28;
            CreateText(panel.transform, "LocalPowerText", "地方势力：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(280, 24)); y -= 28;
            CreateText(panel.transform, "NeighborsText", "相邻：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(280, 40)); y -= 44;
            CreateText(panel.transform, "LandStructureText", "土地结构：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(280, 24)); y -= 28;
            CreateText(panel.transform, "CustomsText", "风俗：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(280, 24)); y -= 28;

            // 关闭按钮
            CreateButton(panel.transform, "CloseButton", "关闭",
                new Vector2(0, 10), new Vector2(0.5f, 0), new Vector2(80, 28));

            return panel;
        }

        private GameObject BuildEmperorPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "EmperorPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(500, 600));

            CreateText(panel.transform, "Title", "帝皇信息",
                new Vector2(0, -10), new Vector2(0.5f, 1), new Vector2(480, 36));

            float y = -50;
            CreateText(panel.transform, "EmperorNameText", "帝皇：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(480, 28)); y -= 32;
            CreateText(panel.transform, "TitleText", "称号：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(480, 24)); y -= 28;
            CreateText(panel.transform, "MechanicNameText", "机制：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(480, 24)); y -= 28;
            CreateText(panel.transform, "MechanicDescText", "",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(480, 40)); y -= 44;
            CreateText(panel.transform, "StatsText", "属性：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(480, 24)); y -= 28;
            CreateText(panel.transform, "ScoreText", "评分：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(480, 24)); y -= 28;
            CreateText(panel.transform, "BurdensText", "负担：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(480, 24)); y -= 28;
            CreateText(panel.transform, "LegitimacyText", "合法性：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(240, 24));
            CreateText(panel.transform, "SuccessionText", "继承风险：",
                new Vector2(250, y), new Vector2(0, 1), new Vector2(240, 24)); y -= 28;
            CreateText(panel.transform, "HeirText", "继承人：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(480, 24)); y -= 28;
            CreateText(panel.transform, "TechProgressText", "科技：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(480, 24)); y -= 28;

            CreateButton(panel.transform, "CloseButton", "关闭",
                new Vector2(0, 10), new Vector2(0.5f, 0), new Vector2(80, 28));

            panel.SetActive(false);
            return panel;
        }

        private GameObject BuildCourtPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "CourtPanel", new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(10, 0), new Vector2(350, 600));

            CreateText(panel.transform, "Title", "朝廷总览",
                new Vector2(0, -10), new Vector2(0.5f, 1), new Vector2(330, 36));

            float y = -50;
            CreateText(panel.transform, "FactionNameText", "势力：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(330, 24)); y -= 28;
            CreateText(panel.transform, "EmperorNameText", "帝皇：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(330, 24)); y -= 28;
            CreateText(panel.transform, "MoneyText", "金钱：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(160, 24));
            CreateText(panel.transform, "FoodText", "粮食：",
                new Vector2(170, y), new Vector2(0, 1), new Vector2(160, 24)); y -= 28;
            CreateText(panel.transform, "LegitimacyText", "合法性：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(160, 24));
            CreateText(panel.transform, "SuccessionText", "继承风险：",
                new Vector2(170, y), new Vector2(0, 1), new Vector2(160, 24)); y -= 28;
            CreateText(panel.transform, "CourtPressureText", "朝局压力：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(160, 24));
            CreateText(panel.transform, "RegionCountText", "领地：",
                new Vector2(170, y), new Vector2(0, 1), new Vector2(160, 24)); y -= 28;
            CreateText(panel.transform, "WeatherText", "天气：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(330, 24)); y -= 28;
            CreateText(panel.transform, "CelestialText", "天象：",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(330, 24)); y -= 36;

            // 回合日志区域
            CreateText(panel.transform, "LogTitle", "回合日志",
                new Vector2(10, y), new Vector2(0, 1), new Vector2(330, 24)); y -= 28;

            GameObject scrollObj = new GameObject("LogScroll");
            scrollObj.transform.SetParent(panel.transform, false);
            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 0);
            scrollRect.offsetMin = new Vector2(10, 10);
            scrollRect.offsetMax = new Vector2(-10, -Mathf.Abs(y));
            scrollRect.sizeDelta = new Vector2(-20, 0);

            ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
            Image scrollBg = scrollObj.AddComponent<Image>();
            scrollBg.color = new Color(0, 0, 0, 0.3f);

            CreateText(scrollObj.transform, "TurnLogText", "",
                new Vector2(0, 0), new Vector2(0, 1), new Vector2(310, 2000));

            CreateButton(panel.transform, "CloseButton", "关闭",
                new Vector2(0, 10), new Vector2(0.5f, 0), new Vector2(80, 28));

            panel.SetActive(false);
            return panel;
        }

        private GameObject BuildEventPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "EventPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 50), new Vector2(450, 350));

            CreateText(panel.transform, "Title", "事件",
                new Vector2(0, -10), new Vector2(0.5f, 1), new Vector2(430, 36));

            CreateText(panel.transform, "EventNameText", "",
                new Vector2(10, -50), new Vector2(0, 1), new Vector2(430, 28));
            CreateText(panel.transform, "CategoryText", "",
                new Vector2(10, -80), new Vector2(0, 1), new Vector2(430, 24));
            CreateText(panel.transform, "ChoiceText", "",
                new Vector2(10, -110), new Vector2(0, 1), new Vector2(430, 200));

            panel.SetActive(false);
            return panel;
        }

        private GameObject BuildBattleReportPanel(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "BattleReportPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 50), new Vector2(400, 300));

            CreateText(panel.transform, "Title", "战报",
                new Vector2(0, -10), new Vector2(0.5f, 1), new Vector2(380, 36));
            CreateText(panel.transform, "AttackerText", "",
                new Vector2(10, -50), new Vector2(0, 1), new Vector2(380, 24));
            CreateText(panel.transform, "DefenderText", "",
                new Vector2(10, -78), new Vector2(0, 1), new Vector2(380, 24));
            CreateText(panel.transform, "ResultText", "",
                new Vector2(10, -110), new Vector2(0, 1), new Vector2(380, 28));
            CreateText(panel.transform, "DetailsText", "",
                new Vector2(10, -142), new Vector2(0, 1), new Vector2(380, 100));

            CreateButton(panel.transform, "CloseButton", "关闭",
                new Vector2(0, 10), new Vector2(0.5f, 0), new Vector2(80, 28));

            panel.SetActive(false);
            return panel;
        }

        // ===== 工具方法 =====

        private GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 anchoredPos, Vector2 size)
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

            return obj;
        }

        private Text CreateText(Transform parent, string name, string content,
            Vector2 anchoredPos, Vector2 anchor, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Text text = obj.AddComponent<Text>();
            text.text = content;
            text.fontSize = Mathf.RoundToInt(UITheme.TextSizeBody);
            text.color = UITheme.TextPrimary;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.font = Font.CreateDynamicFontFromOSFont(UITheme.FontName, 14);

            return text;
        }

        private Button CreateButton(Transform parent, string name, string label,
            Vector2 anchoredPos, Vector2 anchor, Vector2 size)
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

            Button btn = obj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = UITheme.ButtonNormal;
            colors.highlightedColor = UITheme.ButtonHover;
            colors.pressedColor = UITheme.ButtonPressed;
            btn.colors = colors;

            Text text = CreateText(obj.transform, "Label", label,
                Vector2.zero, new Vector2(0.5f, 0.5f), size);
            text.alignment = TextAnchor.MiddleCenter;
            text.color = UITheme.ButtonText;

            return btn;
        }
    }
}
