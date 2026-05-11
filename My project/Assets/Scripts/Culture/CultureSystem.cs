using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class CultureSystem : MonoBehaviour, IGameSystem
    {
        private static readonly Dictionary<string, CustomEffect> CustomEffects = new Dictionary<string, CustomEffect>
        {
            { "martial", new CustomEffect { manpowerQuality = 15, rebellionRisk = 5, taxEfficiency = 0, researchSpeed = 0 } },
            { "scholarly", new CustomEffect { manpowerQuality = -5, rebellionRisk = 0, taxEfficiency = 0, researchSpeed = 10 } },
            { "mercantile", new CustomEffect { manpowerQuality = 0, rebellionRisk = 3, taxEfficiency = 10, researchSpeed = 0 } },
            { "agrarian", new CustomEffect { manpowerQuality = 0, rebellionRisk = -3, taxEfficiency = 5, researchSpeed = 0 } },
            { "frontier", new CustomEffect { manpowerQuality = 10, rebellionRisk = 5, taxEfficiency = -5, researchSpeed = 0 } },
            { "pluralistic", new CustomEffect { manpowerQuality = 0, rebellionRisk = 0, taxEfficiency = 0, researchSpeed = 5 } }
        };

        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            for (int i = 0; i < context.State.regions.Count; i++)
            {
                RegionState region = context.State.regions[i];
                ProcessRegionCustoms(context, region);
            }
        }

        private void ProcessRegionCustoms(GameContext context, RegionState region)
        {
            if (region.customs == null || region.customs.Length == 0)
            {
                // 从历史层数据推导风俗
                region.customs = DeriveCustoms(context, region.id);
            }

            // 计算风俗稳定度
            int stability = CalculateCustomStability(context, region);
            region.customStability = stability;

            // 应用风俗效果
            for (int i = 0; i < region.customs.Length; i++)
            {
                CustomEffect effect;
                if (CustomEffects.TryGetValue(region.customs[i], out effect))
                {
                    ApplyCustomEffect(region, effect, stability);
                }
            }
        }

        private string[] DeriveCustoms(GameContext context, string regionId)
        {
            HistoricalLayerDefinition layer = FindHistoricalLayer(context, regionId);
            if (layer == null) return new string[] { "agrarian" };

            List<string> customs = new List<string>();

            // 基于地理标签推导风俗
            if (layer.geographyTags != null)
            {
                for (int i = 0; i < layer.geographyTags.Length; i++)
                {
                    string tag = layer.geographyTags[i];
                    if (tag.Contains("frontier") || tag.Contains("steppe")) customs.Add("frontier");
                    if (tag.Contains("plain") || tag.Contains("basin")) customs.Add("agrarian");
                    if (tag.Contains("coast") || tag.Contains("trade")) customs.Add("mercantile");
                    if (tag.Contains("mountain")) customs.Add("martial");
                }
            }

            // 基于兵器传统推导
            if (layer.weaponTraditions != null)
            {
                for (int i = 0; i < layer.weaponTraditions.Length; i++)
                {
                    if (layer.weaponTraditions[i].Contains("cavalry") || layer.weaponTraditions[i].Contains("archer"))
                    {
                        if (!customs.Contains("martial")) customs.Add("martial");
                    }
                }
            }

            // 基于自定义标签
            if (layer.customTags != null)
            {
                for (int i = 0; i < layer.customTags.Length; i++)
                {
                    string tag = layer.customTags[i];
                    if (tag.Contains("scholar") || tag.Contains("confucian")) customs.Add("scholarly");
                    if (tag.Contains("multiethnic") || tag.Contains("market")) customs.Add("pluralistic");
                }
            }

            // 确保至少有一个风俗
            if (customs.Count == 0) customs.Add("agrarian");

            // 去重
            return RemoveDuplicates(customs);
        }

        private int CalculateCustomStability(GameContext context, RegionState region)
        {
            FactionState owner = context.State.FindFaction(region.ownerFactionId);
            if (owner == null) return 50;

            int stability = 50;

            // 整合度影响风俗稳定
            stability += region.integration / 5;

            // 合法性影响
            stability += (owner.legitimacy - 50) / 10;

            // 帝皇多元治理能力
            EmperorDefinition emperor = context.Data.GetEmperor(owner.emperorId);
            if (emperor != null)
            {
                // 康熙的多元调和机制
                if (emperor.uniqueMechanic != null && emperor.uniqueMechanic.id == "duo_yuan_tiao_he")
                {
                    stability += 10;
                }

                // 外交属性影响
                stability += emperor.stats.diplomacy / 20;
            }

            return Mathf.Clamp(stability, 0, 100);
        }

        private static void ApplyCustomEffect(RegionState region, CustomEffect effect, int stability)
        {
            // 风俗效果随稳定度线性缩放
            float factor = stability / 100f;

            // 风俗不稳定的负面效果放大
            if (stability < 40)
            {
                region.rebellionRisk += Mathf.RoundToInt(effect.rebellionRisk * 1.5f);
            }
            else
            {
                region.rebellionRisk += Mathf.RoundToInt(effect.rebellionRisk * factor);
            }
        }

        public int GetManpowerQualityModifier(RegionState region)
        {
            if (region.customs == null) return 0;

            int modifier = 0;
            for (int i = 0; i < region.customs.Length; i++)
            {
                CustomEffect effect;
                if (CustomEffects.TryGetValue(region.customs[i], out effect))
                {
                    modifier += Mathf.RoundToInt(effect.manpowerQuality * region.customStability / 100f);
                }
            }

            return modifier;
        }

        public int GetTaxEfficiencyModifier(RegionState region)
        {
            if (region.customs == null) return 0;

            int modifier = 0;
            for (int i = 0; i < region.customs.Length; i++)
            {
                CustomEffect effect;
                if (CustomEffects.TryGetValue(region.customs[i], out effect))
                {
                    modifier += Mathf.RoundToInt(effect.taxEfficiency * region.customStability / 100f);
                }
            }

            return modifier;
        }

        public int GetResearchSpeedModifier(RegionState region)
        {
            if (region.customs == null) return 0;

            int modifier = 0;
            for (int i = 0; i < region.customs.Length; i++)
            {
                CustomEffect effect;
                if (CustomEffects.TryGetValue(region.customs[i], out effect))
                {
                    modifier += Mathf.RoundToInt(effect.researchSpeed * region.customStability / 100f);
                }
            }

            return modifier;
        }

        private static HistoricalLayerDefinition FindHistoricalLayer(GameContext context, string regionId)
        {
            foreach (HistoricalLayerDefinition layer in context.Data.HistoricalLayers.Values)
            {
                if (layer.regionId == regionId) return layer;
            }

            return null;
        }

        private static string[] RemoveDuplicates(List<string> list)
        {
            Dictionary<string, bool> seen = new Dictionary<string, bool>();
            List<string> result = new List<string>();

            for (int i = 0; i < list.Count; i++)
            {
                if (!seen.ContainsKey(list[i]))
                {
                    seen[list[i]] = true;
                    result.Add(list[i]);
                }
            }

            return result.ToArray();
        }
    }

    public sealed class CustomEffect
    {
        public int manpowerQuality;
        public int rebellionRisk;
        public int taxEfficiency;
        public int researchSpeed;
    }
}
