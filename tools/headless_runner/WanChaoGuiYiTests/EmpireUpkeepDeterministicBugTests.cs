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
        private const int TrialCount = 5;
        private readonly ITestOutputHelper output;

        public EmpireUpkeepDeterministicBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void Same_Initial_State_Must_Produce_Same_Economy_Outcome()
        {
            int[] moneyDeltas = new int[TrialCount];
            int[] foodDeltas = new int[TrialCount];
            for (int trial = 0; trial < TrialCount; trial++)
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
                moneyDeltas[trial] = moneyBefore - faction.money;
                foodDeltas[trial] = foodBefore - faction.food;
                output.WriteLine("trial " + trial + ": money delta=" + moneyDeltas[trial] + " food delta=" + foodDeltas[trial]);
            }

            for (int trial = 1; trial < TrialCount; trial++)
            {
                Assert.Equal(moneyDeltas[0], moneyDeltas[trial]);
                Assert.Equal(foodDeltas[0], foodDeltas[trial]);
            }
        }
    }
}
