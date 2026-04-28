namespace WanChaoGuiYi
{
    public sealed class Emperor
    {
        public EmperorDefinition Definition { get; private set; }
        public FactionState Faction { get; private set; }

        public Emperor(EmperorDefinition definition, FactionState faction)
        {
            Definition = definition;
            Faction = faction;
        }
    }
}
