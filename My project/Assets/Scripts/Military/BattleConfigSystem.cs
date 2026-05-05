using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    /// <summary>
    /// 电子斗蛐蛐配置系统：处理玩家的帝皇+将领、兵种+装备、阵型+战术配置
    /// </summary>
    public sealed class BattleConfigSystem : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        private BattleConfig currentConfig;
        private Dictionary<string, FormationDefinition> formations;
        private Dictionary<string, BattleTactic> tactics;

        public BattleConfig CurrentConfig { get { return currentConfig; } }

        private void Awake()
        {
            InitializeFormations();
            InitializeTactics();
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

        private void InitializeTactics()
        {
            tactics = new Dictionary<string, BattleTactic>
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

        public BattleConfig CreateDefaultConfig()
        {
            currentConfig = new BattleConfig
            {
                configId = "config_" + System.Guid.NewGuid().ToString(),
                emperorId = "qin_shi_huang",
                generalIds = new string[0],
                unitIds = new string[] { "infantry", "infantry", "cavalry" },
                equipmentIds = new string[0],
                formationId = "crane_wing",
                tacticIds = new string[] { "fire_attack", "charge" }
            };

            return currentConfig;
        }

        public bool SetEmperor(string emperorId)
        {
            if (currentConfig == null) return false;

            EmperorDefinition emperor = gameManager.Data.GetEmperor(emperorId);
            if (emperor == null) return false;

            currentConfig.emperorId = emperorId;
            return true;
        }

        public bool AddGeneral(string generalId)
        {
            if (currentConfig == null) return false;

            GeneralDefinition general = null;
            foreach (var kvp in gameManager.Data.Generals)
            {
                if (kvp.Key == generalId)
                {
                    general = kvp.Value;
                    break;
                }
            }
            if (general == null) return false;

            for (int i = 0; i < currentConfig.generalIds.Length; i++)
            {
                if (currentConfig.generalIds[i] == generalId) return false;
            }

            List<string> generals = new List<string>(currentConfig.generalIds);
            generals.Add(generalId);
            currentConfig.generalIds = generals.ToArray();

            return true;
        }

        public bool RemoveGeneral(string generalId)
        {
            if (currentConfig == null) return false;

            List<string> generals = new List<string>(currentConfig.generalIds);
            if (!generals.Remove(generalId)) return false;
            currentConfig.generalIds = generals.ToArray();

            return true;
        }

        public bool AddUnit(string unitId)
        {
            if (currentConfig == null) return false;

            UnitDefinition unit = gameManager.Data.GetUnit(unitId);
            if (unit == null) return false;

            List<string> units = new List<string>(currentConfig.unitIds);
            units.Add(unitId);
            currentConfig.unitIds = units.ToArray();

            return true;
        }

        public bool RemoveUnit(string unitId)
        {
            if (currentConfig == null) return false;

            List<string> units = new List<string>(currentConfig.unitIds);
            if (!units.Remove(unitId)) return false;
            currentConfig.unitIds = units.ToArray();

            return true;
        }

        public bool SetFormation(string formationId)
        {
            if (currentConfig == null) return false;
            if (!formations.ContainsKey(formationId)) return false;

            currentConfig.formationId = formationId;
            return true;
        }

        public bool AddTactic(string tacticId)
        {
            if (currentConfig == null) return false;
            if (!tactics.ContainsKey(tacticId)) return false;

            for (int i = 0; i < currentConfig.tacticIds.Length; i++)
            {
                if (currentConfig.tacticIds[i] == tacticId) return false;
            }

            List<string> tacticList = new List<string>(currentConfig.tacticIds);
            tacticList.Add(tacticId);
            currentConfig.tacticIds = tacticList.ToArray();

            return true;
        }

        public bool RemoveTactic(string tacticId)
        {
            if (currentConfig == null) return false;

            List<string> tacticList = new List<string>(currentConfig.tacticIds);
            if (!tacticList.Remove(tacticId)) return false;
            currentConfig.tacticIds = tacticList.ToArray();

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

        public BattleTactic GetTactic(string tacticId)
        {
            BattleTactic tactic;
            if (tactics.TryGetValue(tacticId, out tactic))
            {
                return tactic;
            }
            return null;
        }

        public List<FormationDefinition> GetAllFormations()
        {
            return new List<FormationDefinition>(formations.Values);
        }

        public List<BattleTactic> GetAllTactics()
        {
            return new List<BattleTactic>(tactics.Values);
        }

        public List<EmperorDefinition> GetAllEmperors()
        {
            return new List<EmperorDefinition>(gameManager.Data.Emperors.Values);
        }

        public List<GeneralDefinition> GetAllGenerals()
        {
            return new List<GeneralDefinition>(gameManager.Data.Generals.Values);
        }

        public List<UnitDefinition> GetAllUnits()
        {
            return new List<UnitDefinition>(gameManager.Data.Units.Values);
        }

        public void ClearConfig()
        {
            currentConfig = null;
        }
    }
}
