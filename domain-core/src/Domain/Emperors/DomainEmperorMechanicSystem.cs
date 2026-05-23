namespace WanChaoGuiYi
{
    public sealed class EmperorMechanicEffect
    {
        public string emperorId;
        public string mechanicId;
        public string mechanicName;
        public string primaryEffectId;
        public string[] effectTags;
        public string explanation;
        public int integrationDelta;
        public int successionRiskDelta;
        public int localPowerDelta;
        public int fiscalPressureDelta;
        public float talentMultiplierDelta;
        public float armyAttackMultiplierDelta;
    }

    public sealed class DomainEmperorMechanicSystem
    {
        public EmperorMechanicEffect BuildPlaystyleEffect(EmperorDefinition emperor)
        {
            if (emperor == null)
            {
                return BuildFallbackEffect(null);
            }

            string tag = emperor.globalMechanicTag ?? string.Empty;
            if (tag == "imperial_standardization" || HasPreferredPolicy(emperor, "standardization"))
            {
                return BuildStandardizationEffect(emperor);
            }

            if (tag == "coalition_absorption" || HasPreferredPolicy(emperor, "talent_absorption"))
            {
                return BuildCoalitionEffect(emperor);
            }

            if (tag == "frontier_expedition" || HasPreferredPolicy(emperor, "frontier_campaign"))
            {
                return BuildFrontierEffect(emperor);
            }

            return BuildFallbackEffect(emperor);
        }

        private static EmperorMechanicEffect BuildStandardizationEffect(EmperorDefinition emperor)
        {
            int administration = GetAdministration(emperor);
            int reform = GetReform(emperor);
            int successionControl = GetSuccessionControl(emperor);

            return new EmperorMechanicEffect
            {
                emperorId = emperor.id,
                mechanicId = GetMechanicId(emperor),
                mechanicName = GetMechanicName(emperor),
                primaryEffectId = "integration_standardization",
                effectTags = new[] { "standardization", "integration", "centralization" },
                integrationDelta = DomainMath.Clamp((administration + reform) / 25 - 3, 1, 8),
                successionRiskDelta = DomainMath.Clamp((100 - successionControl) / 18, 1, 6),
                explanation = BuildExplanation(emperor, "制度标准化加速新地整合，但中央集权压力会抬高继承风险。")
            };
        }

        private static EmperorMechanicEffect BuildCoalitionEffect(EmperorDefinition emperor)
        {
            int charisma = GetCharisma(emperor);
            int diplomacy = GetDiplomacy(emperor);
            int successionControl = GetSuccessionControl(emperor);

            return new EmperorMechanicEffect
            {
                emperorId = emperor.id,
                mechanicId = GetMechanicId(emperor),
                mechanicName = GetMechanicName(emperor),
                primaryEffectId = "coalition_talent_absorption",
                effectTags = new[] { "talent", "coalition", "local_compromise" },
                talentMultiplierDelta = DomainMath.Clamp((charisma + diplomacy) / 2000f, 0.03f, 0.12f),
                localPowerDelta = -DomainMath.Clamp(diplomacy / 22, 1, 5),
                successionRiskDelta = DomainMath.Clamp((100 - successionControl) / 25, 1, 5),
                explanation = BuildExplanation(emperor, "功臣联盟提升人才吸纳和地方妥协效率，但宗室与功臣平衡仍会留下继承隐患。")
            };
        }

        private static EmperorMechanicEffect BuildFrontierEffect(EmperorDefinition emperor)
        {
            int military = GetMilitary(emperor);
            int expansion = GetExpansion(emperor);
            int riskTolerance = GetRiskTolerance(emperor);

            return new EmperorMechanicEffect
            {
                emperorId = emperor.id,
                mechanicId = GetMechanicId(emperor),
                mechanicName = GetMechanicName(emperor),
                primaryEffectId = "frontier_expedition_pressure",
                effectTags = new[] { "military", "frontier", "expedition" },
                armyAttackMultiplierDelta = DomainMath.Clamp(military / 1000f, 0.04f, 0.12f),
                fiscalPressureDelta = DomainMath.Clamp((expansion + riskTolerance) / 35, 1, 6),
                explanation = BuildExplanation(emperor, "边疆远征提升进攻效率，同时把财政与民生压力推到台前。")
            };
        }

        private static EmperorMechanicEffect BuildFallbackEffect(EmperorDefinition emperor)
        {
            return new EmperorMechanicEffect
            {
                emperorId = emperor != null ? emperor.id : string.Empty,
                mechanicId = GetMechanicId(emperor),
                mechanicName = GetMechanicName(emperor),
                primaryEffectId = "balanced_imperial_rule",
                effectTags = new[] { "balanced" },
                explanation = BuildExplanation(emperor, "当前帝皇使用均衡治理效果，等待后续专属机制细化。")
            };
        }

        private static bool HasPreferredPolicy(EmperorDefinition emperor, string policyId)
        {
            if (emperor == null || emperor.preferredPolicies == null || string.IsNullOrEmpty(policyId)) return false;
            for (int i = 0; i < emperor.preferredPolicies.Length; i++)
            {
                if (emperor.preferredPolicies[i] == policyId) return true;
            }

            return false;
        }

        private static string BuildExplanation(EmperorDefinition emperor, string effectText)
        {
            string mechanicName = GetMechanicName(emperor);
            string description = emperor != null && emperor.uniqueMechanic != null ? emperor.uniqueMechanic.description : string.Empty;
            if (string.IsNullOrEmpty(description)) return mechanicName + "：" + effectText;
            return mechanicName + "：" + description + " " + effectText;
        }

        private static string GetMechanicId(EmperorDefinition emperor)
        {
            return emperor != null && emperor.uniqueMechanic != null ? emperor.uniqueMechanic.id : string.Empty;
        }

        private static string GetMechanicName(EmperorDefinition emperor)
        {
            return emperor != null && emperor.uniqueMechanic != null ? emperor.uniqueMechanic.name : "未定义机制";
        }

        private static int GetMilitary(EmperorDefinition emperor)
        {
            return emperor != null && emperor.stats != null ? emperor.stats.military : 0;
        }

        private static int GetAdministration(EmperorDefinition emperor)
        {
            return emperor != null && emperor.stats != null ? emperor.stats.administration : 0;
        }

        private static int GetReform(EmperorDefinition emperor)
        {
            return emperor != null && emperor.stats != null ? emperor.stats.reform : 0;
        }

        private static int GetCharisma(EmperorDefinition emperor)
        {
            return emperor != null && emperor.stats != null ? emperor.stats.charisma : 0;
        }

        private static int GetDiplomacy(EmperorDefinition emperor)
        {
            return emperor != null && emperor.stats != null ? emperor.stats.diplomacy : 0;
        }

        private static int GetSuccessionControl(EmperorDefinition emperor)
        {
            return emperor != null && emperor.stats != null ? emperor.stats.successionControl : 0;
        }

        private static int GetExpansion(EmperorDefinition emperor)
        {
            return emperor != null && emperor.aiPersonality != null ? emperor.aiPersonality.expansion : 0;
        }

        private static int GetRiskTolerance(EmperorDefinition emperor)
        {
            return emperor != null && emperor.aiPersonality != null ? emperor.aiPersonality.riskTolerance : 0;
        }
    }
}
