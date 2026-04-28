namespace WanChaoGuiYi
{
    public interface IGameSystem
    {
        void Initialize(GameContext context);
        void OnTurnStart(GameContext context);
        void ExecuteTurn(GameContext context);
        void OnTurnEnd(GameContext context);
    }

    public abstract class GameSystemBase : IGameSystem
    {
        public virtual void Initialize(GameContext context) { }
        public virtual void OnTurnStart(GameContext context) { }
        public virtual void ExecuteTurn(GameContext context) { }
        public virtual void OnTurnEnd(GameContext context) { }
    }
}
