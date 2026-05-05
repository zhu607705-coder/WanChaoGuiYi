using System;
using System.Collections.Generic;

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
        public EmperorScore score;
        public UniqueMechanicDefinition uniqueMechanic;
        public DiplomacySkillDefinition[] diplomacySkills;
        public string[] historicalBurdens;
        public string[] preferredPolicies;
        public AiPersonality aiPersonality;
        public string sourceReference;
    }

    [Serializable]
    public sealed class EmperorScore
    {
        public int virtue;
        public int wisdom;
        public int physique;
        public int aesthetics;
        public int diligence;
        public int ambition;
        public int dignity;
        public int tolerance;
        public int selfControl;
        public int personnelManagement;
        public int nationalPower;
        public int popularSupport;

        public float CalculateTotal()
        {
            return virtue * 0.12f + wisdom * 0.08f + physique * 0.04f +
                   aesthetics * 0.04f + diligence * 0.05f + ambition * 0.05f +
                   dignity * 0.08f + tolerance * 0.05f + selfControl * 0.06f +
                   personnelManagement * 0.10f + nationalPower * 0.15f +
                   popularSupport * 0.18f;
        }
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
    public sealed class DiplomacySkillDefinition
    {
        public string id;
        public string name;
        public int moneyCost;
        public int talentCost;
        public int cooldownTurns;
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
        public string regionSpecialization;
        public bool supplyNode;
        public string gameplaySourceReference;
        public int localPower;
        public int rebellionRisk;
        public string[] neighbors;
        public EraProfile eraProfile;
    }

    [Serializable]
    public sealed class MapRegionShapeTable
    {
        public int schemaVersion;
        public string mapScope;
        public string precision;
        public string designReference;
        public MapRegionShapeDefinition[] items;
    }

    [Serializable]
    public sealed class MapRegionShapeDefinition
    {
        public string id;
        public string regionId;
        public MapPoint center;
        public MapPoint labelOffset;
        public int renderOrder;
        public MapPoint[] boundary;
    }

    [Serializable]
    public sealed class MapPoint
    {
        public float x;
        public float y;
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
        public string sourceReference;
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
        public string emperorId;
        public int minTurn;
        public string era;
        public int minArmyStrength;
        public int maxArmyStrength;
        public int minSuccessionRisk;
        public int minCourtFactionPressure;
        public int minRebellionRisk;
        public int minPopularDissatisfaction;
        public int minLocalPower;
        public int minFrontierThreat;
        public string policyUsed;
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
        public string sourceReference;
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

    [Serializable]
    public sealed class GeneralTable
    {
        public int schemaVersion;
        public GeneralDefinition[] items;
    }

    [Serializable]
    public sealed class GeneralDefinition
    {
        public string id;
        public string name;
        public string title;
        public string portraitAssetPath;
        public string era;
        public int military;
        public int loyalty;
        public string specialAbility;
        public string specialAbilityName;
        public string specialAbilityDesc;
        public TerrainBonus terrainBonus;
        public UnitBonus unitBonus;
        public string sourceReference;
    }

    [Serializable]
    public sealed class TerrainBonus
    {
        public int river_plain;
        public int river_delta;
        public int mountain;
        public int mountain_pass;
        public int open_plain;
        public int frontier_plain;
        public int steppe_edge;
        public int huai_river_plain;
        public int mountain_coast;
        public int subtropical;
    }

    [Serializable]
    public sealed class UnitBonus
    {
        public int infantry;
        public int cavalry;
        public int crossbowmen;
        public int siege_engineer;
        public int frontier_cavalry;
        public int garrison;
        public int river_navy;
        public int fire_lance_guard;
    }

    [Serializable]
    public sealed class BuildingTable
    {
        public int schemaVersion;
        public BuildingDefinition[] items;
    }

    [Serializable]
    public sealed class BuildingDefinition
    {
        public string id;
        public string name;
        public string category;
        public string requiresTech;
        public int cost;
        public EffectSet effects;
        public string sourceReference;
    }

    // ========== 战役系统数据结构 ==========

    public enum BattlePhase
    {
        Preparation,
        Execution,
        Resolution
    }

    [Serializable]
    public sealed class BattleSetup
    {
        public string battleId;
        public string attackerFactionId;
        public string defenderFactionId;
        public string terrainId;
        public string attackerFormationId;
        public string defenderFormationId;
        public BattleDeployment[] attackerDeployments;
        public BattleDeployment[] defenderDeployments;
    }

    [Serializable]
    public sealed class BattleDeployment
    {
        public string armyId;
        public string generalId;
        public string formationPosition;
        public int soldierCount;
    }

    [Serializable]
    public sealed class FormationDefinition
    {
        public string id;
        public string name;
        public string description;
        public int attackBonus;
        public int defenseBonus;
        public int mobilityBonus;
        public string[] effectiveAgainst;
        public string[] weakAgainst;
    }

    [Serializable]
    public sealed class BattleTactic
    {
        public string id;
        public string name;
        public string description;
        public int cooldownTurns;
        public int moraleCost;
        public EffectSet effects;
        public string[] requiredFormation;
    }

    [Serializable]
    public sealed class BattleAction
    {
        public string actionType;
        public string unitId;
        public string targetId;
        public string tacticId;
        public string newFormationId;
    }

    [Serializable]
    public sealed class BattleLog
    {
        public int turn;
        public string actionType;
        public string actorId;
        public string targetId;
        public string description;
        public int damageDealt;
        public int damageReceived;
        public int moraleChange;
    }

    [Serializable]
    public sealed class EquipmentDefinition
    {
        public string id;
        public string name;
        public string slot;
        public string era;
        public string requiresTech;
        public int cost;
        public int attackBonus;
        public int defenseBonus;
        public int mobilityBonus;
        public int siegeBonus;
    }

    public struct EquipmentBonus
    {
        public int attack;
        public int defense;
        public int mobility;
        public int siege;
    }

    // ========== 电子斗蛐蛐系统数据结构 ==========

    [Serializable]
    public sealed class BattleConfig
    {
        public string configId;
        public string emperorId;
        public string[] generalIds;
        public string[] unitIds;
        public string[] equipmentIds;
        public string formationId;
        public string[] tacticIds;
    }

    [Serializable]
    public sealed class BattleSession
    {
        public string sessionId;
        public string player1Id;
        public string player2Id;
        public BattleConfig player1Config;
        public BattleConfig player2Config;
        public BattleState state;
        public int currentTurn;
        public List<BattleAction> actions;
        public BattleResult result;
    }

    public enum BattleState
    {
        WaitingForPlayers,
        Configuration,
        InProgress,
        Finished
    }

    [Serializable]
    public sealed class BattleSync
    {
        public string sessionId;
        public int turn;
        public BattleAction player1Action;
        public BattleAction player2Action;
        public bool player1Ready;
        public bool player2Ready;
    }

    [Serializable]
    public sealed class BattleReplay
    {
        public string sessionId;
        public BattleConfig player1Config;
        public BattleConfig player2Config;
        public List<BattleSync> turns;
        public BattleResult result;
        public long timestamp;
    }

    [Serializable]
    public sealed class PlayerBattleState
    {
        public string playerId;
        public int[] unitSoldiers;
        public int[] unitMorale;
        public string currentFormation;
        public int[] tacticCooldowns;
        public bool isReady;
    }
}
