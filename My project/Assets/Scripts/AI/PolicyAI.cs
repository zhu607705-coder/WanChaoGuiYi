using UnityEngine;

namespace WanChaoGuiYi
{
    public sealed class PolicyAI : MonoBehaviour
    {
        public string ChoosePolicy(GameContext context, FactionState faction)
        {
            if (context == null || context.State == null || context.Data == null || faction == null)
            {
                return string.Empty;
            }

            EmperorDefinition emperor = context.Data.GetEmperor(faction.emperorId);
            string bestPolicyId = string.Empty;
            int bestScore = int.MinValue;

            foreach (PolicyDefinition policy in context.Data.Policies.Values)
            {
                if (!IsCandidatePolicy(faction, policy)) continue;

                int score = ScorePolicy(context, faction, emperor, policy);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestPolicyId = policy.id;
                }
            }

            return bestScore >= 12 ? bestPolicyId : string.Empty;
        }

        private static bool IsCandidatePolicy(FactionState faction, PolicyDefinition policy)
        {
            if (faction == null || policy == null) return false;
            if (faction.completedReformIds.Contains(policy.id)) return false;
            return CanPay(faction, policy.cost);
        }

        private static bool CanPay(FactionState faction, CostSet cost)
        {
            if (cost == null) return true;
            return faction.money >= cost.money &&
                   faction.food >= cost.food &&
                   faction.legitimacy >= cost.legitimacy;
        }

        private static int ScorePolicy(GameContext context, FactionState faction, EmperorDefinition emperor, PolicyDefinition policy)
        {
            int maxRebellion;
            int maxLocalPower;
            int lowAcceptanceCount;
            int occupationCount;
            ReadInternalPressure(context, faction, out maxRebellion, out maxLocalPower, out lowAcceptanceCount, out occupationCount);

            int score = 10;
            EffectSet effects = policy.effects;
            RiskSet risks = policy.risks;

            score += IsPreferredPolicy(emperor, policy.id) ? 24 : 0;
            score += ScoreCategoryAndTags(faction, emperor, policy);

            if (faction.money < 120)
            {
                score += Value(effects != null ? effects.money : 0, 5);
                score += Value(effects != null ? effects.taxEfficiency : 0, 3);
                score += Value(effects != null ? effects.taxBase : 0, 3);
                score -= Cost(policy.cost != null ? policy.cost.money : 0, 8);
            }

            if (faction.food < 160)
            {
                score += Value(effects != null ? effects.food : 0, 5);
                score -= Cost(policy.cost != null ? policy.cost.food : 0, 5);
                if (HasTag(policy, "relief")) score -= 10;
            }

            if (faction.legitimacy < 50)
            {
                score += Value(effects != null ? effects.legitimacy : 0, 5);
                score -= Cost(policy.cost != null ? policy.cost.legitimacy : 0, 3);
            }

            if (maxRebellion >= 45 || lowAcceptanceCount > 0)
            {
                score += Value(-(effects != null ? effects.rebellionRisk : 0), 4);
                score -= Value(risks != null ? risks.rebellionRisk : 0, 3);
                if (HasTag(policy, "relief") || HasTag(policy, "mandate")) score += 14;
            }

            if (maxLocalPower >= 55 || occupationCount > 0)
            {
                score += Value(-(effects != null ? effects.localPower : 0), 4);
                score += Value(effects != null ? effects.integrationSpeed : 0, 2);
                score -= Value(risks != null ? risks.localPower : 0, 3);
                score -= Value(risks != null ? risks.annexationPressure : 0, 2);
                if (HasTag(policy, "centralization") || HasTag(policy, "law")) score += 10;
            }

            if (faction.successionRisk >= 55 || faction.courtFactionPressure >= 55)
            {
                score += Value(-(effects != null ? effects.successionRisk : 0), 5);
                score += Value(-(effects != null ? effects.factionPressure : 0), 3);
                score -= Value(risks != null ? risks.successionRisk : 0, 4);
                score -= Value(risks != null ? risks.factionPressure : 0, 2);
                if (HasTag(policy, "talent")) score += 8;
            }

            if (IsInWar(context, faction) || (emperor != null && emperor.aiPersonality != null && emperor.aiPersonality.expansion >= 65))
            {
                score += Value(effects != null ? effects.armyMorale : 0, 4);
                score += Value(effects != null ? effects.manpowerToArmy : 0, 3);
                if (HasTag(policy, "military") || HasTag(policy, "frontier")) score += 12;
            }

            int riskBurden = CalculateRiskBurden(risks);
            int riskTolerance = emperor != null && emperor.aiPersonality != null ? emperor.aiPersonality.riskTolerance : 50;
            score -= riskBurden * DomainMath.Max(1, 75 - riskTolerance) / 75;

            int costBurden = CalculateCostBurden(policy.cost);
            score -= costBurden / 12;
            return score;
        }

