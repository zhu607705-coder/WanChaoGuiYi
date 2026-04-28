using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class ReformSystem : MonoBehaviour, IGameSystem
    {
        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void ExecuteTurn(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public bool ApplyPolicy(GameContext context, string factionId, string policyId)
        {
            FactionState faction = context.State.FindFaction(factionId);
            PolicyDefinition policy = context.Data.GetPolicy(policyId);
            if (faction == null || policy == null) return false;

            if (!CanPay(faction, policy.cost)) return false;

            Pay(faction, policy.cost);
            ApplyEffects(faction, policy.effects);
            ApplyRisks(faction, policy.risks);
            context.Events.Publish(new GameEvent(GameEventType.PolicyApplied, factionId, policy));
            context.State.AddLog("policy", faction.name + "执行政策：" + policy.name);
            return true;
        }

        private static bool CanPay(FactionState faction, CostSet cost)
        {
            if (cost == null) return true;
            return faction.money >= cost.money && faction.food >= cost.food && faction.legitimacy >= cost.legitimacy;
        }

        private static void Pay(FactionState faction, CostSet cost)
        {
            if (cost == null) return;
            faction.money -= cost.money;
            faction.food -= cost.food;
            faction.legitimacy -= cost.legitimacy;
        }

        private static void ApplyEffects(FactionState faction, EffectSet effects)
        {
            if (effects == null) return;
            faction.legitimacy = ClampPercent(faction.legitimacy + effects.legitimacy);
            faction.courtFactionPressure = ClampPercent(faction.courtFactionPressure + effects.factionPressure);
            faction.money += effects.money;
            faction.food += effects.food;
            faction.successionRisk = ClampPercent(faction.successionRisk + effects.successionRisk);
        }

        private static void ApplyRisks(FactionState faction, RiskSet risks)
        {
            if (risks == null) return;
            faction.courtFactionPressure = ClampPercent(faction.courtFactionPressure + risks.factionPressure);
            faction.legitimacy = ClampPercent(faction.legitimacy - risks.treasuryPressure);
            faction.successionRisk = ClampPercent(faction.successionRisk + risks.successionRisk);
        }

        private static int ClampPercent(int value)
        {
            if (value < 0) return 0;
            if (value > 100) return 100;
            return value;
        }
    }
}
