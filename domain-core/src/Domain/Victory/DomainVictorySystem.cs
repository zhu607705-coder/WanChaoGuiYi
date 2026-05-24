namespace WanChaoGuiYi
{
    public sealed class VictoryEvaluationPayload
    {
        public string victoryId;
        public bool achieved;
        public int fragmentationScore;
        public int maxFragmentation;
        public int completedCoreReforms;
        public int requiredCoreReforms;
        public int treasuryStability;
        public int minTreasuryStability;
        public int maxObservedAnnexationPressure;
        public int maxAnnexationPressure;
        public string reason;
    }

    public sealed class DomainVictorySystem
    {
        public VictoryEvaluationPayload EvaluateThreeGenerationDynasty(
            GameState state,
            FactionState faction,
            VictoryConditionDefinition condition)
        {
            VictoryRequirement requirements = condition != null && condition.requirements != null
                ? condition.requirements
                : new VictoryRequirement();

            VictoryEvaluationPayload payload = new VictoryEvaluationPayload
            {
                victoryId = condition != null && !string.IsNullOrEmpty(condition.id)
                    ? condition.id
                    : "three_generation_dynasty",
                maxFragmentation = requirements.maxFragmentation,
                fragmentationScore = CalculateFragmentationScore(state, faction)
            };

            if (state == null || faction == null)
            {
                payload.reason = "三代延续未达成：缺少局势或势力。";
                return payload;
            }

            if (requirements.stableSuccessions > 0 && faction.stableSuccessions < requirements.stableSuccessions)
            {
                payload.reason = "三代延续未达成：平稳继承次数不足。";
                return payload;
            }

            if (requirements.minLegitimacy > 0 && faction.legitimacy < requirements.minLegitimacy)
            {
                payload.reason = "三代延续未达成：合法性不足。";
                return payload;
            }

            if (requirements.maxFragmentation > 0 && payload.fragmentationScore > requirements.maxFragmentation)
            {
                payload.reason = "三代延续未达成：分裂度" + payload.fragmentationScore +
                                 "高于上限" + requirements.maxFragmentation + "。";
                return payload;
            }

            payload.achieved = true;
            payload.reason = "三代延续达成：三代传承、法统和分裂度均达标。";
            return payload;
        }

        public VictoryEvaluationPayload EvaluateInstitutionalOrder(
            GameState state,
            FactionState faction,
            VictoryConditionDefinition condition)
        {
            VictoryRequirement requirements = condition != null && condition.requirements != null
                ? condition.requirements
                : new VictoryRequirement();

            VictoryEvaluationPayload payload = new VictoryEvaluationPayload
            {
                victoryId = condition != null && !string.IsNullOrEmpty(condition.id)
                    ? condition.id
                    : "institutional_order",
                requiredCoreReforms = requirements.completedCoreReforms,
                minTreasuryStability = requirements.minTreasuryStability,
                maxAnnexationPressure = requirements.maxAnnexationPressure,
                completedCoreReforms = CountCompletedCoreReforms(faction),
                treasuryStability = faction != null ? DomainMath.Clamp(faction.treasuryStability, 0, 100) : 0,
                maxObservedAnnexationPressure = CalculateMaxObservedAnnexationPressure(state, faction)
            };

            if (state == null || faction == null)
            {
                payload.reason = "制度胜利未达成：缺少局势或势力。";
                return payload;
            }

            string blockers = BuildInstitutionalOrderBlockers(faction, requirements, payload);
            if (!string.IsNullOrEmpty(blockers))
            {
                payload.reason = "制度胜利未达成：" + blockers + "。";
                return payload;
            }

            payload.achieved = true;
            payload.reason = "制度胜利达成：核心改革、财政稳定、法统和土地压力均达标。";
            return payload;
        }

        public int CalculateFragmentationScore(GameState state, FactionState faction)
        {
            if (state == null || faction == null || faction.regionIds == null || faction.regionIds.Count == 0)
            {
                return 100;
            }

            int totalPressure = 0;
            int countedRegions = 0;

            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = state.FindRegion(faction.regionIds[i]);
                if (region == null || region.ownerFactionId != faction.id) continue;

                int disintegration = DomainMath.Clamp(100 - region.integration, 0, 100);
                int pressure = DomainMath.RoundToInt((
                    DomainMath.Clamp(region.rebellionRisk, 0, 100) +
                    DomainMath.Clamp(region.localPower, 0, 100) +
                    DomainMath.Clamp(region.annexationPressure, 0, 100) +
                    disintegration) / 4f);

                totalPressure += pressure;
                countedRegions++;
            }

            if (countedRegions == 0) return 100;
            return DomainMath.Clamp(DomainMath.RoundToInt(totalPressure / (float)countedRegions), 0, 100);
        }

        private static int CountCompletedCoreReforms(FactionState faction)
        {
            if (faction == null || faction.completedReformIds == null) return 0;

            System.Collections.Generic.HashSet<string> uniqueReformIds =
                new System.Collections.Generic.HashSet<string>(System.StringComparer.Ordinal);
            for (int i = 0; i < faction.completedReformIds.Count; i++)
            {
                string reformId = faction.completedReformIds[i];
                if (string.IsNullOrWhiteSpace(reformId)) continue;
                uniqueReformIds.Add(reformId.Trim());
            }

            return uniqueReformIds.Count;
        }

        private static int CalculateMaxObservedAnnexationPressure(GameState state, FactionState faction)
        {
            if (state == null || faction == null || faction.regionIds == null || faction.regionIds.Count == 0)
            {
                return 100;
            }

            int maxPressure = 0;
            bool found = false;
            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = state.FindRegion(faction.regionIds[i]);
                if (region == null || region.ownerFactionId != faction.id) continue;

                int pressure = DomainMath.Clamp(region.annexationPressure, 0, 100);
                if (!found || pressure > maxPressure)
                {
                    maxPressure = pressure;
                    found = true;
                }
            }

            return found ? maxPressure : 100;
        }

        private static string BuildInstitutionalOrderBlockers(
            FactionState faction,
            VictoryRequirement requirements,
            VictoryEvaluationPayload payload)
        {
            string reason = "";
            AppendBlocker(
                ref reason,
                requirements.completedCoreReforms > 0 && payload.completedCoreReforms < requirements.completedCoreReforms,
                "核心改革" + payload.completedCoreReforms + "/" + requirements.completedCoreReforms);
            AppendBlocker(
                ref reason,
                requirements.minLegitimacy > 0 && faction.legitimacy < requirements.minLegitimacy,
                "合法性" + faction.legitimacy + "/" + requirements.minLegitimacy);
            AppendBlocker(
                ref reason,
                requirements.minTreasuryStability > 0 && payload.treasuryStability < requirements.minTreasuryStability,
                "财政稳定" + payload.treasuryStability + "/" + requirements.minTreasuryStability);
            AppendBlocker(
                ref reason,
                requirements.maxAnnexationPressure > 0 && payload.maxObservedAnnexationPressure > requirements.maxAnnexationPressure,
                "兼并压力" + payload.maxObservedAnnexationPressure + "/" + requirements.maxAnnexationPressure);
            return reason;
        }

        private static void AppendBlocker(ref string reason, bool blocked, string text)
        {
            if (!blocked) return;
            if (!string.IsNullOrEmpty(reason)) reason += "，";
            reason += text;
        }
    }
}
