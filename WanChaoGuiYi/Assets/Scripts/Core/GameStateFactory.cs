using System.Collections.Generic;

namespace WanChaoGuiYi
{
    public static class GameStateFactory
    {
        public static GameState CreateDefault(DataRepository data, string playerFactionId)
        {
            GameState state = new GameState
            {
                turn = 1,
                year = 1,
                season = Season.Spring,
                playerFactionId = playerFactionId
            };

            CreateFactions(state, data);
            AssignRegions(state, data);
            state.AddLog("system", "新局初始化完成。");
            return state;
        }

        private static void CreateFactions(GameState state, DataRepository data)
        {
            foreach (EmperorDefinition emperor in data.Emperors.Values)
            {
                FactionState faction = new FactionState
                {
                    id = "faction_" + emperor.id,
                    name = emperor.name + "势力",
                    emperorId = emperor.id,
                    money = 300,
                    food = 400,
                    legitimacy = 60,
                    courtFactionPressure = 20,
                    successionRisk = 20,
                    stableSuccessions = 0,
                    heir = new HeirState
                    {
                        name = emperor.name + "继承人",
                        age = 18,
                        legitimacy = 55,
                        ability = 50
                    }
                };

                state.factions.Add(faction);
            }
        }

        private static void AssignRegions(GameState state, DataRepository data)
        {
            List<FactionState> factions = state.factions;
            if (factions.Count == 0) return;

            int index = 0;
            foreach (RegionDefinition definition in data.Regions.Values)
            {
                FactionState owner = factions[index % factions.Count];
                RegionState region = new RegionState
                {
                    id = definition.id,
                    ownerFactionId = owner.id,
                    population = definition.population,
                    foodOutput = definition.foodOutput,
                    taxOutput = definition.taxOutput,
                    manpower = definition.manpower,
                    localPower = definition.localPower,
                    rebellionRisk = definition.rebellionRisk,
                    integration = 70,
                    annexationPressure = CalculateInitialAnnexation(definition),
                    landStructure = definition.landStructure != null ? definition.landStructure.Clone() : new LandStructure(),
                    customs = DeriveInitialCustoms(data, definition.id),
                    customStability = 60
                };

                state.regions.Add(region);
                owner.regionIds.Add(region.id);
                index++;
            }
        }

        private static int CalculateInitialAnnexation(RegionDefinition definition)
        {
            if (definition.landStructure == null) return 30;
            return ClampToPercent((int)(definition.landStructure.localElites * 100f));
        }

        private static int ClampToPercent(int value)
        {
            if (value < 0) return 0;
            if (value > 100) return 100;
            return value;
        }

        private static string[] DeriveInitialCustoms(DataRepository data, string regionId)
        {
            // 从历史层数据推导初始风俗
            foreach (HistoricalLayerDefinition layer in data.HistoricalLayers.Values)
            {
                if (layer.regionId != regionId) continue;

                System.Collections.Generic.List<string> customs = new System.Collections.Generic.List<string>();

                if (layer.geographyTags != null)
                {
                    for (int i = 0; i < layer.geographyTags.Length; i++)
                    {
                        string tag = layer.geographyTags[i];
                        if (tag.Contains("frontier") || tag.Contains("steppe")) customs.Add("frontier");
                        if (tag.Contains("plain") || tag.Contains("basin")) customs.Add("agrarian");
                        if (tag.Contains("coast") || tag.Contains("trade")) customs.Add("mercantile");
                        if (tag.Contains("mountain")) customs.Add("martial");
                    }
                }

                if (layer.customTags != null)
                {
                    for (int i = 0; i < layer.customTags.Length; i++)
                    {
                        string tag = layer.customTags[i];
                        if (tag.Contains("scholar") || tag.Contains("confucian")) customs.Add("scholarly");
                        if (tag.Contains("multiethnic") || tag.Contains("market")) customs.Add("pluralistic");
                    }
                }

                if (customs.Count == 0) customs.Add("agrarian");
                return RemoveDuplicates(customs);
            }

            return new string[] { "agrarian" };
        }

        private static string[] RemoveDuplicates(System.Collections.Generic.List<string> list)
        {
            System.Collections.Generic.Dictionary<string, bool> seen = new System.Collections.Generic.Dictionary<string, bool>();
            System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>();

            for (int i = 0; i < list.Count; i++)
            {
                if (!seen.ContainsKey(list[i]))
                {
                    seen[list[i]] = true;
                    result.Add(list[i]);
                }
            }

            return result.ToArray();
        }
    }
}
