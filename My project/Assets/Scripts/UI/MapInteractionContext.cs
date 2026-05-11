using System;

namespace WanChaoGuiYi
{
    public enum MapInteractionMode
    {
        Governance,
        Diplomacy,
        War
    }

    [Serializable]
    public sealed class SelectionContext
    {
        public MapInteractionMode mode;
        public string selectedRegionId;
        public string selectedArmyId;
        public string ownerFactionId;
        public string targetFactionId;
        public bool isFriendly;
        public bool isNeighbor;
        public bool isHostile;
        public string modeEntryReason;
        public string[] availableActions = new string[0];
        public string[] disabledReasons = new string[0];

        public bool HasAvailableAction(string actionId)
        {
            return Contains(availableActions, actionId);
        }

        public bool HasDisabledReasonContaining(string token)
        {
            if (string.IsNullOrEmpty(token) || disabledReasons == null) return false;

            for (int i = 0; i < disabledReasons.Length; i++)
            {
                string reason = disabledReasons[i];
                if (!string.IsNullOrEmpty(reason) && reason.Contains(token))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Contains(string[] values, string expected)
        {
            if (string.IsNullOrEmpty(expected) || values == null) return false;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == expected)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
