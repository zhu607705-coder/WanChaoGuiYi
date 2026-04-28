using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WanChaoGuiYi
{
    public sealed class CourtPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text factionNameText;
        [SerializeField] private Text emperorNameText;
        [SerializeField] private Text moneyText;
        [SerializeField] private Text foodText;
        [SerializeField] private Text legitimacyText;
        [SerializeField] private Text successionRiskText;
        [SerializeField] private Text courtPressureText;
        [SerializeField] private Text regionCountText;
        [SerializeField] private Text talentCountText;
        [SerializeField] private Text turnLogText;
        [SerializeField] private ScrollRect turnLogScrollRect;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
        }

        public void Show(FactionState faction, string emperorName, List<TurnLogEntry> recentLogs)
        {
            if (faction == null) return;

            if (panelRoot != null) panelRoot.SetActive(true);

            SetText(factionNameText, faction.name);
            SetText(emperorNameText, "帝皇：" + emperorName);
            SetText(moneyText, "金钱：" + faction.money);
            SetText(foodText, "粮食：" + faction.food);
            SetText(legitimacyText, "合法性：" + faction.legitimacy);
            SetText(successionRiskText, "继承风险：" + faction.successionRisk);
            SetText(courtPressureText, "朝局压力：" + faction.courtFactionPressure);
            SetText(regionCountText, "领地：" + faction.regionIds.Count);
            SetText(talentCountText, "人才：" + faction.talentIds.Count);
            SetText(turnLogText, FormatLogs(recentLogs));

            if (turnLogScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                turnLogScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }

        private static string FormatLogs(List<TurnLogEntry> logs)
        {
            if (logs == null || logs.Count == 0) return "暂无记录。";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int start = Mathf.Max(0, logs.Count - 30);
            for (int i = start; i < logs.Count; i++)
            {
                TurnLogEntry entry = logs[i];
                sb.AppendLine("[T" + entry.turn + "] " + entry.message);
            }

            return sb.ToString();
        }
    }
}
