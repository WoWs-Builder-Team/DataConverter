namespace WoWsShipBuilder.DataStructures.Projectile;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class Bomb : Projectile
{
    public float Damage { get; init; }

    public float Penetration { get; init; }

    public BombType BombType { get; init; }

    public float AirDrag { get; init; }

    public float FuseTimer { get; init; }

    public float ArmingThreshold { get; init; }

    public float Caliber { get; init; }

    public float Krupp { get; init; }

    public float Mass { get; init; }

    public float RicochetAngle { get; init; }

    public float AlwaysRicochetAngle { get; init; }

    public float MuzzleVelocity { get; init; }

    public float FireChance { get; init; }

    public float SplashCoeff { get; init; }

    public float ExplosionRadius { get; init; }

    public float DepthSplashRadius { get; init; }

    public float SplashDamageCoefficient { get; init; }
}
