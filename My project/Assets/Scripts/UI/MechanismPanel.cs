using UnityEngine;
using UnityEngine.UI;

namespace WanChaoGuiYi
{
    public sealed class MechanismPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text detailsText;
        [SerializeField] private Button applyPolicyButton;
        [SerializeField] private Button diplomacyButton;
        [SerializeField] private Button borderButton;
        [SerializeField] private Button enterWarModeButton;
        [SerializeField] private Button espionageButton;
        [SerializeField] private Button closeButton;

        private GameContext context;
        private FactionState faction;
        private ReformSystem reformSystem;
        private VictorySystem victorySystem;
        private DiplomacySystem diplomacySystem;
        private EspionageSystem espionageSystem;
        private SelectionContext selectionContext;
        private System.Func<SelectionContext> enterWarModeRequested;

        private const string DiplomacySourceReference = "《左传·僖公四年》《史记·秦始皇本纪》";
        private const string BorderSourceReference = "《汉书·食货志》《明史·兵志》";

        public void Bind(GameObject root, Text title, Text details, Button applyPolicy, Button diplomacy, Button border, Button enterWarMode, Button espionage, Button close)
        {
            panelRoot = root;
            titleText = title;
            detailsText = details;
            applyPolicyButton = applyPolicy;
            diplomacyButton = diplomacy;
            borderButton = border;
            enterWarModeButton = enterWarMode;
            espionageButton = espionage;
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
            if (applyPolicyButton != null)
            {
                applyPolicyButton.onClick.RemoveListener(ApplyFirstAvailablePolicy);
                applyPolicyButton.onClick.AddListener(ApplyFirstAvailablePolicy);
            }

            if (diplomacyButton != null)
            {
                diplomacyButton.onClick.RemoveListener(AdvanceFirstDiplomaticRelation);
                diplomacyButton.onClick.AddListener(AdvanceFirstDiplomaticRelation);
            }

            if (borderButton != null)
            {
                borderButton.onClick.RemoveListener(ApplyBorderControl);
                borderButton.onClick.AddListener(ApplyBorderControl);
            }

            if (enterWarModeButton != null)
            {
                enterWarModeButton.onClick.RemoveListener(EnterWarModeFromSelection);
                enterWarModeButton.onClick.AddListener(EnterWarModeFromSelection);
            }

            if (espionageButton != null)
            {
                espionageButton.onClick.RemoveListener(StartFirstEspionageOperation);
                espionageButton.onClick.AddListener(StartFirstEspionageOperation);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
                closeButton.onClick.AddListener(Hide);
            }
        }

        public void Show(GameContext gameContext, FactionState playerFaction, ReformSystem reforms, VictorySystem victory, DiplomacySystem diplomacy, EspionageSystem espionage)
        {
            Show(gameContext, playerFaction, reforms, victory, diplomacy, espionage, null, null);
        }

        public void Show(GameContext gameContext, FactionState playerFaction, ReformSystem reforms, VictorySystem victory, DiplomacySystem diplomacy, EspionageSystem espionage, SelectionContext mapSelectionContext, System.Func<SelectionContext> enterWarModeCallback)
        {
            if (gameContext == null || playerFaction == null) return;

            context = gameContext;
            faction = playerFaction;
            reformSystem = reforms;
            victorySystem = victory;
            diplomacySystem = diplomacy;
            espionageSystem = espionage;
            selectionContext = mapSelectionContext;
            enterWarModeRequested = enterWarModeCallback;

            UIPanelVisibility.Show(panelRoot);
            SetText(titleText, "政策、外交、谍报与胜利");
            RefreshDetails();
            RefreshActionButtons();
        }

        public void Hide()
        {
            UIPanelVisibility.Hide(panelRoot);
        }

        public void UpdateSelectionContext(SelectionContext mapSelectionContext)
        {
            selectionContext = mapSelectionContext;
            RefreshDetails();
            RefreshActionButtons();
        }

        private void ApplyFirstAvailablePolicy()
        {
            if (context == null || faction == null || reformSystem == null) return;

            PolicyDefinition policy = FindFirstAffordablePolicy();
            if (policy == null)
            {
                context.State.AddLog("policy", "当前没有可执行的政策。");
                RefreshDetails();
                return;
            }

            bool applied = reformSystem.ApplyPolicy(context, faction.id, policy.id);
            if (!applied)
            {
                context.State.AddLog("policy", "政策执行失败：" + policy.name);
            }

            RefreshDetails();
            RefreshActionButtons();
        }

