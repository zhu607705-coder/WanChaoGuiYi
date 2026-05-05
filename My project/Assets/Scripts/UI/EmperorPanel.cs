using UnityEngine;
using UnityEngine.UI;

namespace WanChaoGuiYi
{
    public sealed class EmperorPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text emperorNameText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text mechanicNameText;
        [SerializeField] private Text mechanicDescText;
        [SerializeField] private Text statsText;
        [SerializeField] private Text burdensText;
        [SerializeField] private Text legitimacyText;
        [SerializeField] private Text successionRiskText;
        [SerializeField] private Text heirText;
        [SerializeField] private Text stableSuccessionsText;
        [SerializeField] private Text skillText;
        [SerializeField] private Button resolveSuccessionButton;
        [SerializeField] private Button useSkillButton;
        [SerializeField] private Button closeButton;

        private GameContext context;
        private FactionState faction;
        private EmperorDefinition emperor;
        private EmperorSkillSystem skillSystem;
        private SuccessionSystem successionSystem;
        private string currentSkillId;

        public string CurrentEmperorId { get; private set; }

        public void Bind(GameObject root, Text emperorName, Text title, Text mechanicName, Text mechanicDesc, Text stats, Text burdens, Text legitimacy, Text successionRisk, Text heir, Text stableSuccessions, Text skills, Button resolveSuccession, Button useSkill, Button close)
        {
            panelRoot = root;
            emperorNameText = emperorName;
            titleText = title;
            mechanicNameText = mechanicName;
            mechanicDescText = mechanicDesc;
            statsText = stats;
            burdensText = burdens;
            legitimacyText = legitimacy;
            successionRiskText = successionRisk;
            heirText = heir;
            stableSuccessionsText = stableSuccessions;
            skillText = skills;
            resolveSuccessionButton = resolveSuccession;
            useSkillButton = useSkill;
            closeButton = close;
            BindButtons();
            Hide();
        }

        private void Awake()
        {
            UIPanelVisibility.Hide(panelRoot);
            BindButtons();
        }

