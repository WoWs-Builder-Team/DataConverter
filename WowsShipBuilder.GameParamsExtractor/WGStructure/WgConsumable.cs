using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure;

public class WgConsumable : WgObject
{
    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public Dictionary<string, WgStatistics> Variants { get; init; } = new();
}

public class WgStatistics
{
    public string DescIDs { get; init; } = string.Empty;

    public string Group { get; init; } = string.Empty;

    public string IconIDs { get; init; } = string.Empty;

    public int NumConsumables { get; init; }

    public float ReloadTime { get; init; }

    public float WorkTime { get; init; }

    public string FightersName { get; init; } = string.Empty;

    public float PreparationTime { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JToken> RawModifiers { get; init; } = new();

    [JsonIgnore]
    public Dictionary<string, float> Modifiers => RawModifiers
        .Where(x => x.Value.Type.Equals(JTokenType.Integer) || x.Value.Type.Equals(JTokenType.Float))
        .Select(entry => (entry.Key, Value: entry.Value.ToObject<float>()))
        .ToDictionary(entry => entry.Key, entry => entry.Value);
}
