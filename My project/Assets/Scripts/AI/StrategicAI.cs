using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class StrategicAI : MonoBehaviour, IGameSystem
    {
        private PolicyAI policyAI;
        private MilitaryAI militaryAI;
        private DiplomacyAI diplomacyAI;

        public void Initialize(GameContext context)
        {
            policyAI = GetComponent<PolicyAI>();
            if (policyAI == null) policyAI = gameObject.AddComponent<PolicyAI>();

            militaryAI = GetComponent<MilitaryAI>();
            if (militaryAI == null) militaryAI = gameObject.AddComponent<MilitaryAI>();

            diplomacyAI = GetComponent<DiplomacyAI>();
            if (diplomacyAI == null) diplomacyAI = gameObject.AddComponent<DiplomacyAI>();
        }

        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            for (int i = 0; i < context.State.factions.Count; i++)
            {
                FactionState faction = context.State.factions[i];
                if (faction.id == context.State.playerFactionId) continue;

                string policyId = policyAI.ChoosePolicy(context, faction);
                if (!string.IsNullOrEmpty(policyId))
                {
                    context.State.AddLog("ai", faction.name + "倾向执行政策：" + policyId);
                }

                string targetRegionId = militaryAI.ChooseExpansionTarget(context, faction);
                if (!string.IsNullOrEmpty(targetRegionId))
                {
                    context.State.AddLog("ai", faction.name + "关注扩张目标：" + targetRegionId);
                }

                diplomacyAI.ExecuteAIDiplomacy(context, faction);
            }
        }
    }
}
