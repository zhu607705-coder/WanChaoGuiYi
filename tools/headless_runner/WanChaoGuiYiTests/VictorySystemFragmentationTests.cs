using Xunit;

namespace WanChaoGuiYi.Tests
{
    public sealed class VictorySystemFragmentationTests
    {
        [Fact]
        public void Victory_System_Should_Block_Three_Generation_When_Fragmentation_Too_High()
        {
            VictoryConditionDefinition condition = BuildThreeGenerationCondition();
            DomainVictorySystem system = new DomainVictorySystem();

            GameState stableState = BuildDynastyState(3, 60, 6, 8, 4, 96);
            FactionState stableFaction = stableState.FindFaction("faction_player");

            VictoryEvaluationPayload stable = system.EvaluateThreeGenerationDynasty(stableState, stableFaction, condition);

            Assert.True(stable.achieved);
            Assert.Equal("three_generation_dynasty", stable.victoryId);
            Assert.Equal(10, stable.maxFragmentation);
            Assert.True(stable.fragmentationScore <= stable.maxFragmentation);
            Assert.Contains("三代", stable.reason);

            GameState dividedState = BuildDynastyState(3, 60, 72, 54, 42, 28);
            FactionState dividedFaction = dividedState.FindFaction("faction_player");

            VictoryEvaluationPayload divided = system.EvaluateThreeGenerationDynasty(dividedState, dividedFaction, condition);

            Assert.False(divided.achieved);
            Assert.Equal("three_generation_dynasty", divided.victoryId);
            Assert.Equal(10, divided.maxFragmentation);
            Assert.True(divided.fragmentationScore > divided.maxFragmentation);
            Assert.Contains("分裂度", divided.reason);
        }

        private static VictoryConditionDefinition BuildThreeGenerationCondition()
        {
            return new VictoryConditionDefinition
            {
                id = "three_generation_dynasty",
                name = "三代延续",
                requirements = new VictoryRequirement
                {
                    stableSuccessions = 3,
                    minLegitimacy = 50,
                    maxFragmentation = 10
                }
            };
        }

        private static GameState BuildDynastyState(
            int stableSuccessions,
            int legitimacy,
            int rebellionRisk,
            int localPower,
            int annexationPressure,
            int integration)
        {
            GameState state = new GameState
            {
                turn = 24,
                year = 12,
                season = Season.Autumn,
                playerFactionId = "faction_player"
            };

            FactionState player = new FactionState
            {
                id = "faction_player",
                name = "玩家王朝",
                emperorId = "qin_shi_huang",
                legitimacy = legitimacy,
                stableSuccessions = stableSuccessions,
                money = 500,
                food = 500
            };
            player.regionIds.Add("capital");
            player.regionIds.Add("frontier");

            state.factions.Add(player);
            state.regions.Add(BuildRegion("capital", player.id, rebellionRisk, localPower, annexationPressure, integration));
            state.regions.Add(BuildRegion("frontier", player.id, rebellionRisk, localPower, annexationPressure, integration));

            return state;
        }

        private static RegionState BuildRegion(
            string id,
            string ownerFactionId,
            int rebellionRisk,
            int localPower,
            int annexationPressure,
            int integration)
        {
            return new RegionState
            {
                id = id,
                ownerFactionId = ownerFactionId,
                population = 100000,
                rebellionRisk = rebellionRisk,
                localPower = localPower,
                annexationPressure = annexationPressure,
                integration = integration,
                occupationStatus = OccupationStatus.Controlled,
                taxContributionPercent = 100,
                foodContributionPercent = 100
            };
        }
    }
}
