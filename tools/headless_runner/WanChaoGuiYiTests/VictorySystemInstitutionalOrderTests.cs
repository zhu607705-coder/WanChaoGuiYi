using Xunit;

namespace WanChaoGuiYi.Tests
{
    public sealed class VictorySystemInstitutionalOrderTests
    {
        [Fact]
        public void Institutional_Order_Field_Sources_Should_Expose_Treasury_Stability_And_Core_Reforms()
        {
            VictoryConditionDefinition condition = BuildInstitutionalOrderCondition();
            DomainVictorySystem system = new DomainVictorySystem();

            GameState blockedState = BuildInstitutionalState(3, 72, 54, 50);
            FactionState blockedFaction = blockedState.FindFaction("faction_player");

            VictoryEvaluationPayload blocked = system.EvaluateInstitutionalOrder(blockedState, blockedFaction, condition);

            Assert.False(blocked.achieved);
            Assert.Equal("institutional_order", blocked.victoryId);
            Assert.Equal(3, blocked.completedCoreReforms);
            Assert.Equal(4, blocked.requiredCoreReforms);
            Assert.Equal(54, blocked.treasuryStability);
            Assert.Equal(65, blocked.minTreasuryStability);
            Assert.Equal(50, blocked.maxObservedAnnexationPressure);
            Assert.Equal(45, blocked.maxAnnexationPressure);
            Assert.Contains("财政稳定", blocked.reason);

            GameState orderedState = BuildInstitutionalState(4, 74, 70, 36);
            FactionState orderedFaction = orderedState.FindFaction("faction_player");

            VictoryEvaluationPayload ordered = system.EvaluateInstitutionalOrder(orderedState, orderedFaction, condition);

            Assert.True(ordered.achieved);
            Assert.Equal("institutional_order", ordered.victoryId);
            Assert.Equal(4, ordered.completedCoreReforms);
            Assert.Equal(4, ordered.requiredCoreReforms);
            Assert.Equal(70, ordered.treasuryStability);
            Assert.Equal(65, ordered.minTreasuryStability);
            Assert.Equal(36, ordered.maxObservedAnnexationPressure);
            Assert.Equal(45, ordered.maxAnnexationPressure);
            Assert.Contains("制度胜利", ordered.reason);
        }

        private static VictoryConditionDefinition BuildInstitutionalOrderCondition()
        {
            return new VictoryConditionDefinition
            {
                id = "institutional_order",
                name = "制度胜利",
                requirements = new VictoryRequirement
                {
                    completedCoreReforms = 4,
                    minLegitimacy = 70,
                    minTreasuryStability = 65,
                    maxAnnexationPressure = 45
                }
            };
        }

        private static GameState BuildInstitutionalState(
            int completedReforms,
            int legitimacy,
            int treasuryStability,
            int annexationPressure)
        {
            GameState state = new GameState
            {
                turn = 32,
                year = 16,
                season = Season.Spring,
                playerFactionId = "faction_player"
            };

            FactionState player = new FactionState
            {
                id = "faction_player",
                name = "玩家王朝",
                emperorId = "qin_shi_huang",
                legitimacy = legitimacy,
                treasuryStability = treasuryStability,
                money = 600,
                food = 600
            };

            for (int i = 0; i < completedReforms; i++)
            {
                player.completedReformIds.Add("core_reform_" + i);
            }

            player.regionIds.Add("capital");
            player.regionIds.Add("frontier");

            state.factions.Add(player);
            state.regions.Add(BuildRegion("capital", player.id, annexationPressure));
            state.regions.Add(BuildRegion("frontier", player.id, annexationPressure - 4));

            return state;
        }

        private static RegionState BuildRegion(string id, string ownerFactionId, int annexationPressure)
        {
            return new RegionState
            {
                id = id,
                ownerFactionId = ownerFactionId,
                population = 100000,
                rebellionRisk = 8,
                localPower = 12,
                annexationPressure = annexationPressure,
                integration = 88,
                occupationStatus = OccupationStatus.Controlled,
                taxContributionPercent = 100,
                foodContributionPercent = 100
            };
        }
    }
}
