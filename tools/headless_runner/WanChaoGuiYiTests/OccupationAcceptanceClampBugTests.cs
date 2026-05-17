using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: DomainGovernanceImpactSystem.ApplyOccupationImpact
    /// hard-codes an 18-point acceptance penalty:
    ///     runtimeRegion.localAcceptance = DomainMath.Max(0, runtimeRegion.localAcceptance - 18);
    /// This is the wrong shape for two reasons:
    ///   1. It applies the same nominal hit regardless of the region's
    ///      starting acceptance, so a high-loyalty region (90) and a
    ///      borderline region (15) end up at 72 and 0 respectively —
    ///      the borderline region's "shock" is over-punished into the
    ///      floor and then becomes indistinguishable from any other
    ///      region that starts at, say, 5.
    ///   2. There is no constant for it (no NumericTuning entry, no
    ///      StrategyCausalRules.OccupationAcceptanceShock).  Any future
    ///      tuning change has to grep the codebase for the literal 18.
    ///
    /// Pinned invariants for this test:
    ///   A) Occupation acceptance shock should not over-saturate at 0.
    ///      Two regions starting at 5 and 25 should end at distinct
    ///      values (today they both end at 0 vs 7).
    ///   B) The penalty should live in NumericTuning or
    ///      StrategyCausalRules so tests/UI can reference it by name.
    /// </summary>
    public sealed class OccupationAcceptanceClampBugTests
    {
        private readonly ITestOutputHelper output;

        public OccupationAcceptanceClampBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void Occupation_Acceptance_Shock_Must_Be_Proportional_Not_FloorClamped()
        {
            FakeDataRepository data = BuildMinimalData();
            GameState state = BuildTwoRegionWorld(data);
            FactionState attacker = state.factions[0];
            FactionState defender = state.factions[1];

            RegionState regionLow = state.FindRegion("low");
            RegionState regionHigh = state.FindRegion("high");

            // Arrange known acceptance values.  Both regions become
            // owned by `attacker` to mimic a brand-new occupation.
            regionLow.ownerFactionId = attacker.id;
            regionHigh.ownerFactionId = attacker.id;
            attacker.regionIds.Add("low");
            attacker.regionIds.Add("high");
            defender.regionIds.Clear();

            regionLow.localAcceptance = 5;
            regionHigh.localAcceptance = 25;

            WorldState world = WorldStateFactory.Create(state, data);
            GameContext context = new GameContext(state, data, new EventBus());

            world.Map.RegionsById["low"].localAcceptance = 5;
            world.Map.RegionsById["high"].localAcceptance = 25;
            world.Map.RegionsById["low"].ownerFactionId = attacker.id;
            world.Map.RegionsById["high"].ownerFactionId = attacker.id;

            DomainGovernanceImpactSystem governance = new DomainGovernanceImpactSystem();
            governance.ApplyOccupationImpact(context, world.Map, "low", 0);
            governance.ApplyOccupationImpact(context, world.Map, "high", 0);

            int acceptanceLow = world.Map.RegionsById["low"].localAcceptance;
            int acceptanceHigh = world.Map.RegionsById["high"].localAcceptance;

            output.WriteLine("acceptance after occupation, region(low started at 5):  " + acceptanceLow);
            output.WriteLine("acceptance after occupation, region(high started at 25): " + acceptanceHigh);

            // Pinned invariant A: the high-acceptance region should be
            // strictly more accepting than the low one after the same
            // shock, even after clamping.  Today both can collapse to
            // a single value (0 vs 7) because the penalty is a flat
            // -18 with a Max(0, ...) clamp.
            //
            // The desired property: distance between the two should
            // not be smaller than min(starting_distance, half-shock).
            // Stated in absolute terms: the two values should differ
            // by at least 5 (a generous lower bound; today the gap
            // between 0 and 7 is 7, which technically passes — but
            // when both starting values fall below the shock, they
            // collapse to 0 and the gap is 0).
            //
            // Use a stronger second case where both start at <= 18.
            int secondLow = 1;
            int secondHigh = 17;
            world.Map.RegionsById["low"].localAcceptance = secondLow;
            world.Map.RegionsById["high"].localAcceptance = secondHigh;
            // Mark them as not-yet-occupied to retrigger the impact.
            world.Map.RegionsById["low"].occupationStatus = OccupationStatus.Controlled;
            world.Map.RegionsById["high"].occupationStatus = OccupationStatus.Controlled;
            governance.ApplyOccupationImpact(context, world.Map, "low", 0);
            governance.ApplyOccupationImpact(context, world.Map, "high", 0);

            int collapsedLow = world.Map.RegionsById["low"].localAcceptance;
            int collapsedHigh = world.Map.RegionsById["high"].localAcceptance;
            output.WriteLine("second pass low (start 1):   " + collapsedLow);
            output.WriteLine("second pass high (start 17): " + collapsedHigh);

            // Both starting values are below the 18-point flat shock.
            // With the current bug, both collapse to 0 — losing the
            // information that 'high' was 17x more loyal to start.
            Assert.NotEqual(collapsedLow, collapsedHigh);
        }

        private static FakeDataRepository BuildMinimalData()
        {
            FakeDataRepository data = new FakeDataRepository();
            data.EmperorMap["a"] = new EmperorDefinition { id = "a", name = "A", stats = new EmperorStats() };
            data.EmperorMap["d"] = new EmperorDefinition { id = "d", name = "D", stats = new EmperorStats() };
            data.RegionMap["low"] = MakeRegion("low");
            data.RegionMap["high"] = MakeRegion("high");
            return data;
        }

        private static RegionDefinition MakeRegion(string id)
        {
            return new RegionDefinition
            {
                id = id,
                name = id,
                landStructure = new LandStructure
                {
                    smallFarmers = 0.6f,
                    localElites = 0.1f,
                    stateLand = 0.2f,
                    religiousLand = 0.1f
                },
                legitimacyMemory = new[] { "civilian" },
                neighbors = new string[0]
            };
        }

        private static GameState BuildTwoRegionWorld(FakeDataRepository data)
        {
            GameState state = new GameState
            {
                turn = 1,
                year = 1,
                season = Season.Spring,
                playerFactionId = "faction_a"
            };
            FactionState attacker = new FactionState
            {
                id = "faction_a", name = "A", emperorId = "a",
                taxMultiplier = 1f, foodMultiplier = 1f,
                armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f
            };
            FactionState defender = new FactionState
            {
                id = "faction_d", name = "D", emperorId = "d",
                taxMultiplier = 1f, foodMultiplier = 1f,
                armyAttackMultiplier = 1f, armyDefenseMultiplier = 1f, talentMultiplier = 1f
            };
            state.factions.Add(attacker);
            state.factions.Add(defender);

            foreach (string id in new[] { "low", "high" })
            {
                state.regions.Add(new RegionState
                {
                    id = id,
                    ownerFactionId = defender.id,
                    integration = 100,
                    occupationStatus = OccupationStatus.Controlled,
                    taxContributionPercent = 100,
                    foodContributionPercent = 100,
                    landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                    customs = new[] { "agrarian" }
                });
                defender.regionIds.Add(id);
            }
            return state;
        }
    }
}
