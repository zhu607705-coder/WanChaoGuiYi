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
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
        }

        public void Show(GameContext context, FactionState faction)
        {
            if (context == null || faction == null) return;

            if (panelRoot != null) panelRoot.SetActive(true);

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
            if (panelRoot != null) panelRoot.SetActive(false);
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
