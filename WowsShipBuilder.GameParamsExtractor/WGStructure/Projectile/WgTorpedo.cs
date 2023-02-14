namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Projectile;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgTorpedo : WgProjectile
{
    public int AlertDist { get; init; }

    public float AlphaDamage { get; init; }

    public float Damage { get; init; }

    public string AmmoType { get; init; } = string.Empty;

    public float BulletDiametr { get; init; }

    public int MaxDist { get; init; }

    public string[] IgnoreClasses { get; init; } = Array.Empty<string>();

    public float Speed { get; init; }

    public float ArmingTime { get; init; }

    public float UwCritical { get; init; }

    public float VisibilityFactor { get; init; }

    public float SplashArmorCoeff { get; init; }

    public float SplashCubeSize { get; init; }

    public string CustomUiPostfix { get; init; } = string.Empty;

    public WgSubmarineTorpedoParams SubmarineTorpedoParams { get; init; } = new();
}

public class WgSubmarineTorpedoParams
{
    public Dictionary<string, float[]> DropTargetAtDistance { get; init; } = new();

    public float[] MaxYaw { get; init; } = Array.Empty<float>();

    public float[] YawChangeSpeed { get; init; } = Array.Empty<float>();

    public float[] MaxVerticalSpeed { get; init; } = Array.Empty<float>();

    public float[] VerticalAcceleration { get; init; } = Array.Empty<float>();

    public float[] SearchRadius { get; init; } = Array.Empty<float>();

    public float[] SearchAngle { get; init; } = Array.Empty<float>();
}
