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
        [SerializeField] private Button focusButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text outcomeRibbonText;
        [SerializeField] private Image outcomeRibbonImage;
        [SerializeField] private Text attackerPowerLabel;
        [SerializeField] private Text defenderPowerLabel;
        [SerializeField] private Text attackerSupplyBadgeText;
        [SerializeField] private Text defenderSupplyBadgeText;
        [SerializeField] private Image attackerPowerBarFill;
        [SerializeField] private Image defenderPowerBarFill;

        private string baseDetailsText;
        private string occupationDetailsText;
        private string governanceDetailsText;
        private string currentFocusRegionId;
        private float resultPulseTimer;
        private System.Action<string> focusRegionRequested;

        public void Bind(GameObject root, Text attacker, Text defender, Text result, Text details, Button focus, Button close)
        {
            panelRoot = root;
            attackerText = attacker;
            defenderText = defender;
            resultText = result;
            detailsText = details;
            focusButton = focus;
            closeButton = close;
            BindBattleVisuals();
            BindButtons();
            Hide();
        }

        private void Awake()
        {
            UIPanelVisibility.Hide(panelRoot);
            BindButtons();
        }

        private void Update()
        {
            if (outcomeRibbonImage == null || resultPulseTimer <= 0f) return;

            resultPulseTimer = Mathf.Max(0f, resultPulseTimer - Time.unscaledDeltaTime);
            float pulse = 1f + Mathf.Sin(resultPulseTimer * 24f) * 0.025f * Mathf.Clamp01(resultPulseTimer / 0.42f);
            outcomeRibbonImage.rectTransform.localScale = new Vector3(pulse, pulse, 1f);
        }

        public void SetFocusHandler(System.Action<string> handler)
        {
            focusRegionRequested = handler;
            BindButtons();
        }

        private void BindButtons()
        {
            if (focusButton != null)
            {
                focusButton.onClick.RemoveListener(FocusBattleRegion);
                focusButton.onClick.AddListener(FocusBattleRegion);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
                closeButton.onClick.AddListener(Hide);
            }
        }

        public void Show(BattleResult result, string attackerName, string defenderName, string regionName)
        {
            Show(result, attackerName, defenderName, regionName, result != null ? result.battleRegionId : null);
        }

        public void Show(BattleResult result, string attackerName, string defenderName, string regionName, string focusRegionId)
        {
            if (result == null) return;

            UIPanelVisibility.Show(panelRoot);
            BringToFront();
            currentFocusRegionId = focusRegionId;
            if (focusButton != null) focusButton.interactable = !string.IsNullOrEmpty(currentFocusRegionId);

            SetText(attackerText, "进攻方：" + attackerName);
            SetText(defenderText, "防守方：" + defenderName);
            SetText(resultText, result.attackerWon ? "进攻方胜" : "防守方胜");
            UpdateBattleVisuals(result);

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
            BringToFront();
            occupationDetailsText = "占领结果：" + regionName + " " + previousOwnerName + " → " + newOwnerName;
            if (!string.IsNullOrEmpty(payload.regionId))
            {
                currentFocusRegionId = payload.regionId;
                if (focusButton != null) focusButton.interactable = true;
            }
            RefreshDetailsText();
        }

        public void AppendGovernanceImpact(GovernanceImpactPayload payload)
        {
            if (payload == null) return;

            UIPanelVisibility.Show(panelRoot);
            BringToFront();
            governanceDetailsText =
                "治理影响：整合" + payload.integration +
                " 税粮" + payload.taxContributionPercent + "/" + payload.foodContributionPercent +
                "% 民变" + payload.rebellionRisk +
                " 地方" + payload.localPower +
                " 预留粮" + payload.occupationReservedFoodAvailable +
                " 合法性" + payload.legitimacyBefore + "→" + payload.legitimacyAfter + "\n" +
                "负反馈：新占领地区整合不足，税粮只按" + payload.taxContributionPercent + "/" + payload.foodContributionPercent +
                "%入账；民变与地方势力上升，合法性承压，前线预留粮只抵扣占后治理粮耗，不会直接恢复税粮。";
            governanceDetailsText += "\n负反馈：占领会削弱合法性，并在治理恢复前压低税粮贡献。";
            RefreshDetailsText();
        }

        public void Hide()
        {
            UIPanelVisibility.Hide(panelRoot);
            currentFocusRegionId = null;
            resultPulseTimer = 0f;
            if (outcomeRibbonImage != null) outcomeRibbonImage.rectTransform.localScale = Vector3.one;
        }

        private void FocusBattleRegion()
        {
            if (string.IsNullOrEmpty(currentFocusRegionId) || focusRegionRequested == null) return;
            focusRegionRequested(currentFocusRegionId);
        }

        private void BringToFront()
        {
            if (panelRoot != null)
            {
                panelRoot.transform.SetAsLastSibling();
            }
        }

        private void RefreshDetailsText()
        {
            SetText(detailsText,
                (baseDetailsText ?? string.Empty) + "\n" +
                (occupationDetailsText ?? string.Empty) + "\n" +
                (governanceDetailsText ?? string.Empty)
            );
        }

        private void BindBattleVisuals()
        {
            if (panelRoot == null) return;

            outcomeRibbonText = FindPanelComponent<Text>("BattleOutcomeRibbonText");
            outcomeRibbonImage = FindPanelComponent<Image>("BattleOutcomeRibbon");
            attackerPowerLabel = FindPanelComponent<Text>("BattleAttackerPowerLabel");
            defenderPowerLabel = FindPanelComponent<Text>("BattleDefenderPowerLabel");
            attackerSupplyBadgeText = FindPanelComponent<Text>("BattleAttackerSupplyBadgeText");
            defenderSupplyBadgeText = FindPanelComponent<Text>("BattleDefenderSupplyBadgeText");
            attackerPowerBarFill = FindPanelComponent<Image>("BattleAttackerPowerUiBarFill");
            defenderPowerBarFill = FindPanelComponent<Image>("BattleDefenderPowerUiBarFill");
        }

        private void UpdateBattleVisuals(BattleResult result)
        {
            if (result == null) return;

            string outcome = result.attackerWon ? "攻方突破" : "守方稳住";
            SetText(outcomeRibbonText, outcome + " | 战报反馈");
            if (outcomeRibbonImage != null)
            {
                outcomeRibbonImage.color = result.attackerWon ? UITheme.BattleWin : UITheme.BattleLoss;
                outcomeRibbonImage.rectTransform.localScale = Vector3.one;
            }

            SetText(attackerPowerLabel, "攻方战力 " + result.attackerPower);
            SetText(defenderPowerLabel, "守方战力 " + result.defenderPower);
            SetText(attackerSupplyBadgeText, "攻方补给 " + FormatSupplyPressureShort(result.attackerSupplyPowerPercent, result.attackerLowestSupply));
            SetText(defenderSupplyBadgeText, "守方补给 " + FormatSupplyPressureShort(result.defenderSupplyPowerPercent, result.defenderLowestSupply));
            SetBadgeBackground(attackerSupplyBadgeText, ResolveSupplyBadgeColor(result.attackerSupplyPowerPercent));
            SetBadgeBackground(defenderSupplyBadgeText, ResolveSupplyBadgeColor(result.defenderSupplyPowerPercent));

            int maxPower = Mathf.Max(1, result.attackerPower, result.defenderPower);
            SetBar(attackerPowerBarFill, result.attackerPower, maxPower, result.attackerWon ? UITheme.MeterGood : UITheme.MeterWarning);
            SetBar(defenderPowerBarFill, result.defenderPower, maxPower, result.attackerWon ? UITheme.MeterWarning : UITheme.MeterGood);
            resultPulseTimer = 0.42f;
        }

        private static void SetBadgeBackground(Text text, Color color)
        {
            if (text == null || text.transform.parent == null) return;
            Image image = text.transform.parent.GetComponent<Image>();
            if (image != null) image.color = color;
        }

        private static void SetBar(Image fill, int value, int maxValue, Color color)
        {
            if (fill == null) return;

            RectTransform parent = fill.transform.parent != null ? fill.transform.parent.GetComponent<RectTransform>() : null;
            RectTransform fillRect = fill.rectTransform;
            float maxWidth = parent != null ? parent.sizeDelta.x : fillRect.sizeDelta.x;
            float width = Mathf.Max(1f, maxWidth * Mathf.Clamp01(value / (float)Mathf.Max(1, maxValue)));
            fillRect.sizeDelta = new Vector2(width, fillRect.sizeDelta.y);
            fill.color = color;
        }

        private static Color ResolveSupplyBadgeColor(int percent)
        {
            if (percent <= StrategyCausalRules.DepletedSupplyBattlePowerPercent) return UITheme.BadgeDanger;
            if (percent < 100) return UITheme.BadgeWarning;
            return UITheme.BadgeNormal;
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

        private static string FormatSupplyPressureShort(int percent, int lowestSupply)
        {
            string supplyText = lowestSupply >= 0 ? lowestSupply.ToString() : "未知";
            return percent + "% / 最低" + supplyText;
        }

        private static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }

        private T FindPanelComponent<T>(string objectName) where T : Component
        {
            if (panelRoot == null || string.IsNullOrEmpty(objectName)) return null;
            Transform child = FindChildRecursive(panelRoot.transform, objectName);
            return child != null ? child.GetComponent<T>() : null;
        }

        private static Transform FindChildRecursive(Transform root, string objectName)
        {
            if (root == null) return null;
            if (root.name == objectName) return root;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChildRecursive(root.GetChild(i), objectName);
                if (found != null) return found;
            }

            return null;
        }
    }
}
