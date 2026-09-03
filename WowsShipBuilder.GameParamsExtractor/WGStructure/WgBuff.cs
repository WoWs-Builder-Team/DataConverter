using System.Collections.Immutable;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure;

/// <summary>
/// A standalone effect definition, stored by the game as type <c>Other</c> and species <c>Modifier</c>.
/// </summary>
/// <remarks>
/// Consumables used to carry their numbers inline in <c>variants.*.logic</c>. The ones added since game build
/// 13015811 name a buff object instead (<c>logic.buff</c> and <c>logic.buffOnSelf</c>) and keep no numbers of their
/// own, so a consumable whose buff is not resolved converts to an empty modifier list.
/// </remarks>
public class WgBuff : WgObject
{
    /// <summary>
    /// The value of <c>typeinfo.type</c> the game uses for buff objects.
    /// </summary>
    public const string GameParamsType = "Other";

    /// <summary>
    /// The value of <c>typeinfo.species</c> that separates buffs from the rest of <see cref="GameParamsType"/>.
    /// That type is a grab bag: only about a third of its objects are buffs, the remainder are interactive objects,
    /// weather, camera paths and similar entries that carry no modifiers and have no converter.
    /// </summary>
    public const string GameParamsSpecies = "Modifier";

    private const string ModifierKey = "modifier";

    private const string LevelKey = "level";

    private const string LevelPrefix = "level_";

    private const float Tolerance = 0.0001f;

    /// <summary>
    /// The per-target maps nested inside a buff are keyed by battle entity, and only six of those entities are ship
    /// classes. Names built from the others (Filth, SpaceStation, Minefield, ...) would be modifiers that never
    /// apply to a ship and that no consumer can render.
    /// </summary>
    private static readonly ImmutableHashSet<string> ShipClassNames = Enum.GetNames<ShipClass>().ToImmutableHashSet(StringComparer.Ordinal);

    public string Index { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets everything the game stores on the buff. The block holding the effects is named <c>modifier</c>,
    /// <c>level_N</c> or <c>level</c> depending on the buff, so it cannot be modelled as one typed property.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JToken> RawData { get; init; } = new();

    /// <summary>
    /// Extracts the effects this buff applies.
    /// </summary>
    /// <returns>
    /// The effects, keyed by modifier name. A buff enumerates every stat the game knows and leaves the ones it does
    /// not touch at their identity value, so only the entries that actually change something are returned.
    /// </returns>
    public ImmutableSortedDictionary<string, float> RetrieveModifiers()
    {
        JObject? effects = SelectEffectBlock();
        if (effects is null)
        {
            return ImmutableSortedDictionary<string, float>.Empty;
        }

        // Sorted rather than hashed so the emitted order does not depend on per-process string hash randomization.
        var results = ImmutableSortedDictionary.CreateBuilder<string, float>(StringComparer.Ordinal);
        foreach (JProperty property in effects.Properties())
        {
            switch (property.Value.Type)
            {
                case JTokenType.Integer:
                case JTokenType.Float:
                    float value = property.Value.ToObject<float>();
                    if (!IsNeutral(value))
                    {
                        results[property.Name] = value;
                    }

                    break;
                case JTokenType.Boolean:
                    // A modifier value is a float, so a flag has to become 1. Only a set flag is an effect: every
                    // buff declares the full list of flags and leaves the ones it does not use at false.
                    if (property.Value.ToObject<bool>())
                    {
                        results[property.Name] = 1f;
                    }

                    break;
                case JTokenType.Object:
                    AddPerShipClassEffects(results, property.Name, (JObject)property.Value);
                    break;
                default:
                    // Strings, arrays and nulls are descriptors, never effects.
                    break;
            }
        }

        return results.ToImmutable();
    }

    /// <summary>
    /// A buff lists every stat the game knows and leaves the untouched ones at their identity value: 1 for a
    /// multiplier, 0 for an additive stat. Those would bury the handful of real effects.
    /// </summary>
    private static bool IsNeutral(float value) => Math.Abs(value) < Tolerance || Math.Abs(value - 1f) < Tolerance;

    private static int ParseTier(string key) => int.TryParse(key.AsSpan(LevelPrefix.Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out int tier) ? tier : -1;

    private static void AddPerShipClassEffects(ImmutableSortedDictionary<string, float>.Builder results, string name, JObject block)
    {
        var perShipClass = block.Properties()
            .Where(property => property.Value.Type is JTokenType.Integer or JTokenType.Float)
            .Where(property => ShipClassNames.Contains(property.Name))
            .ToDictionary(property => property.Name, property => property.Value.ToObject<float>(), StringComparer.Ordinal);

        if (perShipClass.Count == 0 || perShipClass.Values.All(IsNeutral))
        {
            return;
        }

        // One value repeated for every class is a single stat that the game merely lists per class, not six stats.
        if (perShipClass.Values.Distinct().Count() == 1)
        {
            results[name] = perShipClass.Values.First();
            return;
        }

        foreach ((string shipClass, float value) in perShipClass.Where(entry => !IsNeutral(entry.Value)))
        {
            results[$"{name}_{shipClass}"] = value;
        }
    }

    private JObject? SelectEffectBlock()
    {
        if (RawData.TryGetValue(ModifierKey, out JToken? modifierBlock) && modifierBlock is JObject modifierObject)
        {
            return modifierObject;
        }

        // An escalating buff declares one block per tier plus a neutral "level" template. The last tier is the fully
        // escalated effect, which is what the rest of the converter reports for tiered data.
        JObject? topTier = RawData
            .Where(entry => entry.Key.StartsWith(LevelPrefix, StringComparison.Ordinal))
            .Select(entry => (Tier: ParseTier(entry.Key), Block: entry.Value as JObject))
            .Where(entry => entry.Tier > 0 && entry.Block is not null)
            .OrderBy(entry => entry.Tier)
            .Select(entry => entry.Block)
            .LastOrDefault();

        return topTier ?? (RawData.TryGetValue(LevelKey, out JToken? levelBlock) ? levelBlock as JObject : null);
    }
}
