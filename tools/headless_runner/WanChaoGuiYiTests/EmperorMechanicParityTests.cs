using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

namespace WanChaoGuiYi.Tests
{
    public sealed class EmperorMechanicParityTests
    {
        [Fact]
        public void Emperor_Mechanics_Should_Create_Three_Distinct_Playstyle_Effects()
        {
            NonUnityJsonDataRepository repository = new NonUnityJsonDataRepository();
            repository.Load(LocateDataDirectory());

            DomainEmperorMechanicSystem system = new DomainEmperorMechanicSystem();

            EmperorMechanicEffect qin = system.BuildPlaystyleEffect(repository.GetEmperor("qin_shi_huang"));
            EmperorMechanicEffect liuBang = system.BuildPlaystyleEffect(repository.GetEmperor("liu_bang"));
            EmperorMechanicEffect hanWu = system.BuildPlaystyleEffect(repository.GetEmperor("han_wu_di"));

            Assert.Equal("integration_standardization", qin.primaryEffectId);
            Assert.Contains("standardization", qin.effectTags);
            Assert.Contains("integration", qin.effectTags);
            Assert.True(qin.integrationDelta > 0);
            Assert.True(qin.successionRiskDelta > 0);
            Assert.Contains("六合同轨", qin.explanation);

            Assert.Equal("coalition_talent_absorption", liuBang.primaryEffectId);
            Assert.Contains("talent", liuBang.effectTags);
            Assert.Contains("local_compromise", liuBang.effectTags);
            Assert.True(liuBang.talentMultiplierDelta > 0f);
            Assert.True(liuBang.localPowerDelta < 0);
            Assert.Contains("布衣共天下", liuBang.explanation);

            Assert.Equal("frontier_expedition_pressure", hanWu.primaryEffectId);
            Assert.Contains("military", hanWu.effectTags);
            Assert.Contains("frontier", hanWu.effectTags);
            Assert.True(hanWu.armyAttackMultiplierDelta > 0f);
            Assert.True(hanWu.fiscalPressureDelta > 0);
            Assert.Contains("外朝远征", hanWu.explanation);

            HashSet<string> distinctPrimaryEffects = new HashSet<string>
            {
                qin.primaryEffectId,
                liuBang.primaryEffectId,
                hanWu.primaryEffectId
            };
            Assert.Equal(3, distinctPrimaryEffects.Count);
        }

        private static string LocateDataDirectory()
        {
            string baseDir = Path.GetDirectoryName(typeof(EmperorMechanicParityTests).GetTypeInfo().Assembly.Location);
            string current = baseDir;
            for (int i = 0; i < 10 && current != null; i++)
            {
                string candidate = Path.Combine(current, "web-strategy-map", "game-data-source", "data");
                if (Directory.Exists(candidate)) return candidate;
                current = Path.GetDirectoryName(current);
            }

            throw new DirectoryNotFoundException("web-strategy-map/game-data-source/data not found near " + baseDir);
        }
    }
}
