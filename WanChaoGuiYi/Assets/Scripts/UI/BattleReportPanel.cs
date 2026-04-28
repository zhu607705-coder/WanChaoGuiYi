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

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
        }

        public void Show(BattleResult result, string attackerName, string defenderName, string regionName)
        {
            if (result == null) return;

            if (panelRoot != null) panelRoot.SetActive(true);

            SetText(attackerText, "进攻方：" + attackerName);
            SetText(defenderText, "防守方：" + defenderName);
            SetText(resultText, result.attackerWon ? "进攻方胜" : "防守方胜");
            SetText(detailsText,
                "战场：" + regionName + "\n" +
                "进攻方战力：" + result.attackerPower + "\n" +
                "防守方战力：" + result.defenderPower
            );
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }
    }
}
