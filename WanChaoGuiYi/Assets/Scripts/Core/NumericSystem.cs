using System.Collections.Generic;
using UnityEngine;

namespace WanChaoGuiYi
{
    public enum NumericDomain
    {
        Economy,
        Military,
        Emperor,
        Diplomacy,
        Espionage,
        Technology,
        Event,
        Ai,
        Victory
    }

    public enum NumericModifierType
    {
        Additive,
        Multiplicative,
        Override
    }

    public enum NumericStat
    {
        // Economy
        TaxIncome,
        FoodIncome,
        MoneyUpkeep,
        FoodUpkeep,
        FiscalPressure,
        // Military
        BattleAttackPower,
        BattleDefensePower,
        ArmyAttack,
        ArmyDefense,
        // Emperor
        TalentGain,
        Legitimacy,
        CourtPressure,
        SuccessionRisk,
        LocalPower,
        RebellionRisk,
        // Diplomacy
        DiplomacyAcceptance,
        TreatyDuration,
        GrudgeDecay,
        OpinionChange,
        // Espionage
        EspionageProgress,
        EspionageRisk,
        EspionageCost,
        // Technology
        TechProgress,
        TechCost,
        ResearchPoints,
        // Event
        EventWeight,
        EventTriggerChance,
        EventCooldown,
        // Ai
        AiActionScore,
        AiExpansionWeight,
        AiDiplomacyWeight,
        AiPolicyWeight,
        // Victory
        VictoryProgress
    }

    public struct NumericModifier
    {
        public NumericDomain domain;
        public NumericStat stat;
        public NumericModifierType type;
        public float value;
        public string source;

        public NumericModifier(NumericDomain domain, NumericStat stat, NumericModifierType type, float value, string source)
        {
            this.domain = domain;
            this.stat = stat;
            this.type = type;
            this.value = value;
            this.source = source;
        }
    }

    public struct NumericResult
    {
        public NumericDomain domain;
        public NumericStat stat;
        public float baseValue;
        public float additive;
        public float multiplier;
        public bool hasOverride;
        public float overrideValue;
        public float finalValue;
    }

    public sealed class NumericContext
    {
        private readonly List<NumericModifier> modifiers = new List<NumericModifier>();

        internal void Add(NumericModifier modifier)
        {
            modifiers.Add(modifier);
        }

        public void Add(NumericDomain domain, NumericStat stat, NumericModifierType type, float value, string source)
        {
            modifiers.Add(new NumericModifier(domain, stat, type, value, source));
        }

        public NumericResult Evaluate(NumericDomain domain, NumericStat stat, float baseValue)
        {
            return NumericEngine.Evaluate(domain, stat, baseValue, modifiers);
        }
    }

    public static class NumericEngine
    {
        public static NumericResult Evaluate(NumericDomain domain, NumericStat stat, float baseValue, IEnumerable<NumericModifier> modifiers)
        {
            NumericResult result = new NumericResult
            {
                domain = domain,
                stat = stat,
                baseValue = baseValue,
                additive = 0f,
                multiplier = 1f,
                hasOverride = false,
                overrideValue = 0f,
                finalValue = baseValue
            };

            foreach (NumericModifier modifier in modifiers)
            {
                if (modifier.domain != domain || modifier.stat != stat) continue;

                switch (modifier.type)
                {
                    case NumericModifierType.Additive:
                        result.additive += modifier.value;
                        break;
                    case NumericModifierType.Multiplicative:
                        result.multiplier += modifier.value;
                        break;
                    case NumericModifierType.Override:
                        result.hasOverride = true;
                        result.overrideValue = modifier.value;
                        break;
                }
            }

            result.finalValue = result.hasOverride
                ? result.overrideValue
                : (result.baseValue + result.additive) * Mathf.Max(0f, result.multiplier);

            return result;
        }
    }

    public static class NumericTuning
    {
        // Economy
        public const float IntegrationTaxFloor = 0.25f;
        public const float IntegrationTaxScale = 0.75f;
        public const float IntegrationFoodFloor = 0.40f;
        public const float IntegrationFoodScale = 0.60f;
        public const float RebellionOutputPenaltyPerPoint = 0.004f;
        public const float LocalPowerTaxPenaltyPerPoint = 0.0025f;
        public const float AnnexationOutputPenaltyPerPoint = 0.003f;
        public const float FiscalDeficitLegitimacyPenalty = 2f;
        public const float FiscalDeficitCourtPressure = 3f;
        public const float ExpansionGovernancePressurePerRegion = 0.35f;
        public const float ExpansionSuccessionPressurePerRegion = 0.12f;
        public const float GovernanceMoneyCostPerRegion = 18f;
        public const float GovernanceFoodCostPerRegion = 24f;
        public const int TreasurySoftCapMoney = 5000;
        public const int TreasurySoftCapFood = 7000;
        public const float MoneyReserveDragRate = 0.10f;
        public const float FoodReserveDragRate = 0.12f;
        public const int HealthyMoneyUpperBound = 20000;
        public const int HealthyFoodUpperBound = 30000;
        public const int CollapseMoneyLowerBound = -5000;
        public const int CollapseFoodLowerBound = -5000;

