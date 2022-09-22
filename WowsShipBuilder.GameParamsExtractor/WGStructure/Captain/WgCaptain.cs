namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Captain;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgCaptain : WgObject
{
    public WgCrewPersonality CrewPersonality { get; set; } = new();

    public Dictionary<string, WgSkill> Skills { get; set; } = new();

    public long Id { get; set; }

    public string Index { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Dictionary<string, WgUniqueSkill> UniqueSkills { get; set; } = new();
}
