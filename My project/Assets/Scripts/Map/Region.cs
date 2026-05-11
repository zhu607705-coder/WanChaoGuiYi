namespace WanChaoGuiYi
{
    public sealed class Region
    {
        public RegionDefinition Definition { get; private set; }
        public RegionState State { get; private set; }

        public Region(RegionDefinition definition, RegionState state)
        {
            Definition = definition;
            State = state;
        }
    }
}
