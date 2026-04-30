using System;
using System.Collections.Generic;

namespace WanChaoGuiYi
{
    public static class GameStateFactory
    {
        public static GameState CreateDefault(IDataRepository data, string playerFactionId)
        {
            GameState state = new GameState
            {
                turn = 1,
                year = 1,
                season = Season.Spring,
                playerFactionId = playerFactionId
            };

            CreateFactions(state, data);
            FindRequiredPlayerFaction(state, playerFactionId);
            AssignRegions(state, data);
            CreateInitialArmies(state, data, playerFactionId);
            state.AddLog("system", "新局初始化完成。");
            return state;
        }

        private static void CreateFactions(GameState state, IDataRepository data)
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

        private static void AssignRegions(GameState state, IDataRepository data)
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
                    occupationStatus = OccupationStatus.Controlled,
                    taxContributionPercent = 70,
                    foodContributionPercent = 70,
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

        private static void CreateInitialArmies(GameState state, IDataRepository data, string playerFactionId)
        {
            if (state.factions.Count < 2 || data.Units.Count == 0 || data.Regions.Count == 0) return;

            FactionState playerFaction = FindRequiredPlayerFaction(state, playerFactionId);
            FactionState enemyFaction = FindFirstNonPlayerFaction(state, playerFaction.id);
            if (enemyFaction == null) return;

            string playerRegionId = ResolveFirstOwnedRegion(playerFaction, data);
            string enemyRegionId = ResolveFirstOwnedNeighbor(playerRegionId, enemyFaction, data);
            if (string.IsNullOrEmpty(enemyRegionId))
            {
                enemyRegionId = ResolveFirstOwnedRegion(enemyFaction, data);
            }

            string unitId = ResolveDefaultUnitId(data);
            if (string.IsNullOrEmpty(playerRegionId) || string.IsNullOrEmpty(enemyRegionId) || string.IsNullOrEmpty(unitId)) return;

            state.armies.Add(CreateArmy("army_player_1", playerFaction.id, playerRegionId, unitId, 3000, 70));
            state.armies.Add(CreateArmy("army_enemy_1", enemyFaction.id, enemyRegionId, unitId, 2600, 65));
            state.AddLog("war", "初始军队已部署：" + playerRegionId + " 与 " + enemyRegionId + "。");
        }

        private static FactionState FindRequiredPlayerFaction(GameState state, string playerFactionId)
        {
            if (string.IsNullOrEmpty(playerFactionId))
            {
                throw new InvalidOperationException("Player faction id is required.");
            }

            FactionState playerFaction = state.FindFaction(playerFactionId);
            if (playerFaction == null)
            {
                throw new InvalidOperationException("Unknown player faction id: " + playerFactionId);
            }

            return playerFaction;
        }

        private static FactionState FindFirstNonPlayerFaction(GameState state, string playerFactionId)
        {
            for (int i = 0; i < state.factions.Count; i++)
            {
                if (state.factions[i].id != playerFactionId) return state.factions[i];
            }

            return null;
        }

        private static string ResolveFirstOwnedRegion(FactionState faction, IDataRepository data)
        {
            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                if (data.Regions.ContainsKey(faction.regionIds[i])) return faction.regionIds[i];
            }

            return null;
        }

        private static string ResolveFirstOwnedNeighbor(string regionId, FactionState faction, IDataRepository data)
        {
            if (string.IsNullOrEmpty(regionId) || faction == null) return null;

            RegionDefinition region;
            if (!data.Regions.TryGetValue(regionId, out region) || region.neighbors == null) return null;

            for (int i = 0; i < region.neighbors.Length; i++)
            {
                if (faction.regionIds.Contains(region.neighbors[i])) return region.neighbors[i];
            }

            return null;
        }

        private static string ResolveDefaultUnitId(IDataRepository data)
        {
            if (data.Units.ContainsKey("infantry")) return "infantry";

            foreach (string unitId in data.Units.Keys)
            {
                return unitId;
            }

            return null;
        }

        private static ArmyState CreateArmy(string id, string ownerFactionId, string regionId, string unitId, int soldiers, int morale)
        {
            return new ArmyState
            {
                id = id,
                ownerFactionId = ownerFactionId,
                regionId = regionId,
                unitId = unitId,
                soldiers = soldiers,
                morale = morale,
                movementProgress = 0
            };
        }

        private static int ClampToPercent(int value)
        {
            if (value < 0) return 0;
            if (value > 100) return 100;
            return value;
        }

        private static string[] DeriveInitialCustoms(IDataRepository data, string regionId)
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
