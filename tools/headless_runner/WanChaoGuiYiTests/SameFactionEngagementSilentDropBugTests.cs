using Xunit;
using Xunit.Abstractions;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Bug under investigation: when two armies of the SAME faction
    /// occupy the same region, DomainEngagementDetector.ClassifyArmies
    /// puts them all on the attacker side, then the
    ///     if (attackerArmyIds.Count == 0 || defenderArmyIds.Count == 0)
    ///         return null;
    /// branch silently drops the call without writing a log line or
    /// publishing any event.  The caller cannot distinguish "no
    /// engagement happened" from "two friendly stacks merged" — there is
    /// no observable signal at all.  The desired invariant: the system
    /// must either record a same-faction co-locate signal in the turn
    /// log or expose a return value that callers can branch on.
    /// </summary>
    public sealed class SameFactionEngagementSilentDropBugTests
    {
        private readonly ITestOutputHelper output;

        public SameFactionEngagementSilentDropBugTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void SameFaction_TwoArmies_InOneRegion_Must_Leave_Observable_Trace()
        {
            FakeDataRepository data = new FakeDataRepository();
            data.EmperorMap["p"] = new EmperorDefinition { id = "p", name = "P", stats = new EmperorStats() };
            data.UnitMap["infantry"] = new UnitDefinition
            {
                id = "infantry",
                name = "Infantry",
                stats = new UnitStats { attack = 10, defense = 10, mobility = 1, siege = 0 },
                upkeep = new CostSet { money = 0, food = 0 }
            };
            data.RegionMap["r0"] = new RegionDefinition
            {
                id = "r0",
                name = "r0",
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

            GameState state = new GameState
            {
                turn = 1,
                year = 1,
                season = Season.Spring,
                playerFactionId = "faction_p"
            };
            FactionState player = new FactionState
            {
                id = "faction_p",
                name = "P",
                emperorId = "p",
                taxMultiplier = 1f,
                foodMultiplier = 1f,
                armyAttackMultiplier = 1f,
                armyDefenseMultiplier = 1f,
                talentMultiplier = 1f
            };
            state.factions.Add(player);
            state.regions.Add(new RegionState
            {
                id = "r0",
                ownerFactionId = player.id,
                integration = 100,
                occupationStatus = OccupationStatus.Controlled,
                taxContributionPercent = 100,
                foodContributionPercent = 100,
                landStructure = new LandStructure { smallFarmers = 0.6f, localElites = 0.1f, stateLand = 0.2f, religiousLand = 0.1f },
                customs = new[] { "agrarian" }
            });
            player.regionIds.Add("r0");

            state.armies.Add(new ArmyState { id = "army_a", ownerFactionId = player.id, regionId = "r0", unitId = "infantry", soldiers = 1000, morale = 70 });
            state.armies.Add(new ArmyState { id = "army_b", ownerFactionId = player.id, regionId = "r0", unitId = "infantry", soldiers = 1000, morale = 70 });

            WorldState world = WorldStateFactory.Create(state, data);
            GameContext context = new GameContext(state, data, new EventBus());

            int logsBefore = state.turnLog.Count;
            int eventsObserved = 0;
            context.Events.Subscribe(GameEventType.ContactDetected, _ => eventsObserved++);
            context.Events.Subscribe(GameEventType.EngagementStarted, _ => eventsObserved++);

            DomainEngagementDetector detector = new DomainEngagementDetector();
            EngagementRuntimeState engagement = detector.DetectRegion(context, world.Map, "r0", "army_a");

            output.WriteLine("engagement returned: " + (engagement == null ? "<null>" : engagement.id));
            output.WriteLine("turn log entries added: " + (state.turnLog.Count - logsBefore));
            output.WriteLine("events observed: " + eventsObserved);

            // Two same-faction armies sharing a region is a real
            // gameplay event (stacks merging or arriving together).
            // The current implementation silently returns null, leaves
            // the turn log untouched, and publishes no events.  At
            // least one observable signal must exist; otherwise UIs and
            // logistics systems cannot react.
            bool observable = engagement != null
                || (state.turnLog.Count - logsBefore) > 0
                || eventsObserved > 0;

            Assert.True(observable, "Same-faction co-located armies left no engagement, no log, and no event.");
        }
    }
}
