using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class WarPressurePulse : MonoBehaviour
    {
        private LineRenderer line;
        private Vector3 baseScale = Vector3.one;
        private Color baseStartColor = Color.white;
        private Color baseEndColor = Color.white;
        private float baseWidth = 0.05f;
        private float scaleAmount = 0.12f;
        private float alphaAmount = 0.24f;
        private float speed = 4.4f;
        private float phase;
        private float elapsedTime;
        private bool pulseActive = true;

        public bool IsPulseActive { get { return pulseActive; } }

        public void Configure(LineRenderer renderer, float width, Vector3 scale, float scalePulse, float alphaPulse, float pulseSpeed, float pulsePhase)
        {
            line = renderer;
            baseWidth = width;
            baseScale = scale;
            scaleAmount = Mathf.Max(0f, scalePulse);
            alphaAmount = Mathf.Clamp01(alphaPulse);
            speed = Mathf.Max(0.1f, pulseSpeed);
            phase = pulsePhase;
            elapsedTime = 0f;
            if (line != null)
            {
                baseStartColor = line.startColor;
                baseEndColor = line.endColor;
            }
        }

        public void SetPulseActive(bool active)
        {
            if (pulseActive == active) return;

            pulseActive = active;
            if (!pulseActive)
            {
                ResetVisual();
            }
        }

        private void Update()
        {
            if (line == null || !pulseActive) return;

            elapsedTime += Time.unscaledDeltaTime;
            float wave = (Mathf.Sin(elapsedTime * speed + phase) + 1f) * 0.5f;
            float width = baseWidth * Mathf.Lerp(1f, 1.28f, wave);
            line.startWidth = width;
            line.endWidth = width;
            transform.localScale = baseScale * (1f + scaleAmount * wave);

            Color start = baseStartColor;
            Color end = baseEndColor;
            start.a = Mathf.Clamp01(baseStartColor.a * Mathf.Lerp(1f - alphaAmount, 1f, wave));
            end.a = Mathf.Clamp01(baseEndColor.a * Mathf.Lerp(1f - alphaAmount, 1f, wave));
            line.startColor = start;
            line.endColor = end;
        }

        private void ResetVisual()
        {
            transform.localScale = baseScale;
            if (line == null) return;

            line.startWidth = baseWidth;
            line.endWidth = baseWidth;
            line.startColor = baseStartColor;
            line.endColor = baseEndColor;
        }
    }
}