        private void BindButtons()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
                closeButton.onClick.AddListener(Hide);
            }

            if (resolveSuccessionButton != null)
            {
                resolveSuccessionButton.onClick.RemoveListener(ResolveCurrentSuccession);
                resolveSuccessionButton.onClick.AddListener(ResolveCurrentSuccession);
            }

            if (useSkillButton != null)
            {
                useSkillButton.onClick.RemoveListener(UseCurrentSkill);
                useSkillButton.onClick.AddListener(UseCurrentSkill);
            }
        }

        public void Show(GameContext gameContext, EmperorDefinition emperorDefinition, FactionState playerFaction, EmperorSkillSystem skills)
        {
            Show(gameContext, emperorDefinition, playerFaction, skills, null);
        }

        public void Show(GameContext gameContext, EmperorDefinition emperorDefinition, FactionState playerFaction, EmperorSkillSystem skills, SuccessionSystem succession)
        {
            if (emperorDefinition == null || playerFaction == null) return;

            context = gameContext;
            emperor = emperorDefinition;
            faction = playerFaction;
            skillSystem = skills;
            successionSystem = succession;
            CurrentEmperorId = emperor.id;
            UIPanelVisibility.Show(panelRoot);

            RefreshDetails();
        }

        public void Hide()
        {
            UIPanelVisibility.Hide(panelRoot);
            CurrentEmperorId = null;
            context = null;
            emperor = null;
            faction = null;
            skillSystem = null;
            successionSystem = null;
            currentSkillId = null;
        }

        private void RefreshDetails()
        {
            if (emperor == null || faction == null) return;

            SetText(emperorNameText, emperor.name);
            SetText(titleText, emperor.title);
            SetText(mechanicNameText, "机制：" + (emperor.uniqueMechanic != null ? emperor.uniqueMechanic.name : "无"));
            SetText(mechanicDescText, emperor.uniqueMechanic != null ? emperor.uniqueMechanic.description : "");
            SetText(statsText, FormatStats(emperor.stats));
            SetText(burdensText, "历史负担：" + FormatArray(emperor.historicalBurdens));
            SetText(legitimacyText, "合法性：" + faction.legitimacy);
            SetText(successionRiskText, "继承风险：" + faction.successionRisk);
            SetText(heirText, FormatHeir(faction.heir));
            SetText(stableSuccessionsText, "平稳继承次数：" + faction.stableSuccessions);
            SetText(skillText, FormatSkills());
            if (resolveSuccessionButton != null) resolveSuccessionButton.interactable = CanResolveSuccession();
            if (useSkillButton != null) useSkillButton.interactable = CanUseCurrentSkill();
        }

        private void ResolveCurrentSuccession()
        {
            if (!CanResolveSuccession())
            {
                if (context != null && context.State != null) context.State.AddLog("succession", "当前无法处理继承。叠加朝局压力或继承风险后再评估。");
                RefreshDetails();
                return;
            }

            successionSystem.ResolveSuccession(context, faction.id);
            RefreshDetails();
        }

        private void UseCurrentSkill()
        {
            if (!CanUseCurrentSkill())
            {
                if (context != null && context.State != null) context.State.AddLog("emperor", "当前帝皇技能不可发动。");
                RefreshDetails();
                return;
            }

            bool used = skillSystem.UseActiveSkill(context, faction.id, currentSkillId, FindFirstTargetFactionId());
            if (!used && context != null && context.State != null)
            {
                context.State.AddLog("emperor", "帝皇技能发动失败。");
            }

            RefreshDetails();
        }

        private static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }

        private string FormatSkills()
        {
            currentSkillId = null;
            if (emperor == null || emperor.diplomacySkills == null || emperor.diplomacySkills.Length == 0) return "主动技能：无";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("主动技能：");
            for (int i = 0; i < emperor.diplomacySkills.Length; i++)
            {
                DiplomacySkillDefinition skill = emperor.diplomacySkills[i];
                if (skill == null) continue;

                if (currentSkillId == null) currentSkillId = skill.id;
                int cooldown = skillSystem != null ? skillSystem.GetCooldownRemaining(faction.id, skill.id) : 0;
                sb.Append("  ").Append(skill.name)
                  .Append(" | 金钱 ").Append(skill.moneyCost)
                  .Append(" | 人才 ").Append(skill.talentCost)
                  .Append(" | 冷却 ").Append(cooldown > 0 ? cooldown.ToString() : "可用")
                  .AppendLine();
            }

            return sb.ToString();
        }

        private bool CanResolveSuccession()
        {
            if (context == null || faction == null || successionSystem == null || faction.heir == null) return false;

            return faction.successionRisk >= 60 || faction.courtFactionPressure >= 70;
        }

        private bool CanUseCurrentSkill()
        {
            if (context == null || faction == null || skillSystem == null || string.IsNullOrEmpty(currentSkillId)) return false;

            DiplomacySkillDefinition skill = FindSkill(currentSkillId);
            if (skill == null) return false;
            if (!skillSystem.CanUseSkill(faction.id, currentSkillId)) return false;
            return faction.money >= skill.moneyCost && faction.talentIds.Count >= skill.talentCost;
        }

        private DiplomacySkillDefinition FindSkill(string skillId)
        {
            if (emperor == null || emperor.diplomacySkills == null) return null;

            for (int i = 0; i < emperor.diplomacySkills.Length; i++)
            {
                DiplomacySkillDefinition skill = emperor.diplomacySkills[i];
                if (skill != null && skill.id == skillId) return skill;
            }

            return null;
        }

        private string FindFirstTargetFactionId()
        {
            if (context == null || context.State == null || faction == null) return null;

            for (int i = 0; i < context.State.factions.Count; i++)
            {
                FactionState target = context.State.factions[i];
                if (target != null && target.id != faction.id) return target.id;
            }

            return null;
        }

        private static string FormatStats(EmperorStats stats)
        {
            if (stats == null) return "";

            return "军事 " + stats.military +
                   " | 行政 " + stats.administration +
                   " | 改革 " + stats.reform +
                   " | 魅力 " + stats.charisma +
                   " | 外交 " + stats.diplomacy +
                   " | 继承掌控 " + stats.successionControl;
        }

        private static string FormatArray(string[] arr)
        {
            if (arr == null || arr.Length == 0) return "无";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < arr.Length; i++)
            {
                if (i > 0) sb.Append("、");
                sb.Append(arr[i]);
            }

            return sb.ToString();
        }

        private static string FormatHeir(HeirState heir)
        {
            if (heir == null) return "继承人：无";

            return "继承人：" + heir.name +
                   " | 年龄：" + heir.age +
                   " | 合法性：" + heir.legitimacy +
                   " | 能力：" + heir.ability;
        }
    }
}
