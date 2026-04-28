using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class CelestialEventSystem : MonoBehaviour, IGameSystem
    {
        private static readonly Dictionary<string, CelestialEventDef> CelestialEvents = new Dictionary<string, CelestialEventDef>
        {
            { "solar_eclipse", new CelestialEventDef { id = "solar_eclipse", name = "日食", probability = 0.04f, legitimacyEffect = -8, description = "日食示警，天命动摇。" } },
            { "comet", new CelestialEventDef { id = "comet", name = "彗星", probability = 0.03f, legitimacyEffect = -5, description = "彗星犯境，人心不安。" } },
            { "five_star_gathering", new CelestialEventDef { id = "five_star_gathering", name = "五星聚", probability = 0.01f, legitimacyEffect = 15, description = "五星聚于一舍，大吉之兆。" } },
            { "lunar_eclipse", new CelestialEventDef { id = "lunar_eclipse", name = "月食", probability = 0.06f, legitimacyEffect = 0, description = "月食，可借机改革。" } },
            { "guest_star", new CelestialEventDef { id = "guest_star", name = "客星", probability = 0.02f, legitimacyEffect = -3, description = "客星现于天际，异象频出。" } }
        };

        private Dictionary<string, int> cooldowns = new Dictionary<string, int>();

        public void Initialize(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }
        public void ExecuteTurn(GameContext context) { }

        public void OnTurnStart(GameContext context)
        {
            // 更新冷却
            List<string> expiredKeys = new List<string>();
            foreach (string key in cooldowns.Keys)
            {
                cooldowns[key]--;
                if (cooldowns[key] <= 0) expiredKeys.Add(key);
            }

            for (int i = 0; i < expiredKeys.Count; i++)
            {
                cooldowns.Remove(expiredKeys[i]);
            }

            // 检查天文事件
            foreach (CelestialEventDef celestialEvent in CelestialEvents.Values)
            {
                if (cooldowns.ContainsKey(celestialEvent.id)) continue;
                if (Random.value > celestialEvent.probability) continue;

                TriggerCelestialEvent(context, celestialEvent);
                cooldowns[celestialEvent.id] = 12; // 冷却 12 回合
                break; // 每回合最多一个天文事件
            }
        }

        private void TriggerCelestialEvent(GameContext context, CelestialEventDef celestialEvent)
        {
            context.State.currentCelestialEventId = celestialEvent.id;

            // 应用合法性效果
            for (int i = 0; i < context.State.factions.Count; i++)
            {
                FactionState faction = context.State.factions[i];

                // 司天监科技降低负面效果
                int effect = celestialEvent.legitimacyEffect;
                if (effect < 0 && HasAstronomyTech(faction))
                {
                    effect = Mathf.RoundToInt(effect * 0.6f); // 降低 40%
                }

                faction.legitimacy = ClampPercent(faction.legitimacy + effect);

                // 五星聚增加继承安全
                if (celestialEvent.id == "five_star_gathering")
                {
                    faction.successionRisk = Mathf.Max(0, faction.successionRisk - 5);
                }

                // 日食增加朝局压力
                if (celestialEvent.id == "solar_eclipse")
                {
                    faction.courtFactionPressure = Mathf.Min(100, faction.courtFactionPressure + 5);
                }
            }

            context.State.AddLog("celestial", "天象异变：" + celestialEvent.name + "——" + celestialEvent.description);
            context.Events.Publish(new GameEvent(GameEventType.CelestialEventOccurred, celestialEvent.id, celestialEvent));
        }

        private bool HasAstronomyTech(FactionState faction)
        {
            return faction.completedTechIds.Contains("astronomical_bureau");
        }

        public string GetCurrentCelestialEvent(GameContext context)
        {
            return context.State.currentCelestialEventId;
        }

        public CelestialEventDef GetCelestialEventDef(string id)
        {
            CelestialEventDef def;
            CelestialEvents.TryGetValue(id, out def);
            return def;
        }

        private static int ClampPercent(int value)
        {
            if (value < 0) return 0;
            if (value > 100) return 100;
            return value;
        }
    }

    public sealed class CelestialEventDef
    {
        public string id;
        public string name;
        public float probability;
        public int legitimacyEffect;
        public string description;
    }
}
