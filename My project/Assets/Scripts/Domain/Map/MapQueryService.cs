using System.Collections.Generic;

namespace WanChaoGuiYi
{
    public sealed class MapQueryService
    {
        private readonly MapState mapState;
        private readonly IMapGraphData mapGraph;

        public MapQueryService(MapState mapState, IMapGraphData mapGraph)
        {
            this.mapState = mapState;
            this.mapGraph = mapGraph;
        }

        public RegionRuntimeState GetRegion(string regionId)
        {
            RegionRuntimeState region;
            return mapState != null && mapState.TryGetRegion(regionId, out region) ? region : null;
        }

        public ArmyRuntimeState GetArmy(string armyId)
        {
            ArmyRuntimeState army;
            return mapState != null && mapState.TryGetArmy(armyId, out army) ? army : null;
        }

        public List<ArmyRuntimeState> GetArmiesInRegion(string regionId)
        {
            return mapState != null ? mapState.GetArmiesInRegion(regionId) : new List<ArmyRuntimeState>();
        }

        public List<ArmyRuntimeState> GetHostileArmies(string regionId, string factionId)
        {
            return mapState != null ? mapState.GetHostileArmiesInRegion(regionId, factionId) : new List<ArmyRuntimeState>();
        }

        public IEnumerable<string> GetNeighborRegions(string regionId)
        {
            return mapGraph != null ? mapGraph.GetNeighbors(regionId) : new string[0];
        }

        public bool AreNeighbors(string regionA, string regionB)
        {
            return mapGraph != null && mapGraph.AreNeighbors(regionA, regionB);
        }

        public bool HasRoute(string startRegionId, string targetRegionId)
        {
            return FindRoute(startRegionId, targetRegionId).Count > 0;
        }

        public bool TryGetEngagementInRegion(string regionId, out EngagementRuntimeState engagement)
        {
            engagement = null;
            return mapState != null && mapState.TryGetEngagementInRegion(regionId, out engagement);
        }

        public List<string> FindRoute(string startRegionId, string targetRegionId)
        {
            List<string> route = new List<string>();
            if (string.IsNullOrEmpty(startRegionId) || string.IsNullOrEmpty(targetRegionId))
            {
                return route;
            }

            if (startRegionId == targetRegionId)
            {
                route.Add(startRegionId);
                return route;
            }

            Queue<string> frontier = new Queue<string>();
            Dictionary<string, string> cameFrom = new Dictionary<string, string>();
            frontier.Enqueue(startRegionId);
            cameFrom[startRegionId] = null;

            while (frontier.Count > 0)
            {
                string current = frontier.Dequeue();
                if (current == targetRegionId) break;

                foreach (string neighbor in GetNeighborRegions(current))
                {
                    if (cameFrom.ContainsKey(neighbor)) continue;
                    cameFrom[neighbor] = current;
                    frontier.Enqueue(neighbor);
                }
            }

            if (!cameFrom.ContainsKey(targetRegionId))
            {
                return route;
            }

            string step = targetRegionId;
            while (step != null)
            {
                route.Insert(0, step);
                cameFrom.TryGetValue(step, out step);
            }

            return route;
        }
    }
}
