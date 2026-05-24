namespace WanChaoGuiYi
{
    public sealed class VictoryEvaluationPayload
    {
        public string victoryId;
        public bool achieved;
        public int fragmentationScore;
        public int maxFragmentation;
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
    }
}
