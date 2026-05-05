namespace WanChaoGuiYi
{
    public sealed class Army
    {
        public ArmyState State { get; private set; }
        public UnitDefinition Unit { get; private set; }

        public Army(ArmyState state, UnitDefinition unit)
        {
            State = state;
            Unit = unit;
        }
    }
}
