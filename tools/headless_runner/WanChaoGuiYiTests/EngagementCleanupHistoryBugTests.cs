using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: DomainEngagementCleanup removes armies
    /// from engagement and clears the engagement when a side empties.
    /// But the cleanup path doesn't record any "engagement aborted
    /// without resolution" event or log line. Players watching the
    /// battle report can therefore see an engagement appear in turn
    /// N and silently disappear in turn N+1 with no after-action
    /// summary.
    ///
    /// Pinned invariant: when an engagement is cleared due to side
    /// emptying (not due to battle resolution), the turn log must
    /// contain an entry mentioning the engagement id or region id.
    /// </summary>
    public sealed class EngagementCleanupHistoryBugTests
    {
        private readonly ITestOutputHelper output;

        public EngagementCleanupHistoryBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void Engagement_Cleared_Without_Battle_Must_Leave_A_Log_Trace()
        {
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(2, out data);
            data.UnitMap["infantry"] = new UnitDefinition
            {
                id = "infantry", name = "Infantry",
                stats = new UnitStats { attack = 10, defense = 10, mobility = 1, siege = 0 },
                upkeep = new CostSet()
            };

            WorldState world = WorldStateFactory.Create(state, data);
            // Build a synthetic engagement and immediately remove the
            // attacker side, then run cleanup.
            EngagementRuntimeState eng = new EngagementRuntimeState
            {
                id = "engagement_solo",
                regionId = "r0",
                phase = EngagementPhase.Forming,
                createdTurn = 1
            };
            eng.attackerArmyIds.Add("ghost_attacker");
            eng.defenderArmyIds.Add("ghost_defender");
            world.Map.AddEngagement(eng);

            int logsBefore = state.turnLog.Count;
            DomainEngagementCleanup.RemoveArmyFromEngagement(world.Map, "engagement_solo", "ghost_attacker");
            DomainEngagementCleanup.ClearEngagementIfSideEmpty(world.Map, "engagement_solo");

            output.WriteLine("logs added: " + (state.turnLog.Count - logsBefore));
            for (int i = logsBefore; i < state.turnLog.Count; i++)
            {
                output.WriteLine("  " + state.turnLog[i].message);
            }

            // The cleanup must leave a trace — empty turn log means
            // the engagement vanished without a record.
            bool traceFound = false;
            for (int i = logsBefore; i < state.turnLog.Count; i++)
            {
                string msg = state.turnLog[i].message ?? "";
                if (msg.Contains("engagement_solo") || msg.Contains("r0") || msg.Contains("接敌") || msg.Contains("解散") || msg.Contains("清理"))
                {
                    traceFound = true;
                    break;
                }
            }
            Assert.True(traceFound,
                "Engagement was cleared without a single log entry; players cannot reconstruct what happened.");
        }
    }
}
