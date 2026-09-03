using System.Collections.Immutable;

namespace WoWsShipBuilder.DataStructures.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public sealed class AntiAir
{
    public AntiAirAura? LongRangeAura { get; init; }

    public AntiAirAura? MediumRangeAura { get; init; }

    public AntiAirAura? ShortRangeAura { get; init; }

    /// <summary>
    /// Gets the Aimed Fire parameters of this ship, or null if it has no air defense module. Independent of the
    /// three auras: a handful of ships carry an air defense module that defines Aimed Fire but no aura at all.
    /// </summary>
    public AntiAirAimedFire? AimedFire { get; init; }
}

public sealed class AntiAirAura
{
    public decimal ConstantDps { get; init; }

    public const decimal DamageInterval = 0.285714285714m;

    public decimal FlakDamage { get; init; }

    public int FlakCloudsNumber { get; init; }

    public decimal HitChance { get; init; }

    public decimal MaxRange { get; init; }

    public decimal MinRange { get; init; }
}

/// <summary>
/// Aimed Fire, the anti-air mechanic added with game update 15.7. A ship with air defense builds up charge over time;
/// once the charge reaches <see cref="RequiredCharge"/> the player can spend it to boost AA damage and to deal
/// instant damage on a cooldown.
/// </summary>
public sealed class AntiAirAimedFire
{
    /// <summary>
    /// Gets the charge required before Aimed Fire can be activated.
    /// </summary>
    public decimal RequiredCharge { get; init; }

    /// <summary>
    /// Gets the charge gained per second. The game data states this as an increment applied on a repeating timer;
    /// dividing the increment by the timer's period here keeps consumers from having to model the timer.
    /// </summary>
    public decimal ChargeGainRate { get; init; }

    /// <summary>
    /// Gets the charge consumed per second while Aimed Fire is active.
    /// </summary>
    public decimal ChargeSpendingRate { get; init; }

    /// <summary>
    /// Gets the seconds the charge is held before it starts to drain.
    /// </summary>
    public decimal DecrementDelay { get; init; }

    /// <summary>
    /// Gets the charge lost per second once <see cref="DecrementDelay"/> has elapsed.
    /// </summary>
    public decimal DecrementRate { get; init; }

    /// <summary>
    /// Gets the seconds between two instant damage hits.
    /// </summary>
    public decimal InstantDamageCooldown { get; init; }

    /// <summary>
    /// Gets the instant damage per hit as a share of maximum health, keyed by the class of the ship being attacked.
    /// This one stays a per-class map, unlike the two multipliers below, because the relevant class is not known
    /// until the shot is fired.
    /// </summary>
    public ImmutableDictionary<ShipClass, decimal> InstantDamagePercentage { get; init; } = ImmutableDictionary<ShipClass, decimal>.Empty;

    /// <summary>
    /// Gets the factor applied to <see cref="AntiAirAura.ConstantDps"/> of every aura while Aimed Fire is active.
    /// The game data tunes this per class of the owning ship, so it is already resolved for that ship and is a plain
    /// number rather than a modifier: the app wires it to a multiplier of its own.
    /// </summary>
    public decimal AuraDamageMultiplier { get; init; }

    /// <summary>
    /// Gets the factor applied to <see cref="AntiAirAura.FlakDamage"/> while Aimed Fire is active. Resolved for the
    /// owning ship's class exactly like <see cref="AuraDamageMultiplier"/>.
    /// </summary>
    public decimal BubbleDamageMultiplier { get; init; }
}