        // Military
        public const float BattleSoldierLogDivisor = 3f;
        public const float BattleMinimumSoldierMultiplier = 0.5f;
        public const float BattlePowerScale = 100f;
        public const int TerrainMountainAttackPenalty = -10;
        public const int TerrainPlainAttackBonus = 5;
        public const int TerrainRiverAttackPenalty = -15;

        // Diplomacy
        public const int MaxOpinion = 100;
        public const int MinOpinion = -100;
        public const int GrudgeDecayPerTurn = 2;
        public const int AllianceDuration = 20;
        public const int NonAggressionDuration = 12;
        public const int VassalDuration = -1;
        public const int TributaryDuration = 10;
        public const int TrapDetectionBase = 30;
        public const int DiplomacyAcceptanceBase = 50;

        // Espionage
        public const int BaseProgressPerTurn = 25;
        public const int BaseDetectionRisk = 20;
        public const int IntelCost = 30;
        public const int SabotageCost = 50;
        public const int RumorsCost = 40;
        public const int AssassinateCost = 80;

        // Technology
        public const int BaseResearchPerTurn = 8;
        public const int ReformBonusDivisor = 20;
        public const int TechAffinityBonus = 2;

        // Event
        public const int MaxEventsPerTurn = 2;
        public const float BaseTriggerChance = 0.15f;
        public const int EventCooldownDefault = 5;

        // Ai
        public const float AiExpansionThreshold = 60f;
        public const float AiDiplomacyThreshold = 40f;
        public const float AiPolicyThreshold = 30f;

        // Victory
        public const int VictoryProgressScale = 100;
    }

    public static class NumericModifierFactory
    {
        public static NumericContext ForFaction(FactionState faction)
        {
            NumericContext context = new NumericContext();
            if (faction == null) return context;

            context.Add(NumericDomain.Economy, NumericStat.TaxIncome, NumericModifierType.Multiplicative, faction.taxMultiplier - 1f, "faction_tax_multiplier");
            context.Add(NumericDomain.Economy, NumericStat.FoodIncome, NumericModifierType.Multiplicative, faction.foodMultiplier - 1f, "faction_food_multiplier");
            context.Add(NumericDomain.Military, NumericStat.ArmyAttack, NumericModifierType.Multiplicative, faction.armyAttackMultiplier - 1f, "faction_army_attack_multiplier");
            context.Add(NumericDomain.Military, NumericStat.ArmyDefense, NumericModifierType.Multiplicative, faction.armyDefenseMultiplier - 1f, "faction_army_defense_multiplier");
            context.Add(NumericDomain.Emperor, NumericStat.TalentGain, NumericModifierType.Multiplicative, faction.talentMultiplier - 1f, "faction_talent_multiplier");

            float expansionPressure = Mathf.Max(0, faction.regionIds.Count - 3);
            context.Add(NumericDomain.Economy, NumericStat.FiscalPressure, NumericModifierType.Additive,
                expansionPressure * NumericTuning.ExpansionGovernancePressurePerRegion, "region_count_governance_pressure");
            context.Add(NumericDomain.Economy, NumericStat.MoneyUpkeep, NumericModifierType.Additive,
                Mathf.Max(0, faction.regionIds.Count - 1) * NumericTuning.GovernanceMoneyCostPerRegion, "region_count_money_governance_cost");
            context.Add(NumericDomain.Economy, NumericStat.FoodUpkeep, NumericModifierType.Additive,
                Mathf.Max(0, faction.regionIds.Count - 1) * NumericTuning.GovernanceFoodCostPerRegion, "region_count_food_governance_cost");
            context.Add(NumericDomain.Emperor, NumericStat.SuccessionRisk, NumericModifierType.Additive,
                expansionPressure * NumericTuning.ExpansionSuccessionPressurePerRegion, "region_count_succession_pressure");

            return context;
        }
    }

    public static class NumericFormulas
    {
        // ========== Economy ==========