        private static void ReadInternalPressure(GameContext context, FactionState faction,
            out int maxRebellion, out int maxLocalPower, out int lowAcceptanceCount, out int occupationCount)
        {
            maxRebellion = 0;
            maxLocalPower = 0;
            lowAcceptanceCount = 0;
            occupationCount = 0;

            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = context.State.FindRegion(faction.regionIds[i]);
                if (region == null) continue;

                if (region.rebellionRisk > maxRebellion) maxRebellion = region.rebellionRisk;
                if (region.localPower > maxLocalPower) maxLocalPower = region.localPower;
                if (region.localAcceptance > 0 && region.localAcceptance < 45) lowAcceptanceCount++;
                if (StrategyMapRulebook.ResolveControlStage(region) != ControlStage.Controlled) occupationCount++;
            }
        }

        private static int ScoreCategoryAndTags(FactionState faction, EmperorDefinition emperor, PolicyDefinition policy)
        {
            int score = 0;
            if (emperor != null && emperor.aiPersonality != null)
            {
                if (policy.category == "military") score += (emperor.aiPersonality.expansion - 50) / 3;
                if (policy.category == "domestic" || policy.category == "reform") score += (emperor.aiPersonality.governance - 50) / 3;
            }

            if (HasTag(policy, "relief") && faction.food >= 220) score += 8;
            if (HasTag(policy, "fiscal") && faction.money < 180) score += 10;
            if (HasTag(policy, "frontier") && faction.food >= 180) score += 8;
            return score;
        }

        private static bool IsPreferredPolicy(EmperorDefinition emperor, string policyId)
        {
            if (emperor == null || emperor.preferredPolicies == null || string.IsNullOrEmpty(policyId)) return false;
            for (int i = 0; i < emperor.preferredPolicies.Length; i++)
            {
                if (emperor.preferredPolicies[i] == policyId) return true;
            }
            return false;
        }

        private static bool HasTag(PolicyDefinition policy, string tag)
        {
            if (policy == null || policy.mechanicTags == null || string.IsNullOrEmpty(tag)) return false;
            for (int i = 0; i < policy.mechanicTags.Length; i++)
            {
                if (policy.mechanicTags[i] == tag) return true;
            }
            return false;
        }

        private static bool IsInWar(GameContext context, FactionState faction)
        {
            if (context == null || context.State == null || faction == null) return false;
            for (int i = 0; i < context.State.diplomaticRelations.Count; i++)
            {
                DiplomaticRelation relation = context.State.diplomaticRelations[i];
                if (relation.status != DiplomacyStatus.AtWar) continue;
                if (relation.factionA == faction.id || relation.factionB == faction.id) return true;
            }
            return false;
        }

        private static int CalculateRiskBurden(RiskSet risks)
        {
            if (risks == null) return 0;
            return DomainMath.Max(0, risks.rebellionRisk) +
                   DomainMath.Max(0, risks.corveePressure / 2) +
                   DomainMath.Max(0, risks.eliteAnger / 2) +
                   DomainMath.Max(0, risks.annexationPressure) +
                   DomainMath.Max(0, risks.factionPressure / 2) +
                   DomainMath.Max(0, risks.treasuryPressure);
        }

        private static int CalculateCostBurden(CostSet cost)
        {
            if (cost == null) return 0;
            return cost.money + cost.food + cost.manpower * 10 + cost.legitimacy * 15;
        }

        private static int Value(int value, int multiplier)
        {
            return value > 0 ? value * multiplier : 0;
        }

        private static int Cost(int value, int divisor)
        {
            return value > 0 && divisor > 0 ? value / divisor : 0;
        }
    }
}
