using System.Collections.Immutable;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Projectile;

public class Torpedo : Projectile
{
    public float SpottingRange { get; init; } // It's visibilityFactor

    public float Damage { get; init; } // (alphaDamage/3)+damage

    public TorpedoType TorpedoType { get; init; }

    public float Caliber { get; init; }

    public float MaxRange { get; init; } // MaxDist*30

    public ImmutableArray<ShipClass> IgnoreClasses { get; init; } = ImmutableArray<ShipClass>.Empty;

    public float Speed { get; init; }

    public float ArmingTime { get; init; }

    public float FloodChance { get; init; } // It's uwCritical

    public float SplashCoeff { get; init; }

    public float ExplosionRadius { get; init; }

    public MagneticTorpedoParams? MagneticTorpedoParams { get; init; }
}

public class MagneticTorpedoParams
{
    public ImmutableDictionary<ShipClass, ImmutableArray<float>> DropTargetAtDistance { get; init; } = ImmutableDictionary<ShipClass, ImmutableArray<float>>.Empty;

    public ImmutableArray<float> MaxTurningSpeed { get; init; } = ImmutableArray<float>.Empty;

    public ImmutableArray<float> TurningAcceleration { get; init; } = ImmutableArray<float>.Empty;

    public ImmutableArray<float> MaxVerticalSpeed { get; init; } = ImmutableArray<float>.Empty;

    public ImmutableArray<float> VerticalAcceleration { get; init; } = ImmutableArray<float>.Empty;

    public ImmutableArray<float> SearchRadius { get; init; } = ImmutableArray<float>.Empty;

    public ImmutableArray<float> SearchAngle { get; init; } = ImmutableArray<float>.Empty;
}
