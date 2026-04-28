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
    }
}
