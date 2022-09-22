namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Captain;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgCrewPersonality
{
    public bool CanResetSkillsForFree { get; set; }

    public List<string> Tags { get; set; } = new();

    public string PersonName { get; set; } = string.Empty;
}
