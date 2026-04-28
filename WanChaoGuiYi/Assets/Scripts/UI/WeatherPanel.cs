using UnityEngine;
using UnityEngine.UI;

namespace WanChaoGuiYi
{
    public sealed class WeatherPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text weatherNameText;
        [SerializeField] private Text weatherEffectText;
        [SerializeField] private Text celestialEventText;
        [SerializeField] private Text resilienceText;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
        }

        public void Show(GameContext context, WeatherSystem weatherSystem, CelestialEventSystem celestialSystem)
        {
            if (context == null || weatherSystem == null) return;

            if (panelRoot != null) panelRoot.SetActive(true);

            WeatherEffect effect = weatherSystem.GetCurrentEffect(context);
            string weatherId = context.State.currentWeatherId;

            SetText(weatherNameText, "当前天气：" + FormatWeatherName(weatherId));

            string effectDesc = "";
            if (effect.foodModifier != 0) effectDesc += "粮食产出 " + (effect.foodModifier > 0 ? "+" : "") + effect.foodModifier + "% ";
            if (effect.populationGrowth != 0) effectDesc += "人口增长 " + effect.populationGrowth + " ";
            if (effect.rebellionRisk != 0) effectDesc += "民变风险 " + (effect.rebellionRisk > 0 ? "+" : "") + effect.rebellionRisk + " ";
            if (effect.armyMovement != 0) effectDesc += "军事行动 " + effect.armyMovement + "% ";

            SetText(weatherEffectText, string.IsNullOrEmpty(effectDesc) ? "无特殊效果" : effectDesc);

            // 天文事件
            if (celestialSystem != null)
            {
                string celestialId = celestialSystem.GetCurrentCelestialEvent(context);
                if (!string.IsNullOrEmpty(celestialId))
                {
                    CelestialEventDef celestialDef = celestialSystem.GetCelestialEventDef(celestialId);
                    if (celestialDef != null)
                    {
                        SetText(celestialEventText, "天象：" + celestialDef.name + " — " + celestialDef.description);
                    }
                }
                else
                {
                    SetText(celestialEventText, "天象：无异象");
                }
            }

            // 天气韧性
            FactionState playerFaction = context.State.FindFaction(context.State.playerFactionId);
            if (playerFaction != null)
            {
                SetText(resilienceText, "天气韧性：" + playerFaction.weatherResilience + " | 灾害抵御：" + playerFaction.disasterMitigation);
            }
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }

        private static string FormatWeatherName(string weather)
        {
            switch (weather)
            {
                case "drought": return "旱灾";
                case "flood": return "水灾";
                case "harvest": return "丰年";
                case "cold": return "寒潮";
                case "plague": return "瘟疫";
                default: return "正常";
            }
        }
    }
}
