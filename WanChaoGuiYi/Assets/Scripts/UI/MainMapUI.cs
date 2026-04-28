using UnityEngine;
using UnityEngine.UI;

namespace WanChaoGuiYi
{
    public sealed class MainMapUI : MonoBehaviour
    {
        [Header("Core References")]
        [SerializeField] private GameManager gameManager;

        [Header("Panels")]
        [SerializeField] private RegionPanel regionPanel;
        [SerializeField] private EmperorPanel emperorPanel;
        [SerializeField] private CourtPanel courtPanel;
        [SerializeField] private EventPanel eventPanel;
        [SerializeField] private BattleReportPanel battleReportPanel;

        [Header("HUD")]
        [SerializeField] private Text turnText;
        [SerializeField] private Text resourceText;
        [SerializeField] private Button nextTurnButton;
        [SerializeField] private Button courtButton;
        [SerializeField] private Button emperorButton;

        private void OnEnable()
        {
            if (gameManager == null) return;

            gameManager.Events.Subscribe(GameEventType.RegionSelected, OnRegionSelected);
            gameManager.Events.Subscribe(GameEventType.TurnEnded, OnTurnEnded);
            gameManager.Events.Subscribe(GameEventType.BattleResolved, OnBattleResolved);
            gameManager.Events.Subscribe(GameEventType.EventTriggered, OnEventTriggered);
            gameManager.Events.Subscribe(GameEventType.SuccessionResolved, OnSuccessionResolved);

            if (nextTurnButton != null) nextTurnButton.onClick.AddListener(AdvanceTurn);
            if (courtButton != null) courtButton.onClick.AddListener(OpenCourtPanel);
            if (emperorButton != null) emperorButton.onClick.AddListener(OpenEmperorPanel);
        }

        private void OnDisable()
        {
            if (gameManager == null) return;

            gameManager.Events.Unsubscribe(GameEventType.RegionSelected, OnRegionSelected);
            gameManager.Events.Unsubscribe(GameEventType.TurnEnded, OnTurnEnded);
            gameManager.Events.Unsubscribe(GameEventType.BattleResolved, OnBattleResolved);
            gameManager.Events.Unsubscribe(GameEventType.EventTriggered, OnEventTriggered);
            gameManager.Events.Unsubscribe(GameEventType.SuccessionResolved, OnSuccessionResolved);

            if (nextTurnButton != null) nextTurnButton.onClick.RemoveListener(AdvanceTurn);
            if (courtButton != null) courtButton.onClick.RemoveListener(OpenCourtPanel);
            if (emperorButton != null) emperorButton.onClick.RemoveListener(OpenEmperorPanel);
        }

        private void Start()
        {
            RefreshHUD();
        }

        public void AdvanceTurn()
        {
            if (gameManager == null) return;

            // 先关闭可能打开的面板
            if (regionPanel != null) regionPanel.Hide();

            gameManager.NextTurn();
            RefreshHUD();
        }

        private void OnRegionSelected(GameEvent gameEvent)
        {
            if (regionPanel == null || gameManager == null || gameManager.State == null) return;

            string regionId = gameEvent.EntityId;
            RegionDefinition definition = gameManager.Data.GetRegion(regionId);
            RegionState state = gameManager.State.FindRegion(regionId);
            regionPanel.Show(definition, state);

            // 设置归属名称
            if (state != null)
            {
                FactionState owner = gameManager.State.FindFaction(state.ownerFactionId);
                regionPanel.SetOwner(owner != null ? owner.name : "未知");
            }
        }

        private void OnTurnEnded(GameEvent gameEvent)
        {
            RefreshHUD();
        }

        private void OnBattleResolved(GameEvent gameEvent)
        {
            if (battleReportPanel == null || gameManager == null) return;

            BattleResult result = gameEvent.Payload as BattleResult;
            if (result == null) return;

            // 查找军队所属势力名称
            ArmyState attacker = FindArmy(result.attackerArmyId);
            ArmyState defender = FindArmy(result.defenderArmyId);
            string attackerName = GetFactionName(attacker != null ? attacker.ownerFactionId : null);
            string defenderName = GetFactionName(defender != null ? defender.ownerFactionId : null);
            string regionName = defender != null ? GetRegionName(defender.regionId) : "";

            battleReportPanel.Show(result, attackerName, defenderName, regionName);
        }

        private void OnEventTriggered(GameEvent gameEvent)
        {
            // EventPanel 已经在 EventSystem 内部处理
        }

        private void OnSuccessionResolved(GameEvent gameEvent)
        {
            RefreshHUD();
            if (gameManager != null && gameManager.State != null)
            {
                FactionState playerFaction = gameManager.State.FindFaction(gameManager.State.playerFactionId);
                if (playerFaction != null)
                {
                    gameManager.State.AddLog("succession", playerFaction.name + "继承完成。");
                }
            }
        }

        private void OpenCourtPanel()
        {
            if (courtPanel == null || gameManager == null || gameManager.State == null) return;

            FactionState playerFaction = gameManager.State.FindFaction(gameManager.State.playerFactionId);
            if (playerFaction == null) return;

            EmperorDefinition emperor = gameManager.Data.GetEmperor(playerFaction.emperorId);
            string emperorName = emperor != null ? emperor.name : "未知";

            courtPanel.Show(playerFaction, emperorName, gameManager.State.turnLog);
        }

        private void OpenEmperorPanel()
        {
            if (emperorPanel == null || gameManager == null || gameManager.State == null) return;

            FactionState playerFaction = gameManager.State.FindFaction(gameManager.State.playerFactionId);
            if (playerFaction == null) return;

            EmperorDefinition emperor = gameManager.Data.GetEmperor(playerFaction.emperorId);
            emperorPanel.Show(emperor, playerFaction);
        }

        private void RefreshHUD()
        {
            if (gameManager == null || gameManager.State == null) return;

            GameState state = gameManager.State;
            SetText(turnText, "回合 " + state.turn + " | " + state.year + "年" + (state.season == Season.Spring ? "春" : "秋"));

            FactionState playerFaction = state.FindFaction(state.playerFactionId);
            if (playerFaction != null)
            {
                SetText(resourceText,
                    "金钱：" + playerFaction.money +
                    " | 粮食：" + playerFaction.food +
                    " | 合法性：" + playerFaction.legitimacy +
                    " | 领地：" + playerFaction.regionIds.Count
                );
            }
        }

        private ArmyState FindArmy(string armyId)
        {
            if (gameManager == null || gameManager.State == null || string.IsNullOrEmpty(armyId)) return null;

            for (int i = 0; i < gameManager.State.armies.Count; i++)
            {
                if (gameManager.State.armies[i].id == armyId) return gameManager.State.armies[i];
            }

            return null;
        }

        private string GetFactionName(string factionId)
        {
            if (gameManager == null || gameManager.State == null || string.IsNullOrEmpty(factionId)) return "未知";

            FactionState faction = gameManager.State.FindFaction(factionId);
            return faction != null ? faction.name : "未知";
        }

        private string GetRegionName(string regionId)
        {
            if (gameManager == null || gameManager.Data == null || string.IsNullOrEmpty(regionId)) return "";

            RegionDefinition definition = gameManager.Data.GetRegion(regionId);
            return definition != null ? definition.name : regionId;
        }

        private static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }
    }
}
