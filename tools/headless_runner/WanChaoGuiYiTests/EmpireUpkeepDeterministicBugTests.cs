using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: same starting state should produce
    /// the same turn outcome. With Dictionary iteration order issues
    /// addressed, this should hold — but other sources of
    /// non-determinism (Random instances inside systems, dictionary
    /// hash randomisation between processes) could regress later.
    ///
    /// Pinned invariant: running ExecuteTurn twice on two identically
    /// constructed states must produce identical (faction.money,
    /// faction.food) deltas.
    /// </summary>
    public sealed class EmpireUpkeepDeterministicBugTests
    {
        private readonly ITestOutputHelper output;

        public EmpireUpkeepDeterministicBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void Same_Initial_State_Must_Produce_Same_Economy_Outcome()
        {
            int[] outcomes = new int[4];
            for (int trial = 0; trial < 2; trial++)
            {
                FakeDataRepository data;
                GameState state = TestFixtures.BuildSinglePlayerWorld(5, out data);
                data.UnitMap["infantry"] = new UnitDefinition
                {
                    id = "infantry", name = "Infantry",
                    stats = new UnitStats { attack = 10, defense = 10, mobility = 1, siege = 0 },
                    upkeep = new CostSet { money = 5, food = 5 }
                };
                FactionState faction = state.factions[0];
                for (int i = 0; i < 5; i++)
                {
                    state.armies.Add(new ArmyState
                    {
                        id = "a" + i,
                        ownerFactionId = faction.id,
                        regionId = "r" + i,
                        unitId = "infantry",
                        soldiers = 1000,
                        morale = 70
                    });
                }
                int moneyBefore = faction.money;
                int foodBefore = faction.food;
                new DomainEconomySystem(null).ExecuteTurn(TestFixtures.BuildContext(state, data));
                outcomes[trial * 2] = moneyBefore - faction.money;
                outcomes[trial * 2 + 1] = foodBefore - faction.food;
            }
            output.WriteLine("trial 0: money delta=" + outcomes[0] + " food delta=" + outcomes[1]);
            output.WriteLine("trial 1: money delta=" + outcomes[2] + " food delta=" + outcomes[3]);
            Assert.Equal(outcomes[0], outcomes[2]);
            Assert.Equal(outcomes[1], outcomes[3]);
        }
    }
}
