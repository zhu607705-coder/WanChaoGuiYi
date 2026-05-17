using System;
using System.Collections.Generic;

namespace WanChaoGuiYi
{
    public enum ArmyTask
    {
        Idle,
        Move,
        Attack,
        Retreat,
        Reinforce,
        Siege
    }

    public enum EngagementPhase
    {
        Forming,
        Resolving,
        Resolved
    }

    public enum OccupationStatus
    {
        Controlled,
        Contested,
        Occupied,
        Rebelling
    }

    public enum RegionSpecialization
    {
        None,
        Grain,
        Military,
        Tax,
        Border,
        Legitimacy,
        Culture,
        Capital
    }

    public enum ControlStage
    {
        Controlled,
        NewlyAttached,
        MilitaryGoverned,
        Pacified,
        Registered
    }

    public enum VisibilityState
    {
        Hidden,
        Known,
        Scouted
    }

    public enum GovernanceActionKind
    {
        Hold,
        Pacify,
        MilitaryGovern,
        RegisterHouseholds,
        Relief,
        TaxPressure,
        Conscription
    }

    public enum MapLensMode
    {
        Governance,
        Risk,
        Economy,
        Legitimacy,
        War,
        Terrain
    }

    [Serializable]
    public sealed class WorldState
    {
        public GameState LegacyState { get; private set; }
        public MapState Map { get; private set; }
        public List<WarRuntimeState> Wars { get; private set; }

        public int Turn { get { return LegacyState != null ? LegacyState.turn : 0; } }
        public Season Season { get { return LegacyState != null ? LegacyState.season : Season.Spring; } }

        public WorldState(GameState legacyState, MapState mapState)
        {
            LegacyState = legacyState;
            Map = mapState;
            Wars = new List<WarRuntimeState>();
        }
    }

    [Serializable]
    public sealed class MapState
    {
        private readonly Dictionary<string, RegionRuntimeState> regionsById = new Dictionary<string, RegionRuntimeState>();
        private readonly Dictionary<string, ArmyRuntimeState> armiesById = new Dictionary<string, ArmyRuntimeState>();
        private readonly Dictionary<string, List<string>> armyIdsByRegionId = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, EngagementRuntimeState> engagementsById = new Dictionary<string, EngagementRuntimeState>();
        private readonly Dictionary<string, string> engagementIdByRegionId = new Dictionary<string, string>();

        [NonSerialized]
        private GameState legacyState;

        public IReadOnlyDictionary<string, RegionRuntimeState> RegionsById { get { return regionsById; } }
        public IReadOnlyDictionary<string, ArmyRuntimeState> ArmiesById { get { return armiesById; } }
        public IReadOnlyDictionary<string, EngagementRuntimeState> EngagementsById { get { return engagementsById; } }

        public void AddRegion(RegionRuntimeState region)
        {
            if (region == null || string.IsNullOrEmpty(region.id)) return;
            regionsById[region.id] = region;
            if (!armyIdsByRegionId.ContainsKey(region.id))
            {
                armyIdsByRegionId.Add(region.id, new List<string>());
            }
        }

        public bool RemoveRegion(string regionId)
        {
            if (string.IsNullOrEmpty(regionId)) return false;

            bool existed = regionsById.Remove(regionId);

            List<string> engagementIdsToRemove = new List<string>();
            foreach (KeyValuePair<string, EngagementRuntimeState> entry in engagementsById)
            {
                if (entry.Value != null && entry.Value.regionId == regionId)
                {
                    engagementIdsToRemove.Add(entry.Key);
                }
            }

            for (int i = 0; i < engagementIdsToRemove.Count; i++)
            {
                RemoveEngagement(engagementIdsToRemove[i]);
            }

            RebuildArmyLocationIndex();
            List<string> armyIdsInRegion;
            if (armyIdsByRegionId.TryGetValue(regionId, out armyIdsInRegion))
            {
                List<string> armyIdsToRemove = new List<string>(armyIdsInRegion);
                for (int i = 0; i < armyIdsToRemove.Count; i++)
                {
                    RemoveArmy(armyIdsToRemove[i]);
                }
            }

            bool hadArmyIndex = armyIdsByRegionId.Remove(regionId);
            bool hadEngagementIndex = engagementIdByRegionId.Remove(regionId);
            RemoveLegacyRegionReferences(regionId);
            return existed || hadArmyIndex || hadEngagementIndex || engagementIdsToRemove.Count > 0;
        }

