using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class FactionSystem : MonoBehaviour, IGameSystem
    {
        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            for (int i = 0; i < context.State.factions.Count; i++)
            {
                FactionState faction = context.State.factions[i];
                int pressure = faction.regionIds.Count > 5 ? 1 : 0;
                if (faction.legitimacy < 40) pressure += 2;
                faction.courtFactionPressure = ClampPercent(faction.courtFactionPressure + pressure);
            }
        }

        private static int ClampPercent(int value)
        {
            if (value < 0) return 0;
            if (value > 100) return 100;
            return value;
        }
    }
}
