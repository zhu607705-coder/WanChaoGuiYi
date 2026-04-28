namespace WanChaoGuiYi
{
    public sealed class Talent
    {
        public TalentDefinition Definition { get; private set; }

        public Talent(TalentDefinition definition)
        {
            Definition = definition;
        }
    }
}
