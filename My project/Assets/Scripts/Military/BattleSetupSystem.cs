using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    /// <summary>
    /// 战前部署系统：处理玩家的兵力部署、阵型选择、将领分配、地形选择
    /// </summary>
    public sealed class BattleSetupSystem : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        private BattleSetup currentSetup;
        private List<BattleDeployment> attackerDeploymentList;
        private List<BattleDeployment> defenderDeploymentList;
        private Dictionary<string, FormationDefinition> formations;

        public BattleSetup CurrentSetup { get { return currentSetup; } }

        private void Awake()
        {
            InitializeFormations();
        }

        private void InitializeFormations()
        {
            formations = new Dictionary<string, FormationDefinition>
            {
                { "crane_wing", new FormationDefinition
                    {
                        id = "crane_wing",
                        name = "鹤翼阵",
                        description = "两翼展开，包抄敌军",
                        attackBonus = 15,
                        defenseBonus = 5,
                        mobilityBonus = 10,
                        effectiveAgainst = new[] { "long_snake" },
                        weakAgainst = new[] { "sharp_point" }
                    }
                },
                { "long_snake", new FormationDefinition
                    {
                        id = "long_snake",
                        name = "长蛇阵",
                        description = "纵深排列，突破敌阵",
                        attackBonus = 20,
                        defenseBonus = 0,
                        mobilityBonus = 15,
                        effectiveAgainst = new[] { "sharp_point" },
                        weakAgainst = new[] { "crane_wing" }
                    }
                },
                { "sharp_point", new FormationDefinition
                    {
                        id = "sharp_point",
                        name = "锥形阵",
                        description = "集中兵力，中央突破",
                        attackBonus = 25,
                        defenseBonus = -5,
                        mobilityBonus = 5,
                        effectiveAgainst = new[] { "crane_wing" },
                        weakAgainst = new[] { "long_snake" }
                    }
                },
                { "square", new FormationDefinition
                    {
                        id = "square",
                        name = "方阵",
                        description = "稳固防守，消耗敌军",
                        attackBonus = 0,
                        defenseBonus = 20,
                        mobilityBonus = -10,
                        effectiveAgainst = new[] { "crane_wing", "long_snake" },
                        weakAgainst = new[] { "sharp_point" }
                    }
                }
            };
        }

        public BattleSetup CreateBattleSetup(string attackerFactionId, string defenderFactionId, string terrainId)
        {
            attackerDeploymentList = new List<BattleDeployment>();
            defenderDeploymentList = new List<BattleDeployment>();

            currentSetup = new BattleSetup
            {
                battleId = "battle_" + attackerFactionId + "_" + defenderFactionId + "_" + gameManager.State.turn,
                attackerFactionId = attackerFactionId,
                defenderFactionId = defenderFactionId,
                terrainId = terrainId,
                attackerFormationId = "crane_wing",
                defenderFormationId = "square",
                attackerDeployments = new BattleDeployment[0],
                defenderDeployments = new BattleDeployment[0]
            };

            return currentSetup;
        }

        public bool SetFormation(string factionId, string formationId)
        {
            if (currentSetup == null) return false;
            if (!formations.ContainsKey(formationId)) return false;

            if (factionId == currentSetup.attackerFactionId)
            {
                currentSetup.attackerFormationId = formationId;
            }
            else if (factionId == currentSetup.defenderFactionId)
            {
                currentSetup.defenderFormationId = formationId;
            }
            else
            {
                return false;
            }

            return true;
        }

        public bool DeployArmy(string factionId, string armyId, string generalId, string position, int soldierCount)
        {
            if (currentSetup == null) return false;

            FactionState faction = gameManager.State.FindFaction(factionId);
            if (faction == null) return false;

            ArmyState army = null;
            for (int i = 0; i < gameManager.State.armies.Count; i++)
            {
                if (gameManager.State.armies[i].id == armyId)
                {
                    army = gameManager.State.armies[i];
                    break;
                }
            }
            if (army == null) return false;

            if (soldierCount > army.soldiers) return false;

            BattleDeployment deployment = new BattleDeployment
            {
                armyId = armyId,
                generalId = generalId,
                formationPosition = position,
                soldierCount = soldierCount
            };

            if (factionId == currentSetup.attackerFactionId)
            {
                attackerDeploymentList.Add(deployment);
                currentSetup.attackerDeployments = attackerDeploymentList.ToArray();
            }
            else if (factionId == currentSetup.defenderFactionId)
            {
                defenderDeploymentList.Add(deployment);
                currentSetup.defenderDeployments = defenderDeploymentList.ToArray();
            }
            else
            {
                return false;
            }

            return true;
        }

        public FormationDefinition GetFormation(string formationId)
        {
            FormationDefinition formation;
            if (formations.TryGetValue(formationId, out formation))
            {
                return formation;
            }
            return null;
        }

        public int CalculateFormationBonus(string attackerFormationId, string defenderFormationId)
        {
            FormationDefinition attackerFormation = GetFormation(attackerFormationId);
            FormationDefinition defenderFormation = GetFormation(defenderFormationId);

            if (attackerFormation == null || defenderFormation == null) return 0;

            int bonus = 0;

            for (int i = 0; i < attackerFormation.effectiveAgainst.Length; i++)
            {
                if (attackerFormation.effectiveAgainst[i] == defenderFormationId)
                {
                    bonus += 20;
                    break;
                }
            }

            for (int i = 0; i < attackerFormation.weakAgainst.Length; i++)
            {
                if (attackerFormation.weakAgainst[i] == defenderFormationId)
                {
                    bonus -= 15;
                    break;
                }
            }

            return bonus;
        }

        public void ClearSetup()
        {
            currentSetup = null;
            attackerDeploymentList = null;
            defenderDeploymentList = null;
        }
    }
}
