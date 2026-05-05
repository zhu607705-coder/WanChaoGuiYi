using System.Collections.Generic;

namespace WanChaoGuiYi
{
    public static class WorldStateFactory
    {
        public static WorldState Create(GameState gameState, IDataRepository data)
        {
            MapState mapState = new MapState();
            if (gameState == null)
            {
                return new WorldState(gameState, mapState);
            }

            for (int i = 0; i < gameState.regions.Count; i++)
            {
                RegionState region = gameState.regions[i];
                RegionDefinition definition = data != null ? data.GetRegion(region.id) : null;
                RegionRuntimeState runtimeRegion = new RegionRuntimeState
                {
                    id = region.id,
                    ownerFactionId = region.ownerFactionId,
                    occupationStatus = region.occupationStatus,
                    integration = region.integration,
                    taxContributionPercent = ResolveContributionPercent(region.taxContributionPercent, region.integration),
                    foodContributionPercent = ResolveContributionPercent(region.foodContributionPercent, region.integration),
                    rebellionRisk = region.rebellionRisk,
                    localPower = region.localPower,
                    annexationPressure = region.annexationPressure
                };
                StrategyMapRulebook.ApplyRuntimeDefaults(definition, region, runtimeRegion, data);
                mapState.AddRegion(runtimeRegion);
            }

            for (int i = 0; i < gameState.armies.Count; i++)
            {
                ArmyState army = gameState.armies[i];
                mapState.AddArmy(new ArmyRuntimeState
                {
                    id = army.id,
                    ownerFactionId = army.ownerFactionId,
                    locationRegionId = army.regionId,
                    targetRegionId = null,
                    route = new List<string>(),
                    task = ArmyTask.Idle,
                    unitId = army.unitId,
                    soldiers = army.soldiers,
                    morale = army.morale,
                    supply = 80,
                    movementPoints = ResolveMovementPoints(army, data),
                    engagementId = null
                });
            }

            return new WorldState(gameState, mapState);
        }

        private static int ResolveContributionPercent(int contributionPercent, int integration)
        {
            int value = contributionPercent > 0 ? contributionPercent : integration;
            if (value < 0) return 0;
            if (value > 100) return 100;
            return value;
        }

        private static int ResolveMovementPoints(ArmyState army, IDataRepository data)
        {
            if (army == null || data == null || string.IsNullOrEmpty(army.unitId)) return 1;

            UnitDefinition unit;
            if (!data.Units.TryGetValue(army.unitId, out unit) || unit == null || unit.stats == null)
            {
                return 1;
            }

            return unit.stats.mobility > 0 ? unit.stats.mobility : 1;
        }
    }
}
