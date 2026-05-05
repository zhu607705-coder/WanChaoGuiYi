using System.Collections.Generic;

namespace WanChaoGuiYi
{
    public interface IDataRepository
    {
        IReadOnlyDictionary<string, EmperorDefinition> Emperors { get; }
        IReadOnlyDictionary<string, RegionDefinition> Regions { get; }
        IReadOnlyDictionary<string, HistoricalLayerDefinition> HistoricalLayers { get; }
        IReadOnlyDictionary<string, PolicyDefinition> Policies { get; }
        IReadOnlyDictionary<string, EventDefinition> Events { get; }
        IReadOnlyDictionary<string, TalentDefinition> Talents { get; }
        IReadOnlyDictionary<string, UnitDefinition> Units { get; }
        IReadOnlyDictionary<string, TechnologyDefinition> Technologies { get; }
        IReadOnlyDictionary<string, VictoryConditionDefinition> VictoryConditions { get; }
        IReadOnlyDictionary<string, GeneralDefinition> Generals { get; }
        IReadOnlyDictionary<string, BuildingDefinition> Buildings { get; }

        EmperorDefinition GetEmperor(string id);
        RegionDefinition GetRegion(string id);
        PolicyDefinition GetPolicy(string id);
        UnitDefinition GetUnit(string id);
    }
}
