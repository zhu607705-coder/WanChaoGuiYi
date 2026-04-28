using UnityEngine;
using UnityEngine.UI;

namespace WanChaoGuiYi
{
    public sealed class EmperorPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text emperorNameText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text mechanicNameText;
        [SerializeField] private Text mechanicDescText;
        [SerializeField] private Text statsText;
        [SerializeField] private Text burdensText;
        [SerializeField] private Text legitimacyText;
        [SerializeField] private Text successionRiskText;
        [SerializeField] private Text heirText;
        [SerializeField] private Text stableSuccessionsText;
        [SerializeField] private Button closeButton;

        public string CurrentEmperorId { get; private set; }

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
        }

        public void Show(EmperorDefinition emperor, FactionState faction)
        {
            if (emperor == null || faction == null) return;

            CurrentEmperorId = emperor.id;
            if (panelRoot != null) panelRoot.SetActive(true);

            SetText(emperorNameText, emperor.name);
            SetText(titleText, emperor.title);
            SetText(mechanicNameText, "机制：" + (emperor.uniqueMechanic != null ? emperor.uniqueMechanic.name : "无"));
            SetText(mechanicDescText, emperor.uniqueMechanic != null ? emperor.uniqueMechanic.description : "");
            SetText(statsText, FormatStats(emperor.stats));
            SetText(burdensText, "历史负担：" + FormatArray(emperor.historicalBurdens));
            SetText(legitimacyText, "合法性：" + faction.legitimacy);
            SetText(successionRiskText, "继承风险：" + faction.successionRisk);
            SetText(heirText, FormatHeir(faction.heir));
            SetText(stableSuccessionsText, "平稳继承次数：" + faction.stableSuccessions);
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            CurrentEmperorId = null;
        }

        private static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }

        private static string FormatStats(EmperorStats stats)
        {
            if (stats == null) return "";

            return "军事 " + stats.military +
                   " | 行政 " + stats.administration +
                   " | 改革 " + stats.reform +
                   " | 魅力 " + stats.charisma +
                   " | 外交 " + stats.diplomacy +
                   " | 继承掌控 " + stats.successionControl;
        }

        private static string FormatArray(string[] arr)
        {
            if (arr == null || arr.Length == 0) return "无";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < arr.Length; i++)
            {
                if (i > 0) sb.Append("、");
                sb.Append(arr[i]);
            }

            return sb.ToString();
        }

        private static string FormatHeir(HeirState heir)
        {
            if (heir == null) return "继承人：无";

            return "继承人：" + heir.name +
                   " | 年龄：" + heir.age +
                   " | 合法性：" + heir.legitimacy +
                   " | 能力：" + heir.ability;
        }
    }
}
