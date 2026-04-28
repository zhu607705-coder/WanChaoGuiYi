using System;
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
        public int turn;
        public int year;
        public Season season;
        public string playerFactionId;
        public List<FactionState> factions = new List<FactionState>();
        public List<RegionState> regions = new List<RegionState>();
        public List<ArmyState> armies = new List<ArmyState>();
        public List<TurnLogEntry> turnLog = new List<TurnLogEntry>();

        // 天气系统
        public string currentWeatherId;
        public string currentCelestialEventId;

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

            if (previousOwner != null)
            {
                previousOwner.regionIds.Remove(regionId);
            }

            if (!newOwner.regionIds.Contains(regionId))
            {
                newOwner.regionIds.Add(regionId);
            }

            region.ownerFactionId = newOwnerFactionId;
            region.integration = 25;
            region.rebellionRisk = Math.Min(100, region.rebellionRisk + 8);

            return new RegionOwnerChangedPayload
            {
                regionId = regionId,
                previousOwnerFactionId = previousOwnerFactionId,
                newOwnerFactionId = newOwnerFactionId
            };
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
                category = category,
                message = message
            });
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
    }

    [Serializable]
    public sealed class RegionState
    {
        public string id;
        public string ownerFactionId;
        public int population;
        public int foodOutput;
        public int taxOutput;
        public int manpower;
        public int localPower;
        public int rebellionRisk;
        public int integration;
        public int annexationPressure;
        public LandStructure landStructure;

        // 风俗系统
        public string[] customs;
        public int customStability;

        // 建筑系统
        public List<string> buildings;
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
        public string id;
        public string ownerFactionId;
        public string regionId;
        public string unitId;
        public int soldiers;
        public int morale;
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
}
