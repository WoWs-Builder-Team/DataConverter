namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Projectile;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgDepthCharge : WgProjectile
{
    public int AlertDist { get; init; }

    public float AlphaDamage { get; init; }

    public string AmmoType { get; init; } = string.Empty;

    public float BurnProb { get; init; }

    public float UwCritical { get; init; }

    public float Timer { get; init; }

    public float Speed { get; init; }

    public float DepthSplashRadius { get; init; }

    public float SpeedDeltaRelative { get; init; }

    public float TimerDeltaAbsolute { get; init; }

    public float[][] PointsOfDamage { get; init; } = Array.Empty<float[]>();
}
