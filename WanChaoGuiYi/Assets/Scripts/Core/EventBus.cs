using System;
using System.Collections.Generic;

namespace WanChaoGuiYi
{
    public enum GameEventType
    {
        GameStarted,
        TurnStarted,
        TurnEnded,
        RegionSelected,
        RegionOwnerChanged,
        PolicyApplied,
        BattleResolved,
        EventTriggered,
        SuccessionResolved,
        VictoryChecked,
        TechResearched,
        WeatherChanged,
        CelestialEventOccurred,
        CustomEventOccurred,
        EquipmentUnlocked,
        ArmyMoveStarted,
        ArmyArrived,
        ContactDetected,
        EngagementStarted,
        RegionOccupied,
        GovernanceImpactApplied
    }

    public sealed class GameEvent
    {
        public GameEventType Type { get; private set; }
        public string EntityId { get; private set; }
        public object Payload { get; private set; }

        public GameEvent(GameEventType type, string entityId, object payload)
        {
            Type = type;
            EntityId = entityId;
            Payload = payload;
        }
    }

    public sealed class RegionOwnerChangedPayload
    {
        public string regionId;
        public string previousOwnerFactionId;
        public string newOwnerFactionId;
    }

    public sealed class MapArmyMovementPayload
    {
        public string armyId;
        public string ownerFactionId;
        public string fromRegionId;
        public string toRegionId;
        public string[] route;
        public string task;
    }

    public sealed class EngagementPayload
    {
        public string engagementId;
        public string regionId;
        public string[] attackerArmyIds;
        public string[] defenderArmyIds;
    }

    public sealed class RegionOccupiedPayload
    {
        public string regionId;
        public string previousOwnerFactionId;
        public string newOwnerFactionId;
        public string engagementId;
    }

    public sealed class GovernanceImpactPayload
    {
        public string regionId;
        public int integration;
        public int taxContributionPercent;
        public int foodContributionPercent;
        public int rebellionRisk;
        public int localPower;
        public int annexationPressure;
    }

    public sealed class EventBus
    {
        private readonly Dictionary<GameEventType, Action<GameEvent>> listeners = new Dictionary<GameEventType, Action<GameEvent>>();

        public void Subscribe(GameEventType type, Action<GameEvent> listener)
        {
            if (listener == null) return;

            Action<GameEvent> existing;
            listeners.TryGetValue(type, out existing);
            listeners[type] = existing + listener;
        }

        public void Unsubscribe(GameEventType type, Action<GameEvent> listener)
        {
            if (listener == null) return;

            Action<GameEvent> existing;
            if (!listeners.TryGetValue(type, out existing)) return;

            existing -= listener;
            if (existing == null)
            {
                listeners.Remove(type);
            }
            else
            {
                listeners[type] = existing;
            }
        }

        public void Publish(GameEvent gameEvent)
        {
            Action<GameEvent> listener;
            if (listeners.TryGetValue(gameEvent.Type, out listener))
            {
                listener.Invoke(gameEvent);
            }
        }
    }
}
