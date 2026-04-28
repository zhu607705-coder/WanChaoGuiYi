using System;

namespace WanChaoGuiYi
{
    [Serializable]
    public sealed class EmperorTable
    {
        public int schemaVersion;
        public string mapScope;
        public EmperorDefinition[] items;
    }

    [Serializable]
    public sealed class EmperorDefinition
    {
        public string id;
        public string name;
        public string title;
        public string[] versionScope;
        public string civilization;
        public string mapScope;
        public string era;
        public string[] legitimacyTypes;
        public string globalMechanicTag;
        public EmperorStats stats;
        public UniqueMechanicDefinition uniqueMechanic;
        public string[] historicalBurdens;
        public string[] preferredPolicies;
        public AiPersonality aiPersonality;
    }

    [Serializable]
    public sealed class PortraitTable
    {
        public int schemaVersion;
        public PortraitStyleBible styleBible;
        public PortraitDefinition[] items;
    }

    [Serializable]
    public sealed class PortraitStyleBible
    {
        public string projectStyle;
        public string framing;
        public string rendering;
        public string background;
        public string negativePrompt;
    }

    [Serializable]
    public sealed class PortraitDefinition
    {
        public string id;
        public string emperorId;
        public string assetPath;
        public string sourceStatus;
        public string version;
        public PortraitVisualIdentity visualIdentity;
        public string prompt;
        public UiCropHints uiCropHints;
    }

    [Serializable]
    public sealed class PortraitVisualIdentity
    {
        public string silhouette;
        public string costume;
        public string[] props;
        public string[] palette;
        public string backgroundMotif;
        public string expression;
    }

    [Serializable]
    public sealed class UiCropHints
    {
        public float headCenterX;
        public float headCenterY;
        public float safeScale;
    }

    [Serializable]
    public sealed class EmperorStats
    {
        public int military;
        public int administration;
        public int reform;
        public int charisma;
        public int diplomacy;
        public int successionControl;
    }

    [Serializable]
    public sealed class UniqueMechanicDefinition
    {
        public string id;
        public string name;
        public string description;
    }

    [Serializable]
    public sealed class AiPersonality
    {
        public int expansion;
        public int governance;
        public int riskTolerance;
    }

    [Serializable]
    public sealed class RegionTable
    {
        public int schemaVersion;
        public string mapScope;
        public string completionStatus;
        public RegionTargetCount mvpTargetRegionCount;
        public RegionDefinition[] items;
    }

    [Serializable]
    public sealed class RegionTargetCount
    {
        public int min;
        public int max;
    }

    [Serializable]
    public sealed class RegionDefinition
    {
        public string id;
        public string name;
        public string terrain;
        public int population;
        public int foodOutput;
        public int taxOutput;
        public int manpower;
        public LandStructure landStructure;
        public string[] legitimacyMemory;
        public int localPower;
        public int rebellionRisk;
        public string[] neighbors;
        public EraProfile eraProfile;
    }

    [Serializable]
    public sealed class HistoricalLayerTable
    {
        public int schemaVersion;
        public string mapScope;
        public string designReference;
        public HistoricalLayerDefinition[] items;
    }

    [Serializable]
    public sealed class HistoricalLayerDefinition
    {
        public string id;
        public string regionId;
        public string climateZone;
        public string[] geographyTags;
        public string[] customTags;
        public string[] weaponTraditions;
        public string[] strategicResources;
        public SeasonalProfile seasonalProfile;
        public YieldModifierSet yieldModifiers;
        public EventWeightSet eventWeights;
        public string[] techAffinities;
        public string uiSummary;
    }

    [Serializable]
    public sealed class SeasonalProfile
    {
        public string spring;
        public string[] summerRisk;
        public string[] autumnBonus;
        public string[] winterRisk;
    }

    [Serializable]
    public sealed class YieldModifierSet
    {
        public int food;
        public int tax;
        public int manpower;
        public int mobility;
        public int legitimacy;
    }

    [Serializable]
    public sealed class EventWeightSet
    {
        public int flood;
        public int drought;
        public int storm;
        public int cold;
        public int astronomy;
    }

    [Serializable]
    public sealed class LandStructure
    {
        public float smallFarmers;
        public float localElites;
        public float stateLand;
        public float religiousLand;

        public LandStructure Clone()
        {
            return new LandStructure
            {
                smallFarmers = smallFarmers,
                localElites = localElites,
                stateLand = stateLand,
                religiousLand = religiousLand
            };
        }
    }

    [Serializable]
    public sealed class EraProfile
    {
        public string classical;
        public string medieval;
        public string early_modern;
    }

    [Serializable]
    public sealed class PolicyTable
    {
        public int schemaVersion;
        public PolicyDefinition[] items;
    }

    [Serializable]
    public sealed class PolicyDefinition
    {
        public string id;
        public string name;
        public string category;
        public CostSet cost;
        public EffectSet effects;
        public RiskSet risks;
        public string[] mechanicTags;
    }

    [Serializable]
    public sealed class CostSet
    {
        public int money;
        public int food;
        public int manpower;
        public int legitimacy;
    }

