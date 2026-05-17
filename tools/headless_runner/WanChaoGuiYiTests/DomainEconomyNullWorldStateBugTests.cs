using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: DomainEconomySystem accepts a null
    /// WorldState in its constructor (we already use this in
    /// EconomyDoubleUpkeepBugTests to skip runtime contribution
    /// adjustment). But the system doesn't document this seam, and
    /// half the methods call worldState.Map without a guard. Recent
    /// runs may have added more null-deref vectors.
    ///
    /// Pinned invariant: passing null WorldState must produce a
    /// turn that doesn't throw — even if contributions don't get
    /// runtime adjustments.
    /// </summary>
    public sealed class DomainEconomyNullWorldStateBugTests
    {
        private readonly ITestOutputHelper output;

        public DomainEconomyNullWorldStateBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void Null_WorldState_Must_Not_Throw_During_ExecuteTurn()
        {
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(3, out data);

            // Ensure we have an army to exercise the upkeep path.
            data.UnitMap["infantry"] = new UnitDefinition
            {
                id = "infantry", name = "Infantry",
                stats = new UnitStats { attack = 10, defense = 10, mobility = 1, siege = 0 },
                upkeep = new CostSet { money = 5, food = 5 }
            };
            state.armies.Add(new ArmyState
            {
                id = "a1",
                ownerFactionId = state.factions[0].id,
                regionId = "r0",
                unitId = "infantry",
                soldiers = 1000,
                morale = 70
            });
            // Also add an army with negative food to trigger grain shortage.
            state.factions[0].food = -5;

            DomainEconomySystem economy = new DomainEconomySystem(null);
            GameContext context = TestFixtures.BuildContext(state, data);

            System.Exception caught = null;
            try { economy.ExecuteTurn(context); }
            catch (System.Exception ex) { caught = ex; }

            output.WriteLine("threw? " + (caught == null ? "<no>" : caught.GetType().Name + ": " + caught.Message));
            Assert.Null(caught);
        }
    }
}
