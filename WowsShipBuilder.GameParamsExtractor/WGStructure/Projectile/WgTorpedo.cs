namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Projectile;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgTorpedo : WgProjectile
{
    public int AlertDist { get; set; }

    public float AlphaDamage { get; set; }

    public float Damage { get; set; }

    public string AmmoType { get; set; } = string.Empty;

    public float BulletDiametr { get; set; }

    public int MaxDist { get; set; }

    public string[] IgnoreClasses { get; set; } = Array.Empty<string>();

    public float Speed { get; set; }

    public float ArmingTime { get; set; }

    public float UwCritical { get; set; }

    public float VisibilityFactor { get; set; }

    public float SplashArmorCoeff { get; set; }

    public float SplashCubeSize { get; set; }

    public string CustomUiPostfix { get; set; } = string.Empty;
}
