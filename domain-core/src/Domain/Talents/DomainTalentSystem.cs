namespace WanChaoGuiYi
{
    public sealed class TalentRecruitmentPayload
    {
        public bool recruited;
        public string talentId;
        public int talentCountBefore;
        public int talentCountAfter;
        public string reason;
    }

    public sealed class TalentAppointmentPayload
    {
        public bool applied;
        public string talentId;
        public string regionId;
        public int annexationPressureBefore;
        public int annexationPressureAfter;
        public int courtPressureBefore;
        public int courtPressureAfter;
        public int politicalPressureDelta;
        public string explanation;
        public string reason;
    }

    public sealed class DomainTalentSystem
    {
        public TalentRecruitmentPayload RecruitTalent(FactionState faction, TalentDefinition talent)
        {
            TalentRecruitmentPayload payload = new TalentRecruitmentPayload();
            if (faction == null)
            {
                payload.reason = "missing_faction";
                return payload;
            }

            if (talent == null || string.IsNullOrEmpty(talent.id))
            {
                payload.reason = "missing_talent";
                return payload;
            }

            EnsureTalentList(faction);
            payload.talentId = talent.id;
            payload.talentCountBefore = faction.talentIds.Count;

            if (faction.talentIds.Contains(talent.id))
            {
                payload.talentCountAfter = faction.talentIds.Count;
                payload.reason = "already_recruited";
                return payload;
            }

            faction.talentIds.Add(talent.id);
            payload.recruited = true;
            payload.talentCountAfter = faction.talentIds.Count;
            payload.reason = "recruited";
            return payload;
        }

        public TalentAppointmentPayload ApplyTalentToRegion(FactionState faction, RegionState region, TalentDefinition talent)
        {
            TalentAppointmentPayload payload = new TalentAppointmentPayload();
            if (faction == null)
            {
                payload.reason = "missing_faction";
                return payload;
            }

            if (region == null)
            {
                payload.reason = "missing_region";
                return payload;
            }

            if (talent == null || string.IsNullOrEmpty(talent.id))
            {
                payload.reason = "missing_talent";
                return payload;
            }

            EnsureTalentList(faction);
            payload.talentId = talent.id;
            payload.regionId = region.id;
            payload.annexationPressureBefore = region.annexationPressure;
            payload.courtPressureBefore = faction.courtFactionPressure;

            if (!faction.talentIds.Contains(talent.id))
            {
                payload.annexationPressureAfter = region.annexationPressure;
                payload.courtPressureAfter = faction.courtFactionPressure;
                payload.reason = "talent_not_recruited";
                return payload;
            }

            int annexationDelta = talent.effects != null ? talent.effects.annexationPressure : 0;
            region.annexationPressure = DomainMath.Clamp(region.annexationPressure + annexationDelta, 0, 100);

            payload.politicalPressureDelta = CalculatePoliticalPressureDelta(talent.politicalCost);
            faction.courtFactionPressure = DomainMath.Clamp(faction.courtFactionPressure + payload.politicalPressureDelta, 0, 100);

            payload.applied = true;
            payload.annexationPressureAfter = region.annexationPressure;
            payload.courtPressureAfter = faction.courtFactionPressure;
            payload.reason = "applied";
            payload.explanation = talent.name + "任事：" +
                                  "兼并压力" + payload.annexationPressureBefore + "→" + payload.annexationPressureAfter +
                                  "，朝局压力" + payload.courtPressureBefore + "→" + payload.courtPressureAfter +
                                  "。";
            return payload;
        }

        private static int CalculatePoliticalPressureDelta(PoliticalCost cost)
        {
            if (cost == null) return 0;
            int pressure = cost.factionPressure + cost.eliteAnger + cost.courtSuspicion;
            return DomainMath.Clamp(pressure, 0, 30);
        }

        private static void EnsureTalentList(FactionState faction)
        {
            if (faction.talentIds == null)
            {
                faction.talentIds = new System.Collections.Generic.List<string>();
            }
        }
    }
}
