using UnityEngine;

namespace WanChaoGuiYi
{
    public interface IEmperorMechanic
    {
        string Id { get; }
        void ApplyPassive(GameContext context, FactionState faction);
        void OnRegionConquered(GameContext context, FactionState faction, RegionState region);
    }

    public sealed class EmperorMechanicSystem : MonoBehaviour, IGameSystem
    {
        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            for (int i = 0; i < context.State.factions.Count; i++)
            {
                FactionState faction = context.State.factions[i];
                EmperorDefinition emperor = context.Data.GetEmperor(faction.emperorId);
                IEmperorMechanic mechanic = EmperorMechanicRegistry.Create(emperor);
                mechanic.ApplyPassive(context, faction);
            }
        }
    }

    public static class EmperorMechanicRegistry
    {
        public static IEmperorMechanic Create(EmperorDefinition emperor)
        {
            if (emperor == null || emperor.uniqueMechanic == null)
            {
                return new DefaultEmperorMechanic();
            }

            switch (emperor.uniqueMechanic.id)
            {
                case "liu_he_tong_gui":
                    return new StandardizationMechanic();
                case "bu_yi_gong_tian_xia":
                    return new CoalitionMechanic();
                case "wai_chao_yuan_zheng":
                    return new FrontierExpeditionMechanic();
                case "xie_ling_chong_zu":
                    return new WartimeReconstructionMechanic();
                case "zhen_guan_jun_chen":
                    return new MinisterialGovernanceMechanic();
                case "bei_jiu_shi_bing_quan":
                    return new CivilianControlMechanic();
                case "chong_zao_ji_ceng":
                    return new GrassrootsRebuildMechanic();
                case "duo_yuan_tiao_he":
                    return new MultiethnicHarmonizationMechanic();
                case "kai_huang_gai_zhi":
                    return new KaiHuangReformMechanic();
                case "shi_nian_kai_tuo":
                    return new ShiNianKaiTuoMechanic();
                case "han_hua_gai_ge":
                    return new HanHuaReformMechanic();
                case "di_ceng_jue_qi":
                    return new DiCengJueQiMechanic();
                case "yi_de_ju_ren":
                    return new YiDeJuRenMechanic();
                default:
                    return new DefaultEmperorMechanic();
            }
        }
    }

    public class DefaultEmperorMechanic : IEmperorMechanic
    {
        public virtual string Id { get { return "default"; } }
        public virtual void ApplyPassive(GameContext context, FactionState faction) { }
        public virtual void OnRegionConquered(GameContext context, FactionState faction, RegionState region) { }
    }

    public sealed class StandardizationMechanic : DefaultEmperorMechanic
    {
        public override string Id { get { return "liu_he_tong_gui"; } }

        public override void ApplyPassive(GameContext context, FactionState faction)
        {
            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = context.State.FindRegion(faction.regionIds[i]);
                if (region != null && region.integration < 100)
                {
                    region.integration = Mathf.Min(100, region.integration + 2);
                    region.rebellionRisk = Mathf.Min(100, region.rebellionRisk + 1);
                }
            }

            faction.successionRisk = Mathf.Min(100, faction.successionRisk + 1);
        }
    }

    public sealed class CoalitionMechanic : DefaultEmperorMechanic
    {
        public override string Id { get { return "bu_yi_gong_tian_xia"; } }

        public override void ApplyPassive(GameContext context, FactionState faction)
        {
            faction.courtFactionPressure = Mathf.Min(100, faction.courtFactionPressure + 1);
            if (faction.legitimacy < 70)
            {
                faction.legitimacy += 1;
            }
        }
    }

    public sealed class CivilianControlMechanic : DefaultEmperorMechanic
    {
        public override string Id { get { return "bei_jiu_shi_bing_quan"; } }

        public override void ApplyPassive(GameContext context, FactionState faction)
        {
            faction.courtFactionPressure = Mathf.Max(0, faction.courtFactionPressure - 1);
            faction.successionRisk = Mathf.Max(0, faction.successionRisk - 1);
        }
    }

    public sealed class GrassrootsRebuildMechanic : DefaultEmperorMechanic
    {
        public override string Id { get { return "chong_zao_ji_ceng"; } }

        public override void ApplyPassive(GameContext context, FactionState faction)
        {
            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = context.State.FindRegion(faction.regionIds[i]);
                if (region == null) continue;

                region.annexationPressure = Mathf.Max(0, region.annexationPressure - 1);
                region.localPower = Mathf.Max(0, region.localPower - 1);
                region.rebellionRisk = Mathf.Min(100, region.rebellionRisk + 1);
            }
        }
    }

    public sealed class FrontierExpeditionMechanic : DefaultEmperorMechanic
    {
        public override string Id { get { return "wai_chao_yuan_zheng"; } }

        public override void ApplyPassive(GameContext context, FactionState faction)
        {
            // 汉武帝：边疆扩张效率更高，但财政压力更大
            faction.money = Mathf.Max(0, faction.money - 5);
            faction.legitimacy = Mathf.Min(100, faction.legitimacy + 1);

            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = context.State.FindRegion(faction.regionIds[i]);
                if (region == null) continue;

                // 边疆地区（高 localPower）整合更快
                if (region.localPower > 60)
                {
                    region.integration = Mathf.Min(100, region.integration + 2);
                }
            }
        }
    }

    public sealed class WartimeReconstructionMechanic : DefaultEmperorMechanic
    {
        public override string Id { get { return "xie_ling_chong_zu"; } }

        public override void ApplyPassive(GameContext context, FactionState faction)
        {
            // 曹操：战乱地区恢复更快，但正统争议更重
            faction.legitimacy = Mathf.Max(0, faction.legitimacy - 1);

            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = context.State.FindRegion(faction.regionIds[i]);
                if (region == null) continue;

                // 低整合度地区恢复更快（屯田效果）
                if (region.integration < 50)
                {
                    region.integration = Mathf.Min(100, region.integration + 3);
                    region.population = Mathf.RoundToInt(region.population * 1.005f);
                }
            }
        }
    }

    public sealed class MinisterialGovernanceMechanic : DefaultEmperorMechanic
    {
        public override string Id { get { return "zhen_guan_jun_chen"; } }

        public override void ApplyPassive(GameContext context, FactionState faction)
        {
            // 李世民：纳谏和人才效果更高，但宗室压力累积
            faction.courtFactionPressure = Mathf.Min(100, faction.courtFactionPressure + 1);

            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = context.State.FindRegion(faction.regionIds[i]);
                if (region == null) continue;

                // 高合法性地区治理更好
                if (faction.legitimacy > 60)
                {
                    region.rebellionRisk = Mathf.Max(0, region.rebellionRisk - 1);
                    region.annexationPressure = Mathf.Max(0, region.annexationPressure - 1);
                }
            }
        }
    }

    public sealed class MultiethnicHarmonizationMechanic : DefaultEmperorMechanic
    {
        public override string Id { get { return "duo_yuan_tiao_he"; } }

        public override void ApplyPassive(GameContext context, FactionState faction)
        {
            // 康熙：多民族治理更强，但治理成本更高
            faction.money = Mathf.Max(0, faction.money - 3);

            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = context.State.FindRegion(faction.regionIds[i]);
                if (region == null) continue;

                // 多民族地区（高 localPower）整合更稳定
                if (region.localPower > 50)
                {
                    region.integration = Mathf.Min(100, region.integration + 2);
                    region.rebellionRisk = Mathf.Max(0, region.rebellionRisk - 1);
                }
            }
        }
    }

    public sealed class KaiHuangReformMechanic : DefaultEmperorMechanic
    {
        public override string Id { get { return "kai_huang_gai_zhi"; } }

        public override void ApplyPassive(GameContext context, FactionState faction)
        {
            // 杨坚：户籍财政效率极高，但猜忌心导致功臣和宗室压力累积
            faction.courtFactionPressure = Mathf.Min(100, faction.courtFactionPressure + 2);
            faction.successionRisk = Mathf.Min(100, faction.successionRisk + 1);

            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = context.State.FindRegion(faction.regionIds[i]);
                if (region == null) continue;

                // 开皇改制：地方势力压低（但有下限）
                region.localPower = Mathf.Max(10, region.localPower - 1);
            }
        }
    }

    public sealed class ShiNianKaiTuoMechanic : DefaultEmperorMechanic
    {
        public override string Id { get { return "shi_nian_kai_tuo"; } }

        private int turnsSinceStart;

        public override void ApplyPassive(GameContext context, FactionState faction)
        {
            turnsSinceStart++;

            // 柴荣：短期高强度改革，但寿命风险递增
            faction.successionRisk = Mathf.Min(100, faction.successionRisk + 2);

            // 前 12 回合（约 6 年）改革效率极高
            if (turnsSinceStart <= 12)
            {
                for (int i = 0; i < faction.regionIds.Count; i++)
                {
                    RegionState region = context.State.FindRegion(faction.regionIds[i]);
                    if (region == null) continue;

                    region.integration = Mathf.Min(100, region.integration + 3);
                    region.annexationPressure = Mathf.Max(0, region.annexationPressure - 2);
                }

                faction.legitimacy = Mathf.Min(100, faction.legitimacy + 1);
            }
            else
            {
                // 超过 12 回合后，寿命风险急剧上升
                faction.successionRisk = Mathf.Min(100, faction.successionRisk + 3);
            }
        }
    }

    public sealed class HanHuaReformMechanic : DefaultEmperorMechanic
    {
        public override string Id { get { return "han_hua_gai_ge"; } }

        public override void ApplyPassive(GameContext context, FactionState faction)
        {
            // 元宏：汉化改革提升文明，但旧势力反弹
            faction.legitimacy = Mathf.Min(100, faction.legitimacy + 1);
            faction.courtFactionPressure = Mathf.Min(100, faction.courtFactionPressure + 2);

            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = context.State.FindRegion(faction.regionIds[i]);
                if (region == null) continue;

                // 汉化地区整合更快，但高地方势力区域反弹
                if (region.localPower > 60)
                {
                    region.rebellionRisk = Mathf.Min(100, region.rebellionRisk + 2);
                }
                else
                {
                    region.integration = Mathf.Min(100, region.integration + 2);
                    region.rebellionRisk = Mathf.Max(0, region.rebellionRisk - 1);
                }
            }
        }
    }

    public sealed class DiCengJueQiMechanic : DefaultEmperorMechanic
    {
        public override string Id { get { return "di_ceng_jue_qi"; } }

        public override void ApplyPassive(GameContext context, FactionState faction)
        {
            // 石勒：军事扩张强，但合法性基础弱
            faction.legitimacy = Mathf.Max(0, faction.legitimacy - 1);

            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = context.State.FindRegion(faction.regionIds[i]);
                if (region == null) continue;

                // 新占领地区（低整合）恢复更快
                if (region.integration < 50)
                {
                    region.integration = Mathf.Min(100, region.integration + 2);
                }
            }

            // 继承风险高
            faction.successionRisk = Mathf.Min(100, faction.successionRisk + 2);
        }
    }

    public sealed class YiDeJuRenMechanic : DefaultEmperorMechanic
    {
        public override string Id { get { return "yi_de_ju_ren"; } }

        public override void ApplyPassive(GameContext context, FactionState faction)
        {
            // 刘备：民心极高，人才凝聚力强，但国力增长慢
            faction.legitimacy = Mathf.Min(100, faction.legitimacy + 2);
            faction.courtFactionPressure = Mathf.Max(0, faction.courtFactionPressure - 1);

            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = context.State.FindRegion(faction.regionIds[i]);
                if (region == null) continue;

                // 民心地区民变风险低
                region.rebellionRisk = Mathf.Max(0, region.rebellionRisk - 1);
            }
        }
    }
}
