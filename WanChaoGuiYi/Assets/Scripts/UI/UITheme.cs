using UnityEngine;

namespace WanChaoGuiYi
{
    public static class UITheme
    {
        // 主色调：古代中国宫廷风格
        public static readonly Color PanelBackground = HexColor("#1A1410");     // 深檀木色
        public static readonly Color PanelBorder = HexColor("#8B7355");         // 青铜色
        public static readonly Color PanelHeader = HexColor("#2D2218");         // 深褐色
        public static readonly Color PanelHeaderAccent = HexColor("#C4A265");   // 金色

        // 文字色
        public static readonly Color TextPrimary = HexColor("#E8DCC8");        // 宣纸色
        public static readonly Color TextSecondary = HexColor("#A89880");      // 旧纸色
        public static readonly Color TextAccent = HexColor("#D4A847");         // 金色
        public static readonly Color TextDanger = HexColor("#C44040");         // 朱砂红
        public static readonly Color TextSuccess = HexColor("#5A8A5A");        // 青铜绿

        // 资源色
        public static readonly Color MoneyColor = HexColor("#D4A847");         // 金钱金
        public static readonly Color FoodColor = HexColor("#7AAA5E");          // 粮食绿
        public static readonly Color LegitimacyColor = HexColor("#C44040");    // 合法性红
        public static readonly Color ResearchColor = HexColor("#6A8FCA");      // 科技蓝
        public static readonly Color DangerColor = HexColor("#CC3333");        // 危险红

        // 按钮
        public static readonly Color ButtonNormal = HexColor("#3D3225");       // 檀木色
        public static readonly Color ButtonHover = HexColor("#4D4235");        // 浅檀木
        public static readonly Color ButtonPressed = HexColor("#2D2218");      // 深檀木
        public static readonly Color ButtonText = HexColor("#E8DCC8");         // 宣纸色

        // 面板透明度
        public static readonly Color PanelBackgroundAlpha = new Color(0.10f, 0.08f, 0.06f, 0.92f);

        // 尺寸
        public const float PanelPadding = 16f;
        public const float PanelMargin = 8f;
        public const float HeaderHeight = 36f;
        public const float ButtonHeight = 32f;
        public const float TextSizeTitle = 20f;
        public const float TextSizeHeader = 16f;
        public const float TextSizeBody = 13f;
        public const float TextSizeSmall = 11f;
        public const float BorderWidth = 2f;

        // 字体名称（需在 Unity 中配置）
        public const string FontName = "Noto Serif SC";

        private static Color HexColor(string hex)
        {
            Color color;
            ColorUtility.TryParseHtmlString(hex, out color);
            return color;
        }

        public static void ApplyPanelStyle(UnityEngine.UI.Image image)
        {
            image.color = PanelBackgroundAlpha;
        }

        public static void ApplyTextStyle(UnityEngine.UI.Text text, Color color, float size)
        {
            text.color = color;
            text.fontSize = Mathf.RoundToInt(size);
            text.fontStyle = FontStyle.Normal;
        }

        public static void ApplyButtonStyle(UnityEngine.UI.Button button)
        {
            Image image = button.GetComponent<Image>();
            if (image != null) image.color = ButtonNormal;

            Text text = button.GetComponentInChildren<Text>();
            if (text != null) ApplyTextStyle(text, ButtonText, TextSizeBody);
        }
    }
}