        public void AddArmy(ArmyRuntimeState army)
        {
            if (army == null || string.IsNullOrEmpty(army.id)) return;
            armiesById[army.id] = army;
            IndexArmyLocation(army.id, army.locationRegionId);
        }

        public bool TryGetRegion(string regionId, out RegionRuntimeState region)
        {
            return regionsById.TryGetValue(regionId, out region);
        }

        public bool TryGetArmy(string armyId, out ArmyRuntimeState army)
        {
            return armiesById.TryGetValue(armyId, out army);
        }

        internal void AttachLegacyState(GameState state)
        {
            legacyState = state;
        }

        internal void AddRuntimeLog(string category, string message)
        {
            if (legacyState != null)
            {
                legacyState.AddLog(category, message);
            }
        }

        public List<ArmyRuntimeState> GetArmiesInRegion(string regionId)
        {
            RebuildArmyLocationIndex();

            List<ArmyRuntimeState> result = new List<ArmyRuntimeState>();
            List<string> armyIds;
            if (!armyIdsByRegionId.TryGetValue(regionId, out armyIds)) return result;

            for (int i = 0; i < armyIds.Count; i++)
            {
                ArmyRuntimeState army;
                if (armiesById.TryGetValue(armyIds[i], out army))
                {
                    result.Add(army);
                }
            }

            return result;
        }

        public List<ArmyRuntimeState> GetHostileArmiesInRegion(string regionId, string factionId)
        {
            List<ArmyRuntimeState> armies = GetArmiesInRegion(regionId);
            List<ArmyRuntimeState> result = new List<ArmyRuntimeState>();
            for (int i = 0; i < armies.Count; i++)
            {
                if (armies[i].ownerFactionId != factionId)
                {
                    result.Add(armies[i]);
                }
            }

            return result;
        }

        public bool MoveArmyToRegion(string armyId, string targetRegionId)
        {
            ArmyRuntimeState army;
            if (!armiesById.TryGetValue(armyId, out army)) return false;
            if (string.IsNullOrEmpty(targetRegionId) || !regionsById.ContainsKey(targetRegionId)) return false;

            RemoveArmyLocationFromAllRegions(armyId);
            army.locationRegionId = targetRegionId;
            IndexArmyLocation(armyId, targetRegionId);
            return true;
        }

        public bool RemoveArmy(string armyId)
        {
            ArmyRuntimeState army;
            if (!armiesById.TryGetValue(armyId, out army)) return false;

            RemoveArmyLocationFromAllRegions(armyId);
            RemoveArmyFromEngagements(armyId);
            armiesById.Remove(armyId);
            return true;
        }

        public void AddEngagement(EngagementRuntimeState engagement)
        {
            if (engagement == null || string.IsNullOrEmpty(engagement.id)) return;
            if (!string.IsNullOrEmpty(engagement.regionId))
            {
                string existingEngagementId;
                if (engagementIdByRegionId.TryGetValue(engagement.regionId, out existingEngagementId) &&
                    existingEngagementId != engagement.id)
                {
                    ClearEngagementArmyReferences(existingEngagementId);
                    engagementsById.Remove(existingEngagementId);
                }
            }

            engagementsById[engagement.id] = engagement;
            if (!string.IsNullOrEmpty(engagement.regionId))
            {
                engagementIdByRegionId[engagement.regionId] = engagement.id;
            }
        }

