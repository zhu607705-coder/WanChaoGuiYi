using System.IO;
using System.Reflection;
using Xunit;

namespace WanChaoGuiYi.Tests
{
    public sealed class TalentSystemParityTests
    {
        [Fact]
        public void Talent_System_Should_Recruit_And_Appoint_Talent_With_Political_Cost()
        {
            NonUnityJsonDataRepository repository = new NonUnityJsonDataRepository();
            repository.Load(LocateDataDirectory());

            TalentDefinition talent = repository.Talents["land_reform_official"];
            FactionState faction = new FactionState
            {
                id = "faction_player",
                name = "Test Dynasty",
                courtFactionPressure = 10
            };
            RegionState region = new RegionState
            {
                id = "guanzhong",
                annexationPressure = 42,
                landStructure = new LandStructure()
            };

            DomainTalentSystem system = new DomainTalentSystem();

            TalentRecruitmentPayload firstRecruitment = system.RecruitTalent(faction, talent);
            TalentRecruitmentPayload duplicateRecruitment = system.RecruitTalent(faction, talent);

            Assert.True(firstRecruitment.recruited);
            Assert.Equal("land_reform_official", firstRecruitment.talentId);
            Assert.Contains("land_reform_official", faction.talentIds);
            Assert.False(duplicateRecruitment.recruited);
            Assert.Equal("already_recruited", duplicateRecruitment.reason);
            Assert.Single(faction.talentIds);

            int pressureBeforeAppointment = faction.courtFactionPressure;
            TalentAppointmentPayload appointment = system.ApplyTalentToRegion(faction, region, talent);

            Assert.True(appointment.applied);
            Assert.Equal("land_reform_official", appointment.talentId);
            Assert.Equal(42, appointment.annexationPressureBefore);
            Assert.Equal(36, appointment.annexationPressureAfter);
            Assert.Equal(36, region.annexationPressure);
            Assert.True(faction.courtFactionPressure > pressureBeforeAppointment);
            Assert.Equal(5, appointment.politicalPressureDelta);
            Assert.Contains("清丈能吏", appointment.explanation);
        }

        private static string LocateDataDirectory()
        {
            string baseDir = Path.GetDirectoryName(typeof(TalentSystemParityTests).GetTypeInfo().Assembly.Location);
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
