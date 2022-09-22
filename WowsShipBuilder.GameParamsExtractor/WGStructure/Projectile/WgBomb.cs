// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Projectile;

public class WgBomb : WgProjectile
{
    public bool AffectedByPtz { get; set; }

    public int AlphaDamage { get; set; }

    public float AlphaPiercingCs { get; set; }

    public float AlphaPiercingHe { get; set; }

    public string AmmoType { get; set; } = string.Empty;

    public bool ApplyPtzCoeff { get; set; }

    public float BulletAirDrag { get; set; }

    public float BulletDetonator { get; set; }

    public float BulletDetonatorThreshold { get; set; }

    public float BulletDiametr { get; set; }

    public float BulletKrupp { get; set; }

    public float BulletMass { get; set; }

    public float BulletRicochetAt { get; set; }

    public float BulletAlwayRiccochetAt { get; set; }

    public float BulletSpeed { get; set; }

    public float BurnProb { get; set; }

    public float DistTile { get; set; }

    public string[] IgnoreClasses { get; set; } = Array.Empty<string>();

    public bool IsBomb { get; set; }

    public float SplashArmorCoeff { get; set; }

    public float SplashCubeSize { get; set; }

    public float DepthSplashRadius { get; set; }

    public float[][] PointsOfDamage { get; set; } = Array.Empty<float[]>();
}
