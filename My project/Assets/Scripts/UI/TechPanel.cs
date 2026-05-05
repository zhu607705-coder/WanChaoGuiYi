using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WanChaoGuiYi
{
    public sealed class TechPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text currentResearchText;
        [SerializeField] private Text progressText;
        [SerializeField] private Text availableTechsText;
        [SerializeField] private Text completedTechsText;
        [SerializeField] private Button setResearchButton;
        [SerializeField] private Button closeButton;

        private GameContext context;
        private FactionState faction;
        private TechSystem techSystem;

        public void Bind(GameObject root, Text title, Text currentResearch, Text progress, Text availableTechs, Text completedTechs, Button setResearch, Button close)
        {
            panelRoot = root;
            titleText = title;
            currentResearchText = currentResearch;
            progressText = progress;
            availableTechsText = availableTechs;
            completedTechsText = completedTechs;
            setResearchButton = setResearch;
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
            if (setResearchButton != null)
            {
                setResearchButton.onClick.RemoveListener(SetFirstAvailableResearch);
                setResearchButton.onClick.AddListener(SetFirstAvailableResearch);
            }

            if (closeButton == null) return;

            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(Hide);
        }

        public void Show(GameContext gameContext, FactionState playerFaction, TechSystem researchSystem)
        {
            if (gameContext == null || playerFaction == null) return;

            context = gameContext;
            faction = playerFaction;
            techSystem = researchSystem;

            RefreshDetails();
        }

        private void RefreshDetails()
        {
            if (context == null || faction == null) return;

            UIPanelVisibility.Show(panelRoot);

            SetText(titleText, "科技研究");

            // 当前研究
            if (!string.IsNullOrEmpty(faction.currentResearchId))
            {
                TechnologyDefinition tech;
                if (context.Data.Technologies.TryGetValue(faction.currentResearchId, out tech))
                {
                    SetText(currentResearchText, "正在研究：" + tech.name);
                    int effectiveCost = Mathf.Max(1, tech.cost);
                    float progress = (float)faction.researchPoints / effectiveCost * 100f;
                    SetText(progressText, "进度：" + faction.researchPoints + "/" + effectiveCost + " (" + progress.ToString("F0") + "%)");
                }
            }
            else
            {
                SetText(currentResearchText, "未选择研究目标");
                SetText(progressText, "");
            }

            // 已完成科技
            if (faction.completedTechIds.Count > 0)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("已完成科技：");
                for (int i = 0; i < faction.completedTechIds.Count; i++)
                {
                    TechnologyDefinition tech;
                    if (context.Data.Technologies.TryGetValue(faction.completedTechIds[i], out tech))
                    {
                        sb.AppendLine("  · " + tech.name + (string.IsNullOrEmpty(tech.sourceReference) ? "" : " [" + tech.sourceReference + "]"));
                    }
                }

                SetText(completedTechsText, sb.ToString());
            }
            else
            {
                SetText(completedTechsText, "尚无已完成科技");
            }

            // 可研究科技
            List<TechnologyDefinition> available = GetAvailableTechs(context, faction);
            if (available.Count > 0)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("可研究科技：");
                for (int i = 0; i < available.Count; i++)
                {
                    TechnologyDefinition tech = available[i];
                    string prereq = "";
                    if (tech.prerequisites != null && tech.prerequisites.Length > 0)
                    {
                        prereq = " (前置：" + string.Join(", ", tech.prerequisites) + ")";
                    }

                    sb.AppendLine("  · " + tech.name + " [" + tech.era + "] 费用：" + tech.cost + prereq);
                }

                SetText(availableTechsText, sb.ToString());
            }
            else
            {
                SetText(availableTechsText, "暂无可研究科技");
            }
        }

        public void Hide()
        {
            UIPanelVisibility.Hide(panelRoot);
        }

        private void SetFirstAvailableResearch()
        {
            if (context == null || faction == null || techSystem == null) return;

            List<TechnologyDefinition> available = GetAvailableTechs(context, faction);
            if (available.Count == 0)
            {
                context.State.AddLog("tech", "当前没有可研究科技。");
                RefreshDetails();
                return;
            }

            TechnologyDefinition tech = available[0];
            techSystem.SetResearch(faction, tech.id);
            context.State.AddLog("tech", "选择研究：" + tech.name);
            RefreshDetails();
        }

        private List<TechnologyDefinition> GetAvailableTechs(GameContext context, FactionState faction)
        {
            List<TechnologyDefinition> available = new List<TechnologyDefinition>();

            foreach (TechnologyDefinition tech in context.Data.Technologies.Values)
            {
                if (faction.completedTechIds.Contains(tech.id)) continue;

                bool hasPrereqs = true;
                if (tech.prerequisites != null)
                {
                    for (int i = 0; i < tech.prerequisites.Length; i++)
                    {
                        if (!faction.completedTechIds.Contains(tech.prerequisites[i]))
                        {
                            hasPrereqs = false;
                            break;
                        }
                    }
                }

                if (hasPrereqs) available.Add(tech);
            }

            return available;
        }

        private static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }
    }
}