        public static int CalculateRegionalTax(RegionState region, FactionState faction)
        {
            return CalculateRegionalTax(region, NumericModifierFactory.ForFaction(faction));
        }

        public static int CalculateRegionalTax(RegionState region, NumericContext numericContext)
        {
            if (region == null || numericContext == null) return 0;
            float baseValue = region.taxOutput * CalculateGovernanceEfficiency(region, true);
            return Mathf.Max(0, Mathf.RoundToInt(numericContext.Evaluate(NumericDomain.Economy, NumericStat.TaxIncome, baseValue).finalValue));
        }

        public static int CalculateRegionalFood(RegionState region, FactionState faction)
        {
            return CalculateRegionalFood(region, NumericModifierFactory.ForFaction(faction));
        }

        public static int CalculateRegionalFood(RegionState region, NumericContext numericContext)
        {
            if (region == null || numericContext == null) return 0;
            float baseValue = region.foodOutput * CalculateGovernanceEfficiency(region, false);
            return Mathf.Max(0, Mathf.RoundToInt(numericContext.Evaluate(NumericDomain.Economy, NumericStat.FoodIncome, baseValue).finalValue));
        }

        public static int CalculateArmyUpkeep(ArmyState army, UnitDefinition unit, FactionState faction, bool food)
        {
            return CalculateArmyUpkeep(army, unit, NumericModifierFactory.ForFaction(faction), food);
        }

        public static int CalculateArmyUpkeep(ArmyState army, UnitDefinition unit, NumericContext numericContext, bool food)
        {
            if (army == null || unit == null || unit.upkeep == null || numericContext == null) return 0;
            int unitUpkeep = food ? unit.upkeep.food : unit.upkeep.money;
            NumericStat stat = food ? NumericStat.FoodUpkeep : NumericStat.MoneyUpkeep;
            float baseValue = Mathf.CeilToInt(army.soldiers / 1000f) * unitUpkeep;
            return Mathf.Max(0, Mathf.RoundToInt(numericContext.Evaluate(NumericDomain.Economy, stat, baseValue).finalValue));
        }

        public static int CalculateTreasuryReserveDrag(FactionState faction, bool food)
        {
            if (faction == null) return 0;
            int value = food ? faction.food : faction.money;
            int softCap = food ? NumericTuning.TreasurySoftCapFood : NumericTuning.TreasurySoftCapMoney;
            float dragRate = food ? NumericTuning.FoodReserveDragRate : NumericTuning.MoneyReserveDragRate;
            return Mathf.Max(0, Mathf.RoundToInt(Mathf.Max(0, value - softCap) * dragRate));
        }

        public static int CalculateGovernanceUpkeep(FactionState faction, bool food)
        {
            return CalculateGovernanceUpkeep(NumericModifierFactory.ForFaction(faction), food);
        }

        public static int CalculateGovernanceUpkeep(NumericContext numericContext, bool food)
        {
            if (numericContext == null) return 0;
            NumericStat stat = food ? NumericStat.FoodUpkeep : NumericStat.MoneyUpkeep;
            return Mathf.Max(0, Mathf.RoundToInt(numericContext.Evaluate(NumericDomain.Economy, stat, 0f).finalValue));
        }

        // ========== Military ==========

        public static int CalculateBattlePower(ArmyState army, UnitDefinition unit, FactionState faction, EquipmentBonus equipmentBonus, bool attacking)
        {
            return CalculateBattlePower(army, unit, NumericModifierFactory.ForFaction(faction), equipmentBonus, attacking);
        }

        public static int CalculateBattlePower(ArmyState army, UnitDefinition unit, NumericContext numericContext, EquipmentBonus equipmentBonus, bool attacking)
        {
            if (army == null || unit == null || unit.stats == null || numericContext == null) return 0;

            int baseStat = attacking ? unit.stats.attack : unit.stats.defense;
            baseStat += attacking ? equipmentBonus.attack : equipmentBonus.defense;

            float soldierMultiplier = Mathf.Log10(Mathf.Max(1, army.soldiers)) / NumericTuning.BattleSoldierLogDivisor;
            if (soldierMultiplier < NumericTuning.BattleMinimumSoldierMultiplier)
            {
                soldierMultiplier = NumericTuning.BattleMinimumSoldierMultiplier;
            }

            float moraleMultiplier = Mathf.Clamp01(army.morale / 100f);
            float baseValue = baseStat * soldierMultiplier * moraleMultiplier * NumericTuning.BattlePowerScale;

            NumericStat stat = attacking ? NumericStat.ArmyAttack : NumericStat.ArmyDefense;
            return Mathf.Max(0, Mathf.RoundToInt(numericContext.Evaluate(NumericDomain.Military, stat, baseValue).finalValue));
        }

