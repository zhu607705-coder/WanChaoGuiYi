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
                    landStructure = definition.landStructure != null ? definition.landStructure.Clone() : new LandStructure()
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
    }
}
