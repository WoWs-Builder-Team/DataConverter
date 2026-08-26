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
    public Dictionary<ShipClass, decimal> InstantDamagePercentage { get; init; } = new();

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
public class WgAimedFireModifiers
{
    [JsonProperty("AAAuraDamage")]
    public Dictionary<ShipClass, decimal> AuraDamage { get; init; } = new();

    [JsonProperty("AABubbleDamage")]
    public Dictionary<ShipClass, decimal> BubbleDamage { get; init; } = new();
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
