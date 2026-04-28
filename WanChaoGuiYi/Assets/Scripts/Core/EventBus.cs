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
        PolicyApplied,
        BattleResolved,
        EventTriggered,
        SuccessionResolved,
        VictoryChecked
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
