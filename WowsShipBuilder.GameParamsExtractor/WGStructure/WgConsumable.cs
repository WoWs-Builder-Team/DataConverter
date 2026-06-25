using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
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

    public string ConsumableType { get; init; } = string.Empty;

    public int NumConsumables { get; init; }

    public float ReloadTime { get; init; }

    public float WorkTime { get; init; }

    [JsonIgnore]
    public string FightersName => Logic.GetValueOrDefault("fightersName", RawModifiers.GetValueOrDefault("fightersName", string.Empty)).ToString();

    public float PreparationTime { get; init; }

    /// <summary>
    /// Lifecycle type of the consumable. 0 = classic charge-based, 1 = time-based ("capacity") pool.
    /// Declared explicitly so it is not emitted as a stray modifier.
    /// </summary>
    public int LifeCycleType { get; init; }

    /// <summary>
    /// For time-based consumables, the total usage time pool in seconds (game field <c>maxCapacity</c>).
    /// Declared explicitly so it is not emitted as a stray modifier.
    /// </summary>
    public float MaxCapacity { get; init; }

    public Dictionary<string, JToken> Logic { get; init; } = new();

    [JsonExtensionData]
    public Dictionary<string, JToken> RawModifiers { get; init; } = new();

    public ImmutableDictionary<string, float> RetrieveModifiers(ILogger logger)
    {
        var defaultModifiers = RawModifiers
            .Where(x => x.Value.Type.Equals(JTokenType.Integer) || x.Value.Type.Equals(JTokenType.Float))
            .Select(entry => (entry.Key, Value: entry.Value.ToObject<float>()));

        List<(string Key, float Value)> additionalModifiers;
        if (RawModifiers.ContainsKey("modifiers"))
        {
            // Legacy processing for modifiers
            logger.LogWarning("Legacy modifier processing detected");
            additionalModifiers = RawModifiers
                .Where(x => x.Key.Equals("modifiers", StringComparison.OrdinalIgnoreCase) && x.Value.Type.Equals(JTokenType.Object))
                .SelectMany(x => x.Value.Children<JProperty>())
                .Where(x => x.Value.Type.Equals(JTokenType.Integer) || x.Value.Type.Equals(JTokenType.Float))
                .Select(prop => (Key: prop.Name, Value: prop.Value.ToObject<float>()))
                .ToList();
        }
        else
        {
            // new processing for additional modifiers
            additionalModifiers = Logic
                .Where(x => x.Value.Type.Equals(JTokenType.Integer) || x.Value.Type.Equals(JTokenType.Float))
                .Select(prop => (prop.Key, Value: prop.Value.ToObject<float>()))
                .ToList();

            if (Logic.TryGetValue("modifiers", out var modifiersToken) && modifiersToken is { Type: JTokenType.Object, HasValues: true })
            {
                modifiersToken.ToObject<Dictionary<string, JToken>>()!
                    .Where(x => x.Value.Type.Equals(JTokenType.Integer) || x.Value.Type.Equals(JTokenType.Float))
                    .ToList()
                    .ForEach(prop => additionalModifiers.Add((prop.Key, Value: prop.Value.ToObject<float>())));
            }
        }

        // resolve duplicate keys, entries in `additionalModifiers` will override those in `defaultModifiers`
        var additionalKeys = additionalModifiers.Select(x => x.Key).ToImmutableHashSet();
        return defaultModifiers.Where(x => !additionalKeys.Contains(x.Key))
            .Concat(additionalModifiers)
            .ToImmutableDictionary(x => x.Key, x => x.Value);
    }
}