        public void RemoveEngagement(string engagementId)
        {
            EngagementRuntimeState engagement;
            if (!engagementsById.TryGetValue(engagementId, out engagement)) return;

            ClearEngagementArmyReferences(engagementId);
            engagementsById.Remove(engagementId);
            if (!string.IsNullOrEmpty(engagement.regionId))
            {
                string currentEngagementId;
                if (engagementIdByRegionId.TryGetValue(engagement.regionId, out currentEngagementId) && currentEngagementId == engagementId)
                {
                    engagementIdByRegionId.Remove(engagement.regionId);
                }
            }
        }

        private void RemoveArmyFromEngagements(string armyId)
        {
            if (string.IsNullOrEmpty(armyId)) return;
            foreach (EngagementRuntimeState engagement in engagementsById.Values)
            {
                if (engagement == null) continue;
                if (engagement.attackerArmyIds != null) engagement.attackerArmyIds.Remove(armyId);
                if (engagement.defenderArmyIds != null) engagement.defenderArmyIds.Remove(armyId);
            }
        }

        private void RemoveLegacyRegionReferences(string regionId)
        {
            if (legacyState == null || string.IsNullOrEmpty(regionId)) return;

            if (legacyState.regions != null)
            {
                legacyState.regions.RemoveAll(region => region != null && region.id == regionId);
            }

            if (legacyState.armies != null)
            {
                legacyState.armies.RemoveAll(army => army != null && army.regionId == regionId);
            }

            if (legacyState.factions == null) return;
            for (int i = 0; i < legacyState.factions.Count; i++)
            {
                FactionState faction = legacyState.factions[i];
                if (faction != null && faction.regionIds != null)
                {
                    faction.regionIds.RemoveAll(id => id == regionId);
                }
            }
        }

        public bool TryGetEngagementInRegion(string regionId, out EngagementRuntimeState engagement)
        {
            engagement = null;
            string engagementId;
            return engagementIdByRegionId.TryGetValue(regionId, out engagementId) && engagementsById.TryGetValue(engagementId, out engagement);
        }

        private void IndexArmyLocation(string armyId, string regionId)
        {
            if (string.IsNullOrEmpty(regionId)) return;
            List<string> armyIds;
            if (!armyIdsByRegionId.TryGetValue(regionId, out armyIds))
            {
                armyIds = new List<string>();
                armyIdsByRegionId.Add(regionId, armyIds);
            }

            if (!armyIds.Contains(armyId))
            {
                armyIds.Add(armyId);
            }
        }

        private void RemoveArmyLocation(string armyId, string regionId)
        {
            List<string> armyIds;
            if (string.IsNullOrEmpty(regionId) || !armyIdsByRegionId.TryGetValue(regionId, out armyIds)) return;
            armyIds.Remove(armyId);
        }

        private void RemoveArmyLocationFromAllRegions(string armyId)
        {
            foreach (List<string> armyIds in armyIdsByRegionId.Values)
            {
                armyIds.RemoveAll(id => id == armyId);
            }
        }

        private void RebuildArmyLocationIndex()
        {
            foreach (List<string> armyIds in armyIdsByRegionId.Values)
            {
                armyIds.Clear();
            }

            foreach (ArmyRuntimeState army in armiesById.Values)
            {
                if (army == null || string.IsNullOrEmpty(army.id)) continue;
                IndexArmyLocation(army.id, army.locationRegionId);
            }
        }

        private void ClearEngagementArmyReferences(string engagementId)
        {
            EngagementRuntimeState engagement;
            if (!engagementsById.TryGetValue(engagementId, out engagement) || engagement == null) return;

            ClearArmyEngagementReferences(engagement.attackerArmyIds, engagementId);
            ClearArmyEngagementReferences(engagement.defenderArmyIds, engagementId);
        }

