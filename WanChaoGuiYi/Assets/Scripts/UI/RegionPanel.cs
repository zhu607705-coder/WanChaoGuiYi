using UnityEngine;
using UnityEngine.UI;

namespace WanChaoGuiYi
{
    public sealed class RegionPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text regionNameText;
        [SerializeField] private Text terrainText;
        [SerializeField] private Text populationText;
        [SerializeField] private Text foodText;
        [SerializeField] private Text taxText;
        [SerializeField] private Text manpowerText;
        [SerializeField] private Text ownerText;
        [SerializeField] private Text integrationText;
        [SerializeField] private Text rebellionText;
        [SerializeField] private Text annexationText;
        [SerializeField] private Text localPowerText;
        [SerializeField] private Text neighborsText;
        [SerializeField] private Text landStructureText;
        [SerializeField] private Text customsText;
        [SerializeField] private Button closeButton;

        public string CurrentRegionId { get; private set; }

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
        }

        public void Show(RegionDefinition definition, RegionState state)
        {
            if (definition == null || state == null) return;

            CurrentRegionId = definition.id;
            if (panelRoot != null) panelRoot.SetActive(true);

            SetText(regionNameText, definition.name);
            SetText(terrainText, "地形：" + FormatTerrain(definition.terrain));
            SetText(populationText, "人口：" + FormatPopulation(state.population));
            SetText(foodText, "粮食产出：" + state.foodOutput);
            SetText(taxText, "税收产出：" + state.taxOutput);
            SetText(manpowerText, "兵源：" + state.manpower);
            SetText(integrationText, "整合度：" + state.integration + "%");
            SetText(rebellionText, "民变风险：" + state.rebellionRisk + "%");
            SetText(annexationText, "土地兼并：" + state.annexationPressure + "%");
            SetText(localPowerText, "地方势力：" + state.localPower);
            SetText(neighborsText, "相邻区域：" + FormatNeighbors(definition.neighbors));
            SetText(landStructureText, FormatLandStructure(state.landStructure));
            SetText(customsText, FormatCustoms(state.customs, state.customStability));
        }

        public void SetOwner(string ownerName)
        {
            SetText(ownerText, "归属：" + ownerName);
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            CurrentRegionId = null;
        }

        private static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }

        private static string FormatTerrain(string terrain)
        {
            switch (terrain)
            {
                case "plain": return "平原";
                case "hill": return "丘陵";
                case "mountain": return "山地";
                case "basin": return "盆地";
                case "river_plain": return "河谷平原";
                case "frontier_plain": return "边塞平原";
                case "plateau": return "高原";
                case "corridor": return "走廊";
                case "river_delta": return "三角洲";
                case "mountain_coast": return "山海";
                case "subtropical": return "亚热带";
                case "frontier_forest": return "边塞林地";
                default: return terrain;
            }
        }

        private static string FormatPopulation(int population)
        {
            if (population >= 10000)
            {
                return (population / 10000f).ToString("F1") + "万";
            }

            return population.ToString();
        }

        private static string FormatNeighbors(string[] neighbors)
        {
            if (neighbors == null || neighbors.Length == 0) return "无";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < neighbors.Length; i++)
            {
                if (i > 0) sb.Append("、");
                sb.Append(neighbors[i]);
            }

            return sb.ToString();
        }

        private static string FormatLandStructure(LandStructure ls)
        {
            if (ls == null) return "";

            return "土地结构：小农 " + Mathf.RoundToInt(ls.smallFarmers * 100) +
                   "% | 地方精英 " + Mathf.RoundToInt(ls.localElites * 100) +
                   "% | 国有 " + Mathf.RoundToInt(ls.stateLand * 100) +
                   "% | 寺院 " + Mathf.RoundToInt(ls.religiousLand * 100) + "%";
        }

        private static string FormatCustoms(string[] customs, int stability)
        {
            if (customs == null || customs.Length == 0) return "风俗：无";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("风俗：");
            for (int i = 0; i < customs.Length; i++)
            {
                if (i > 0) sb.Append("、");
                sb.Append(FormatCustomName(customs[i]));
            }

            sb.Append(" | 稳定度：" + stability + "%");
            return sb.ToString();
        }

        private static string FormatCustomName(string custom)
        {
            switch (custom)
            {
                case "martial": return "尚武";
                case "scholarly": return "崇文";
                case "mercantile": return "重商";
                case "agrarian": return "农耕";
                case "frontier": return "边塞";
                case "pluralistic": return "多元";
                default: return custom;
            }
        }
    }
}
