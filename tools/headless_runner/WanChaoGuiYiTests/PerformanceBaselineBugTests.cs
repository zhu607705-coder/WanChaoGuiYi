using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Performance baselines for Domain Core hot paths.
    ///
    /// Pinned invariants:
    ///   1. CalculateBattlePower on a typical army is sub-millisecond.
    ///   2. 10,000 sequential CalculateBattlePower calls complete
    ///      in under 200ms (i.e. < 20μs each).
    ///   3. ChangeRegionOwner on a state with 100 regions completes
    ///      in under 1ms.
    ///   4. WorldStateFactory.Create on a state with 100 regions
    ///      and 10 armies completes in under 50ms.
    ///
    /// Generous numbers, but they catch O(n²) regressions and
    /// accidental allocations in hot paths.
    /// </summary>
    public sealed class PerformanceBaselineBugTests
    {
        private readonly ITestOutputHelper output;

        public PerformanceBaselineBugTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void CalculateBattlePower_Hot_Loop_Is_Fast()
        {
            ArmyState army = new ArmyState { id = "a", soldiers = 1000, morale = 70 };
            UnitDefinition unit = new UnitDefinition
            {
                id = "u",
                stats = new UnitStats { attack = 10, defense = 10, mobility = 1, siege = 0 },
                upkeep = new CostSet()
            };
            FactionState faction = new FactionState
            {
                id = "f",
                taxMultiplier = 1f, foodMultiplier = 1f,
                armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f
            };

            // Warm up
            for (int i = 0; i < 100; i++)
            {
                NumericFormulas.CalculateBattlePower(army, unit, faction, default(EquipmentBonus), true);
            }

            Stopwatch sw = Stopwatch.StartNew();
            int sum = 0;
            for (int i = 0; i < 10_000; i++)
            {
                sum += NumericFormulas.CalculateBattlePower(army, unit, faction, default(EquipmentBonus), true);
            }
            sw.Stop();

            output.WriteLine("10,000 calls took " + sw.ElapsedMilliseconds + "ms (sum=" + sum + ")");
            Assert.True(sw.ElapsedMilliseconds < 200,
                "10,000 CalculateBattlePower calls took " + sw.ElapsedMilliseconds + "ms; budget is 200ms.");
        }

        [Fact]
        public void ChangeRegionOwner_On_Large_State_Is_Sub_Millisecond()
        {
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(100, out data);
            data.EmperorMap["enemy"] = new EmperorDefinition { id = "enemy", name = "E", stats = new EmperorStats() };
            FactionState enemy = new FactionState
            {
                id = "faction_enemy", name = "E", emperorId = "enemy",
                taxMultiplier = 1f, foodMultiplier = 1f,
                armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f
            };
            state.factions.Add(enemy);

            // Warm up
            state.ChangeRegionOwner("r0", enemy.id);
            state.ChangeRegionOwner("r0", state.factions[0].id);

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                string target = i % 2 == 0 ? enemy.id : state.factions[0].id;
                string regionId = "r" + (i % 100);
                state.ChangeRegionOwner(regionId, target);
            }
            sw.Stop();
            output.WriteLine("100 ChangeRegionOwner on 100-region state: " + sw.ElapsedMilliseconds + "ms");
            Assert.True(sw.ElapsedMilliseconds < 100,
                "100 ChangeRegionOwner calls took " + sw.ElapsedMilliseconds + "ms; budget 100ms (1ms each).");
        }

        [Fact]
        public void WorldStateFactory_On_Large_State_Is_Fast()
        {
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(100, out data);
            data.UnitMap["infantry"] = new UnitDefinition
            {
                id = "infantry",
                name = "Infantry",
                stats = new UnitStats { attack = 10, defense = 10, mobility = 1, siege = 0 },
                upkeep = new CostSet()
            };
            FactionState faction = state.factions[0];
            for (int i = 0; i < 10; i++)
            {
                state.armies.Add(new ArmyState
                {
                    id = "army_" + i,
                    ownerFactionId = faction.id,
                    regionId = "r" + (i * 10),
                    unitId = "infantry",
                    soldiers = 1000,
                    morale = 70
                });
            }

            // Warm up
            WorldStateFactory.Create(state, data);

            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 10; i++)
            {
                WorldStateFactory.Create(state, data);
            }
            sw.Stop();

            output.WriteLine("10 WorldStateFactory.Create on 100-region/10-army state: " + sw.ElapsedMilliseconds + "ms");
            Assert.True(sw.ElapsedMilliseconds < 500,
                "10 builds took " + sw.ElapsedMilliseconds + "ms; budget 500ms (50ms each).");
        }

        [Fact]
        public void Memory_No_Excessive_Allocation_Per_Turn()
        {
            // Crude but useful: take a memory snapshot before and
            // after running 100 economy turns, and assert delta is
            // bounded.
            FakeDataRepository data;
            GameState state = TestFixtures.BuildSinglePlayerWorld(20, out data);
            data.UnitMap["infantry"] = new UnitDefinition
            {
                id = "infantry",
                name = "Infantry",
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
            DomainEconomySystem economy = new DomainEconomySystem(null);
            GameContext context = TestFixtures.BuildContext(state, data);

            // Warm up + force GC.
            for (int i = 0; i < 5; i++) economy.ExecuteTurn(context);
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
            long before = System.GC.GetTotalMemory(true);

            for (int i = 0; i < 100; i++) economy.ExecuteTurn(context);
            long after = System.GC.GetTotalMemory(false);

            long deltaKb = (after - before) / 1024;
            output.WriteLine("100 economy turns allocated approximately " + deltaKb + " KB (turnLog included).");

            // Bound: 100 turns should not allocate more than 5 MB
            // residual (after-before, post-warmup). turnLog cap kicks
            // in at 2000 entries so memory is bounded by design.
            Assert.True(deltaKb < 5_000,
                "100 economy turns allocated " + deltaKb + " KB residual; budget 5 MB.");
        }
    }
}