        private void AdvanceFirstDiplomaticRelation()
        {
            if (context == null || faction == null || diplomacySystem == null) return;

            FactionState target = ResolveDiplomacyTarget();
            if (target == null)
            {
                context.State.AddLog("diplomacy", "当前没有可交涉势力。");
                RefreshDetails();
                return;
            }

            DiplomaticRelation relation = diplomacySystem.FindRelation(context, faction.id, target.id);
            DiplomacyResult result = relation != null && relation.status == DiplomacyStatus.AtWar
                ? diplomacySystem.MakePeace(context, faction.id, target.id)
                : diplomacySystem.DeclareWar(context, faction.id, target.id);

            if (result == null || !result.success)
            {
                context.State.AddLog("diplomacy", "外交行动失败：" + (result != null ? result.reason : "unknown"));
            }

            RefreshDetails();
            RefreshActionButtons();
        }

        private void ApplyBorderControl()
        {
            if (context == null || faction == null) return;

            FactionState target = ResolveDiplomacyTarget();
            if (target == null)
            {
                context.State.AddLog("border", "当前没有可管控的边境目标。");
                RefreshDetails();
                return;
            }

            if (selectionContext != null && !selectionContext.isNeighbor)
            {
                context.State.AddLog("border", "边境管控需要相邻目标：" + target.name);
                RefreshDetails();
                return;
            }

            if (!CanPayBorderControl())
            {
                context.State.AddLog("border", "边境管控资源不足：money " + StrategyCausalRules.BorderControlMoneyCost + " food " + StrategyCausalRules.BorderControlFoodCost);
                RefreshDetails();
                return;
            }

            DiplomaticRelation relation = diplomacySystem != null ? diplomacySystem.FindRelation(context, faction.id, target.id) : null;
            if (StrategyCausalRules.ApplyBorderControl(faction, relation))
            {
                context.Events.Publish(new GameEvent(GameEventType.DiplomacyProposalRejected, target.id, relation));
            }

            context.State.AddLog("border", "边境管控执行：" + target.name + " | Source: " + BorderSourceReference);
            RefreshDetails();
            RefreshActionButtons();
        }

        private void EnterWarModeFromSelection()
        {
            if (enterWarModeRequested == null || selectionContext == null || !selectionContext.HasAvailableAction("enter_war_mode"))
            {
                if (context != null && context.State != null)
                {
                    context.State.AddLog("war", "当前选择不能进入战争模式。");
                }
                RefreshDetails();
                return;
            }

            SelectionContext updatedContext = enterWarModeRequested();
            if (updatedContext != null)
            {
                selectionContext = updatedContext;
            }

            if (context != null && context.State != null)
            {
                context.State.AddLog("war", "已进入战争模式：" + ResolveSelectedRegionLabel());
            }

            RefreshDetails();
            RefreshActionButtons();
        }

        private void StartFirstEspionageOperation()
        {
            if (context == null || faction == null || espionageSystem == null) return;

            FactionState target = ResolveDiplomacyTarget();
            if (target == null)
            {
                context.State.AddLog("espionage", "当前没有可谍报目标。");
                RefreshDetails();
                return;
            }

            string targetRegionId = ResolveEspionageTargetRegionId(target);
            EspionageResult result = espionageSystem.StartOperation(context, faction.id, target.id, EspionageActionType.ScoutIntel, targetRegionId);
            if (result == null || !result.success)
            {
                context.State.AddLog("espionage", "谍报行动失败：" + (result != null ? result.reason : "unknown"));
            }

            RefreshDetails();
            RefreshActionButtons();
        }

        private void RefreshDetails()
        {
            if (context == null || faction == null) return;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("可执行政策：");
            AppendPolicyList(sb);
            sb.AppendLine();
            AppendDiplomacyStatus(sb);
            sb.AppendLine();
            AppendEspionageStatus(sb);
            sb.AppendLine();
            AppendVictoryStatus(sb);
            SetText(detailsText, sb.ToString());
        }

        private void AppendPolicyList(System.Text.StringBuilder sb)
        {
            int count = 0;
            foreach (PolicyDefinition policy in context.Data.Policies.Values)
            {
                if (policy == null || faction.completedReformIds.Contains(policy.id)) continue;
                if (!CanPay(policy.cost)) continue;

                sb.AppendLine("  · " + policy.name + " [" + policy.category + "] " + FormatCost(policy.cost));
                if (!string.IsNullOrEmpty(policy.sourceReference))
                {
                    sb.AppendLine("    Source: " + policy.sourceReference);
                }
                count++;
                if (count >= 5) break;
            }

            if (count == 0) sb.AppendLine("  暂无可执行政策");
        }

