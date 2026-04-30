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

        public List<ArmyRuntimeState> GetArmiesInRegion(string regionId)
        {
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

        public void MoveArmyToRegion(string armyId, string targetRegionId)
        {
            ArmyRuntimeState army;
            if (!armiesById.TryGetValue(armyId, out army)) return;

            RemoveArmyLocation(armyId, army.locationRegionId);
            army.locationRegionId = targetRegionId;
            IndexArmyLocation(armyId, targetRegionId);
        }

        public bool RemoveArmy(string armyId)
        {
            ArmyRuntimeState army;
            if (!armiesById.TryGetValue(armyId, out army)) return false;

            RemoveArmyLocation(armyId, army.locationRegionId);
            armiesById.Remove(armyId);
            return true;
        }

        public void AddEngagement(EngagementRuntimeState engagement)
        {
            if (engagement == null || string.IsNullOrEmpty(engagement.id)) return;
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
    }

    [Serializable]
    public sealed class ArmyRuntimeState
    {
        public string id;
        public string ownerFactionId;
        public string locationRegionId;
        public string targetRegionId;
        public List<string> route = new List<string>();
        public ArmyTask task;
        public string unitId;
        public int soldiers;
        public int morale;
        public int supply;
        public int movementPoints;
        public string engagementId;
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
        public List<string> attackerArmyIds = new List<string>();
        public List<string> defenderArmyIds = new List<string>();
        public string initiatingArmyId;
        public string initiatingFactionId;
        public int createdTurn;
        public BattleResult result;
    }
}
