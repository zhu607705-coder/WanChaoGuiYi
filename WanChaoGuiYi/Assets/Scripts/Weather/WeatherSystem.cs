using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class WeatherSystem : MonoBehaviour, IGameSystem
    {
        private static readonly string[] WeatherTypes = new string[]
        {
            "normal", "drought", "flood", "harvest", "cold", "plague"
        };

        private static readonly Dictionary<string, WeatherEffect> WeatherEffects = new Dictionary<string, WeatherEffect>
        {
            { "normal", new WeatherEffect { foodModifier = 0, populationGrowth = 0, rebellionRisk = 0, armyMovement = 0 } },
            { "drought", new WeatherEffect { foodModifier = -30, populationGrowth = -2, rebellionRisk = 5, armyMovement = 0 } },
            { "flood", new WeatherEffect { foodModifier = -20, populationGrowth = -3, rebellionRisk = 3, armyMovement = -20 } },
            { "harvest", new WeatherEffect { foodModifier = 20, populationGrowth = 2, rebellionRisk = -2, armyMovement = 0 } },
            { "cold", new WeatherEffect { foodModifier = -15, populationGrowth = -2, rebellionRisk = 2, armyMovement = -30 } },
            { "plague", new WeatherEffect { foodModifier = -10, populationGrowth = -5, rebellionRisk = 8, armyMovement = -10 } }
        };

        public void Initialize(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }
        public void ExecuteTurn(GameContext context) { }

        public void OnTurnStart(GameContext context)
        {
            string weather = GenerateWeather(context);
            context.State.currentWeatherId = weather;

            WeatherEffect effect;
            if (!WeatherEffects.TryGetValue(weather, out effect))
            {
                effect = WeatherEffects["normal"];
            }

            // 从原始数据恢复 foodOutput，再应用天气修正
            for (int i = 0; i < context.State.regions.Count; i++)
            {
                RegionState region = context.State.regions[i];
                RestoreBaseFoodOutput(context, region);
                ApplyWeatherToRegion(region, effect);
            }

            if (weather != "normal")
            {
                context.State.AddLog("weather", "本回合天气：" + FormatWeatherName(weather));
                context.Events.Publish(new GameEvent(GameEventType.WeatherChanged, weather, effect));
            }
        }

        private void RestoreBaseFoodOutput(GameContext context, RegionState region)
        {
            RegionDefinition def;
            if (context.Data.Regions.TryGetValue(region.id, out def))
            {
                region.foodOutput = def.foodOutput;
            }
        }

        private void ApplyWeatherToRegion(RegionState region, WeatherEffect effect)
        {
            region.foodOutput = Mathf.Max(0, region.foodOutput + Mathf.RoundToInt(region.foodOutput * effect.foodModifier / 100f));
        }

        private string GenerateWeather(GameContext context)
        {
            float droughtWeight = context.State.season == Season.Spring ? 0.08f : 0.12f;
            float floodWeight = context.State.season == Season.Spring ? 0.10f : 0.06f;
            float harvestWeight = context.State.season == Season.Autumn ? 0.15f : 0.03f;
            float coldWeight = context.State.season == Season.Autumn ? 0.08f : 0.04f;
            float plagueWeight = 0.03f;

            float resilience = GetAverageWeatherResilience(context);
            float resilienceFactor = Mathf.Clamp01(1f - resilience / 200f);

            droughtWeight *= resilienceFactor;
            floodWeight *= resilienceFactor;
            plagueWeight *= resilienceFactor;

            float roll = Random.value;
            float cumulative = 0f;

            cumulative += droughtWeight;
            if (roll < cumulative) return "drought";

            cumulative += floodWeight;
            if (roll < cumulative) return "flood";

            cumulative += harvestWeight;
            if (roll < cumulative) return "harvest";

            cumulative += coldWeight;
            if (roll < cumulative) return "cold";

            cumulative += plagueWeight;
            if (roll < cumulative) return "plague";

            return "normal";
        }

        private float GetAverageWeatherResilience(GameContext context)
        {
            float total = 0f;
            int count = 0;

            for (int i = 0; i < context.State.factions.Count; i++)
            {
                total += context.State.factions[i].weatherResilience;
                count++;
            }

            return count > 0 ? total / count : 0f;
        }

        public WeatherEffect GetCurrentEffect(GameContext context)
        {
            WeatherEffect effect;
            if (WeatherEffects.TryGetValue(context.State.currentWeatherId, out effect))
            {
                return effect;
            }

            return WeatherEffects["normal"];
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

    public sealed class WeatherEffect
    {
        public int foodModifier;
        public int populationGrowth;
        public int rebellionRisk;
        public int armyMovement;
    }
}
