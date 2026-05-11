using UnityEngine;

namespace WanChaoGuiYi
{
    internal static class StrategyLayoutSpec
    {
        public static readonly Vector2 LensBarPosition = new Vector2(14, -96);
        public static readonly Vector2 LensBarSize = new Vector2(390, 32);
        public static readonly Vector2 OutlinerDockedPosition = new Vector2(-14, -92);
        public static readonly Vector2 OutlinerRegionPanelAvoidPosition = new Vector2(-636, -92);
        public static readonly Vector2 OutlinerSize = new Vector2(296, 310);
        public static readonly Vector2 OutlinerCollapsedSize = new Vector2(74, 38);
        public static readonly Vector2 LogisticsQueuePosition = new Vector2(-14, -416);
        public static readonly Vector2 LogisticsQueueSize = new Vector2(296, 118);

        public static readonly Vector2 TopLeftAnchor = new Vector2(0, 1);
        public static readonly Vector2 TopRightAnchor = new Vector2(1, 1);

        public const float LensButtonStartX = 92f;
        public const float LensButtonStepX = 50f;
        public const float OutlinerEntryStartY = -108f;
        public const float OutlinerEntryStepY = 28f;
    }
}
