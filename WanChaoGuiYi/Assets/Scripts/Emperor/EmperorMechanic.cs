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
}