        private void AppendDiplomacyStatus(System.Text.StringBuilder sb)
        {
            sb.AppendLine("外交状态：");
            if (diplomacySystem == null)
            {
                sb.AppendLine("  外交系统未就绪");
                return;
            }

            FactionState target = ResolveDiplomacyTarget();
            if (target == null)
            {
                sb.AppendLine("  暂无其他势力");
                return;
            }

            DiplomaticRelation relation = diplomacySystem.FindRelation(context, faction.id, target.id);
            if (relation == null)
            {
                sb.AppendLine("  与" + target.name + "尚无外交关系");
                return;
            }

            sb.AppendLine("  目标：" + target.name);
            if (selectionContext != null && !string.IsNullOrEmpty(selectionContext.selectedRegionId))
            {
                sb.AppendLine("  选区：" + ResolveSelectedRegionLabel() + " | 模式：" + selectionContext.mode + " | 原因：" + selectionContext.modeEntryReason);
            }
            sb.AppendLine("  状态：" + FormatDiplomacyStatus(relation.status));
            sb.AppendLine("  好感：" + relation.opinion + " | 宿怨：" + relation.grudge);
            sb.AppendLine("  剩余回合：" + (relation.turnsRemaining > 0 ? relation.turnsRemaining.ToString() : "长期"));
            sb.AppendLine("  Diplomacy Cost: neutral war 0; treaty break costs legitimacy.");
            sb.AppendLine("  Diplomacy Source: " + DiplomacySourceReference);
            sb.AppendLine("  Border Cost: money " + StrategyCausalRules.BorderControlMoneyCost + " food " + StrategyCausalRules.BorderControlFoodCost);
            sb.AppendLine("  Border Impact: pressure only with opinion -" + StrategyCausalRules.BorderControlOpinionPenalty + " and grudge +" + StrategyCausalRules.BorderControlGrudgeIncrease + "; no direct positive state is granted.");
            sb.AppendLine("  Border Source: " + BorderSourceReference);
            sb.AppendLine("  War Mode: enter explicitly before dispatch.");
        }

        private void AppendEspionageStatus(System.Text.StringBuilder sb)
        {
            sb.AppendLine("谍报行动：");
            int count = 0;
            for (int i = 0; i < context.State.activeOperations.Count; i++)
            {
                EspionageOperation operation = context.State.activeOperations[i];
                if (operation == null || operation.agentFactionId != faction.id) continue;

                FactionState target = context.State.FindFaction(operation.targetFactionId);
                sb.AppendLine("  · " + FormatEspionageAction(operation.actionType) + " → " + (target != null ? target.name : operation.targetFactionId) + " 进度" + operation.progress + "% 风险" + operation.detectionRisk + "%");
                count++;
                if (count >= 4) break;
            }

            if (count == 0) sb.AppendLine("  暂无进行中的行动");
            sb.AppendLine("  可用人才：" + faction.talentIds.Count + " | 反谍：" + faction.espionageDefense);
        }

        private void AppendVictoryStatus(System.Text.StringBuilder sb)
        {
            sb.AppendLine("胜利状态：");
            if (victorySystem == null)
            {
                sb.AppendLine("  胜利系统未就绪");
                return;
            }

            VictoryCheckResult result = victorySystem.CheckVictory(context, faction);
            if (result.achieved)
            {
                sb.AppendLine("  已达成：" + result.conditionName);
            }
            else
            {
                sb.AppendLine("  进度：" + result.progress.ToString("F0") + "%");
                sb.AppendLine("  领地：" + faction.regionIds.Count + "/" + context.State.regions.Count);
                sb.AppendLine("  合法性：" + faction.legitimacy);
                sb.AppendLine("  核心改革：" + faction.completedReformIds.Count);
            }
        }

        private PolicyDefinition FindFirstAffordablePolicy()
        {
            foreach (PolicyDefinition policy in context.Data.Policies.Values)
            {
                if (policy == null || faction.completedReformIds.Contains(policy.id)) continue;
                if (CanPay(policy.cost)) return policy;
            }

            return null;
        }

        private FactionState FindFirstOtherFaction()
        {
            if (context == null || context.State == null || faction == null) return null;

            for (int i = 0; i < context.State.factions.Count; i++)
            {
                FactionState candidate = context.State.factions[i];
                if (candidate != null && candidate.id != faction.id) return candidate;
            }

            return null;
        }

