// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Captain;

public class WgCrewPersonality
{
    public bool CanResetSkillsForFree { get; set; }

    public List<string> Tags { get; set; } = new();

    public string PersonName { get; set; } = string.Empty;
}
