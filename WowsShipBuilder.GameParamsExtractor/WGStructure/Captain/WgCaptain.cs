namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Captain;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgCaptain : WgObject
{
    public WgCrewPersonality CrewPersonality { get; init; } = new();

    public Dictionary<string, WgSkill> Skills { get; init; } = new();

    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public Dictionary<string, WgUniqueSkill> UniqueSkills { get; init; } = new();
}