    [Serializable]
    public sealed class EffectSet
    {
        public int integrationSpeed;
        public int taxEfficiency;
        public int taxBase;
        public int annexationPressure;
        public int rebellionRisk;
        public int legitimacy;
        public int manpowerToArmy;
        public int talentChance;
        public int courtCapacity;
        public int armyMorale;
        public int generalLoyalty;
        public int factionPressure;
        public int localPower;
        public int money;
        public int food;
        public int populationGrowth;
        public int successionRisk;
        public int treasuryStability;
        public int treasuryPressure;
        public int battlePower;
        public int science;
        public int culture;
        public int techProgress;
        public int weatherResilience;
        public int disasterMitigation;
        public int astronomyInsight;
        public int weaponQuality;
        public int mobility;
        public int landSurveyEfficiency;
        public int treasuryControl;
        public int frontierIntegration;
        public int multiethnicAcceptance;
    }

    [Serializable]
    public sealed class RiskSet
    {
        public int corveePressure;
        public int rebellionRisk;
        public int eliteAnger;
        public int treasuryPressure;
        public int populationGrowth;
        public int annexationPressure;
        public int factionPressure;
        public int localPower;
        public int armyMorale;
        public int taxEfficiency;
        public int legitimacy;
        public int money;
        public int science;
        public int successionRisk;
        public int astronomyInsight;
        public int weatherDamage;
    }

    [Serializable]
    public sealed class EventTable
    {
        public int schemaVersion;
        public EventDefinition[] items;
    }

    [Serializable]
    public sealed class EventDefinition
    {
        public string id;
        public string name;
        public string category;
        public EventTrigger trigger;
        public EventChoiceDefinition[] choices;
        public int cooldownTurns;
    }

    [Serializable]
    public sealed class EventTrigger
    {
        public int minSuccessionRisk;
        public int minCourtFactionPressure;
        public string policyUsed;
        public int minLocalPower;
        public bool recentBattleWon;
        public string terrainTag;
    }

    [Serializable]
    public sealed class EventChoiceDefinition
    {
        public string id;
        public string label;
        public EffectSet effects;
    }

    [Serializable]
    public sealed class TalentTable
    {
        public int schemaVersion;
        public TalentDefinition[] items;
    }

    [Serializable]
    public sealed class TalentDefinition
    {
        public string id;
        public string name;
        public string role;
        public string rarity;
        public EffectSet effects;
        public PoliticalCost politicalCost;
    }

    [Serializable]
    public sealed class PoliticalCost
    {
        public int factionPressure;
        public int popularSatisfaction;
        public int eliteAnger;
        public int courtSuspicion;
    }

    [Serializable]
    public sealed class UnitTable
    {
        public int schemaVersion;
        public UnitDefinition[] items;
    }

    [Serializable]
    public sealed class UnitDefinition
    {
        public string id;
        public string name;
        public string category;
        public CostSet cost;
        public CostSet upkeep;
        public UnitStats stats;
    }

    [Serializable]
    public sealed class UnitStats
    {
        public int attack;
        public int defense;
        public int mobility;
        public int siege;
    }

    [Serializable]
    public sealed class TechnologyTable
    {
        public int schemaVersion;
        public string designReference;
        public TechnologyDefinition[] items;
    }

    [Serializable]
    public sealed class TechnologyDefinition
    {
        public string id;
        public string name;
        public string kind;
        public string era;
        public int cost;
        public string[] prerequisites;
        public BoostDefinition boost;
        public UnlockSet unlocks;
        public EffectSet effects;
        public string uiSummary;
    }

    [Serializable]
    public sealed class BoostDefinition
    {
        public string id;
        public string name;
        public string description;
        public int progressBonus;
    }

    [Serializable]
    public sealed class UnlockSet
    {
        public string[] units;
        public string[] policies;
        public string[] events;
        public string[] mechanicTags;
    }

    [Serializable]
    public sealed class ChronicleEventTable
    {
        public int schemaVersion;
        public string designReference;
        public ChronicleEventDefinition[] items;
    }

    [Serializable]
    public sealed class ChronicleEventDefinition
    {
        public string id;
        public string name;
        public string eventType;
        public string[] eraScope;
        public TurnWindow turnWindow;
        public string[] regionScopeTags;
        public string[] requiredTechs;
        public string[] weatherTags;
        public string[] astronomyTags;
        public int triggerWeight;
        public ChronicleChoiceDefinition[] choices;
        public int cooldownTurns;
        public string uiSummary;
    }

    [Serializable]
    public sealed class TurnWindow
    {
        public int startTurn;
        public int endTurn;
    }

    [Serializable]
    public sealed class ChronicleChoiceDefinition
    {
        public string id;
        public string label;
        public EffectSet effects;
        public RiskSet risks;
        public string[] followUpTags;
    }

    [Serializable]
    public sealed class VictoryConditionTable
    {
        public int schemaVersion;
        public VictoryConditionDefinition[] items;
    }

    [Serializable]
    public sealed class VictoryConditionDefinition
    {
        public string id;
        public string name;
        public string description;
        public VictoryRequirement requirements;
    }

    [Serializable]
    public sealed class VictoryRequirement
    {
        public bool controlAllKeyRegions;
        public int minLegitimacy;
        public int stableSuccessions;
        public int maxFragmentation;
        public int completedCoreReforms;
        public int minTreasuryStability;
        public int maxAnnexationPressure;
    }
}
