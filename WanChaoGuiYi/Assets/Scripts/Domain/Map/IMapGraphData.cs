using System.Collections.Generic;

namespace WanChaoGuiYi
{
    public interface IMapGraphData
    {
        bool AreNeighbors(string regionA, string regionB);
        IEnumerable<string> GetNeighbors(string regionId);
    }
}
