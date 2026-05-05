using UnityEngine;
using UnityEngine.UI;

namespace WanChaoGuiYi
{
    public sealed class BattleReportPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text attackerText;
        [SerializeField] private Text defenderText;
        [SerializeField] private Text resultText;
        [SerializeField] private Text detailsText;
        [SerializeField] private Button closeButton;

        private string baseDetailsText;
        private string occupationDetailsText;
        private string governanceDetailsText;

        public void Bind(GameObject root, Text attacker, Text defender, Text result, Text details, Button close)
        {
            panelRoot = root;
            attackerText = attacker;
            defenderText = defender;
            resultText = result;
            detailsText = details;
            closeButton = close;
            BindCloseButton();
            Hide();
        }

        private void Awake()
        {
            UIPanelVisibility.Hide(panelRoot);
            BindCloseButton();
        }

        private void BindCloseButton()
        {
            if (closeButton == null) return;

            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(Hide);
        }

        public void Show(BattleResult result, string attackerName, string defenderName, string regionName)
        {
            if (result == null) return;

            UIPanelVisibility.Show(panelRoot);

            SetText(attackerText, "进攻方：" + attackerName);
            SetText(defenderText, "防守方：" + defenderName);
            SetText(resultText, result.attackerWon ? "进攻方胜" : "防守方胜");

            baseDetailsText =
                "战场：" + regionName + "\n" +
                "进攻方战力：" + result.attackerPower + "\n" +
                "防守方战力：" + result.defenderPower + "\n" +
                "补给修正：攻 " + FormatSupplyPressure(result.attackerSupplyPowerPercent, result.attackerLowestSupply) +
                " | 守 " + FormatSupplyPressure(result.defenderSupplyPowerPercent, result.defenderLowestSupply) + "\n" +
                "兵力变化：攻 " + FormatSoldierChange(result.attackerSoldiersBefore, result.attackerSoldiersAfter) +
                " | 守 " + FormatSoldierChange(result.defenderSoldiersBefore, result.defenderSoldiersAfter);
            occupationDetailsText = "占领结果：本次战斗未改变地区归属";
            governanceDetailsText = "治理影响：无新增占领治理影响";
            RefreshDetailsText();
        }

        public void AppendOccupation(RegionOccupiedPayload payload, string previousOwnerName, string newOwnerName, string regionName)
        {
            if (payload == null) return;

            UIPanelVisibility.Show(panelRoot);
            occupationDetailsText = "占领结果：" + regionName + " " + previousOwnerName + " → " + newOwnerName;
            RefreshDetailsText();
        }

        public void AppendGovernanceImpact(GovernanceImpactPayload payload)
        {
            if (payload == null) return;

            UIPanelVisibility.Show(panelRoot);
            governanceDetailsText =
                "治理影响：整合" + payload.integration +
                " 税粮" + payload.taxContributionPercent + "/" + payload.foodContributionPercent +
                "% 民变" + payload.rebellionRisk +
                " 地方" + payload.localPower +
                " 合法性" + payload.legitimacyBefore + "→" + payload.legitimacyAfter + "\n" +
                "负反馈：新占领地区整合不足，税粮只按" + payload.taxContributionPercent + "/" + payload.foodContributionPercent +
                "%入账；民变与地方势力上升，合法性承压，需安抚或提升整合后才能稳定贡献。";
            governanceDetailsText += "\nNegative feedback: occupation reduces legitimacy and keeps tax-food contribution below normal until governance recovers.";
            RefreshDetailsText();
        }

        public void Hide()
        {
            UIPanelVisibility.Hide(panelRoot);
        }

        private void RefreshDetailsText()
        {
            SetText(detailsText,
                (baseDetailsText ?? string.Empty) + "\n" +
                (occupationDetailsText ?? string.Empty) + "\n" +
                (governanceDetailsText ?? string.Empty)
            );
        }

        private static string FormatSoldierChange(int before, int after)
        {
            if (before <= 0 && after <= 0) return "未记录";

            int delta = after - before;
            string sign = delta >= 0 ? "+" : string.Empty;
            return before + "→" + after + "(" + sign + delta + ")";
        }

        private static string FormatSupplyPressure(int percent, int lowestSupply)
        {
            string supplyText = lowestSupply >= 0 ? lowestSupply.ToString() : "未知";
            return percent + "% (最低补给 " + supplyText + ")";
        }

        private static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }
    }
}
