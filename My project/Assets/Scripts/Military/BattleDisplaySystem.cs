using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    /// <summary>
    /// 电子斗蛐蛐显示系统：处理战局和数值的清晰显示
    /// </summary>
    public sealed class BattleDisplaySystem : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        private BattleSession currentSession;
        private Dictionary<string, PlayerBattleState> playerStates;

        public BattleSession CurrentSession { get { return currentSession; } }

        private void Awake()
        {
            playerStates = new Dictionary<string, PlayerBattleState>();
        }

        public void SetSession(BattleSession session)
        {
            currentSession = session;
            InitializePlayerStates();
        }

        private void InitializePlayerStates()
        {
            if (currentSession == null) return;

            playerStates.Clear();

            if (currentSession.player1Config != null)
            {
                PlayerBattleState state1 = new PlayerBattleState
                {
                    playerId = currentSession.player1Id,
                    unitSoldiers = new int[currentSession.player1Config.unitIds.Length],
                    unitMorale = new int[currentSession.player1Config.unitIds.Length],
                    currentFormation = currentSession.player1Config.formationId,
                    tacticCooldowns = new int[currentSession.player1Config.tacticIds.Length],
                    isReady = false
                };

                for (int i = 0; i < state1.unitSoldiers.Length; i++)
                {
                    state1.unitSoldiers[i] = 1000;
                    state1.unitMorale[i] = 100;
                }

                playerStates[currentSession.player1Id] = state1;
            }

            if (currentSession.player2Config != null)
            {
                PlayerBattleState state2 = new PlayerBattleState
                {
                    playerId = currentSession.player2Id,
                    unitSoldiers = new int[currentSession.player2Config.unitIds.Length],
                    unitMorale = new int[currentSession.player2Config.unitIds.Length],
                    currentFormation = currentSession.player2Config.formationId,
                    tacticCooldowns = new int[currentSession.player2Config.tacticIds.Length],
                    isReady = false
                };

                for (int i = 0; i < state2.unitSoldiers.Length; i++)
                {
                    state2.unitSoldiers[i] = 1000;
                    state2.unitMorale[i] = 100;
                }

                playerStates[currentSession.player2Id] = state2;
            }
        }

        public PlayerBattleState GetPlayerState(string playerId)
        {
            PlayerBattleState state;
            if (playerStates.TryGetValue(playerId, out state))
            {
                return state;
            }
            return null;
        }

        public void UpdatePlayerState(string playerId, PlayerBattleState newState)
        {
            if (playerStates.ContainsKey(playerId))
            {
                playerStates[playerId] = newState;
            }
        }

        public string GetBattleStatusText()
        {
            if (currentSession == null) return "无对战";

            switch (currentSession.state)
            {
                case BattleState.WaitingForPlayers:
                    return "等待玩家加入...";
                case BattleState.Configuration:
                    return "配置阶段";
                case BattleState.InProgress:
                    return "对战中 - 回合 " + currentSession.currentTurn;
                case BattleState.Finished:
                    return "对战结束";
                default:
                    return "未知状态";
            }
        }

        public string GetPlayerStatusText(string playerId)
        {
            PlayerBattleState state = GetPlayerState(playerId);
            if (state == null) return "未知玩家";

            string status = "玩家: " + playerId + "\n";
            status += "阵型: " + GetFormationName(state.currentFormation) + "\n";

            for (int i = 0; i < state.unitSoldiers.Length; i++)
            {
                status += "单位" + (i + 1) + ": 兵力" + state.unitSoldiers[i] + " 士气" + state.unitMorale[i] + "\n";
            }

            return status;
        }

        public string GetBattleResultText()
        {
            if (currentSession == null || currentSession.result == null) return "无结果";

            BattleResult result = currentSession.result;
            string resultText = "战斗结果:\n";
            resultText += "攻击方战力: " + result.attackerPower + "\n";
            resultText += "防御方战力: " + result.defenderPower + "\n";
            resultText += "阵型加成: " + result.formationBonus + "\n";
            resultText += "战术加成: " + result.tacticBonus + "\n";
            resultText += "地形加成: " + result.terrainBonus + "\n";
            resultText += "将领加成: " + result.generalBonus + "\n";
            resultText += "胜利方: " + (result.attackerWon ? "攻击方" : "防御方");

            return resultText;
        }

        private string GetFormationName(string formationId)
        {
            switch (formationId)
            {
                case "crane_wing": return "鹤翼阵";
                case "long_snake": return "长蛇阵";
                case "sharp_point": return "锥形阵";
                case "square": return "方阵";
                default: return formationId;
            }
        }

        public void ClearDisplay()
        {
            currentSession = null;
            playerStates.Clear();
        }
    }
}
