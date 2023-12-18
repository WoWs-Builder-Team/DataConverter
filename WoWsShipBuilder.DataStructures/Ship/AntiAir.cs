namespace WoWsShipBuilder.DataStructures.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public sealed class AntiAir
{
    public AntiAirAura? LongRangeAura { get; init; }

    public AntiAirAura? MediumRangeAura { get; init; }

    public AntiAirAura? ShortRangeAura { get; init; }
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
