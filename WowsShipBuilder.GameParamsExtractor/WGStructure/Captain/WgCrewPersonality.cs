namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Captain;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgCrewPersonality
{
    public bool CanResetSkillsForFree { get; init; }

    public List<string> Tags { get; init; } = new();

    public string PersonName { get; init; } = string.Empty;
}
