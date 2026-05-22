namespace WanChaoGuiYi
{
    public sealed class SuccessionCrisisPayload
    {
        public bool triggered;
        public string factionId;
        public string regionId;
        public int successionRiskBefore;
        public int successionRiskAfter;
        public int legitimacyBefore;
        public int legitimacyAfter;
        public int courtPressureBefore;
        public int courtPressureAfter;
        public int rebellionRiskBefore;
        public int rebellionRiskAfter;
        public string reason;
    }

    public sealed class SuccessionStabilizationPayload
    {
        public bool applied;
        public string factionId;
        public int moneyBefore;
        public int moneyAfter;
        public int legitimacyBefore;
        public int legitimacyAfter;
        public int successionRiskBefore;
        public int successionRiskAfter;
        public int courtPressureBefore;
        public int courtPressureAfter;
        public int stableSuccessionsBefore;
        public int stableSuccessionsAfter;
        public string reason;
    }

    public sealed class DomainSuccessionSystem : IGameSystem
    {
        public const int CrisisRiskThreshold = 70;
        public const int CrisisPressureScoreThreshold = 95;
        public const int StabilizeMoneyCost = 90;
        public const int StabilizeLegitimacyCost = 2;
        public const int StabilizeSuccessionRiskReduction = 30;
        public const int StabilizeCourtPressureReduction = 22;

        public void Initialize(GameContext context) { }
        public void OnTurnStart(GameContext context) { }
        public void OnTurnEnd(GameContext context) { }

        public void ExecuteTurn(GameContext context)
        {
            if (context == null || context.State == null || context.State.factions == null) return;

            for (int i = 0; i < context.State.factions.Count; i++)
            {
                TryTriggerSuccessionCrisis(context, context.State.factions[i]);
            }
        }

        public SuccessionCrisisPayload TryTriggerSuccessionCrisis(GameContext context, FactionState faction)
        {
            SuccessionCrisisPayload payload = BuildCrisisPayload(context, faction);
            if (payload == null || !ShouldTriggerCrisis(faction, payload)) return payload;

            RegionState shockRegion = FindMostFragileOwnedRegion(context.State, faction);
            int rebellionBefore = shockRegion != null ? shockRegion.rebellionRisk : 0;
            int legitimacyCost = CalculateCrisisLegitimacyCost(faction);

            payload.triggered = true;
            payload.regionId = shockRegion != null ? shockRegion.id : null;
            payload.rebellionRiskBefore = rebellionBefore;

            faction.successionRisk = DomainMath.Clamp(faction.successionRisk + 6, 0, 100);
            faction.legitimacy = DomainMath.Clamp(faction.legitimacy - legitimacyCost, 0, 100);
            faction.courtFactionPressure = DomainMath.Clamp(faction.courtFactionPressure + 12, 0, 100);
            if (faction.heir != null)
            {
                faction.heir.legitimacy = DomainMath.Clamp(faction.heir.legitimacy - 5, 0, 100);
            }

            if (shockRegion != null)
            {
                shockRegion.rebellionRisk = DomainMath.Clamp(shockRegion.rebellionRisk + 9, 0, 100);
                shockRegion.localPower = DomainMath.Clamp(shockRegion.localPower + 4, 0, 100);
                shockRegion.annexationPressure = DomainMath.Clamp(shockRegion.annexationPressure + 3, 0, 100);
                shockRegion.localAcceptance = DomainMath.Clamp(shockRegion.localAcceptance - 5, 0, 100);
            }

            payload.successionRiskAfter = faction.successionRisk;
            payload.legitimacyAfter = faction.legitimacy;
            payload.courtPressureAfter = faction.courtFactionPressure;
            payload.rebellionRiskAfter = shockRegion != null ? shockRegion.rebellionRisk : 0;
            payload.reason = "继承风险" + payload.successionRiskBefore +
                             "，朝局压力" + payload.courtPressureBefore +
                             "，宗室承认不足，引发继承危机。";

            if (context.State != null)
            {
                context.State.AddLog("succession", faction.name + "继承危机：法统" +
                    payload.legitimacyBefore + "→" + payload.legitimacyAfter +
                    "，朝局" + payload.courtPressureBefore + "→" + payload.courtPressureAfter +
                    "，继承风险" + payload.successionRiskBefore + "→" + payload.successionRiskAfter +
                    (shockRegion != null ? "，" + shockRegion.id + "民变" + payload.rebellionRiskBefore + "→" + payload.rebellionRiskAfter : "") +
                    "。原因：扩张后的宗室和朝臣争位压力外溢。");
            }

            if (context.Events != null)
            {
                context.Events.Publish(new GameEvent(GameEventType.EventTriggered, faction.id, payload));
            }

            return payload;
        }

        public SuccessionStabilizationPayload StabilizeSuccession(GameContext context, FactionState faction)
        {
            SuccessionStabilizationPayload payload = new SuccessionStabilizationPayload();
            if (faction == null)
            {
                payload.reason = "missing_faction";
                return payload;
            }

            payload.factionId = faction.id;
            payload.moneyBefore = faction.money;
            payload.legitimacyBefore = faction.legitimacy;
            payload.successionRiskBefore = faction.successionRisk;
            payload.courtPressureBefore = faction.courtFactionPressure;
            payload.stableSuccessionsBefore = faction.stableSuccessions;

            if (faction.money < StabilizeMoneyCost || faction.legitimacy < StabilizeLegitimacyCost)
            {
                payload.reason = "not_enough_resources_for_succession_stabilization";
                payload.moneyAfter = faction.money;
                payload.legitimacyAfter = faction.legitimacy;
                payload.successionRiskAfter = faction.successionRisk;
                payload.courtPressureAfter = faction.courtFactionPressure;
                payload.stableSuccessionsAfter = faction.stableSuccessions;
                return payload;
            }

            faction.money = DomainMath.Max(0, faction.money - StabilizeMoneyCost);
            faction.legitimacy = DomainMath.Clamp(faction.legitimacy - StabilizeLegitimacyCost, 0, 100);
            faction.successionRisk = DomainMath.Clamp(faction.successionRisk - StabilizeSuccessionRiskReduction, 0, 100);
            faction.courtFactionPressure = DomainMath.Clamp(faction.courtFactionPressure - StabilizeCourtPressureReduction, 0, 100);
            if (faction.successionRisk <= CrisisRiskThreshold && faction.courtFactionPressure <= 75)
            {
                faction.stableSuccessions += 1;
            }

            payload.applied = true;
            payload.moneyAfter = faction.money;
            payload.legitimacyAfter = faction.legitimacy;
            payload.successionRiskAfter = faction.successionRisk;
            payload.courtPressureAfter = faction.courtFactionPressure;
            payload.stableSuccessionsAfter = faction.stableSuccessions;
            payload.reason = "立储安宗：支出金钱安抚宗室，牺牲少量法统换取继承风险和朝局压力下降。";

            if (context != null && context.State != null)
            {
                context.State.AddLog("succession", faction.name + "继承续命：金钱" +
                    payload.moneyBefore + "→" + payload.moneyAfter +
                    "，法统" + payload.legitimacyBefore + "→" + payload.legitimacyAfter +
                    "，继承风险" + payload.successionRiskBefore + "→" + payload.successionRiskAfter +
                    "，朝局" + payload.courtPressureBefore + "→" + payload.courtPressureAfter +
                    "。原因：立储、宗室安抚和朝臣分利改变危机走向。");
            }

            if (context != null && context.Events != null)
            {
                context.Events.Publish(new GameEvent(GameEventType.SuccessionResolved, faction.id, payload));
            }

            return payload;
        }

        private static SuccessionCrisisPayload BuildCrisisPayload(GameContext context, FactionState faction)
        {
            if (context == null || context.State == null || faction == null) return null;

            return new SuccessionCrisisPayload
            {
                triggered = false,
                factionId = faction.id,
                successionRiskBefore = faction.successionRisk,
                successionRiskAfter = faction.successionRisk,
                legitimacyBefore = faction.legitimacy,
                legitimacyAfter = faction.legitimacy,
                courtPressureBefore = faction.courtFactionPressure,
                courtPressureAfter = faction.courtFactionPressure,
                reason = "below_crisis_threshold"
            };
        }

        private static bool ShouldTriggerCrisis(FactionState faction, SuccessionCrisisPayload payload)
        {
            if (faction == null || payload == null) return false;
            int pressureScore = faction.successionRisk +
                                faction.courtFactionPressure / 2 +
                                DomainMath.Max(0, faction.regionIds.Count - 5) * 2 +
                                CalculateHeirFragility(faction.heir);
            return faction.successionRisk >= CrisisRiskThreshold || pressureScore >= CrisisPressureScoreThreshold;
        }

        private static int CalculateCrisisLegitimacyCost(FactionState faction)
        {
            int heirFragility = CalculateHeirFragility(faction != null ? faction.heir : null);
            int court = faction != null ? faction.courtFactionPressure : 0;
            return DomainMath.Clamp(6 + court / 20 + heirFragility / 10, 4, 14);
        }

        private static int CalculateHeirFragility(HeirState heir)
        {
            if (heir == null) return 24;
            int fragility = 0;
            if (heir.age < 16) fragility += 10;
            if (heir.legitimacy < 50) fragility += (50 - heir.legitimacy) / 2;
            if (heir.ability < 45) fragility += (45 - heir.ability) / 2;
            return fragility;
        }

        private static RegionState FindMostFragileOwnedRegion(GameState state, FactionState faction)
        {
            if (state == null || faction == null || faction.regionIds == null) return null;

            RegionState best = null;
            int bestScore = int.MinValue;
            for (int i = 0; i < faction.regionIds.Count; i++)
            {
                RegionState region = state.FindRegion(faction.regionIds[i]);
                if (region == null) continue;

                int score = region.rebellionRisk + region.localPower + region.annexationPressure + (100 - region.localAcceptance) / 2;
                if (score <= bestScore) continue;
                bestScore = score;
                best = region;
            }

            return best;
        }
    }
}
