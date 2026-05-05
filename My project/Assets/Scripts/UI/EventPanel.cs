using UnityEngine;
using UnityEngine.UI;

namespace WanChaoGuiYi
{
    public sealed class EventPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text eventNameText;
        [SerializeField] private Text eventCategoryText;
        [SerializeField] private Text choiceText;
        [SerializeField] private Button[] choiceButtons;
        [SerializeField] private Button closeButton;

        private GameContext context;
        private EventDefinition currentEvent;
        private string currentFactionId;

        public void Bind(GameObject root, Text eventName, Text eventCategory, Text choice, Button[] choices, Button close)
        {
            panelRoot = root;
            eventNameText = eventName;
            eventCategoryText = eventCategory;
            choiceText = choice;
            choiceButtons = choices;
            closeButton = close;
            BindButtons();
            Hide();
        }

        public void Initialize(GameContext gameContext)
        {
            context = gameContext;
        }

        public void Show(EventDefinition eventDefinition, string factionId)
        {
            if (eventDefinition == null) return;

            currentEvent = eventDefinition;
            currentFactionId = factionId;

            UIPanelVisibility.Show(panelRoot);

            SetText(eventNameText, eventDefinition.name);
            SetText(eventCategoryText, FormatCategory(eventDefinition.category));
            SetText(choiceText, FormatChoices(eventDefinition.choices));
            RefreshChoiceButtons(eventDefinition.choices);
        }

        public void ChooseOption(int index)
        {
            if (currentEvent == null || context == null) return;
            if (currentEvent.choices == null || index < 0 || index >= currentEvent.choices.Length) return;

            EventChoiceDefinition choice = currentEvent.choices[index];
            FactionState faction = context.State.FindFaction(currentFactionId);

            if (faction != null && choice.effects != null)
            {
                faction.legitimacy = ClampPercent(faction.legitimacy + choice.effects.legitimacy);
                faction.courtFactionPressure = ClampPercent(faction.courtFactionPressure + choice.effects.factionPressure);
                faction.money += choice.effects.money;
            }

            context.State.AddLog("event", "处理事件：" + currentEvent.name + "，选择：" + choice.label);
            context.Events.Publish(new GameEvent(GameEventType.EventTriggered, currentFactionId, choice));

            Hide();
        }

        public void Hide()
        {
            UIPanelVisibility.Hide(panelRoot);
            currentEvent = null;
            currentFactionId = null;
            RefreshChoiceButtons(null);
        }

        private void BindButtons()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }

            if (choiceButtons == null) return;
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                Button button = choiceButtons[i];
                if (button == null) continue;

                int choiceIndex = i;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(delegate { ChooseOption(choiceIndex); });
            }
        }

        private void RefreshChoiceButtons(EventChoiceDefinition[] choices)
        {
            if (choiceButtons != null)
            {
                for (int i = 0; i < choiceButtons.Length; i++)
                {
                    Button button = choiceButtons[i];
                    if (button == null) continue;

                    bool available = choices != null && i < choices.Length;
                    button.gameObject.SetActive(available);
                    if (available)
                    {
                        Text label = button.GetComponentInChildren<Text>();
                        SetText(label, choices[i].label);
                    }
                }
            }

            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(true);
            }
        }

        private static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }

        private static string FormatCategory(string category)
        {
            switch (category)
            {
                case "succession": return "继承";
                case "fiscal": return "财政";
                case "military": return "军事";
                case "rebellion": return "民变";
                case "talent": return "人才";
                default: return category;
            }
        }

        private static string FormatChoices(EventChoiceDefinition[] choices)
        {
            if (choices == null || choices.Length == 0) return "无选项";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < choices.Length; i++)
            {
                sb.AppendLine((i + 1) + ". " + choices[i].label);
            }

            return sb.ToString();
        }

        private static int ClampPercent(int value)
        {
            if (value < 0) return 0;
            if (value > 100) return 100;
            return value;
        }
    }
}
