using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace WanChaoGuiYi
{
    public sealed class NonUnityJsonDataRepository : IDataRepository
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true
        };

        private readonly Dictionary<string, EmperorDefinition> emperors = new Dictionary<string, EmperorDefinition>();
        private readonly Dictionary<string, RegionDefinition> regions = new Dictionary<string, RegionDefinition>();
        private readonly Dictionary<string, HistoricalLayerDefinition> historicalLayers = new Dictionary<string, HistoricalLayerDefinition>();
        private readonly Dictionary<string, PolicyDefinition> policies = new Dictionary<string, PolicyDefinition>();
        private readonly Dictionary<string, EventDefinition> events = new Dictionary<string, EventDefinition>();
        private readonly Dictionary<string, TalentDefinition> talents = new Dictionary<string, TalentDefinition>();
        private readonly Dictionary<string, UnitDefinition> units = new Dictionary<string, UnitDefinition>();
        private readonly Dictionary<string, TechnologyDefinition> technologies = new Dictionary<string, TechnologyDefinition>();
        private readonly Dictionary<string, VictoryConditionDefinition> victoryConditions = new Dictionary<string, VictoryConditionDefinition>();
        private readonly Dictionary<string, GeneralDefinition> generals = new Dictionary<string, GeneralDefinition>();
        private readonly Dictionary<string, BuildingDefinition> buildings = new Dictionary<string, BuildingDefinition>();

        public IReadOnlyDictionary<string, EmperorDefinition> Emperors { get { return emperors; } }
        public IReadOnlyDictionary<string, RegionDefinition> Regions { get { return regions; } }
        public IReadOnlyDictionary<string, HistoricalLayerDefinition> HistoricalLayers { get { return historicalLayers; } }
        public IReadOnlyDictionary<string, PolicyDefinition> Policies { get { return policies; } }
        public IReadOnlyDictionary<string, EventDefinition> Events { get { return events; } }
        public IReadOnlyDictionary<string, TalentDefinition> Talents { get { return talents; } }
        public IReadOnlyDictionary<string, UnitDefinition> Units { get { return units; } }
        public IReadOnlyDictionary<string, TechnologyDefinition> Technologies { get { return technologies; } }
        public IReadOnlyDictionary<string, VictoryConditionDefinition> VictoryConditions { get { return victoryConditions; } }
        public IReadOnlyDictionary<string, GeneralDefinition> Generals { get { return generals; } }
        public IReadOnlyDictionary<string, BuildingDefinition> Buildings { get { return buildings; } }

        public void Load(string dataDirectory)
        {
            if (string.IsNullOrEmpty(dataDirectory)) throw new ArgumentException("Data directory is required.", "dataDirectory");
            if (!Directory.Exists(dataDirectory)) throw new DirectoryNotFoundException("Data directory not found: " + dataDirectory);

            emperors.Clear();
            regions.Clear();
            historicalLayers.Clear();
            policies.Clear();
            events.Clear();
            talents.Clear();
            units.Clear();
            technologies.Clear();
            victoryConditions.Clear();
            generals.Clear();
            buildings.Clear();

            Register(LoadTable<EmperorTable>(dataDirectory, "emperors").items, emperors, "emperor");
            Register(LoadTable<RegionTable>(dataDirectory, "regions").items, regions, "region");
            Register(LoadTable<HistoricalLayerTable>(dataDirectory, "historical_layers").items, historicalLayers, "historical layer");
            Register(LoadTable<PolicyTable>(dataDirectory, "policies").items, policies, "policy");
            Register(LoadTable<EventTable>(dataDirectory, "events").items, events, "event");
            Register(LoadTable<TalentTable>(dataDirectory, "talents").items, talents, "talent");
            Register(LoadTable<UnitTable>(dataDirectory, "units").items, units, "unit");
            Register(LoadTable<TechnologyTable>(dataDirectory, "technologies").items, technologies, "technology");
            Register(LoadTable<VictoryConditionTable>(dataDirectory, "victory_conditions").items, victoryConditions, "victory condition");
            Register(LoadTable<GeneralTable>(dataDirectory, "generals").items, generals, "general");
            Register(LoadTable<BuildingTable>(dataDirectory, "buildings").items, buildings, "building");
        }

        public EmperorDefinition GetEmperor(string id)
        {
            return GetById(emperors, id, "emperor");
        }

        public RegionDefinition GetRegion(string id)
        {
            return GetById(regions, id, "region");
        }

        public PolicyDefinition GetPolicy(string id)
        {
            return GetById(policies, id, "policy");
        }

        public UnitDefinition GetUnit(string id)
        {
            return GetById(units, id, "unit");
        }

        private static T LoadTable<T>(string dataDirectory, string tableName)
        {
            string path = Path.Combine(dataDirectory, tableName + ".json");
            if (!File.Exists(path)) throw new FileNotFoundException("Missing JSON table: " + tableName, path);

            string json = File.ReadAllText(path);
            T table = JsonSerializer.Deserialize<T>(json, JsonOptions);
            if (table == null) throw new InvalidOperationException("Failed to parse JSON table: " + tableName);
            return table;
        }

        private static void Register<T>(T[] items, Dictionary<string, T> target, string label) where T : class
        {
            if (items == null) throw new InvalidOperationException("JSON table has no items for " + label);

            for (int i = 0; i < items.Length; i++)
            {
                string id = ExtractId(items[i]);
                if (string.IsNullOrWhiteSpace(id)) throw new InvalidOperationException("Missing id in " + label + " at index " + i);
                if (target.ContainsKey(id)) throw new InvalidOperationException("Duplicate " + label + " id: " + id);
                target.Add(id, items[i]);
            }
        }

        private static string ExtractId(object item)
        {
            EmperorDefinition emperor = item as EmperorDefinition;
            if (emperor != null) return emperor.id;

            RegionDefinition region = item as RegionDefinition;
            if (region != null) return region.id;

            HistoricalLayerDefinition historicalLayer = item as HistoricalLayerDefinition;
            if (historicalLayer != null) return historicalLayer.id;

            PolicyDefinition policy = item as PolicyDefinition;
            if (policy != null) return policy.id;

            EventDefinition eventDefinition = item as EventDefinition;
            if (eventDefinition != null) return eventDefinition.id;

            TalentDefinition talent = item as TalentDefinition;
            if (talent != null) return talent.id;

            UnitDefinition unit = item as UnitDefinition;
            if (unit != null) return unit.id;

            TechnologyDefinition technology = item as TechnologyDefinition;
            if (technology != null) return technology.id;

            VictoryConditionDefinition victory = item as VictoryConditionDefinition;
            if (victory != null) return victory.id;

            GeneralDefinition general = item as GeneralDefinition;
            if (general != null) return general.id;

            BuildingDefinition building = item as BuildingDefinition;
            if (building != null) return building.id;

            return string.Empty;
        }

        private static T GetById<T>(Dictionary<string, T> source, string id, string label)
        {
            T value;
            if (!source.TryGetValue(id, out value)) throw new KeyNotFoundException("Unknown " + label + " id: " + id);
            return value;
        }
    }
}
