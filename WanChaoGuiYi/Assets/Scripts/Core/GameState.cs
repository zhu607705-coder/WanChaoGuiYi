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
    }

    [Serializable]
    public sealed class TurnLogEntry
    {
        public int turn;
        public string category;
        public string message;
    }
}
