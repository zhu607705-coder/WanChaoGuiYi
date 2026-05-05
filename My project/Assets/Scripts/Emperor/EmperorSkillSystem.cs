using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    /// <summary>
    /// 帝皇技能统一管理系统
    /// 每个帝皇在所有游戏系统中都有被动加成和主动技能
    /// 被动加成在对应系统的 ExecuteTurn 中自动生效
    /// 主动技能由玩家/AI手动触发，有冷却和资源消耗
    /// </summary>
    public sealed class EmperorSkillSystem : MonoBehaviour, IGameSystem
    {
        private readonly Dictionary<string, ActiveSkillCooldown> cooldowns = new Dictionary<string, ActiveSkillCooldown>();

        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context)
        {
            List<string> keys = new List<string>(cooldowns.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                ActiveSkillCooldown cd = cooldowns[keys[i]];
                if (cd.remainingTurns > 0)
                {
                    cd.remainingTurns--;
                }
                if (cd.remainingTurns <= 0)
                {
                    cooldowns.Remove(keys[i]);
                }
            }
        }

        public void ExecuteTurn(GameContext context)
        {
            for (int i = 0; i < context.State.factions.Count; i++)
            {
                FactionState faction = context.State.factions[i];
                EmperorDefinition emperor = context.Data.GetEmperor(faction.emperorId);
                if (emperor == null) continue;

                // 每回合重置乘数，避免累积
                faction.taxMultiplier = 1f;
                faction.foodMultiplier = 1f;
                faction.armyAttackMultiplier = 1f;
                faction.armyDefenseMultiplier = 1f;
                faction.talentMultiplier = 1f;

                ApplyEconomyPassive(context, faction, emperor);
                ApplyMilitaryPassive(context, faction, emperor);
                ApplyGovernancePassive(context, faction, emperor);
                ApplyTechPassive(context, faction, emperor);
                ApplyLegitimacyPassive(context, faction, emperor);
            }
        }

        // ========== 主动技能执行 ==========

        public bool CanUseSkill(string factionId, string skillId)
        {
            string key = factionId + "_" + skillId;
            ActiveSkillCooldown cd;
            if (cooldowns.TryGetValue(key, out cd) && cd.remainingTurns > 0)
            {
                return false;
            }
            return true;
        }

        public int GetCooldownRemaining(string factionId, string skillId)
        {
            string key = factionId + "_" + skillId;
            ActiveSkillCooldown cd;
            if (cooldowns.TryGetValue(key, out cd))
            {
                return cd.remainingTurns;
            }
            return 0;
        }

        public bool UseActiveSkill(GameContext context, string factionId, string skillId, string targetFactionId = null)
        {
            FactionState faction = context.State.FindFaction(factionId);
            if (faction == null) return false;

            EmperorDefinition emperor = context.Data.GetEmperor(faction.emperorId);
            if (emperor == null) return false;

            if (!CanUseSkill(factionId, skillId)) return false;

            // 检查技能是否属于该帝皇
            DiplomacySkillDefinition skill = FindSkill(emperor, skillId);
            if (skill == null) return false;

            // 检查资源
            if (faction.money < skill.moneyCost) return false;
            if (faction.talentIds.Count < skill.talentCost) return false;

            // 执行技能效果
            bool success = ExecuteSkillEffect(context, faction, emperor, skillId, targetFactionId);
            if (!success) return false;

            // 消耗资源
            faction.money -= skill.moneyCost;
            for (int i = 0; i < skill.talentCost; i++)
            {
                if (faction.talentIds.Count > 0)
                {
                    faction.talentIds.RemoveAt(0);
                }
            }

            // 设置冷却
            string key = factionId + "_" + skillId;
            cooldowns[key] = new ActiveSkillCooldown { remainingTurns = skill.cooldownTurns };

            context.State.AddLog("emperor", emperor.name + "发动技能：" + skill.name);
            context.Events.Publish(new GameEvent(GameEventType.EmperorSkillUsed, factionId, skill));

            return true;
        }

        private DiplomacySkillDefinition FindSkill(EmperorDefinition emperor, string skillId)
        {
            if (emperor.diplomacySkills == null) return null;
            for (int i = 0; i < emperor.diplomacySkills.Length; i++)
            {
                if (emperor.diplomacySkills[i].id == skillId)
                {
                    return emperor.diplomacySkills[i];
                }
            }
            return null;
        }

        // ========== 被动效果 ==========

        private void ApplyEconomyPassive(GameContext context, FactionState faction, EmperorDefinition emperor)
        {
            int admin = emperor.stats.administration;
            if (admin > 70)
            {
                float bonus = (admin - 70) / 100f;
                ApplyTaxBonus(context, faction, bonus);
            }
        }

        private void ApplyMilitaryPassive(GameContext context, FactionState faction, EmperorDefinition emperor)
        {
            int mil = emperor.stats.military;
            if (mil > 70)
            {
                float bonus = (mil - 70) / 100f;
                ApplyArmyStatBonus(context, faction, "attack", bonus);
            }
        }

        private void ApplyGovernancePassive(GameContext context, FactionState faction, EmperorDefinition emperor)
        {
            int charisma = emperor.stats.charisma;
            if (charisma > 70)
            {
                float bonus = (charisma - 70) / 200f;
                ApplyTalentBonus(context, faction, bonus);
            }
        }

        private void ApplyTechPassive(GameContext context, FactionState faction, EmperorDefinition emperor)
        {
            int reform = emperor.stats.reform;
            if (reform > 70)
            {
                faction.researchPoints += (reform - 70) / 20;
            }
        }

        private void ApplyLegitimacyPassive(GameContext context, FactionState faction, EmperorDefinition emperor)
        {
            int virtue = emperor.score.virtue;
            if (virtue > 70)
            {
                faction.legitimacy = Mathf.Min(100, faction.legitimacy + 1);
            }
        }

        // ========== 乘数应用 ==========

        private void ApplyTaxBonus(GameContext context, FactionState faction, float multiplier)
        {
            faction.taxMultiplier += multiplier;
        }

        private void ApplyFoodBonus(GameContext context, FactionState faction, float multiplier)
        {
            faction.foodMultiplier += multiplier;
        }

        private void ApplyArmyStatBonus(GameContext context, FactionState faction, string stat, float multiplier)
        {
            switch (stat)
            {
                case "attack":
                case "frontier_attack":
                case "cavalry_attack":
                    faction.armyAttackMultiplier += multiplier;
                    break;
                case "defense":
                    faction.armyDefenseMultiplier += multiplier;
                    break;
                case "discipline":
                    faction.armyAttackMultiplier += multiplier * 0.5f;
                    faction.armyDefenseMultiplier += multiplier * 0.5f;
                    break;
                case "recruit":
                    faction.talentMultiplier += multiplier;
                    break;
            }
        }

        private void ApplyTalentBonus(GameContext context, FactionState faction, float multiplier)
        {
            faction.talentMultiplier += multiplier;
        }

        // ========== 主动技能效果 ==========

        private delegate bool SkillExecutor(GameContext context, FactionState faction, string targetFactionId);
        private Dictionary<string, SkillExecutor> skillExecutors;

        private void InitSkillExecutors()
        {
            skillExecutors = new Dictionary<string, SkillExecutor>
            {
                { "yuan_jiao_jin_gong", (c, f, t) => ExecuteYuanJiaoJinGong(c, f, t) },
                { "shu_tong_wen_che_tong_gui", (c, f, t) => ExecuteShuTongWen(c, f) },
                { "yue_fa_san_zhang", (c, f, t) => ExecuteYueFaSanZhang(c, f) },
                { "feng_gong_chen", (c, f, t) => ExecuteFengGongChen(c, f) },
                { "tui_en_ling", (c, f, t) => ExecuteTuiEnLing(c, f) },
                { "yan_jiu_xiong_nu", (c, f, t) => ExecuteYanJiuXiongNu(c, f, t) },
                { "wu_shu_ju_xian", (c, f, t) => ExecuteWuShuJuXian(c, f) },
                { "lian_huan_ji", (c, f, t) => ExecuteLianHuanJi(c, f, t) },
                { "wei_wu_xia_jiang", (c, f, t) => ExecuteWeiWuXiaJiang(c, f) },
                { "duan_tou_ci_sha", (c, f, t) => ExecuteDuanTouCiSha(c, f, t) },
                { "xuan_wu_men_zhi_bian", (c, f, t) => ExecuteXuanWuMenZhiBian(c, f) },
                { "zhen_guan_zhi_zhi", (c, f, t) => ExecuteZhenGuanZhiZhi(c, f) },
                { "chen_qiao_bing_bian", (c, f, t) => ExecuteChenQiaoBingBian(c, f) },
                { "bei_song_chu_bing", (c, f, t) => ExecuteBeiSongChuBing(c, f, t) },
                { "gao_zhu_qiang_bing", (c, f, t) => ExecuteGaoZhuQiangBing(c, f) },
                { "xie_min_du_jiang", (c, f, t) => ExecuteXieMinDuJiang(c, f) }
            };
        }

        private bool ExecuteSkillEffect(GameContext context, FactionState faction, EmperorDefinition emperor, string skillId, string targetFactionId)
        {
            if (skillExecutors == null) InitSkillExecutors();

            SkillExecutor executor;
            if (skillExecutors.TryGetValue(skillId, out executor))
            {
                return executor(context, faction, targetFactionId);
            }
            return false;
        }

        // 技能实现（简化版本）
        private bool ExecuteYuanJiaoJinGong(GameContext context, FactionState faction, string targetFactionId) { return true; }
        private bool ExecuteShuTongWen(GameContext context, FactionState faction) { return true; }
        private bool ExecuteYueFaSanZhang(GameContext context, FactionState faction) { return true; }
        private bool ExecuteFengGongChen(GameContext context, FactionState faction) { return true; }
        private bool ExecuteTuiEnLing(GameContext context, FactionState faction) { return true; }
        private bool ExecuteYanJiuXiongNu(GameContext context, FactionState faction, string targetFactionId) { return true; }
        private bool ExecuteWuShuJuXian(GameContext context, FactionState faction) { return true; }
        private bool ExecuteLianHuanJi(GameContext context, FactionState faction, string targetFactionId) { return true; }
        private bool ExecuteWeiWuXiaJiang(GameContext context, FactionState faction) { return true; }
        private bool ExecuteDuanTouCiSha(GameContext context, FactionState faction, string targetFactionId) { return true; }
        private bool ExecuteXuanWuMenZhiBian(GameContext context, FactionState faction) { return true; }
        private bool ExecuteZhenGuanZhiZhi(GameContext context, FactionState faction) { return true; }
        private bool ExecuteChenQiaoBingBian(GameContext context, FactionState faction) { return true; }
        private bool ExecuteBeiSongChuBing(GameContext context, FactionState faction, string targetFactionId) { return true; }
        private bool ExecuteGaoZhuQiangBing(GameContext context, FactionState faction) { return true; }
        private bool ExecuteXieMinDuJiang(GameContext context, FactionState faction) { return true; }
    }

    public sealed class ActiveSkillCooldown
    {
        public int remainingTurns;
    }
}
