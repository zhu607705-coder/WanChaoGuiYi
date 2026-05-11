using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class SuccessionSystem : MonoBehaviour, IGameSystem
    {
        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            for (int i = 0; i < context.State.factions.Count; i++)
            {
                FactionState faction = context.State.factions[i];
                faction.successionRisk = Mathf.Clamp(faction.successionRisk + CalculateRiskDrift(faction), 0, 100);

                if (faction.heir != null)
                {
                    faction.heir.age = faction.heir.age + (context.State.season == Season.Autumn ? 1 : 0);
                }
            }
        }

        public void ResolveSuccession(GameContext context, string factionId)
        {
            FactionState faction = context.State.FindFaction(factionId);
            if (faction == null || faction.heir == null) return;

            int stability = faction.heir.legitimacy + faction.heir.ability - faction.successionRisk;
            if (stability >= 70)
            {
                faction.stableSuccessions += 1;
                faction.legitimacy = Mathf.Min(100, faction.legitimacy + 5);
            }
            else
            {
                faction.legitimacy = Mathf.Max(0, faction.legitimacy - 15);
                faction.courtFactionPressure = Mathf.Min(100, faction.courtFactionPressure + 20);
            }

            faction.successionRisk = 20;
            context.Events.Publish(new GameEvent(GameEventType.SuccessionResolved, factionId, faction));
        }

        private static int CalculateRiskDrift(FactionState faction)
        {
            int drift = 1;
            if (faction.courtFactionPressure > 60) drift += 2;
            if (faction.legitimacy < 40) drift += 2;
            return drift;
        }
    }
}
