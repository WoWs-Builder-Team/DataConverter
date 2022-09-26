namespace WoWsShipBuilder.DataStructures.Projectile;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class ArtilleryShell : Projectile
{
    public float Damage { get; set; }
    public float Penetration { get; set; }
    public ShellType ShellType { get; set; }
    public float AirDrag { get; set; }
    public float FuseTimer { get; set; }
    public float ArmingThreshold { get; set; }
    public float Caliber { get; set; }
    public float Krupp { get; set; }
    public float Mass { get; set; }
    public float RicochetAngle { get; set; }
    public float AlwaysRicochetAngle { get; set; }
    public float MuzzleVelocity { get; set; }
    public float FireChance { get; set; }
    public float SplashCoeff { get; set; }
    public float ExplosionRadius { get; set; }
    public float DepthSplashRadius { get; set; }
    public float SplashDamageCoefficient { get; set; }
}
