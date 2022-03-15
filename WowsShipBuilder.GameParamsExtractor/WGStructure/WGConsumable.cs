using GameParamsExtractor.WGStructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure;

public class WgConsumable : WGObject
{
    public long Id { get; set; }

    public string Index { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Dictionary<string, Statistics> Variants { get; set; } = new();
}

public class Statistics
{
    public string DescIDs { get; set; } = string.Empty;

    public string Group { get; set; } = string.Empty;

    public string IconIDs { get; set; } = string.Empty;

    public int NumConsumables { get; set; }

    public float ReloadTime { get; set; }

    public float WorkTime { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JToken> RawModifiers { get; set; } = new();

    [JsonIgnore]
    public Dictionary<string, float> Modifiers => RawModifiers
        .Where(x => x.Value.Type.Equals(JTokenType.Integer) || x.Value.Type.Equals(JTokenType.Float))
        .Select(entry => (entry.Key, Value: entry.Value.ToObject<float>()))
        .ToDictionary(entry => entry.Key, entry => entry.Value);
}
