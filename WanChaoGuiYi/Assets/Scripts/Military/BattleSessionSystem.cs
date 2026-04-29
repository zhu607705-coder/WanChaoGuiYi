using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    /// <summary>
    /// 电子斗蛐蛐对战系统：处理实时对战的会话管理、同步和结算
    /// </summary>
    public sealed class BattleSessionSystem : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private BattleResolver battleResolver;

        private Dictionary<string, BattleSession> activeSessions;
        private Dictionary<string, BattleSync> pendingSyncs;

        public Dictionary<string, BattleSession> ActiveSessions { get { return activeSessions; } }

        private void Awake()
        {
            activeSessions = new Dictionary<string, BattleSession>();
            pendingSyncs = new Dictionary<string, BattleSync>();
        }

        public BattleSession CreateSession(string player1Id, string player2Id)
        {
            BattleSession session = new BattleSession
            {
                sessionId = "session_" + System.Guid.NewGuid().ToString(),
                player1Id = player1Id,
                player2Id = player2Id,
                state = BattleState.WaitingForPlayers,
                currentTurn = 0,
                actions = new List<BattleAction>(),
                result = null
            };

            activeSessions[session.sessionId] = session;
            return session;
        }

        public bool JoinSession(string sessionId, string playerId)
        {
            BattleSession session;
            if (!activeSessions.TryGetValue(sessionId, out session)) return false;

            if (session.state != BattleState.WaitingForPlayers) return false;

            if (session.player1Id == playerId || session.player2Id == playerId)
            {
                session.state = BattleState.Configuration;
                return true;
            }

            return false;
        }

        public bool SubmitConfig(string sessionId, string playerId, BattleConfig config)
        {
            BattleSession session;
            if (!activeSessions.TryGetValue(sessionId, out session)) return false;

            if (session.state != BattleState.Configuration) return false;

            if (session.player1Id == playerId)
            {
                session.player1Config = config;
            }
            else if (session.player2Id == playerId)
            {
                session.player2Config = config;
            }
            else
            {
                return false;
            }

            if (session.player1Config != null && session.player2Config != null)
            {
                session.state = BattleState.InProgress;
                StartBattle(session);
            }

            return true;
        }

        private void StartBattle(BattleSession session)
        {
            session.currentTurn = 0;
            session.actions = new List<BattleAction>();

            BattleSetup setup = new BattleSetup
            {
                battleId = session.sessionId,
                attackerFactionId = session.player1Id,
                defenderFactionId = session.player2Id,
                terrainId = "plain",
                attackerFormationId = session.player1Config.formationId,
                defenderFormationId = session.player2Config.formationId,
                attackerDeployments = CreateDeploymentsFromConfig(session.player1Config),
                defenderDeployments = CreateDeploymentsFromConfig(session.player2Config)
            };

            BattleSync sync = new BattleSync
            {
                sessionId = session.sessionId,
                turn = 0,
                player1Ready = false,
                player2Ready = false
            };

            pendingSyncs[session.sessionId] = sync;
        }

        private BattleDeployment[] CreateDeploymentsFromConfig(BattleConfig config)
        {
            List<BattleDeployment> deployments = new List<BattleDeployment>();

            for (int i = 0; i < config.unitIds.Length; i++)
            {
                BattleDeployment deployment = new BattleDeployment
                {
                    armyId = config.configId + "_army_" + i,
                    generalId = i < config.generalIds.Length ? config.generalIds[i] : null,
                    formationPosition = i == 0 ? "前锋" : (i == 1 ? "中军" : "后卫"),
                    soldierCount = 1000
                };
                deployments.Add(deployment);
            }

            return deployments.ToArray();
        }

        public bool SubmitAction(string sessionId, string playerId, BattleAction action)
        {
            BattleSession session;
            if (!activeSessions.TryGetValue(sessionId, out session)) return false;

            if (session.state != BattleState.InProgress) return false;

            BattleSync sync;
            if (!pendingSyncs.TryGetValue(sessionId, out sync)) return false;

            if (session.player1Id == playerId)
            {
                sync.player1Action = action;
                sync.player1Ready = true;
            }
            else if (session.player2Id == playerId)
            {
                sync.player2Action = action;
                sync.player2Ready = true;
            }
            else
            {
                return false;
            }

            if (sync.player1Ready && sync.player2Ready)
            {
                ProcessTurn(session, sync);
            }

            return true;
        }

        private void ProcessTurn(BattleSession session, BattleSync sync)
        {
            if (sync.player1Action != null)
            {
                session.actions.Add(sync.player1Action);
            }

            if (sync.player2Action != null)
            {
                session.actions.Add(sync.player2Action);
            }

            session.currentTurn++;
            sync.turn = session.currentTurn;

            sync.player1Ready = false;
            sync.player2Ready = false;
            sync.player1Action = null;
            sync.player2Action = null;

            if (session.currentTurn >= 10)
            {
                EndBattle(session);
            }
        }

        private void EndBattle(BattleSession session)
        {
            session.state = BattleState.Finished;

            BattleSetup setup = new BattleSetup
            {
                battleId = session.sessionId,
                attackerFactionId = session.player1Id,
                defenderFactionId = session.player2Id,
                terrainId = "plain",
                attackerFormationId = session.player1Config.formationId,
                defenderFormationId = session.player2Config.formationId,
                attackerDeployments = CreateDeploymentsFromConfig(session.player1Config),
                defenderDeployments = CreateDeploymentsFromConfig(session.player2Config)
            };

            GameContext context = new GameContext
            {
                State = gameManager.State,
                Data = gameManager.Data,
                Events = gameManager.Events
            };

            List<BattleLog> battleLog = new List<BattleLog>();
            session.result = battleResolver.ResolveSemiAuto(context, setup, battleLog);

            BattleReplay replay = new BattleReplay
            {
                sessionId = session.sessionId,
                player1Config = session.player1Config,
                player2Config = session.player2Config,
                turns = new List<BattleSync>(),
                result = session.result,
                timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }

        public BattleSession GetSession(string sessionId)
        {
            BattleSession session;
            if (activeSessions.TryGetValue(sessionId, out session))
            {
                return session;
            }
            return null;
        }

        public void RemoveSession(string sessionId)
        {
            activeSessions.Remove(sessionId);
            pendingSyncs.Remove(sessionId);
        }
    }
}
