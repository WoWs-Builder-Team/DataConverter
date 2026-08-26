using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

public class WgAirDefense : WgArmamentModule
{
    public bool IsAa { get; init; }

    /// <summary>
    /// The Aimed Fire parameters of this module, added with game update 15.7. Null for a module without them.
    /// Declared explicitly rather than left to <see cref="Other"/>: it is the only nested object on an air defense
    /// module that is not an AA aura, and <see cref="WgStructureHelper.FindAaAuras"/> must keep filtering on
    /// hitChance, which Aimed Fire does not have.
    /// </summary>
    public WgAimedFire? AimedFire { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JToken> Other { get; init; } = new();

    [JsonIgnore]
    public Dictionary<string, WgAaAura> AntiAirAuras => Other.FindAaAuras();
}

public class WgAaAura
{
    public decimal AreaDamage { get; init; }

    public decimal AreaDamagePeriod { get; init; }

    public decimal BubbleDamage { get; init; }

    [JsonRequired]
    public decimal HitChance { get; init; }

    public int InnerBubbleCount { get; init; }

    public int OuterBubbleCount { get; init; }

    public decimal MaxDistance { get; init; }

    public decimal MinDistance { get; init; }

    public string Type { get; init; } = string.Empty;
}

/// <summary>
/// Aimed Fire, the anti-air mechanic added with game update 15.7. The ship accumulates charge while it has air
/// defense, and spending that charge boosts its AA damage and periodically deals instant damage.
/// </summary>
public class WgAimedFire
{
    public decimal RequiredCharge { get; init; }

    public decimal ChargeSpendingRate { get; init; }

    public decimal DecrementRate { get; init; }

    public decimal DecrementDelay { get; init; }

    public decimal InstantDamageCooldown { get; init; }

    /// <summary>
    /// Instant damage per hit as a share of maximum health, keyed by the class of the ship being attacked rather
    /// than of the ship carrying the module: every ship class ships the same map, and it singles out Destroyer.
    /// </summary>
    /// <remarks>
    /// Deserialized with string keys because a strongly typed dictionary throws on an unknown key, which would fail
    /// a whole nation's conversion. Buff data already mixes in non-ship entity types such as SpaceStation, so a
    /// future build doing the same here should drop the entry rather than abort.
    /// </remarks>
    public Dictionary<string, decimal> InstantDamagePercentage { get; init; } = new();

    /// <summary>
    /// Gets <see cref="InstantDamagePercentage"/> reduced to the entries that name a real ship class.
    /// </summary>
    [JsonIgnore]
    public Dictionary<ShipClass, decimal> InstantDamagePercentageByClass => ToShipClassMap(InstantDamagePercentage);

    internal static Dictionary<ShipClass, decimal> ToShipClassMap(Dictionary<string, decimal> source)
    {
        var result = new Dictionary<ShipClass, decimal>();
        foreach ((string key, decimal value) in source)
        {
            if (Enum.TryParse(key, true, out ShipClass shipClass))
            {
                result[shipClass] = value;
            }
        }

        return result;
    }

    public WgAimedFireModifiers Modifiers { get; init; } = new();

    /// <summary>
    /// Defines how the charge builds up: a fixed increment applied on a repeating timer. The converter turns the two
    /// into a rate per second.
    /// </summary>
    public WgAimedFireTrigger? GameLogicTrigger { get; init; }
}

/// <summary>
/// The damage multipliers Aimed Fire applies while active. Both are keyed by the class of the ship that owns the
/// module, so exactly one entry of each map is ever relevant.
/// </summary>
/// <remarks>
/// Both maps use string keys for the same reason as <see cref="WgAimedFire.InstantDamagePercentage"/>: a strongly
/// typed dictionary throws on an unknown key and would fail a whole nation's conversion.
/// </remarks>
public class WgAimedFireModifiers
{
    [JsonProperty("AAAuraDamage")]
    public Dictionary<string, decimal> AuraDamage { get; init; } = new();

    [JsonProperty("AABubbleDamage")]
    public Dictionary<string, decimal> BubbleDamage { get; init; } = new();

    /// <summary>
    /// Gets the continuous-damage multiplier for the given ship class, or one if the map does not mention it.
    /// </summary>
    public decimal AuraDamageFor(ShipClass shipClass) => WgAimedFire.ToShipClassMap(AuraDamage).GetValueOrDefault(shipClass, 1m);

    /// <summary>
    /// Gets the flak-damage multiplier for the given ship class, or one if the map does not mention it.
    /// </summary>
    public decimal BubbleDamageFor(ShipClass shipClass) => WgAimedFire.ToShipClassMap(BubbleDamage).GetValueOrDefault(shipClass, 1m);
}

public class WgAimedFireTrigger
{
    public WgAimedFireActivator? Activator { get; init; }

    public WgAimedFireAction? Action { get; init; }
}

public class WgAimedFireActivator
{
    /// <summary>
    /// Seconds between two charge ticks.
    /// </summary>
    public decimal Duration { get; init; }
}

public class WgAimedFireAction
{
    /// <summary>
    /// Charge added by a single tick of the activator.
    /// </summary>
    public decimal ProgressIncrement { get; init; }
}
