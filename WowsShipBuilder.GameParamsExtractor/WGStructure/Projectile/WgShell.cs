namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Projectile;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgShell : WgProjectile
{
    public bool AffectedByPtz { get; init; }

    public float AlphaDamage { get; init; }

    public float AlphaPiercingCs { get; init; }

    public float AlphaPiercingHe { get; init; }

    public string AmmoType { get; init; } = string.Empty;

    public bool ApplyPtzCoeff { get; init; }

    public float BulletAirDrag { get; init; }

    public float BulletDetonator { get; init; }

    public float BulletDetonatorThreshold { get; init; }

    public float BulletDiametr { get; init; }

    public float BulletKrupp { get; init; }

    public float BulletMass { get; init; }

    public float BulletRicochetAt { get; init; }

    public float BulletAlwaysRicochetAt { get; init; }

    public float BulletSpeed { get; init; }

    public float BurnProb { get; init; }

    public float DistTile { get; init; }

    public float SplashArmorCoeff { get; init; }

    public float SplashCubeSize { get; init; }

    public string[] IgnoreClasses { get; init; } = Array.Empty<string>();

    public bool IsBomb { get; init; }

    public float DepthSplashRadius { get; init; }

    public float[][] PointsOfDamage { get; init; } = Array.Empty<float[]>();
}