        // ========== Emperor ==========

        public static int CalculateFiscalLegitimacyPenalty(FactionState faction)
        {
            return CalculateFiscalLegitimacyPenalty(NumericModifierFactory.ForFaction(faction));
        }

        public static int CalculateFiscalLegitimacyPenalty(NumericContext numericContext)
        {
            if (numericContext == null) return 0;
            return Mathf.RoundToInt(numericContext.Evaluate(NumericDomain.Economy, NumericStat.FiscalPressure,
                NumericTuning.FiscalDeficitLegitimacyPenalty).finalValue);
        }

        public static int CalculateFiscalCourtPressure(FactionState faction)
        {
            return CalculateFiscalCourtPressure(NumericModifierFactory.ForFaction(faction));
        }

        public static int CalculateFiscalCourtPressure(NumericContext numericContext)
        {
            if (numericContext == null) return 0;
            return Mathf.RoundToInt(numericContext.Evaluate(NumericDomain.Economy, NumericStat.FiscalPressure,
                NumericTuning.FiscalDeficitCourtPressure).finalValue);
        }

        public static int CalculateExpansionSuccessionPressure(FactionState faction)
        {
            return CalculateExpansionSuccessionPressure(NumericModifierFactory.ForFaction(faction));
        }

        public static int CalculateExpansionSuccessionPressure(NumericContext numericContext)
        {
            if (numericContext == null) return 0;
            return Mathf.RoundToInt(numericContext.Evaluate(NumericDomain.Emperor, NumericStat.SuccessionRisk, 0f).finalValue);
        }

        public static int CalculateRegionalTaxBase(RegionState region)
        {
            if (region == null) return 0;
            float baseValue = region.taxOutput * CalculateGovernanceEfficiency(region, true);
            return Mathf.Max(0, Mathf.RoundToInt(baseValue));
        }

        // ========== Diplomacy ==========

        public static int CalculateDiplomacyAcceptance(FactionState proposer, FactionState target, NumericContext numericContext)
        {
            if (proposer == null || target == null || numericContext == null) return 0;
            float baseValue = NumericTuning.DiplomacyAcceptanceBase;
            return Mathf.RoundToInt(numericContext.Evaluate(NumericDomain.Diplomacy, NumericStat.DiplomacyAcceptance, baseValue).finalValue);
        }

        public static int CalculateTreatyDuration(string treatyType, NumericContext numericContext)
        {
            if (numericContext == null) return 0;
            int baseDuration;
            switch (treatyType)
            {
                case "alliance": baseDuration = NumericTuning.AllianceDuration; break;
                case "non_aggression": baseDuration = NumericTuning.NonAggressionDuration; break;
                case "vassal": baseDuration = NumericTuning.VassalDuration; break;
                case "tributary": baseDuration = NumericTuning.TributaryDuration; break;
                default: baseDuration = NumericTuning.NonAggressionDuration; break;
            }
            if (baseDuration < 0) return -1;
            return Mathf.Max(1, Mathf.RoundToInt(numericContext.Evaluate(NumericDomain.Diplomacy, NumericStat.TreatyDuration, baseDuration).finalValue));
        }

        public static int CalculateGrudgeDecay(DiplomaticRelation relation, NumericContext numericContext)
        {
            if (relation == null || numericContext == null) return 0;
            return Mathf.Max(0, Mathf.RoundToInt(numericContext.Evaluate(NumericDomain.Diplomacy, NumericStat.GrudgeDecay, NumericTuning.GrudgeDecayPerTurn).finalValue));
        }

        // ========== Espionage ==========

        public static int CalculateEspionageProgress(EspionageOperation operation, NumericContext numericContext)
        {
            if (operation == null || numericContext == null) return 0;
            return Mathf.Max(1, Mathf.RoundToInt(numericContext.Evaluate(NumericDomain.Espionage, NumericStat.EspionageProgress, NumericTuning.BaseProgressPerTurn).finalValue));
        }

        public static int CalculateEspionageDetectionRisk(FactionState agent, FactionState target, NumericContext numericContext)
        {
            if (numericContext == null) return 0;
            float baseRisk = NumericTuning.BaseDetectionRisk;
            if (target != null) baseRisk -= target.espionageDefense;
            return Mathf.Clamp(Mathf.RoundToInt(numericContext.Evaluate(NumericDomain.Espionage, NumericStat.EspionageRisk, baseRisk).finalValue), 5, 95);
        }

