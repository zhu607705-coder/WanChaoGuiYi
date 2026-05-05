using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class PolicyAI : MonoBehaviour
    {
        public string ChoosePolicy(GameContext context, FactionState faction)
        {
            EmperorDefinition emperor = context.Data.GetEmperor(faction.emperorId);
            if (emperor.preferredPolicies != null && emperor.preferredPolicies.Length > 0)
            {
                for (int i = 0; i < emperor.preferredPolicies.Length; i++)
                {
                    if (context.Data.Policies.ContainsKey(emperor.preferredPolicies[i]))
                    {
                        return emperor.preferredPolicies[i];
                    }
                }
            }

            foreach (PolicyDefinition policy in context.Data.Policies.Values)
            {
                return policy.id;
            }

            return string.Empty;
        }
    }
}
