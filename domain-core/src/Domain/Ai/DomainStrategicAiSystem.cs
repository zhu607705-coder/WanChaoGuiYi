namespace WanChaoGuiYi
{
    public sealed class StrategicAiIntentPayload
    {
        public string intentId;
        public string targetRegionId;
        public int score;
        public string reason;
        public float expansionWeight;
        public int governancePressure;
        public int resourcePressure;
    }

    public sealed class DomainStrategicAiSystem
    {
        private const int RecoverResourcePressureThreshold = 60;
        private const int StabilizeGovernancePressureThreshold = 70;
        private const int ExpandWeightThreshold = 60;

        public StrategicAiIntentPayload SelectIntent(GameState state, FactionState faction, EmperorDefinition emperor, IDataRepository data)
        {
            StrategicAiIntentPayload payload = new StrategicAiIntentPayload
            {
                intentId = "recover",
                targetRegionId = string.Empty,
                reason = "资源休整：输入不足，无法形成进取或治理意图。"
            };

            if (state == null || faction == null)
            {
                payload.reason = "资源休整：缺少局势或势力。";
                return payload;
            }

            string pressureRegionId;
            string expansionTargetId;
            payload.expansionWeight = CalculateExpansionWeight(faction, emperor);
            payload.governancePressure = CalculateGovernancePressure(state, faction, out pressureRegionId);
            payload.resourcePressure = CalculateResourcePressure(faction);
            expansionTargetId = SelectExpansionTarget(state, faction, data);

            if (payload.resourcePressure >= RecoverResourcePressureThreshold)
            {
                payload.intentId = "recover";
                payload.targetRegionId = string.Empty;
                payload.score = payload.resourcePressure;
                payload.reason = "资源休整：金钱或粮食不足，暂停进取以恢复国力。";
                return payload;
            }

            if (payload.governancePressure >= StabilizeGovernancePressureThreshold)
            {
                payload.intentId = "stabilize";
                payload.targetRegionId = pressureRegionId ?? string.Empty;
                payload.score = payload.governancePressure;
                payload.reason = "治理整顿：叛乱、地方势力或兼并压力过高，优先稳住内地。";
                return payload;
            }

            if (!string.IsNullOrEmpty(expansionTargetId) && payload.expansionWeight >= ExpandWeightThreshold)
            {
                payload.intentId = "expand";
                payload.targetRegionId = expansionTargetId;
                payload.score = DomainMath.RoundToInt(payload.expansionWeight);
                payload.reason = "扩张进取：皇帝扩张倾向强且资源允许，边境存在可争取目标。";
                return payload;
            }

            payload.intentId = !string.IsNullOrEmpty(pressureRegionId) ? "stabilize" : "recover";
            payload.targetRegionId = !string.IsNullOrEmpty(pressureRegionId) ? pressureRegionId : string.Empty;
            payload.score = DomainMath.Max(payload.governancePressure, payload.resourcePressure);
            payload.reason = !string.IsNullOrEmpty(pressureRegionId)
                ? "治理整顿：缺少可靠扩张窗口，维持地方秩序。"
                : "资源休整：缺少相邻扩张目标，保留实力。";
            return payload;
        }

        private static float CalculateExpansionWeight(FactionState faction, EmperorDefinition emperor)
        {
            float expansion = emperor != null && emperor.aiPersonality != null ? emperor.aiPersonality.expansion : 0f;
            float riskTolerance = emperor != null && emperor.aiPersonality != null ? emperor.aiPersonality.riskTolerance : 0f;
            float resourceBonus = faction != null ? DomainMath.Clamp((faction.money + faction.food) / 120f, 0f, 12f) : 0f;
            return DomainMath.Clamp(expansion + riskTolerance / 5f + resourceBonus, 0f, 100f);
        }

        private static int CalculateResourcePressure(FactionState faction)
        {
            if (faction == null) return 100;
            int moneyPressure = DomainMath.Clamp(80 - faction.money, 0, 80);
            int foodPressure = DomainMath.Clamp(80 - faction.food, 0, 80);
            int combinedScarcity = faction.money < 50 && faction.food < 50 ? 20 : 0;
            return DomainMath.Clamp(DomainMath.Max(moneyPressure, foodPressure) + combinedScarcity, 0, 100);
        }

        private static int CalculateGovernancePressure(GameState state, FactionState faction, out string pressureRegionId)
        {
            pressureRegionId = string.Empty;
            if (state == null || faction == null || faction.regionIds == null) return 0;

            int bestPressure = DomainMath.Clamp(60 - faction.legitimacy, 0, 60);
            bestPressure += DomainMath.Max(faction.courtFactionPressure, faction.successionRisk) / 3;

            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = state.FindRegion(faction.regionIds[i]);
                if (region == null) continue;

                int pressure = region.rebellionRisk;
                pressure += region.localPower / 2;
                pressure += region.annexationPressure / 2;
                pressure += DomainMath.Clamp(55 - region.integration, 0, 55) / 2;

                if (pressure > bestPressure)
                {
                    bestPressure = pressure;
                    pressureRegionId = region.id;
                }
            }

            if (string.IsNullOrEmpty(pressureRegionId) && faction.regionIds.Count > 0)
            {
                pressureRegionId = faction.regionIds[0];
            }

            return DomainMath.Clamp(bestPressure, 0, 100);
        }

        private static string SelectExpansionTarget(GameState state, FactionState faction, IDataRepository data)
        {
            if (state == null || faction == null || faction.regionIds == null || data == null || data.Regions == null) return string.Empty;

            string bestTargetId = string.Empty;
            int bestScore = int.MinValue;

            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionDefinition ownedDefinition;
                if (!data.Regions.TryGetValue(faction.regionIds[i], out ownedDefinition) || ownedDefinition == null || ownedDefinition.neighbors == null) continue;

                for (int n = 0; n < ownedDefinition.neighbors.Length; n++)
                {
                    string neighborId = ownedDefinition.neighbors[n];
                    RegionState neighborState = state.FindRegion(neighborId);
                    if (neighborState == null || neighborState.ownerFactionId == faction.id) continue;

                    int targetScore = 100 - neighborState.rebellionRisk - neighborState.localPower / 2 - neighborState.annexationPressure / 2;
                    if (targetScore > bestScore)
                    {
                        bestScore = targetScore;
                        bestTargetId = neighborId;
                    }
                }
            }

            return bestTargetId;
        }
    }
}