        public static int CalculateEspionageCost(string actionType, NumericContext numericContext)
        {
            if (numericContext == null) return 0;
            int baseCost;
            switch (actionType)
            {
                case "scout_intel": baseCost = NumericTuning.IntelCost; break;
                case "sabotage": baseCost = NumericTuning.SabotageCost; break;
                case "spread_rumors": baseCost = NumericTuning.RumorsCost; break;
                case "assassinate": baseCost = NumericTuning.AssassinateCost; break;
                default: baseCost = NumericTuning.IntelCost; break;
            }
            return Mathf.Max(1, Mathf.RoundToInt(numericContext.Evaluate(NumericDomain.Espionage, NumericStat.EspionageCost, baseCost).finalValue));
        }

        // ========== Technology ==========

        public static int CalculateResearchPoints(FactionState faction, EmperorDefinition emperor, NumericContext numericContext)
        {
            if (numericContext == null) return 0;
            float basePoints = NumericTuning.BaseResearchPerTurn;
            if (emperor != null) basePoints += emperor.stats.reform / (float)NumericTuning.ReformBonusDivisor;
            basePoints += faction.talentIds.Count;
            return Mathf.Max(1, Mathf.RoundToInt(numericContext.Evaluate(NumericDomain.Technology, NumericStat.ResearchPoints, basePoints).finalValue));
        }

        public static int CalculateTechCost(TechnologyDefinition tech, NumericContext numericContext)
        {
            if (tech == null || numericContext == null) return 0;
            return Mathf.Max(1, Mathf.RoundToInt(numericContext.Evaluate(NumericDomain.Technology, NumericStat.TechCost, tech.cost).finalValue));
        }

        // ========== Event ==========

        public static float CalculateEventTriggerChance(NumericContext numericContext)
        {
            if (numericContext == null) return 0f;
            return Mathf.Clamp01(numericContext.Evaluate(NumericDomain.Event, NumericStat.EventTriggerChance, NumericTuning.BaseTriggerChance).finalValue);
        }

        public static int CalculateEventCooldown(NumericContext numericContext)
        {
            if (numericContext == null) return 0;
            return Mathf.Max(1, Mathf.RoundToInt(numericContext.Evaluate(NumericDomain.Event, NumericStat.EventCooldown, NumericTuning.EventCooldownDefault).finalValue));
        }

        // ========== Ai ==========

        public static float CalculateAiExpansionWeight(FactionState faction, NumericContext numericContext)
        {
            if (faction == null || numericContext == null) return 0f;
            return numericContext.Evaluate(NumericDomain.Ai, NumericStat.AiExpansionWeight, NumericTuning.AiExpansionThreshold).finalValue;
        }

        public static float CalculateAiDiplomacyWeight(FactionState faction, NumericContext numericContext)
        {
            if (faction == null || numericContext == null) return 0f;
            return numericContext.Evaluate(NumericDomain.Ai, NumericStat.AiDiplomacyWeight, NumericTuning.AiDiplomacyThreshold).finalValue;
        }

        public static float CalculateAiPolicyWeight(FactionState faction, NumericContext numericContext)
        {
            if (faction == null || numericContext == null) return 0f;
            return numericContext.Evaluate(NumericDomain.Ai, NumericStat.AiPolicyWeight, NumericTuning.AiPolicyThreshold).finalValue;
        }

        // ========== Victory ==========

        public static float CalculateVictoryProgress(FactionState faction, NumericContext numericContext)
        {
            if (faction == null || numericContext == null) return 0f;
            return Mathf.Clamp01(numericContext.Evaluate(NumericDomain.Victory, NumericStat.VictoryProgress, 0f).finalValue / NumericTuning.VictoryProgressScale);
        }

        // ========== Private Helpers ==========

        private static float CalculateGovernanceEfficiency(RegionState region, bool tax)
        {
            float integrationFactor = tax
                ? NumericTuning.IntegrationTaxFloor + region.integration / 100f * NumericTuning.IntegrationTaxScale
                : NumericTuning.IntegrationFoodFloor + region.integration / 100f * NumericTuning.IntegrationFoodScale;
            float rebellionPenalty = region.rebellionRisk * NumericTuning.RebellionOutputPenaltyPerPoint;
            float localPowerPenalty = tax ? region.localPower * NumericTuning.LocalPowerTaxPenaltyPerPoint : 0f;
            float annexationPenalty = region.annexationPressure * NumericTuning.AnnexationOutputPenaltyPerPoint;

            return Mathf.Clamp(integrationFactor - rebellionPenalty - localPowerPenalty - annexationPenalty, 0.10f, 1.25f);
        }
    }
}
