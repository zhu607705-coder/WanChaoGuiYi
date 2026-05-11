using UnityEngine;

namespace WanChaoGuiYi
{
    public static class UITheme
    {
        // 主色调：深墨底、青玉面、鎏金重点。
        public static readonly Color PanelBackground = HexColor("#101514");
        public static readonly Color PanelBorder = HexColor("#C69A4A");
        public static readonly Color PanelHeader = HexColor("#1B2925");
        public static readonly Color PanelHeaderAccent = HexColor("#E1BE6A");
        public static readonly Color SectionBackground = new Color(0.12f, 0.17f, 0.15f, 0.46f);
        public static readonly Color DecisionBandBackground = new Color(0.20f, 0.16f, 0.08f, 0.52f);
        public static readonly Color SourceBandBackground = new Color(0.08f, 0.12f, 0.13f, 0.42f);

        // 文字色
        public static readonly Color TextPrimary = HexColor("#F5E9D0");
        public static readonly Color TextSecondary = HexColor("#BDB197");
        public static readonly Color TextAccent = HexColor("#E1BE6A");
        public static readonly Color TextDanger = HexColor("#D86A50");
        public static readonly Color TextSuccess = HexColor("#8DBE83");

        // 资源色
        public static readonly Color MoneyColor = HexColor("#D7B45A");
        public static readonly Color FoodColor = HexColor("#8EB86F");
        public static readonly Color LegitimacyColor = HexColor("#C95D50");
        public static readonly Color ResearchColor = HexColor("#6FA4C8");
        public static readonly Color DangerColor = HexColor("#C95543");
        public static readonly Color MeterTrack = new Color(0.035f, 0.05f, 0.048f, 0.86f);
        public static readonly Color MeterGood = HexColor("#8DBE83");
        public static readonly Color MeterWarning = HexColor("#D7B45A");
        public static readonly Color MeterDanger = HexColor("#C95543");
        public static readonly Color MeterTax = HexColor("#D7B45A");
        public static readonly Color MeterFood = HexColor("#8EB86F");
        public static readonly Color BadgeNormal = new Color(0.12f, 0.20f, 0.17f, 0.88f);
        public static readonly Color BadgeWarning = new Color(0.25f, 0.19f, 0.08f, 0.92f);
        public static readonly Color BadgeDanger = new Color(0.27f, 0.10f, 0.08f, 0.94f);
        public static readonly Color BattleWin = new Color(0.45f, 0.30f, 0.10f, 0.94f);
        public static readonly Color BattleLoss = new Color(0.24f, 0.10f, 0.08f, 0.94f);

        // 按钮
        public static readonly Color ButtonNormal = new Color(0.08f, 0.12f, 0.105f, 0.78f);
        public static readonly Color ButtonHover = new Color(0.18f, 0.25f, 0.21f, 0.92f);
        public static readonly Color ButtonPressed = new Color(0.06f, 0.08f, 0.07f, 0.98f);
        public static readonly Color ButtonText = HexColor("#F1E7D2");

        // 面板透明度
        public static readonly Color PanelBackgroundAlpha = new Color(0.045f, 0.055f, 0.052f, 0.955f);
        public static readonly Color HudBackgroundAlpha = new Color(0.035f, 0.045f, 0.047f, 0.84f);
        public static readonly Color ButtonBorder = new Color(0.77f, 0.60f, 0.30f, 0.42f);

        // 尺寸
        public const float PanelPadding = 16f;
        public const float PanelMargin = 8f;
        public const float HeaderHeight = 36f;
        public const float ButtonHeight = 32f;
        public const float TextSizeTitle = 20f;
        public const float TextSizeHeader = 16f;
        public const float TextSizeBody = 12f;
        public const float TextSizeSmall = 10f;
        public const float BorderWidth = 2f;

        // 字体名称，后续可在 Unity 中替换为项目字体。
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
            UnityEngine.UI.Image image = button.GetComponent<UnityEngine.UI.Image>();
            if (image != null) image.color = ButtonNormal;

            UnityEngine.UI.Text text = button.GetComponentInChildren<UnityEngine.UI.Text>();
            if (text != null) ApplyTextStyle(text, ButtonText, TextSizeBody);
        }
    }
}
