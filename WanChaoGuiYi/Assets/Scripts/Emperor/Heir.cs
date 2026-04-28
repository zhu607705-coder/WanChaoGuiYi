namespace WanChaoGuiYi
{
    public sealed class Heir
    {
        public HeirState State { get; private set; }

        public Heir(HeirState state)
        {
            State = state;
        }

        public bool IsAdult()
        {
            return State != null && State.age >= 16;
        }
    }
}
