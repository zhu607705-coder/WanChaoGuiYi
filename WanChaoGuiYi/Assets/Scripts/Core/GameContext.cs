namespace WanChaoGuiYi
{
    public sealed class GameContext
    {
        public GameState State { get; private set; }
        public DataRepository Data { get; private set; }
        public EventBus Events { get; private set; }

        public GameContext(GameState state, DataRepository data, EventBus events)
        {
            State = state;
            Data = data;
            Events = events;
        }

        public RegionOwnerChangedPayload ChangeRegionOwner(string regionId, string newOwnerFactionId)
        {
            RegionOwnerChangedPayload ownerChange = State.ChangeRegionOwner(regionId, newOwnerFactionId);
            if (ownerChange == null) return null;

            State.AddLog("map", ownerChange.regionId + "归属变更。");
            Events.Publish(new GameEvent(GameEventType.RegionOwnerChanged, ownerChange.regionId, ownerChange));
            return ownerChange;
        }
    }
}
