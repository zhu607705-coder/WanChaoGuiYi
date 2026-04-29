using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WanChaoGuiYi
{
    public sealed class DataRepository : MonoBehaviour
    {
        [Header("JSON Tables")]
        [SerializeField] private TextAsset emperorsJson;
        [SerializeField] private TextAsset portraitsJson;
        [SerializeField] private TextAsset regionsJson;
        [SerializeField] private TextAsset mapRegionShapesJson;
        [SerializeField] private TextAsset historicalLayersJson;
        [SerializeField] private TextAsset policiesJson;
        [SerializeField] private TextAsset eventsJson;
        [SerializeField] private TextAsset chronicleEventsJson;
        [SerializeField] private TextAsset talentsJson;
        [SerializeField] private TextAsset unitsJson;
        [SerializeField] private TextAsset technologiesJson;
        [SerializeField] private TextAsset victoryConditionsJson;
        [SerializeField] private TextAsset generalsJson;
        [SerializeField] private TextAsset buildingsJson;

        private readonly Dictionary<string, EmperorDefinition> emperors = new Dictionary<string, EmperorDefinition>();
        private readonly Dictionary<string, PortraitDefinition> portraits = new Dictionary<string, PortraitDefinition>();
        private readonly Dictionary<string, RegionDefinition> regions = new Dictionary<string, RegionDefinition>();
        private readonly Dictionary<string, MapRegionShapeDefinition> mapRegionShapes = new Dictionary<string, MapRegionShapeDefinition>();
        private readonly Dictionary<string, MapRegionShapeDefinition> mapRegionShapesByRegionId = new Dictionary<string, MapRegionShapeDefinition>();
        private readonly Dictionary<string, HistoricalLayerDefinition> historicalLayers = new Dictionary<string, HistoricalLayerDefinition>();
        private readonly Dictionary<string, PolicyDefinition> policies = new Dictionary<string, PolicyDefinition>();
        private readonly Dictionary<string, EventDefinition> events = new Dictionary<string, EventDefinition>();
        private readonly Dictionary<string, ChronicleEventDefinition> chronicleEvents = new Dictionary<string, ChronicleEventDefinition>();
        private readonly Dictionary<string, TalentDefinition> talents = new Dictionary<string, TalentDefinition>();
        private readonly Dictionary<string, UnitDefinition> units = new Dictionary<string, UnitDefinition>();
        private readonly Dictionary<string, TechnologyDefinition> technologies = new Dictionary<string, TechnologyDefinition>();
        private readonly Dictionary<string, VictoryConditionDefinition> victoryConditions = new Dictionary<string, VictoryConditionDefinition>();
        private readonly Dictionary<string, GeneralDefinition> generals = new Dictionary<string, GeneralDefinition>();
        private readonly Dictionary<string, BuildingDefinition> buildings = new Dictionary<string, BuildingDefinition>();

        public IReadOnlyDictionary<string, EmperorDefinition> Emperors { get { return emperors; } }
        public IReadOnlyDictionary<string, PortraitDefinition> Portraits { get { return portraits; } }
        public IReadOnlyDictionary<string, RegionDefinition> Regions { get { return regions; } }
        public IReadOnlyDictionary<string, MapRegionShapeDefinition> MapRegionShapes { get { return mapRegionShapes; } }
        public IReadOnlyDictionary<string, MapRegionShapeDefinition> MapRegionShapesByRegionId { get { return mapRegionShapesByRegionId; } }
        public IReadOnlyDictionary<string, HistoricalLayerDefinition> HistoricalLayers { get { return historicalLayers; } }
        public IReadOnlyDictionary<string, PolicyDefinition> Policies { get { return policies; } }
        public IReadOnlyDictionary<string, EventDefinition> Events { get { return events; } }
        public IReadOnlyDictionary<string, ChronicleEventDefinition> ChronicleEvents { get { return chronicleEvents; } }
        public IReadOnlyDictionary<string, TalentDefinition> Talents { get { return talents; } }
        public IReadOnlyDictionary<string, UnitDefinition> Units { get { return units; } }
        public IReadOnlyDictionary<string, TechnologyDefinition> Technologies { get { return technologies; } }
        public IReadOnlyDictionary<string, VictoryConditionDefinition> VictoryConditions { get { return victoryConditions; } }
        public IReadOnlyDictionary<string, GeneralDefinition> Generals { get { return generals; } }
        public IReadOnlyDictionary<string, BuildingDefinition> Buildings { get { return buildings; } }

        public bool IsLoaded { get; private set; }

        public void Load()
        {
            emperors.Clear();
            portraits.Clear();
            regions.Clear();
            mapRegionShapes.Clear();
            mapRegionShapesByRegionId.Clear();
            historicalLayers.Clear();
            policies.Clear();
            events.Clear();
            chronicleEvents.Clear();
            talents.Clear();
            units.Clear();
            technologies.Clear();
            victoryConditions.Clear();
            generals.Clear();
            buildings.Clear();

            Register(LoadTable<EmperorTable>(emperorsJson, "emperors").items, emperors, "emperor");
            Register(LoadTable<PortraitTable>(portraitsJson, "portraits").items, portraits, "portrait");
            Register(LoadTable<RegionTable>(regionsJson, "regions").items, regions, "region");
            Register(LoadTable<MapRegionShapeTable>(mapRegionShapesJson, "map_region_shapes").items, mapRegionShapes, "map region shape");
            IndexMapRegionShapesByRegionId();
            Register(LoadTable<HistoricalLayerTable>(historicalLayersJson, "historical_layers").items, historicalLayers, "historical layer");
            Register(LoadTable<PolicyTable>(policiesJson, "policies").items, policies, "policy");
            Register(LoadTable<EventTable>(eventsJson, "events").items, events, "event");
            Register(LoadTable<ChronicleEventTable>(chronicleEventsJson, "chronicle_events").items, chronicleEvents, "chronicle event");
            Register(LoadTable<TalentTable>(talentsJson, "talents").items, talents, "talent");
            Register(LoadTable<UnitTable>(unitsJson, "units").items, units, "unit");
            Register(LoadTable<TechnologyTable>(technologiesJson, "technologies").items, technologies, "technology");
            Register(LoadTable<VictoryConditionTable>(victoryConditionsJson, "victory_conditions").items, victoryConditions, "victory condition");
            Register(LoadTable<GeneralTable>(generalsJson, "generals").items, generals, "general");
            Register(LoadTable<BuildingTable>(buildingsJson, "buildings").items, buildings, "building");

            ValidateRegionNeighbors();
            IsLoaded = true;
        }

        public EmperorDefinition GetEmperor(string id)
        {
            return GetById(emperors, id, "emperor");
        }

        public PortraitDefinition GetPortrait(string id)
        {
            return GetById(portraits, id, "portrait");
        }

        public RegionDefinition GetRegion(string id)
        {
            return GetById(regions, id, "region");
        }

        public MapRegionShapeDefinition GetMapRegionShape(string id)
        {
            return GetById(mapRegionShapes, id, "map region shape");
        }

        public bool TryGetMapRegionShapeByRegionId(string regionId, out MapRegionShapeDefinition shape)
        {
            return mapRegionShapesByRegionId.TryGetValue(regionId, out shape);
        }

        public HistoricalLayerDefinition GetHistoricalLayer(string id)
        {
            return GetById(historicalLayers, id, "historical layer");
        }

        public PolicyDefinition GetPolicy(string id)
        {
            return GetById(policies, id, "policy");
        }

        public ChronicleEventDefinition GetChronicleEvent(string id)
        {
            return GetById(chronicleEvents, id, "chronicle event");
        }

        public UnitDefinition GetUnit(string id)
        {
            return GetById(units, id, "unit");
        }

        public TechnologyDefinition GetTechnology(string id)
        {
            return GetById(technologies, id, "technology");
        }

        private static T LoadTable<T>(TextAsset asset, string label)
        {
            asset = ResolveTextAsset(asset, label);
            if (asset == null)
            {
                throw new InvalidOperationException("Missing JSON table: " + label);
            }

            T table = JsonUtility.FromJson<T>(asset.text);
            if (table == null)
            {
                throw new InvalidOperationException("Failed to parse JSON table: " + label);
            }

            return table;
        }

        private static TextAsset ResolveTextAsset(TextAsset assignedAsset, string label)
        {
            if (assignedAsset != null)
            {
                return assignedAsset;
            }

#if UNITY_EDITOR
            string path = "Assets/Data/" + label + ".json";
            return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
#else
            return null;
#endif
        }

        private static void Register<T>(T[] items, Dictionary<string, T> target, string label) where T : class
        {
            if (items == null)
            {
                throw new InvalidOperationException("JSON table has no items for " + label);
            }

            for (int i = 0; i < items.Length; i++)
            {
                string id = ExtractId(items[i]);
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new InvalidOperationException("Missing id in " + label + " at index " + i);
                }

                if (target.ContainsKey(id))
                {
                    throw new InvalidOperationException("Duplicate " + label + " id: " + id);
                }

                target.Add(id, items[i]);
            }
        }

        private static string ExtractId(object item)
        {
            EmperorDefinition emperor = item as EmperorDefinition;
            if (emperor != null) return emperor.id;

            PortraitDefinition portrait = item as PortraitDefinition;
            if (portrait != null) return portrait.id;

            RegionDefinition region = item as RegionDefinition;
            if (region != null) return region.id;

            MapRegionShapeDefinition mapRegionShape = item as MapRegionShapeDefinition;
            if (mapRegionShape != null) return mapRegionShape.id;

            HistoricalLayerDefinition historicalLayer = item as HistoricalLayerDefinition;
            if (historicalLayer != null) return historicalLayer.id;

            PolicyDefinition policy = item as PolicyDefinition;
            if (policy != null) return policy.id;

            EventDefinition eventDefinition = item as EventDefinition;
            if (eventDefinition != null) return eventDefinition.id;

            ChronicleEventDefinition chronicleEvent = item as ChronicleEventDefinition;
            if (chronicleEvent != null) return chronicleEvent.id;

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
            if (!source.TryGetValue(id, out value))
            {
                throw new KeyNotFoundException("Unknown " + label + " id: " + id);
            }

            return value;
        }

        private void IndexMapRegionShapesByRegionId()
        {
            foreach (MapRegionShapeDefinition shape in mapRegionShapes.Values)
            {
                if (shape == null || string.IsNullOrWhiteSpace(shape.regionId))
                {
                    throw new InvalidOperationException("Map region shape missing regionId");
                }

                if (!regions.ContainsKey(shape.regionId))
                {
                    throw new InvalidOperationException("Map region shape " + shape.id + " references missing region " + shape.regionId);
                }

                if (mapRegionShapesByRegionId.ContainsKey(shape.regionId))
                {
                    throw new InvalidOperationException("Duplicate map region shape for region: " + shape.regionId);
                }

                mapRegionShapesByRegionId.Add(shape.regionId, shape);
            }
        }

        private void ValidateRegionNeighbors()
        {
            foreach (RegionDefinition region in regions.Values)
            {
                if (region.neighbors == null) continue;

                for (int i = 0; i < region.neighbors.Length; i++)
                {
                    string neighborId = region.neighbors[i];
                    if (!regions.ContainsKey(neighborId))
                    {
                        throw new InvalidOperationException("Region " + region.id + " references missing neighbor " + neighborId);
                    }
                }
            }
        }
    }
}