        private FactionState ResolveDiplomacyTarget()
        {
            if (context == null || context.State == null || faction == null) return null;

            if (selectionContext != null && !string.IsNullOrEmpty(selectionContext.selectedRegionId))
            {
                string targetFactionId = !string.IsNullOrEmpty(selectionContext.targetFactionId)
                    ? selectionContext.targetFactionId
                    : selectionContext.ownerFactionId;
                if (!string.IsNullOrEmpty(targetFactionId) && targetFactionId != faction.id)
                {
                    FactionState selectedTarget = context.State.FindFaction(targetFactionId);
                    if (selectedTarget != null)
                    {
                        return selectedTarget;
                    }
                }

                return null;
            }

            return FindFirstOtherFaction();
        }

        private string ResolveEspionageTargetRegionId(FactionState target)
        {
            if (target == null || context == null || context.State == null) return null;

            if (selectionContext != null && !string.IsNullOrEmpty(selectionContext.selectedRegionId))
            {
                RegionState selectedRegion = context.State.FindRegion(selectionContext.selectedRegionId);
                if (selectedRegion != null && selectedRegion.ownerFactionId == target.id)
                {
                    return selectedRegion.id;
                }
            }

            for (int i = 0; i < target.regionIds.Count; i++)
            {
                RegionState region = context.State.FindRegion(target.regionIds[i]);
                if (region != null && region.visibilityState == VisibilityState.Hidden)
                {
                    return region.id;
                }
            }

            for (int i = 0; i < target.regionIds.Count; i++)
            {
                RegionState region = context.State.FindRegion(target.regionIds[i]);
                if (region != null && region.visibilityState != VisibilityState.Scouted)
                {
                    return region.id;
                }
            }

            return target.regionIds.Count > 0 ? target.regionIds[0] : null;
        }

        private string ResolveSelectedRegionLabel()
        {
            if (context == null || context.Data == null || selectionContext == null || string.IsNullOrEmpty(selectionContext.selectedRegionId))
            {
                return "none";
            }

            RegionDefinition definition = context.Data.GetRegion(selectionContext.selectedRegionId);
            return definition != null ? definition.name + "(" + definition.id + ")" : selectionContext.selectedRegionId;
        }

        private bool CanPayBorderControl()
        {
            return StrategyCausalRules.CanApplyBorderControl(faction);
        }

        private void RefreshActionButtons()
        {
            FactionState target = ResolveDiplomacyTarget();
            if (diplomacyButton != null)
            {
                diplomacyButton.interactable = target != null && diplomacySystem != null;
            }

            if (borderButton != null)
            {
                bool canUseBorder = target != null &&
                                    CanPayBorderControl() &&
                                    selectionContext != null &&
                                    selectionContext.HasAvailableAction("inspect_border") &&
                                    selectionContext.isNeighbor;
                borderButton.interactable = canUseBorder;
            }

            if (enterWarModeButton != null)
            {
                enterWarModeButton.interactable = selectionContext != null &&
                                                  selectionContext.HasAvailableAction("enter_war_mode") &&
                                                  selectionContext.isNeighbor &&
                                                  enterWarModeRequested != null;
            }

            if (espionageButton != null)
            {
                espionageButton.interactable = target != null && espionageSystem != null;
            }
        }

        private bool CanPay(CostSet cost)
        {
            if (cost == null) return true;
            return faction.money >= cost.money && faction.food >= cost.food && faction.legitimacy >= cost.legitimacy;
        }

        private static string FormatCost(CostSet cost)
        {
            if (cost == null) return "无消耗";
            return "金钱" + cost.money + " 粮食" + cost.food + " 合法性" + cost.legitimacy;
        }

        private static string FormatDiplomacyStatus(DiplomacyStatus status)
        {
            switch (status)
            {
                case DiplomacyStatus.Neutral: return "中立";
                case DiplomacyStatus.NonAggression: return "互不侵犯";
                case DiplomacyStatus.Alliance: return "同盟";
                case DiplomacyStatus.Vassal: return "称臣";
                case DiplomacyStatus.Tributary: return "纳贡";
                case DiplomacyStatus.AtWar: return "交战";
                default: return "未知";
            }
        }

        private static string FormatEspionageAction(EspionageActionType actionType)
        {
            switch (actionType)
            {
                case EspionageActionType.ScoutIntel: return "刺探情报";
                case EspionageActionType.Sabotage: return "离间策反";
                case EspionageActionType.SpreadRumors: return "散布谣言";
                case EspionageActionType.Assassinate: return "暗杀";
                default: return "未知";
            }
        }

        private static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }
    }
}
