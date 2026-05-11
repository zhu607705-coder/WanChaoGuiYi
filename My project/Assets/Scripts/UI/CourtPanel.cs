using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace WanChaoGuiYi
{
    public sealed class CourtPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text factionNameText;
        [SerializeField] private Text emperorNameText;
        [SerializeField] private Text moneyText;
        [SerializeField] private Text foodText;
        [SerializeField] private Text legitimacyText;
        [SerializeField] private Text successionRiskText;
        [SerializeField] private Text courtPressureText;
        [SerializeField] private Text regionCountText;
        [SerializeField] private Text talentCountText;
        [SerializeField] private Text turnLogText;
        [SerializeField] private ScrollRect turnLogScrollRect;
        [SerializeField] private RectTransform generalPortraitGridContent;
        [SerializeField] private Button recruitArmyButton;
        [SerializeField] private Button equipArmyButton;
        [SerializeField] private Button closeButton;

        private GameContext context;
        private FactionState faction;
        private EquipmentSystem equipmentSystem;
        private readonly Dictionary<string, Sprite> portraitSpriteCache = new Dictionary<string, Sprite>();

        public void Bind(GameObject root, Text factionName, Text emperorName, Text money, Text food, Text legitimacy, Text successionRisk, Text courtPressure, Text regionCount, Text talentCount, Text turnLog, ScrollRect turnLogScroll, RectTransform generalPortraitGrid, Button recruitArmy, Button equipArmy, Button close)
        {
            panelRoot = root;
            factionNameText = factionName;
            emperorNameText = emperorName;
            moneyText = money;
            foodText = food;
            legitimacyText = legitimacy;
            successionRiskText = successionRisk;
            courtPressureText = courtPressure;
            regionCountText = regionCount;
            talentCountText = talentCount;
            turnLogText = turnLog;
            turnLogScrollRect = turnLogScroll;
            generalPortraitGridContent = generalPortraitGrid;
            recruitArmyButton = recruitArmy;
            equipArmyButton = equipArmy;
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
            if (recruitArmyButton != null)
            {
                recruitArmyButton.onClick.RemoveListener(RecruitArmy);
                recruitArmyButton.onClick.AddListener(RecruitArmy);
            }

            if (equipArmyButton != null)
            {
                equipArmyButton.onClick.RemoveListener(EquipFirstAvailableArmy);
                equipArmyButton.onClick.AddListener(EquipFirstAvailableArmy);
            }

            if (closeButton == null) return;

            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(Hide);
        }

        public void Show(FactionState playerFaction, string emperorName, List<TurnLogEntry> recentLogs)
        {
            Show(null, playerFaction, emperorName, recentLogs, null);
        }

        public void Show(GameContext gameContext, FactionState playerFaction, string emperorName, List<TurnLogEntry> recentLogs, EquipmentSystem equipment)
        {
            if (playerFaction == null) return;

            context = gameContext;
            faction = playerFaction;
            equipmentSystem = equipment;

            UIPanelVisibility.Show(panelRoot);

            SetText(factionNameText, playerFaction.name);
            SetText(emperorNameText, "帝皇：" + emperorName);
            SetText(moneyText, "金钱：" + playerFaction.money);
            SetText(foodText, "粮食：" + playerFaction.food);
            SetText(legitimacyText, "合法性：" + playerFaction.legitimacy);
            SetText(successionRiskText, "继承风险：" + playerFaction.successionRisk);
            SetText(courtPressureText, "朝局压力：" + playerFaction.courtFactionPressure);
            SetText(regionCountText, "领地：" + playerFaction.regionIds.Count);
            SetText(talentCountText, "人才：" + playerFaction.talentIds.Count + " | 装备：" + FormatFirstArmyEquipment());
            SetText(turnLogText, FormatLogs(recentLogs));
            RebuildGeneralPortraitGrid();

            if (turnLogScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                turnLogScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        public void Hide()
        {
            UIPanelVisibility.Hide(panelRoot);
            context = null;
            faction = null;
            equipmentSystem = null;
        }

        private void RecruitArmy()
        {
            if (context == null || faction == null) return;

            RegionState musterRegion = FindFirstPlayerRegion();
            UnitDefinition unit = FindFirstAvailableUnit();
            if (musterRegion == null || unit == null)
            {
                context.State.AddLog("army", "当前无法募兵：缺少可用地区或兵种。") ;
                RefreshCurrentCourt();
                return;
            }

            int moneyCost = unit.cost != null ? unit.cost.money : 120;
            int foodCost = unit.cost != null ? unit.cost.food : 80;
            int manpowerCost = unit.cost != null ? unit.cost.manpower : 600;
            if (faction.money < moneyCost || faction.food < foodCost || musterRegion.manpower < manpowerCost)
            {
                context.State.AddLog("army", "募兵资源不足：需要金钱" + moneyCost + "、粮食" + foodCost + "、兵源" + manpowerCost + "。") ;
                RefreshCurrentCourt();
                return;
            }

            faction.money -= moneyCost;
            faction.food -= foodCost;
            musterRegion.manpower = Mathf.Max(0, musterRegion.manpower - manpowerCost);
            ArmyState army = new ArmyState
            {
                id = "army_player_" + (context.State.armies.Count + 1),
                ownerFactionId = faction.id,
                regionId = musterRegion.id,
                unitId = unit.id,
                soldiers = Mathf.Max(500, manpowerCost),
                morale = 60
            };
            context.State.armies.Add(army);
            context.State.AddLog("army", faction.name + "在" + musterRegion.id + "募兵成军：" + unit.name + "。") ;
            RefreshCurrentCourt();
        }

        private RegionState FindFirstPlayerRegion()
        {
            if (context == null || faction == null) return null;

            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = context.State.FindRegion(faction.regionIds[i]);
                if (region != null) return region;
            }

            return null;
        }

        private UnitDefinition FindFirstAvailableUnit()
        {
            if (context == null) return null;

            foreach (UnitDefinition unit in context.Data.Units.Values)
            {
                if (unit != null) return unit;
            }

            return null;
        }

        private void EquipFirstAvailableArmy()
        {
            if (context == null || faction == null || equipmentSystem == null) return;

            ArmyState army = FindFirstPlayerArmy();
            if (army == null)
            {
                context.State.AddLog("equipment", "当前没有可整备军队。");
                RefreshCurrentCourt();
                return;
            }

            EquipmentDefinition equipment = FindFirstAvailableEquipmentForArmy(army);
            if (equipment == null)
            {
                context.State.AddLog("equipment", "当前没有可用装备。");
                RefreshCurrentCourt();
                return;
            }

            bool equipped = equipmentSystem.EquipArmy(army, equipment.id);
            if (equipped)
            {
                context.State.AddLog("equipment", faction.name + "为" + army.id + "装备" + equipment.name + "。");
            }
            else
            {
                context.State.AddLog("equipment", "装备整备失败：" + equipment.name);
            }

            RefreshCurrentCourt();
        }

        private ArmyState FindFirstPlayerArmy()
        {
            if (context == null || faction == null) return null;

            for (int i = 0; i < context.State.armies.Count; i++)
            {
                ArmyState army = context.State.armies[i];
                if (army != null && army.ownerFactionId == faction.id) return army;
            }

            return null;
        }

        private EquipmentDefinition FindFirstAvailableEquipmentForArmy(ArmyState army)
        {
            if (equipmentSystem == null || faction == null || army == null) return null;

            List<EquipmentDefinition> available = equipmentSystem.GetAvailableEquipment(faction);
            for (int i = 0; i < available.Count; i++)
            {
                EquipmentDefinition equipment = available[i];
                if (equipment == null) continue;
                if (equipment.slot == "weapon" && army.weaponSlot == equipment.id) continue;
                if (equipment.slot == "armor" && army.armorSlot == equipment.id) continue;
                if (equipment.slot == "special" && army.specialSlot == equipment.id) continue;
                return equipment;
            }

            return null;
        }

        private void RefreshCurrentCourt()
        {
            if (context == null || faction == null) return;

            EmperorDefinition emperor = context.Data.GetEmperor(faction.emperorId);
            Show(context, faction, emperor != null ? emperor.name : "未知", context.State.turnLog, equipmentSystem);
        }

        private string FormatFirstArmyEquipment()
        {
            ArmyState army = FindFirstPlayerArmy();
            if (army == null) return "无军队";

            return "武器 " + FormatEquipmentName(army.weaponSlot) + " / 护甲 " + FormatEquipmentName(army.armorSlot) + " / 特装 " + FormatEquipmentName(army.specialSlot);
        }

        private string FormatEquipmentName(string equipmentId)
        {
            if (string.IsNullOrEmpty(equipmentId)) return "无";
            if (equipmentSystem == null) return equipmentId;

            EquipmentDefinition equipment = equipmentSystem.GetEquipment(equipmentId);
            return equipment != null ? equipment.name : equipmentId;
        }

        private void RebuildGeneralPortraitGrid()
        {
            if (generalPortraitGridContent == null) return;

            for (int i = generalPortraitGridContent.childCount - 1; i >= 0; i--)
            {
                Destroy(generalPortraitGridContent.GetChild(i).gameObject);
            }

            if (context == null || context.Data == null || context.Data.Generals == null) return;

            foreach (GeneralDefinition general in context.Data.Generals.Values)
            {
                if (general == null) continue;
                CreateGeneralPortraitCard(general);
            }
        }

        private void CreateGeneralPortraitCard(GeneralDefinition general)
        {
            GameObject card = new GameObject("GeneralPortraitCard_" + general.id);
            card.transform.SetParent(generalPortraitGridContent, false);
            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(74f, 96f);
            Image cardBackground = card.AddComponent<Image>();
            cardBackground.color = new Color(0.18f, 0.14f, 0.10f, 0.88f);

            GameObject portrait = new GameObject("GeneralPortrait_" + general.id);
            portrait.transform.SetParent(card.transform, false);
            RectTransform portraitRect = portrait.AddComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0.5f, 1f);
            portraitRect.anchorMax = new Vector2(0.5f, 1f);
            portraitRect.anchoredPosition = new Vector2(0f, -32f);
            portraitRect.sizeDelta = new Vector2(64f, 64f);
            Image portraitImage = portrait.AddComponent<Image>();
            portraitImage.preserveAspect = true;
            portraitImage.color = Color.white;
            portraitImage.sprite = LoadPortraitSprite(general.portraitAssetPath);

            Text label = CreateRuntimeText(card.transform, "GeneralPortraitLabel_" + general.id, general.name, new Vector2(0f, -82f), new Vector2(68f, 20f));
            label.alignment = TextAnchor.MiddleCenter;
            label.fontSize = Mathf.RoundToInt(UITheme.TextSizeSmall);
            label.color = UITheme.TextSecondary;
        }

        private Sprite LoadPortraitSprite(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return null;

            Sprite cached;
            if (portraitSpriteCache.TryGetValue(assetPath, out cached)) return cached;

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string filePath = Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(filePath)) return null;

            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.name = Path.GetFileNameWithoutExtension(filePath);
            if (!texture.LoadImage(bytes)) return null;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 256f);
            sprite.name = texture.name;
            portraitSpriteCache[assetPath] = sprite;
            return sprite;
        }

        private static Text CreateRuntimeText(Transform parent, string name, string content, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            Text text = obj.AddComponent<Text>();
            text.text = content;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return text;
        }

        private static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }

        private static string FormatLogs(List<TurnLogEntry> logs)
        {
            if (logs == null || logs.Count == 0) return "暂无记录。";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int start = Mathf.Max(0, logs.Count - 30);
            for (int i = start; i < logs.Count; i++)
            {
                TurnLogEntry entry = logs[i];
                sb.AppendLine("[T" + entry.turn + "] " + entry.message);
            }

            return sb.ToString();
        }
    }
}
