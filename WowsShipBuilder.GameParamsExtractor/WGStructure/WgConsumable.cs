using System.Collections.Immutable;
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

    public string FightersName => Logic.GetValueOrDefault("fightersName", string.Empty).ToString();

    public float PreparationTime { get; init; }

    public Dictionary<string, JToken> Logic { get; init; } = new();

    [JsonExtensionData]
    public Dictionary<string, JToken> RawModifiers { get; init; } = new();

    [JsonIgnore]
    public ImmutableDictionary<string, float> Modifiers => RetrieveModifiers();

    private ImmutableDictionary<string, float> RetrieveModifiers()
    {
        var defaultModifiers = RawModifiers
            .Where(x => x.Value.Type.Equals(JTokenType.Integer) || x.Value.Type.Equals(JTokenType.Float))
            .Select(entry => (entry.Key, Value: entry.Value.ToObject<float>()));
        var additionalModifiers = Logic
            .Where(x => x.Value.Type.Equals(JTokenType.Integer) || x.Value.Type.Equals(JTokenType.Float))
            .Select(prop => (prop.Key, Value: prop.Value.ToObject<float>()))
            .ToList();

        // resolve duplicate keys, entries in `additionalModifiers` will override those in `defaultModifiers`
        var additionalKeys = additionalModifiers.Select(x => x.Key).ToImmutableHashSet();
        return defaultModifiers.Where(x => !additionalKeys.Contains(x.Key))
            .Concat(additionalModifiers)
            .ToImmutableDictionary(x => x.Key, x => x.Value);
    }
}
