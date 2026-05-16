using System.Collections.Generic;

namespace WanChaoGuiYi
{
    public sealed class MapGraphData : IMapGraphData
    {
        private readonly IDataRepository data;

        public MapGraphData(IDataRepository data)
        {
            this.data = data;
        }

        public bool AreNeighbors(string regionA, string regionB)
        {
            if (data == null || string.IsNullOrEmpty(regionA) || string.IsNullOrEmpty(regionB)) return false;

            RegionDefinition region = data.GetRegion(regionA);
            if (region == null || region.neighbors == null) return false;

            for (int i = 0; i < region.neighbors.Length; i++)
            {
                if (region.neighbors[i] == regionB) return true;
            }

            return false;
        }

        public IEnumerable<string> GetNeighbors(string regionId)
        {
            if (data == null || string.IsNullOrEmpty(regionId)) return new string[0];

            RegionDefinition region = data.GetRegion(regionId);
            return region != null && region.neighbors != null ? region.neighbors : new string[0];
        }
    }
}
