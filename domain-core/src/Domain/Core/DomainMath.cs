using System;

namespace WanChaoGuiYi
{
    public static class DomainMath
    {
        public static int Max(int a, int b)
        {
            return Math.Max(a, b);
        }

        public static float Max(float a, float b)
        {
            return Math.Max(a, b);
        }

        public static int Min(int a, int b)
        {
            return Math.Min(a, b);
        }

        public static float Min(float a, float b)
        {
            return Math.Min(a, b);
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static float Clamp01(float value)
        {
            return Clamp(value, 0f, 1f);
        }

        public static int RoundToInt(float value)
        {
            return (int)Math.Round(value, MidpointRounding.AwayFromZero);
        }

        public static int CeilToInt(float value)
        {
            return (int)Math.Ceiling(value);
        }

        public static float Log10(float value)
        {
            return (float)Math.Log10(value);
        }
    }
}
