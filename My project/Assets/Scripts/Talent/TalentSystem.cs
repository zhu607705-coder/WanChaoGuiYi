using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class TalentSystem : MonoBehaviour, IGameSystem
    {
        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            for (int i = 0; i < context.State.factions.Count; i++)
            {
                FactionState faction = context.State.factions[i];
                if (faction.talentIds.Count > 0) continue;
                TryGrantStarterTalent(context, faction);
            }
        }

        private static void TryGrantStarterTalent(GameContext context, FactionState faction)
        {
            foreach (TalentDefinition talent in context.Data.Talents.Values)
            {
                faction.talentIds.Add(talent.id);
                context.State.AddLog("talent", faction.name + "获得人才：" + talent.name);
                return;
            }
        }
    }
}
