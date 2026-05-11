using UnityEngine;
using UnityEngine.UI;

namespace WanChaoGuiYi
{
    [DisallowMultipleComponent]
    public sealed class AudioDebugHUD : MonoBehaviour
    {
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private bool visible = true;

        private Text statusText;
        private float nextRefreshTime;

        private void Awake()
        {
            if (audioManager == null)
            {
                audioManager = GetComponent<AudioManager>();
            }
        }

        private void Start()
        {
            EnsureUi();
            RefreshNow();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefreshTime) return;
            RefreshNow();
            nextRefreshTime = Time.unscaledTime + 0.25f;
        }

        public void Bind(AudioManager manager)
        {
            audioManager = manager;
            RefreshNow();
        }

        public string GetCurrentText()
        {
            return statusText != null ? statusText.text : string.Empty;
        }

        private void EnsureUi()
        {
            if (!visible || statusText != null) return;

            GameObject existing = GameObject.Find("AudioDebugHUD");
            if (existing != null)
            {
                statusText = existing.GetComponentInChildren<Text>();
                return;
            }

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("AudioDebugCanvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            GameObject root = new GameObject("AudioDebugHUD");
            root.transform.SetParent(canvas.transform, false);
            RectTransform rect = root.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(12f, 12f);
            rect.sizeDelta = new Vector2(430f, 42f);

            Image background = root.AddComponent<Image>();
            background.color = new Color(0.035f, 0.045f, 0.047f, 0.72f);

            GameObject textObject = new GameObject("AudioDebugHUDText");
            textObject.transform.SetParent(root.transform, false);
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 4f);
            textRect.offsetMax = new Vector2(-10f, -4f);

            statusText = textObject.AddComponent<Text>();
            statusText.font = ResolveBuiltinFont();
            statusText.fontSize = 10;
            statusText.color = UITheme.TextSecondary;
            statusText.alignment = TextAnchor.MiddleLeft;
            statusText.horizontalOverflow = HorizontalWrapMode.Overflow;
            statusText.verticalOverflow = VerticalWrapMode.Truncate;
        }

        private static Font ResolveBuiltinFont()
        {
            Font font = TryLoadBuiltinFont("LegacyRuntime.ttf");
            return font != null ? font : TryLoadBuiltinFont("Arial.ttf");
        }

        private static Font TryLoadBuiltinFont(string path)
        {
            try
            {
                return Resources.GetBuiltinResource<Font>(path);
            }
            catch (System.ArgumentException)
            {
                return null;
            }
        }

        private void RefreshNow()
        {
            if (statusText == null || audioManager == null) return;
            statusText.text = audioManager.GetAudioDebugSummary();
        }
    }
}