        private void ClearArmyEngagementReferences(List<string> armyIds, string engagementId)
        {
            if (armyIds == null) return;
            for (int i = 0; i < armyIds.Count; i++)
            {
                ArmyRuntimeState army;
                if (armiesById.TryGetValue(armyIds[i], out army) && army.engagementId == engagementId)
                {
                    army.engagementId = null;
                }
            }
        }
    }

    [Serializable]
    public sealed class RegionRuntimeState
    {
        public string id;
        public string ownerFactionId;
        public OccupationStatus occupationStatus;
        public int integration;
        public int taxContributionPercent;
        public int foodContributionPercent;
        public int rebellionRisk;
        public int localPower;
        public int annexationPressure;
        public RegionSpecialization regionSpecialization;
        public ControlStage controlStage;
        public int occupationReservedFood;
        public int occupationPacificationQueueStep;
        public int occupationPacificationQueueTurnsRemaining;
        public int localAcceptance;
        public VisibilityState visibilityState;
        public bool supplyNode;
    }

    [Serializable]
    public sealed class ArmyRuntimeState
    {
        private int moraleValue;

        public string id;
        public string ownerFactionId;
        public string locationRegionId;
        public string targetRegionId;
        public List<string> route = new List<string>();
        public ArmyTask task;
        public string unitId;
        public int soldiers;
        public int morale
        {
            get { return moraleValue; }
            set { moraleValue = DomainMath.Clamp(value, 0, 100); }
        }
        public int supply;
        public int movementPoints;
        public string engagementId;
        public string frontlinePreparedTargetRegionId;
        public int frontlineReservedFood;
        public int frontlinePreparedTurn;
        public string frontlineLogisticsTargetRegionId;
        public string frontlineLogisticsSupplyNodeRegionId;
        public int frontlineLogisticsTotalTurns;
        public int frontlineLogisticsTurnsRemaining;
        public int frontlineLogisticsFoodRemaining;
        public int frontlineLogisticsFoodPerTurn;
        public int frontlineLogisticsSupplyNeedRemaining;
        public int frontlineLogisticsReserveNeedRemaining;
        public int frontlineLogisticsSupplyNodeBuildFoodRemaining;
        public int frontlineLogisticsCompletedSegments;
        public int frontlineLogisticsInterceptionRisk;
        public int frontlineLogisticsLostFood;
        public string frontlineLogisticsConvoyId;
        public int frontlineLogisticsPriority;
        public bool frontlineLogisticsPaused;
        public int frontlineLogisticsRaidPressure;
        public int frontlineLogisticsLastRaidTurn;
        public string frontlineLogisticsLastRaidFactionId;
        public int frontlineLogisticsLastRaidLoss;
    }

    [Serializable]
    public sealed class WarRuntimeState
    {
        public string id;
        public string attackerFactionId;
        public string defenderFactionId;
        public List<string> theaterRegionIds = new List<string>();
        public List<string> engagementIds = new List<string>();
    }

    [Serializable]
    public sealed class EngagementRuntimeState
    {
        public string id;
        public string regionId;
        public EngagementPhase phase;
        public CompactStringList attackerArmyIds = new CompactStringList();
        public CompactStringList defenderArmyIds = new CompactStringList();
        public string initiatingArmyId;
        public string initiatingFactionId;
        public int createdTurn;
        public BattleResult result;
    }

    [Serializable]
    public sealed class CompactStringList : List<string>
    {
        public new void Clear()
        {
            base.Clear();
            TrimExcess();
        }

        public new bool Remove(string item)
        {
            bool removed = base.Remove(item);
            TrimIfEmpty();
            return removed;
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            TrimIfEmpty();
        }

        public new int RemoveAll(Predicate<string> match)
        {
            int removed = base.RemoveAll(match);
            TrimIfEmpty();
            return removed;
        }

        private void TrimIfEmpty()
        {
            if (Count == 0)
            {
                TrimExcess();
            }
        }
    }
}
