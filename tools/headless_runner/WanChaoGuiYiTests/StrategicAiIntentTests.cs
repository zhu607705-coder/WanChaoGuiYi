using Xunit;

namespace WanChaoGuiYi.Tests
{
    public sealed class StrategicAiIntentTests
    {
        [Fact]
        public void Strategic_Ai_Should_Select_Explainable_Intent_From_Personality_And_Pressure()
        {
            FakeDataRepository data = new FakeDataRepository();
            GameState state = BuildStrategicAiState(data);
            FactionState aiFaction = state.FindFaction("faction_ai");
            EmperorDefinition expansionEmperor = BuildEmperor("han_wu_di", 88, 42, 74);
            DomainStrategicAiSystem system = new DomainStrategicAiSystem();

            StrategicAiIntentPayload expansionIntent = system.SelectIntent(state, aiFaction, expansionEmperor, data);

            Assert.Equal("expand", expansionIntent.intentId);
            Assert.Equal("frontier_gate", expansionIntent.targetRegionId);
            Assert.True(expansionIntent.expansionWeight >= 60f);
            Assert.Contains("扩张", expansionIntent.reason);

            state.FindRegion("core").rebellionRisk = 78;
            state.FindRegion("core").localPower = 60;
            state.FindRegion("core").annexationPressure = 45;

            StrategicAiIntentPayload stabilizeIntent = system.SelectIntent(state, aiFaction, expansionEmperor, data);

            Assert.Equal("stabilize", stabilizeIntent.intentId);
            Assert.Equal("core", stabilizeIntent.targetRegionId);
            Assert.True(stabilizeIntent.governancePressure >= 70);
            Assert.Contains("治理", stabilizeIntent.reason);

            state.FindRegion("core").rebellionRisk = 12;
            state.FindRegion("core").localPower = 8;
            state.FindRegion("core").annexationPressure = 4;
            aiFaction.money = 24;
            aiFaction.food = 31;

            StrategicAiIntentPayload recoverIntent = system.SelectIntent(state, aiFaction, expansionEmperor, data);

            Assert.Equal("recover", recoverIntent.intentId);
            Assert.Equal(string.Empty, recoverIntent.targetRegionId);
            Assert.True(recoverIntent.resourcePressure >= 60);
            Assert.Contains("资源", recoverIntent.reason);
            Assert.Equal("faction_rival", state.FindRegion("frontier_gate").ownerFactionId);
        }

        private static EmperorDefinition BuildEmperor(string id, int expansion, int governance, int riskTolerance)
        {
            return new EmperorDefinition
            {
                id = id,
                name = id,
                aiPersonality = new AiPersonality
                {
                    expansion = expansion,
                    governance = governance,
                    riskTolerance = riskTolerance
                },
                stats = new EmperorStats()
            };
        }

        private static GameState BuildStrategicAiState(FakeDataRepository data)
        {
            data.RegionMap["core"] = new RegionDefinition
            {
                id = "core",
                name = "腹地",
                neighbors = new[] { "frontier_gate" },
                localPower = 8,
                rebellionRisk = 12
            };
            data.RegionMap["frontier_gate"] = new RegionDefinition
            {
                id = "frontier_gate",
                name = "边关",
                neighbors = new[] { "core" },
                localPower = 22,
                rebellionRisk = 18
            };

            GameState state = new GameState
            {
                turn = 1,
                year = 1,
                season = Season.Spring
            };
            FactionState aiFaction = new FactionState
            {
                id = "faction_ai",
                name = "AI Dynasty",
                emperorId = "han_wu_di",
                money = 420,
                food = 460,
                legitimacy = 72,
                courtFactionPressure = 18,
                successionRisk = 22
            };
            aiFaction.regionIds.Add("core");
            FactionState rivalFaction = new FactionState
            {
                id = "faction_rival",
                name = "Rival",
                money = 160,
                food = 160,
                legitimacy = 50
            };
            rivalFaction.regionIds.Add("frontier_gate");

            state.factions.Add(aiFaction);
            state.factions.Add(rivalFaction);
            state.regions.Add(new RegionState
            {
                id = "core",
                ownerFactionId = "faction_ai",
                localPower = 8,
                rebellionRisk = 12,
                annexationPressure = 4,
                integration = 92
            });
            state.regions.Add(new RegionState
            {
                id = "frontier_gate",
                ownerFactionId = "faction_rival",
                localPower = 22,
                rebellionRisk = 18,
                annexationPressure = 9,
                integration = 55
            });

            return state;
        }
    }
}
