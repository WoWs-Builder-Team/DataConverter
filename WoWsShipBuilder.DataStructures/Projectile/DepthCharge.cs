using System.Collections.Immutable;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Projectile;

public class DepthCharge : Projectile
{
    public float Damage { get; init; }

    public float FireChance { get; init; }

    public float FloodChance { get; init; }

    public float DetonationTimer { get; init; }

    public float SinkingSpeed { get; init; }

    public float ExplosionRadius { get; init; }

    public float SinkingSpeedRng { get; init; }

    public float DetonationTimerRng { get; init; }

    public ImmutableDictionary<float, ImmutableList<float>> PointsOfDamage { get; init; } = ImmutableDictionary<float, ImmutableList<float>>.Empty;
}
