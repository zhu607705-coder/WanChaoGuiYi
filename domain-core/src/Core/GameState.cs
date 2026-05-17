using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace WanChaoGuiYi
{
    public enum Season
    {
        Spring,
        Autumn
    }

    [Serializable]
    public sealed class GameState
    {
        public const int MaxTurnLogEntries = 2000;
        public const int MaxCurrentTurnLogEntries = MaxTurnLogEntries * 2;

        public int turn;
        public int year;
        public Season season;
        public string playerFactionId;
        public List<FactionState> factions = new List<FactionState>();
        public List<RegionState> regions = new List<RegionState>();
        public List<ArmyState> armies = new List<ArmyState>();
        public List<TurnLogEntry> turnLog = new List<TurnLogEntry>();

        [NonSerialized]
        private MapState runtimeMap;

        // 天气系统
        public string currentWeatherId;
        public string currentCelestialEventId;

        // 外交系统
        public DiplomaticRelationList diplomaticRelations = new DiplomaticRelationList();

        // 谍报系统
        public List<EspionageOperation> activeOperations = new List<EspionageOperation>();

        public FactionState FindFaction(string id)
        {
            for (int i = 0; i < factions.Count; i++)
            {
                if (factions[i].id == id) return factions[i];
            }

            return null;
        }

        public RegionState FindRegion(string id)
        {
            for (int i = 0; i < regions.Count; i++)
            {
                if (regions[i].id == id) return regions[i];
            }

            return null;
        }

        public RegionOwnerChangedPayload ChangeRegionOwner(string regionId, string newOwnerFactionId)
        {
            RegionState region = FindRegion(regionId);
            if (region == null || string.IsNullOrEmpty(newOwnerFactionId)) return null;
            if (region.ownerFactionId == newOwnerFactionId) return null;

            string previousOwnerFactionId = region.ownerFactionId;
            FactionState previousOwner = FindFaction(previousOwnerFactionId);
            FactionState newOwner = FindFaction(newOwnerFactionId);
            if (newOwner == null) return null;
            if (newOwner.regionIds == null) return null;
            if (previousOwner != null && previousOwner.regionIds == null) return null;

            if (previousOwner != null)
            {
                previousOwner.regionIds.RemoveAll(id => id == regionId);
            }

            newOwner.regionIds.RemoveAll(id => id == regionId);
            newOwner.regionIds.Add(regionId);

            region.ownerFactionId = newOwnerFactionId;
            SyncRuntimeRegionOwner(regionId, newOwnerFactionId);

            return new RegionOwnerChangedPayload
            {
                regionId = regionId,
                previousOwnerFactionId = previousOwnerFactionId,
                newOwnerFactionId = newOwnerFactionId
            };
        }

        internal void AttachRuntimeMap(MapState mapState)
        {
            runtimeMap = mapState;
        }

        internal void SyncRuntimeRegionOwner(string regionId, string ownerFactionId)
        {
            RegionRuntimeState runtimeRegion;
            if (runtimeMap != null && runtimeMap.TryGetRegion(regionId, out runtimeRegion) && runtimeRegion != null)
            {
                runtimeRegion.ownerFactionId = ownerFactionId;
            }
        }

        public void AdvanceHalfYear()
        {
            turn++;
            if (season == Season.Spring)
            {
                season = Season.Autumn;
            }
            else
            {
                season = Season.Spring;
                year++;
            }
        }

        public void AddLog(string category, string message)
        {
            turnLog.Add(new TurnLogEntry
            {
                turn = turn,
                category = category ?? string.Empty,
                message = message ?? string.Empty
            });

            PruneTurnLog();
        }

        private void PruneTurnLog()
        {
            if (turnLog.Count <= MaxTurnLogEntries) return;

            PruneWholePastTurnLogGroups(false);

            if (turnLog.Count > MaxCurrentTurnLogEntries)
            {
                PruneWholePastTurnLogGroups(true);
            }

            while (turnLog.Count > MaxCurrentTurnLogEntries)
            {
                turnLog.RemoveAt(0);
            }
        }

        private void PruneWholePastTurnLogGroups(bool forceWhenOversized)
        {
            while (turnLog.Count > MaxTurnLogEntries)
            {
                int oldestPastTurn;
                int oldestPastTurnCount;
                if (!TryFindOldestPastTurnLogGroup(out oldestPastTurn, out oldestPastTurnCount)) return;

                if (!forceWhenOversized && turnLog.Count - oldestPastTurnCount < MaxTurnLogEntries) return;

                turnLog.RemoveAll(entry => entry.turn == oldestPastTurn);
            }
        }

        private bool TryFindOldestPastTurnLogGroup(out int oldestPastTurn, out int oldestPastTurnCount)
        {
            oldestPastTurn = 0;
            oldestPastTurnCount = 0;
            bool found = false;

            for (int i = 0; i < turnLog.Count; i++)
            {
                if (turnLog[i].turn == turn) continue;

                if (!found)
                {
                    oldestPastTurn = turnLog[i].turn;
                    found = true;
                }

                if (turnLog[i].turn == oldestPastTurn)
                {
                    oldestPastTurnCount++;
                }
            }

            return found;
        }
    }

    [Serializable]
    public sealed class FactionState
    {
        public string id;
        public string name;
        public string emperorId;
        public int money;
        public int food;
        public int legitimacy;
        public int courtFactionPressure;
        public int successionRisk;
        public int stableSuccessions;
        public HeirState heir;
        public List<string> regionIds = new List<string>();
        public List<string> talentIds = new List<string>();
        public List<string> completedReformIds = new List<string>();

        // 科技系统
        public int researchPoints;
        public List<string> completedTechIds = new List<string>();
        public string currentResearchId;

        // 天气系统
        public int weatherResilience;
        public int disasterMitigation;

        // 谍报系统
        public int espionageDefense;

        // 帝皇被动乘数（由 EmperorSkillSystem 每回合重置，不累积）
        public float taxMultiplier = 1f;
        public float foodMultiplier = 1f;
        public float armyAttackMultiplier = 1f;
        public float armyDefenseMultiplier = 1f;
        public float talentMultiplier = 1f;
    }

    [Serializable]
    public sealed class RegionState
    {
        private int integrationValue;
        private OccupationStatus occupationStatusValue;
        private int taxContributionPercentValue;
        private int foodContributionPercentValue;

        public string id;
        public string ownerFactionId;
        public int population;
        public int foodOutput;
        public int taxOutput;
        public int manpower;
        public int localPower;
        public int rebellionRisk;
        public int integration
        {
            get { return integrationValue; }
            set
            {
                integrationValue = value;
                NormalizeContributionCaps();
            }
        }

        public OccupationStatus occupationStatus
        {
            get { return occupationStatusValue; }
            set
            {
                occupationStatusValue = value;
                NormalizeContributionCaps();
            }
        }

        public int taxContributionPercent
        {
            get { return taxContributionPercentValue; }
            set { taxContributionPercentValue = NormalizeContributionPercent(value); }
        }

        public int foodContributionPercent
        {
            get { return foodContributionPercentValue; }
            set { foodContributionPercentValue = NormalizeContributionPercent(value); }
        }

        public int annexationPressure;
        public LandStructure landStructure;
        public RegionSpecialization regionSpecialization;
        public ControlStage controlStage;
        public int occupationReservedFood;
        public int occupationPacificationQueueStep;
        public int occupationPacificationQueueTurnsRemaining;
        public int localAcceptance;
        public VisibilityState visibilityState;
        public bool supplyNode;

        // 风俗系统
        public string[] customs;
        public int customStability;

        // 建筑系统
        public List<string> buildings;

        private void NormalizeContributionCaps()
        {
            taxContributionPercentValue = NormalizeContributionPercent(taxContributionPercentValue);
            foodContributionPercentValue = NormalizeContributionPercent(foodContributionPercentValue);
        }

        private int NormalizeContributionPercent(int value)
        {
            int clamped = DomainMath.Clamp(value, 0, 100);
            if ((occupationStatusValue == OccupationStatus.Occupied ||
                    integrationValue <= StrategyCausalRules.OccupiedIntegration) &&
                clamped > StrategyCausalRules.OccupiedContributionPercent)
            {
                return StrategyCausalRules.OccupiedContributionPercent;
            }

            return clamped;
        }
    }

    [Serializable]
    public sealed class HeirState
    {
        public string name;
        public int age;
        public int legitimacy;
        public int ability;
    }

    [Serializable]
    public sealed class ArmyState
    {
        private int moraleValue;

        public string id;
        public string ownerFactionId;
        public string regionId;
        public string unitId;
        public int soldiers;
        public int morale
        {
            get { return moraleValue; }
            set { moraleValue = DomainMath.Clamp(value, 0, 100); }
        }
        public int movementProgress;

        // 装备系统
        public string weaponSlot;
        public string armorSlot;
        public string specialSlot;
    }

    [Serializable]
    public sealed class TurnLogEntry
    {
        public int turn;
        public string category;
        public string message;
    }

    // ========== 外交系统 ==========

    public enum DiplomacyStatus
    {
        Neutral,
        NonAggression,
        Alliance,
        Vassal,
        Tributary,
        AtWar
    }

    [Serializable]
    public sealed class DiplomaticRelation
    {
        public string factionA;
        public string factionB;
        public DiplomacyStatus status;
        public int opinion;
        public int turnsRemaining;
        public int grudge;
        public bool isPlayerInvolved;

        public string GetOtherFaction(string selfId)
        {
            return selfId == factionA ? factionB : factionA;
        }
    }

    [Serializable]
    public sealed class DiplomaticRelationList : Collection<DiplomaticRelation>
    {
        protected override void InsertItem(int index, DiplomaticRelation item)
        {
            if (item == null || string.IsNullOrEmpty(item.factionA) || string.IsNullOrEmpty(item.factionB))
            {
                return;
            }

            int existingIndex = FindPairIndex(item.factionA, item.factionB);
            if (existingIndex >= 0)
            {
                base.SetItem(existingIndex, item);
                return;
            }

            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, DiplomaticRelation item)
        {
            if (item == null || string.IsNullOrEmpty(item.factionA) || string.IsNullOrEmpty(item.factionB))
            {
                return;
            }

            int existingIndex = FindPairIndex(item.factionA, item.factionB);
            if (existingIndex >= 0 && existingIndex != index)
            {
                base.RemoveItem(existingIndex);
                if (existingIndex < index)
                {
                    index--;
                }
            }

            base.SetItem(index, item);
        }

        private int FindPairIndex(string factionA, string factionB)
        {
            for (int i = 0; i < Count; i++)
            {
                DiplomaticRelation relation = this[i];
                if (relation == null) continue;
                if ((relation.factionA == factionA && relation.factionB == factionB) ||
                    (relation.factionA == factionB && relation.factionB == factionA))
                {
                    return i;
                }
            }

            return -1;
        }
    }

    // ========== 谍报系统 ==========

    public enum EspionageActionType
    {
        ScoutIntel,
        Sabotage,
        SpreadRumors,
        Assassinate
    }

    [Serializable]
    public sealed class EspionageOperation
    {
        public string id;
        public string agentFactionId;
        public string targetFactionId;
        public EspionageActionType actionType;
        public int progress;
        public int detectionRisk;
        public string targetEntityId;
    }

    [Serializable]
    public sealed class DiplomacyProposal
    {
        public string fromFactionId;
        public string toFactionId;
        public DiplomacyStatus proposedStatus;
        public int moneyOffer;
        public int foodOffer;
        public bool isTrap;
    }
}
