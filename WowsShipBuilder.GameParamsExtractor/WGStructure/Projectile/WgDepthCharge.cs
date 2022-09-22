namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Projectile;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgDepthCharge : WgProjectile
{
    public int AlertDist { get; set; }

    public float AlphaDamage { get; set; }

    public string AmmoType { get; set; } = string.Empty;

    public float BurnProb { get; set; }

    public float UwCritical { get; set; }

    public float Timer { get; set; }

    public float Speed { get; set; }

    public float DepthSplashRadius { get; set; }

    public float SpeedDeltaRelative { get; set; }

    public float TimerDeltaAbsolute { get; set; }

    public float[][] PointsOfDamage { get; set; } = Array.Empty<float[]>();
}
