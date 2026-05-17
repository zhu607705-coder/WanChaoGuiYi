using System;
using System.Collections.Generic;

namespace WanChaoGuiYi.Tests
{
    /// <summary>
    /// Minimal in-memory data repository so unit tests can construct a GameState
    /// without depending on JSON files. Each test composes only the rows it needs.
    /// </summary>
    internal sealed class FakeDataRepository : IDataRepository
    {
        public Dictionary<string, EmperorDefinition> EmperorMap { get; } = new Dictionary<string, EmperorDefinition>();
        public RegionDefinitionMap RegionMap { get; } = new RegionDefinitionMap();
        public Dictionary<string, HistoricalLayerDefinition> HistoricalLayerMap { get; } = new Dictionary<string, HistoricalLayerDefinition>();
        public Dictionary<string, PolicyDefinition> PolicyMap { get; } = new Dictionary<string, PolicyDefinition>();
        public Dictionary<string, EventDefinition> EventMap { get; } = new Dictionary<string, EventDefinition>();
        public Dictionary<string, TalentDefinition> TalentMap { get; } = new Dictionary<string, TalentDefinition>();
        public Dictionary<string, UnitDefinition> UnitMap { get; } = new Dictionary<string, UnitDefinition>();
        public Dictionary<string, TechnologyDefinition> TechnologyMap { get; } = new Dictionary<string, TechnologyDefinition>();
        public Dictionary<string, VictoryConditionDefinition> VictoryConditionMap { get; } = new Dictionary<string, VictoryConditionDefinition>();
        public Dictionary<string, GeneralDefinition> GeneralMap { get; } = new Dictionary<string, GeneralDefinition>();
        public Dictionary<string, BuildingDefinition> BuildingMap { get; } = new Dictionary<string, BuildingDefinition>();

        public IReadOnlyDictionary<string, EmperorDefinition> Emperors { get { return EmperorMap; } }
        public IReadOnlyDictionary<string, RegionDefinition> Regions { get { return RegionMap; } }
        public IReadOnlyDictionary<string, HistoricalLayerDefinition> HistoricalLayers { get { return HistoricalLayerMap; } }
        public IReadOnlyDictionary<string, PolicyDefinition> Policies { get { return PolicyMap; } }
        public IReadOnlyDictionary<string, EventDefinition> Events { get { return EventMap; } }
        public IReadOnlyDictionary<string, TalentDefinition> Talents { get { return TalentMap; } }
        public IReadOnlyDictionary<string, UnitDefinition> Units { get { return UnitMap; } }
        public IReadOnlyDictionary<string, TechnologyDefinition> Technologies { get { return TechnologyMap; } }
        public IReadOnlyDictionary<string, VictoryConditionDefinition> VictoryConditions { get { return VictoryConditionMap; } }
        public IReadOnlyDictionary<string, GeneralDefinition> Generals { get { return GeneralMap; } }
        public IReadOnlyDictionary<string, BuildingDefinition> Buildings { get { return BuildingMap; } }

        public EmperorDefinition GetEmperor(string id)
        {
            EmperorDefinition value;
            EmperorMap.TryGetValue(id, out value);
            return value;
        }

        public RegionDefinition GetRegion(string id)
        {
            RegionDefinition value;
            RegionMap.TryGetValue(id, out value);
            return value;
        }

        public PolicyDefinition GetPolicy(string id)
        {
            PolicyDefinition value;
            PolicyMap.TryGetValue(id, out value);
            return value;
        }

        public UnitDefinition GetUnit(string id)
        {
            UnitDefinition value;
            UnitMap.TryGetValue(id, out value);
            return value;
        }
    }

    internal sealed class RegionDefinitionMap : Dictionary<string, RegionDefinition>
    {
        public new RegionDefinition this[string key]
        {
            get { return base[key]; }
            set
            {
                Validate(key, value);
                base.Add(key, value);
            }
        }

        public new void Add(string key, RegionDefinition value)
        {
            Validate(key, value);
            base.Add(key, value);
        }

        private void Validate(string key, RegionDefinition value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("Region id is required.");
            }

            if (value == null || string.IsNullOrEmpty(value.id))
            {
                throw new InvalidOperationException("Region item is missing id.");
            }

            if (value.id != key)
            {
                throw new InvalidOperationException("Region map key does not match region id: " + key + " != " + value.id);
            }

            if (ContainsKey(key))
            {
                throw new InvalidOperationException("Duplicate region id: " + key);
            }
        }
    }

    internal static class TestFixtures
    {
        /// <summary>
        /// Builds a minimal GameState with one player faction and a chain of N
        /// linearly-connected regions all owned by that faction. No armies.
        /// Used for economy invariants that rely only on region count.
        /// </summary>
        public static GameState BuildSinglePlayerWorld(int regionCount, out FakeDataRepository data)
        {
            data = new FakeDataRepository();
            data.EmperorMap["test_player"] = new EmperorDefinition
            {
                id = "test_player",
                name = "Test",
                stats = new EmperorStats()
            };

            GameState state = new GameState
            {
                turn = 1,
                year = 1,
                season = Season.Spring,
                playerFactionId = "faction_test_player"
            };

            FactionState faction = new FactionState
            {
                id = "faction_test_player",
                name = "Test势力",
                emperorId = "test_player",
                money = 100000,
                food = 100000,
                legitimacy = 60,
                taxMultiplier = 1f,
                foodMultiplier = 1f,
                armyAttackMultiplier = 1f,
                armyDefenseMultiplier = 1f,
                talentMultiplier = 1f
            };
            state.factions.Add(faction);

            for (int i = 0; i < regionCount; i++)
            {
                string regionId = "r" + i;
                List<string> neighbors = new List<string>();
                if (i > 0) neighbors.Add("r" + (i - 1));
                if (i < regionCount - 1) neighbors.Add("r" + (i + 1));

                data.RegionMap[regionId] = new RegionDefinition
                {
                    id = regionId,
                    name = regionId,
                    population = 100000,
                    foodOutput = 0,
                    taxOutput = 0,
                    manpower = 50,
                    localPower = 0,
                    rebellionRisk = 0,
                    landStructure = new LandStructure
                    {
                        smallFarmers = 0.6f,
                        localElites = 0.1f,
                        stateLand = 0.2f,
                        religiousLand = 0.1f
                    },
                    legitimacyMemory = new[] { "civilian" },
                    neighbors = neighbors.ToArray()
                };
                RegionState region = new RegionState
                {
                    id = regionId,
                    ownerFactionId = faction.id,
                    population = 100000,
                    foodOutput = 0,
                    taxOutput = 0,
                    manpower = 50,
                    localPower = 0,
                    rebellionRisk = 0,
                    integration = 100,
                    occupationStatus = OccupationStatus.Controlled,
                    taxContributionPercent = 100,
                    foodContributionPercent = 100,
                    annexationPressure = 0,
                    landStructure = new LandStructure
                    {
                        smallFarmers = 0.6f,
                        localElites = 0.1f,
                        stateLand = 0.2f,
                        religiousLand = 0.1f
                    },
                    customs = new[] { "agrarian" },
                    customStability = 60
                };
                state.regions.Add(region);
                faction.regionIds.Add(regionId);
            }

            return state;
        }

        public static GameContext BuildContext(GameState state, IDataRepository data)
        {
            return new GameContext(state, data, new EventBus());
        }

        public static WorldState BuildWorldState(GameState state, IDataRepository data)
        {
            return WorldStateFactory.Create(state, data);
        }
    }
}
