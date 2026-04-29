using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    /// <summary>
    /// 战中调整系统：处理玩家的部队移动、战术技能、阵型调整、撤退/追击
    /// </summary>
    public sealed class BattleExecutionSystem : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private BattleSetupSystem setupSystem;

        private BattleSetup currentBattle;
        private int currentTurn;
        private List<BattleLog> battleLog;
        private Dictionary<string, int> tacticCooldowns;
        private Dictionary<string, BattleTactic> tacticDefinitions;

        public BattleSetup CurrentBattle { get { return currentBattle; } }
        public int CurrentTurn { get { return currentTurn; } }
        public List<BattleLog> BattleLog { get { return battleLog; } }

        public void StartBattle(BattleSetup setup)
        {
            currentBattle = setup;
            currentTurn = 0;
            battleLog = new List<BattleLog>();
            tacticCooldowns = new Dictionary<string, int>();
            InitializeTacticDefinitions();
        }

        private void InitializeTacticDefinitions()
        {
            tacticDefinitions = new Dictionary<string, BattleTactic>
            {
                { "fire_attack", new BattleTactic
                    {
                        id = "fire_attack",
                        name = "火攻",
                        description = "使用火攻造成大量伤害",
                        cooldownTurns = 3,
                        moraleCost = 15,
                        effects = new EffectSet { battlePower = 30 },
                        requiredFormation = new[] { "crane_wing" }
                    }
                },
                { "ambush", new BattleTactic
                    {
                        id = "ambush",
                        name = "伏击",
                        description = "伏击敌军，造成混乱",
                        cooldownTurns = 4,
                        moraleCost = 20,
                        effects = new EffectSet { battlePower = 25 },
                        requiredFormation = new[] { "long_snake" }
                    }
                },
                { "charge", new BattleTactic
                    {
                        id = "charge",
                        name = "冲锋",
                        description = "集中兵力冲锋突破",
                        cooldownTurns = 2,
                        moraleCost = 10,
                        effects = new EffectSet { battlePower = 20 },
                        requiredFormation = new[] { "sharp_point" }
                    }
                },
                { "defend", new BattleTactic
                    {
                        id = "defend",
                        name = "坚守",
                        description = "坚守阵地，消耗敌军",
                        cooldownTurns = 2,
                        moraleCost = 5,
                        effects = new EffectSet { battlePower = 10 },
                        requiredFormation = new[] { "square" }
                    }
                }
            };
        }

        public bool ExecuteAction(BattleAction action)
        {
            if (currentBattle == null) return false;

            switch (action.actionType)
            {
                case "move":
                    return ExecuteMove(action);
                case "tactic":
                    return ExecuteTactic(action);
                case "formation_change":
                    return ExecuteFormationChange(action);
                case "retreat":
                    return ExecuteRetreat(action);
                case "pursue":
                    return ExecutePursue(action);
                default:
                    return false;
            }
        }

        private bool ExecuteMove(BattleAction action)
        {
            BattleLog log = new BattleLog
            {
                turn = currentTurn,
                actionType = "move",
                actorId = action.unitId,
                targetId = action.targetId,
                description = "部队移动到有利位置"
            };
            battleLog.Add(log);
            return true;
        }

        private bool ExecuteTactic(BattleAction action)
        {
            if (string.IsNullOrEmpty(action.tacticId)) return false;

            int cooldown;
            if (tacticCooldowns.TryGetValue(action.tacticId, out cooldown) && cooldown > 0)
            {
                return false;
            }

            BattleTactic tactic = GetTacticDefinition(action.tacticId);
            if (tactic == null) return false;

            if (tactic.requiredFormation != null && tactic.requiredFormation.Length > 0)
            {
                string currentFormation = GetCurrentFormation(action.unitId);
                bool hasRequiredFormation = false;
                for (int i = 0; i < tactic.requiredFormation.Length; i++)
                {
                    if (tactic.requiredFormation[i] == currentFormation)
                    {
                        hasRequiredFormation = true;
                        break;
                    }
                }
                if (!hasRequiredFormation) return false;
            }

            BattleLog log = new BattleLog
            {
                turn = currentTurn,
                actionType = "tactic",
                actorId = action.unitId,
                targetId = action.targetId,
                description = "使用战术：" + tactic.name,
                damageDealt = tactic.effects.battlePower,
                moraleChange = -tactic.moraleCost
            };
            battleLog.Add(log);

            tacticCooldowns[action.tacticId] = tactic.cooldownTurns;

            return true;
        }

        private bool ExecuteFormationChange(BattleAction action)
        {
            if (string.IsNullOrEmpty(action.newFormationId)) return false;

            FormationDefinition formation = setupSystem.GetFormation(action.newFormationId);
            if (formation == null) return false;

            BattleLog log = new BattleLog
            {
                turn = currentTurn,
                actionType = "formation_change",
                actorId = action.unitId,
                description = "变阵为：" + formation.name
            };
            battleLog.Add(log);

            return true;
        }

        private bool ExecuteRetreat(BattleAction action)
        {
            BattleLog log = new BattleLog
            {
                turn = currentTurn,
                actionType = "retreat",
                actorId = action.unitId,
                description = "部队撤退",
                moraleChange = -20
            };
            battleLog.Add(log);

            return true;
        }

        private bool ExecutePursue(BattleAction action)
        {
            BattleLog log = new BattleLog
            {
                turn = currentTurn,
                actionType = "pursue",
                actorId = action.unitId,
                targetId = action.targetId,
                description = "追击敌军",
                damageDealt = 15,
                moraleChange = -10
            };
            battleLog.Add(log);

            return true;
        }

        public void AdvanceTurn()
        {
            currentTurn++;

            List<string> keys = new List<string>(tacticCooldowns.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                tacticCooldowns[keys[i]]--;
                if (tacticCooldowns[keys[i]] <= 0)
                {
                    tacticCooldowns.Remove(keys[i]);
                }
            }
        }

        private BattleTactic GetTacticDefinition(string tacticId)
        {
            if (tacticDefinitions == null) return null;

            BattleTactic tactic;
            if (tacticDefinitions.TryGetValue(tacticId, out tactic))
            {
                return tactic;
            }
            return null;
        }

        private string GetCurrentFormation(string unitId)
        {
            if (currentBattle == null) return null;

            for (int i = 0; i < currentBattle.attackerDeployments.Length; i++)
            {
                if (currentBattle.attackerDeployments[i].armyId == unitId)
                {
                    return currentBattle.attackerFormationId;
                }
            }

            for (int i = 0; i < currentBattle.defenderDeployments.Length; i++)
            {
                if (currentBattle.defenderDeployments[i].armyId == unitId)
                {
                    return currentBattle.defenderFormationId;
                }
            }

            return null;
        }

        public void EndBattle()
        {
            currentBattle = null;
            currentTurn = 0;
            battleLog = null;
            tacticCooldowns = null;
        }
    }
}
