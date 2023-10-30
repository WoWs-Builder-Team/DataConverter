using System.Text.Json;
using System.Text.Json.Serialization;
using WoWsShipBuilder.DataStructures;

namespace WowsShipBuilder.GameParamsExtractor.WGStructure;

public class SkillsTiers
{
    private Dictionary<ShipClass, List<SkillRow>>? positionsByClass;

    [JsonExtensionData]
    public Dictionary<string, JsonElement> RawPositionsByClass { get; init; } = new();

    public Dictionary<ShipClass, List<SkillRow>> GetPositionsByClass() => positionsByClass ??= RawPositionsByClass
        .Select(entry => (Enum.Parse<ShipClass>(entry.Key, true), entry.Value.Deserialize<List<SkillRow>>()))
        .Where(entry => entry.Item2 != null)
        .ToDictionary(entry => entry.Item1, entry => entry.Item2!);
}

public class SkillRow
{
    private List<List<int>>? skillGroups;

    [JsonExtensionData]
    public Dictionary<string, JsonElement> RawSkillGroups { get; init; } = new();

    public List<List<int>> GetSkillGroups() =>
        skillGroups ??= RawSkillGroups.OrderBy(entry => entry.Key)
            .Select(entry => entry.Value.Deserialize<List<int>>()!)
            .ToList();
}
