using Xunit;
using System.IO;
using System.Reflection;

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

        [Fact]
        public void Institutional_Order_Should_Count_Unique_Core_Reforms_Only()
        {
            VictoryConditionDefinition condition = BuildInstitutionalOrderCondition();
            DomainVictorySystem system = new DomainVictorySystem();

            GameState state = BuildInstitutionalStateWithReformIds(
                new string[] { "central_reform", "central_reform", "fiscal_order", "fiscal_order", "" },
                74,
                70,
                36);
            FactionState faction = state.FindFaction("faction_player");

            VictoryEvaluationPayload payload = system.EvaluateInstitutionalOrder(state, faction, condition);

            Assert.False(payload.achieved);
            Assert.Equal("institutional_order", payload.victoryId);
            Assert.Equal(2, payload.completedCoreReforms);
            Assert.Equal(4, payload.requiredCoreReforms);
            Assert.Contains("核心改革", payload.reason);
        }

        [Fact]
        public void Institutional_Order_Should_Use_Repository_Victory_Condition_Thresholds()
        {
            NonUnityJsonDataRepository repository = new NonUnityJsonDataRepository();
            repository.Load(LocateDataDirectory());
            VictoryConditionDefinition condition = repository.VictoryConditions["institutional_order"];
            DomainVictorySystem system = new DomainVictorySystem();

            GameState blockedState = BuildInstitutionalStateWithReformIds(
                new string[] { "central_reform", "fiscal_order", "audit_order" },
                condition.requirements.minLegitimacy,
                condition.requirements.minTreasuryStability,
                condition.requirements.maxAnnexationPressure);
            FactionState blockedFaction = blockedState.FindFaction("faction_player");

            VictoryEvaluationPayload blocked = system.EvaluateInstitutionalOrder(blockedState, blockedFaction, condition);

            Assert.False(blocked.achieved);
            Assert.Equal("institutional_order", blocked.victoryId);
            Assert.Equal(3, blocked.completedCoreReforms);
            Assert.Equal(condition.requirements.completedCoreReforms, blocked.requiredCoreReforms);
            Assert.Equal(condition.requirements.minTreasuryStability, blocked.minTreasuryStability);
            Assert.Equal(condition.requirements.maxAnnexationPressure, blocked.maxAnnexationPressure);
            Assert.Contains("核心改革", blocked.reason);

            GameState orderedState = BuildInstitutionalStateWithReformIds(
                new string[] { "central_reform", "fiscal_order", "audit_order", "law_code" },
                condition.requirements.minLegitimacy,
                condition.requirements.minTreasuryStability,
                condition.requirements.maxAnnexationPressure);
            FactionState orderedFaction = orderedState.FindFaction("faction_player");

            VictoryEvaluationPayload ordered = system.EvaluateInstitutionalOrder(orderedState, orderedFaction, condition);

            Assert.True(ordered.achieved);
            Assert.Equal(4, ordered.completedCoreReforms);
            Assert.Equal(condition.requirements.completedCoreReforms, ordered.requiredCoreReforms);
            Assert.Equal(condition.requirements.minTreasuryStability, ordered.treasuryStability);
            Assert.Equal(condition.requirements.maxAnnexationPressure, ordered.maxObservedAnnexationPressure);
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

        private static GameState BuildInstitutionalStateWithReformIds(
            string[] reformIds,
            int legitimacy,
            int treasuryStability,
            int annexationPressure)
        {
            GameState state = BuildInstitutionalState(0, legitimacy, treasuryStability, annexationPressure);
            FactionState player = state.FindFaction("faction_player");
            for (int i = 0; i < reformIds.Length; i++)
            {
                player.completedReformIds.Add(reformIds[i]);
            }

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

        private static string LocateDataDirectory()
        {
            string baseDir = Path.GetDirectoryName(typeof(VictorySystemInstitutionalOrderTests).GetTypeInfo().Assembly.Location);
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
