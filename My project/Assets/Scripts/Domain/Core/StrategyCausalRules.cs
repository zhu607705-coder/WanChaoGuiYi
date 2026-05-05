namespace WanChaoGuiYi
{
    public static class StrategyCausalRules
    {
        public const int BorderControlMoneyCost = 20;
        public const int BorderControlFoodCost = 10;
        public const int BorderControlOpinionPenalty = 3;
        public const int BorderControlGrudgeIncrease = 2;

        public const float EmergencyTaxMultiplierIncrease = 0.25f;
        public const int EmergencyTaxRebellionIncrease = 8;
        public const int EmergencyTaxLegitimacyCost = 2;

        public const int ConscriptionManpowerCost = 8;
        public const int ConscriptionSoldiersPerManpower = 100;
        public const int ConscriptionPopulationCostPerManpower = 120;
        public const int ConscriptionRebellionIncrease = 5;

        public const int WarMovementSupplyCost = 5;
        public const int LowSupplyBattleThreshold = 20;
        public const int LowSupplyBattlePowerPercent = 75;
        public const int DepletedSupplyBattlePowerPercent = 55;
        public const int OccupationLegitimacyCost = 2;
        public const int OccupiedIntegration = 25;
        public const int OccupiedContributionPercent = 35;
        public const int OccupationRebellionRiskIncrease = 12;
        public const int OccupationLocalPowerIncrease = 8;
        public const int OccupationAnnexationPressureIncrease = 10;
        public const int GrainShortageSupplyLoss = 8;
        public const int GrainShortageRebellionRiskIncrease = 6;
        public const int GrainShortageAcceptanceCost = 5;
        public const int FoodAidOpinionGain = 10;

        public static bool CanApplyBorderControl(FactionState faction)
        {
            return faction != null &&
                   faction.money >= BorderControlMoneyCost &&
                   faction.food >= BorderControlFoodCost;
        }

        public static bool ApplyBorderControl(FactionState faction, DiplomaticRelation relation)
        {
            if (!CanApplyBorderControl(faction) || relation == null) return false;

            faction.money -= BorderControlMoneyCost;
            faction.food -= BorderControlFoodCost;
            relation.opinion = DomainMath.Max(NumericTuning.MinOpinion, relation.opinion - BorderControlOpinionPenalty);
            relation.grudge = DomainMath.Min(100, relation.grudge + BorderControlGrudgeIncrease);
            return true;
        }

        public static void ApplyEmergencyTaxPressure(FactionState faction, RegionState region)
        {
            if (faction == null || region == null) return;

            faction.taxMultiplier += EmergencyTaxMultiplierIncrease;
            faction.legitimacy = DomainMath.Max(0, faction.legitimacy - EmergencyTaxLegitimacyCost);
            region.rebellionRisk = DomainMath.Min(100, region.rebellionRisk + EmergencyTaxRebellionIncrease);
        }

        public static int ApplyConscriptionDraft(RegionState region, ArmyState army)
        {
            if (region == null || army == null) return 0;

            int manpowerSpent = DomainMath.Min(region.manpower, ConscriptionManpowerCost);
            int soldiersRaised = manpowerSpent * ConscriptionSoldiersPerManpower;
            int populationCost = manpowerSpent * ConscriptionPopulationCostPerManpower;

            region.manpower = DomainMath.Max(0, region.manpower - manpowerSpent);
            region.population = DomainMath.Max(0, region.population - populationCost);
            region.rebellionRisk = DomainMath.Min(100, region.rebellionRisk + ConscriptionRebellionIncrease);
            army.soldiers += soldiersRaised;
            return soldiersRaised;
        }

        public static void ApplyArmyMovementSupplyCost(ArmyRuntimeState army)
        {
            if (army == null) return;
            army.supply = DomainMath.Max(0, army.supply - WarMovementSupplyCost);
        }

        public static int ApplyBattleSupplyPressure(int basePower, ArmyRuntimeState army)
        {
            int percent = CalculateBattleSupplyPowerPercent(army);
            return DomainMath.Max(0, DomainMath.RoundToInt(basePower * (percent / 100f)));
        }

        public static int CalculateBattleSupplyPowerPercent(ArmyRuntimeState army)
        {
            if (army == null) return 100;
            return CalculateBattleSupplyPowerPercentForSupply(army.supply);
        }

        public static int CalculateBattleSupplyPowerPercentForSupply(int supply)
        {
            if (supply <= 0) return DepletedSupplyBattlePowerPercent;
            if (supply < LowSupplyBattleThreshold) return LowSupplyBattlePowerPercent;
            return 100;
        }

        public static bool HasBattleSupplyPenalty(ArmyRuntimeState army)
        {
            return CalculateBattleSupplyPowerPercent(army) < 100;
        }

        public static void ApplyOccupationLegitimacyCost(FactionState faction)
        {
            if (faction == null) return;
            faction.legitimacy = DomainMath.Max(0, faction.legitimacy - OccupationLegitimacyCost);
        }

        public static bool ApplyGrainShortagePressure(FactionState faction, RegionState region, ArmyRuntimeState army)
        {
            if (faction == null || faction.food >= 0) return false;

            if (region != null)
            {
                region.rebellionRisk = DomainMath.Min(100, region.rebellionRisk + GrainShortageRebellionRiskIncrease);
                region.localAcceptance = DomainMath.Max(0, region.localAcceptance - GrainShortageAcceptanceCost);
            }

            if (army != null)
            {
                army.supply = DomainMath.Max(0, army.supply - GrainShortageSupplyLoss);
            }

            return true;
        }

        public static bool ApplyFoodAid(FactionState fromFaction, FactionState toFaction, DiplomaticRelation relation, int foodAmount)
        {
            if (fromFaction == null || toFaction == null || relation == null || foodAmount <= 0) return false;
            if (fromFaction.food < foodAmount) return false;

            fromFaction.food -= foodAmount;
            toFaction.food += foodAmount;
            relation.opinion = DomainMath.Min(NumericTuning.MaxOpinion, relation.opinion + FoodAidOpinionGain);
            return true;
        }
    }
}
