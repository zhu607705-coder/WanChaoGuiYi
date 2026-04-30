using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class MapGraph : MonoBehaviour, IMapGraphData
    {
        private readonly Dictionary<string, HashSet<string>> adjacency = new Dictionary<string, HashSet<string>>();

        public void Build(DataRepository data)
        {
            adjacency.Clear();
            foreach (RegionDefinition region in data.Regions.Values)
            {
                if (!adjacency.ContainsKey(region.id))
                {
                    adjacency.Add(region.id, new HashSet<string>());
                }

                if (region.neighbors == null) continue;

                for (int i = 0; i < region.neighbors.Length; i++)
                {
                    adjacency[region.id].Add(region.neighbors[i]);
                }
            }
        }

        public bool AreNeighbors(string regionA, string regionB)
        {
            HashSet<string> neighbors;
            return adjacency.TryGetValue(regionA, out neighbors) && neighbors.Contains(regionB);
        }

        public IEnumerable<string> GetNeighbors(string regionId)
        {
            HashSet<string> neighbors;
            if (!adjacency.TryGetValue(regionId, out neighbors))
            {
                return new string[0];
            }

            return neighbors;
        }
    }
}
