using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class MilitaryAI : MonoBehaviour
    {
        public string ChooseExpansionTarget(GameContext context, FactionState faction)
        {
            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionDefinition owned = context.Data.GetRegion(faction.regionIds[i]);
                if (owned.neighbors == null) continue;

                for (int j = 0; j < owned.neighbors.Length; j++)
                {
                    RegionState neighbor = context.State.FindRegion(owned.neighbors[j]);
                    if (neighbor != null && neighbor.ownerFactionId != faction.id)
                    {
                        return neighbor.id;
                    }
                }
            }

            return string.Empty;
        }
    }
}
