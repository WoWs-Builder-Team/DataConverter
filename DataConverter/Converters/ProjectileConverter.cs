using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DataConverter.Data;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Projectile;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Projectile;

namespace DataConverter.Converters;

public static class ProjectileConverter
{
    private static readonly HashSet<string> ReportedProjectileTypes = new();

    /// <summary>
    /// Converter method that transforms a <see cref="WgProjectile"/> object into a <see cref="Projectile"/> object.
    /// </summary>
    /// <param name="wgProjectile">The list of projectile data extracted from game params.</param>
    /// <param name="logger">A logger used to log information about the execution.</param>
    /// <returns>A dictionary mapping an ID to a <see cref="Projectile"/> object that contains the transformed data based on WGs data.</returns>
    /// <exception cref="InvalidOperationException">Occurs if the provided data cannot be processed.</exception>
    public static Dictionary<string, Projectile> ConvertProjectile(IEnumerable<WgProjectile> wgProjectile, ILogger? logger)
    {
        //Dictionary containing all projectiles
        Dictionary<string, Projectile> projectileList = new Dictionary<string, Projectile>();

        //iterate over the entire list to convert and sort everything
        foreach (var currentWgProjectile in wgProjectile)
        {
            DataCache.TranslationNames.Add(currentWgProjectile.Name);
            if (!Enum.TryParse(currentWgProjectile.TypeInfo.Species, out ProjectileType currentWgProjectileType))
            {
                if (ReportedProjectileTypes.Add(currentWgProjectile.TypeInfo.Species))
                {
                    logger?.LogWarning("Projectile type not recognized: {}", currentWgProjectile.TypeInfo.Species);
                }
                continue;
            }

            switch (currentWgProjectileType)
            {
                case ProjectileType.Artillery:
                    var shell = ConvertArtilleryShell(currentWgProjectile, currentWgProjectileType);
                    projectileList.Add(shell.Name, shell);
                    break;

                case ProjectileType.Bomb:
                case ProjectileType.SkipBomb:
                    var bomb = ConvertBomb(currentWgProjectile, currentWgProjectileType);
                    projectileList.Add(bomb.Name, bomb);
                    break;

                case ProjectileType.Torpedo:
                    var torpedo = ConvertTorpedo(currentWgProjectile, currentWgProjectileType);
                    projectileList.Add(torpedo.Name, torpedo);
                    break;

                case ProjectileType.DepthCharge:
                    var depthCharge = ConvertDepthCharge(currentWgProjectile, currentWgProjectileType);
                    projectileList.Add(depthCharge.Name, depthCharge);
                    break;

                case ProjectileType.Rocket:
                    var rocket = ConvertRocket(currentWgProjectile, currentWgProjectileType);
                    projectileList.Add(rocket.Name, rocket);
                    break;

                default:
                    throw new InvalidOperationException($"Nation: {currentWgProjectile.TypeInfo.Nation}, ID: {currentWgProjectile.Id}");
            }
        }

        // dictionary containing all the projectiles
        return projectileList;
    }

    private static Rocket ConvertRocket(WgProjectile currentWgProjectile, ProjectileType currentWgProjectileType)
    {
        //create our object type
        var rocket = new RocketBuilder
        {
            Id = currentWgProjectile.Id,
            Index = currentWgProjectile.Index,
            Name = currentWgProjectile.Name,
            ProjectileType = currentWgProjectileType,
        };

        //cast WGProjectile object into a WGBomb object
        WgRocket currentWgRocket = (WgRocket)currentWgProjectile;

        RocketType rocketType = Enum.Parse<RocketType>(currentWgRocket.AmmoType);
        rocket.RocketType = rocketType;

        if (rocketType == RocketType.HE)
        {
            rocket.Penetration = currentWgRocket.AlphaPiercingHe;

            //HE RicochetAngle = 91 => not relevant => shell.RicochetAngle is set to default value
            rocket.FireChance = currentWgRocket.BurnProb;

            //HE Krupp = 3 => not relevant => shell.Krupp is set to default value
            rocket.SplashCoeff = currentWgRocket.SplashArmorCoeff;
            rocket.ExplosionRadius = currentWgRocket.SplashCubeSize * 30 / 2;
        }
        else
        {
            //AP Penetration is not a fixed value => shell.Penetration is set to default value
            rocket.AlwaysRicochetAngle = currentWgRocket.BulletAlwayRiccochetAt;
            rocket.RicochetAngle = currentWgRocket.BulletRicochetAt;

            //AP FireChance = 0 => not relevant => shell.FireChance is set to default value
            rocket.Krupp = currentWgRocket.BulletKrupp;
        }

        rocket.Damage = currentWgRocket.AlphaDamage;
        rocket.AirDrag = currentWgRocket.BulletAirDrag;
        rocket.FuseTimer = currentWgRocket.BulletDetonator;
        rocket.ArmingThreshold = currentWgRocket.BulletDetonatorThreshold;
        rocket.Caliber = currentWgRocket.BulletDiametr;
        rocket.Mass = currentWgRocket.BulletMass;
        rocket.MuzzleVelocity = currentWgRocket.BulletSpeed;
        rocket.DepthSplashRadius = currentWgRocket.DepthSplashRadius * 30;
        rocket.SplashDamageCoefficient = currentWgRocket.PointsOfDamage[0][^1];
        return rocket.Build();
    }

    private static DepthCharge ConvertDepthCharge(WgProjectile currentWgProjectile, ProjectileType currentWgProjectileType)
    {
        var currentWgDepthCharge = (WgDepthCharge)currentWgProjectile;

        var pointsOfDamage = new Dictionary<float, List<float>>();
        foreach (float[] data in currentWgDepthCharge.PointsOfDamage.Reverse())
        {
            float range = data[0];
            float dmgCoeff = data[1];
            if (pointsOfDamage.TryGetValue(dmgCoeff, out List<float>? value))
            {
                value.Add(range);
                value.Sort();
                value.Reverse();
            }
            else
            {
                pointsOfDamage.Add(dmgCoeff, new() { range });
            }
        }

        // create our object type
        DepthCharge depthCharge = new DepthCharge
        {
            Id = currentWgProjectile.Id,
            Index = currentWgProjectile.Index,
            Name = currentWgProjectile.Name,
            ProjectileType = currentWgProjectileType,
            Damage = currentWgDepthCharge.AlphaDamage,
            FireChance = currentWgDepthCharge.BurnProb,
            FloodChance = currentWgDepthCharge.UwCritical,
            DetonationTimer = currentWgDepthCharge.Timer,
            DetonationTimerRng = currentWgDepthCharge.TimerDeltaAbsolute,
            SinkingSpeed = currentWgDepthCharge.Speed,
            SinkingSpeedRng = currentWgDepthCharge.SpeedDeltaRelative,
            ExplosionRadius = currentWgDepthCharge.DepthSplashRadius * 30,
            PointsOfDamage = pointsOfDamage.ToImmutableDictionary(),
        };
        return depthCharge;
    }

    private static Torpedo ConvertTorpedo(WgProjectile currentWgProjectile, ProjectileType currentWgProjectileType)
    {
        //cast WGProjectile object into a WGTorpedo object
        WgTorpedo currentWgTorpedo = (WgTorpedo)currentWgProjectile;

        var torpedoType = currentWgTorpedo.AmmoType.Equals("torpedo_deepwater", StringComparison.OrdinalIgnoreCase) ? TorpedoType.DeepWater : TorpedoType.Standard;
        MagneticTorpedoParams? magneticTorpedoParams = null;

        if (currentWgTorpedo.CustomUiPostfix.Equals("_subDefault"))
        {
            torpedoType = TorpedoType.Magnetic;
            magneticTorpedoParams = new MagneticTorpedoParams
            {
                TurningAcceleration = currentWgTorpedo.SubmarineTorpedoParams.YawChangeSpeed.Take(2).ToImmutableArray(),
                MaxTurningSpeed = currentWgTorpedo.SubmarineTorpedoParams.MaxYaw.Take(2).ToImmutableArray(),
                VerticalAcceleration = currentWgTorpedo.SubmarineTorpedoParams.VerticalAcceleration.Take(2).ToImmutableArray(),
                MaxVerticalSpeed = currentWgTorpedo.SubmarineTorpedoParams.MaxVerticalSpeed.Take(2).ToImmutableArray(),
                SearchAngle = currentWgTorpedo.SubmarineTorpedoParams.SearchAngle.Take(2).ToImmutableArray(),
                SearchRadius = currentWgTorpedo.SubmarineTorpedoParams.SearchRadius.Take(2).Select(x => x / 1000).ToImmutableArray(),
                DropTargetAtDistance = currentWgTorpedo.SubmarineTorpedoParams.DropTargetAtDistance.Where(item => !item.Key.Equals("default")).ToImmutableDictionary(item => Enum.Parse<ShipClass>(item.Key), item => item.Value.Take(2).ToImmutableArray()),
            };
        }

        //create our object type
        Torpedo torpedo = new Torpedo
        {
            Id = currentWgProjectile.Id,
            Index = currentWgProjectile.Index,
            Name = currentWgProjectile.Name,
            ProjectileType = currentWgProjectileType,
            Damage = (currentWgTorpedo.AlphaDamage / 3) + currentWgTorpedo.Damage,
            SpottingRange = currentWgTorpedo.VisibilityFactor,
            ArmingTime = currentWgTorpedo.ArmingTime,
            Caliber = currentWgTorpedo.BulletDiametr,
            MaxRange = currentWgTorpedo.MaxDist * 30,
            Speed = currentWgTorpedo.Speed,
            FloodChance = currentWgTorpedo.UwCritical,
            SplashCoeff = currentWgTorpedo.SplashArmorCoeff,
            ExplosionRadius = currentWgTorpedo.SplashCubeSize * 30 / 2,
            IgnoreClasses = currentWgTorpedo.IgnoreClasses.Select(Enum.Parse<ShipClass>).ToImmutableArray(),
            TorpedoType = torpedoType,
            MagneticTorpedoParams = magneticTorpedoParams,
        };
        return torpedo;
    }

    private static Bomb ConvertBomb(WgProjectile currentWgProjectile, ProjectileType currentWgProjectileType)
    {
        //create our object type
        var bomb = new BombBuilder
        {
            Id = currentWgProjectile.Id,
            Index = currentWgProjectile.Index,
            Name = currentWgProjectile.Name,
            ProjectileType = currentWgProjectileType,
        };

        //cast WGProjectile object into a WGBomb object
        WgBomb currentWgBomb = (WgBomb)currentWgProjectile;

        BombType bombType = Enum.Parse<BombType>(currentWgBomb.AmmoType);
        bomb.BombType = bombType;

        if (bombType == BombType.HE)
        {
            bomb.Penetration = currentWgBomb.AlphaPiercingHe;

            //HE RicochetAngle = 91 => not relevant => shell.RicochetAngle is set to default value
            bomb.FireChance = currentWgBomb.BurnProb;

            //HE Krupp = 3 => not relevant => shell.Krupp is set to default value
            bomb.SplashCoeff = currentWgBomb.SplashArmorCoeff;
            bomb.ExplosionRadius = currentWgBomb.SplashCubeSize * 30 / 2;
        }
        else
        {
            //For bombs, AP Penetration is a fixed value. Don't know how to calculate it tho. => shell.Penetration is set to default value
            bomb.RicochetAngle = currentWgBomb.BulletRicochetAt;
            bomb.AlwaysRicochetAngle = currentWgBomb.BulletAlwayRiccochetAt;

            //AP FireChance = 0 => not relevant => shell.FireChance is set to default value
            bomb.Krupp = currentWgBomb.BulletKrupp;
        }

        bomb.Damage = currentWgBomb.AlphaDamage;
        bomb.AirDrag = currentWgBomb.BulletAirDrag;
        bomb.FuseTimer = currentWgBomb.BulletDetonator;
        bomb.ArmingThreshold = currentWgBomb.BulletDetonatorThreshold;
        bomb.Caliber = currentWgBomb.BulletDiametr;
        bomb.Mass = currentWgBomb.BulletMass;
        bomb.MuzzleVelocity = currentWgBomb.BulletSpeed;
        bomb.DepthSplashRadius = currentWgBomb.DepthSplashRadius * 30;
        bomb.SplashDamageCoefficient = currentWgBomb.PointsOfDamage[0][^1];
        return bomb.Build();
    }

    private static ArtilleryShell ConvertArtilleryShell(WgProjectile currentWgProjectile, ProjectileType currentWgProjectileType)
    {
        //create our object type
        var shell = new ArtilleryShellBuilder()
        {
            Id = currentWgProjectile.Id,
            Index = currentWgProjectile.Index,
            Name = currentWgProjectile.Name,
            ProjectileType = currentWgProjectileType,
        };

        //cast WGProjectile object into a WGShell object
        WgShell currentWgShell = (WgShell)currentWgProjectile;

        ShellType shellType;

        //change SAP shell ammoType form CS to SAP
        if (currentWgShell.AmmoType.Equals("CS"))
        {
            shellType = ShellType.SAP;
            shell.ShellType = shellType;

            shell.Penetration = currentWgShell.AlphaPiercingCs;
            shell.RicochetAngle = currentWgShell.BulletRicochetAt;
            shell.AlwaysRicochetAngle = currentWgShell.BulletAlwaysRicochetAt;

            //SAP FireChance = 0 => not relevant => shell.FireChance is set to default value
            //SAP Krupp = 3 => not relevant => shell.Krupp is set to default value
        }
        else
        {
            shellType = Enum.Parse<ShellType>(currentWgShell.AmmoType);
            shell.ShellType = shellType;

            if (shellType == ShellType.HE)
            {
                shell.Penetration = currentWgShell.AlphaPiercingHe;

                //HE RicochetAngle = 91 => not relevant => shell.RicochetAngle is set to default value
                shell.FireChance = currentWgShell.BurnProb;

                //HE Krupp = 3 => not relevant => shell.Krupp is set to default value
                shell.SplashCoeff = currentWgShell.SplashArmorCoeff;
                shell.ExplosionRadius = currentWgShell.SplashCubeSize * 30 / 2;
            }
            else
            {
                //AP Penetration is not a fixed value => shell.Penetration is set to default value
                shell.RicochetAngle = currentWgShell.BulletRicochetAt;
                shell.AlwaysRicochetAngle = currentWgShell.BulletAlwaysRicochetAt;

                //AP FireChance = 0 => not relevant => shell.FireChance is set to default value
                shell.Krupp = currentWgShell.BulletKrupp;
            }
        }

        shell.Damage = currentWgShell.AlphaDamage;
        shell.AirDrag = currentWgShell.BulletAirDrag;
        shell.FuseTimer = currentWgShell.BulletDetonator;
        shell.ArmingThreshold = currentWgShell.BulletDetonatorThreshold;
        shell.Caliber = currentWgShell.BulletDiametr;
        shell.Mass = currentWgShell.BulletMass;
        shell.MuzzleVelocity = currentWgShell.BulletSpeed;
        shell.DepthSplashRadius = currentWgShell.DepthSplashRadius * 30;
        shell.SplashDamageCoefficient = currentWgShell.PointsOfDamage[0][^1];
        return shell.Build();
    }

    private sealed class ArtilleryShellBuilder : Projectile
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

        public ArtilleryShell Build()
        {
            return new()
            {
                Id = Id,
                Index = Index,
                Name = Name,
                ProjectileType = ProjectileType,
                Damage = Damage,
                Penetration = Penetration,
                ShellType = ShellType,
                AirDrag = AirDrag,
                FuseTimer = FuseTimer,
                ArmingThreshold = ArmingThreshold,
                Caliber = Caliber,
                Krupp = Krupp,
                Mass = Mass,
                RicochetAngle = RicochetAngle,
                AlwaysRicochetAngle = AlwaysRicochetAngle,
                MuzzleVelocity = MuzzleVelocity,
                FireChance = FireChance,
                SplashCoeff = SplashCoeff,
                ExplosionRadius = ExplosionRadius,
                DepthSplashRadius = DepthSplashRadius,
                SplashDamageCoefficient = SplashDamageCoefficient,
            };
        }
    }

    private sealed class BombBuilder : Projectile
    {
        public float Damage { get; set; }

        public float Penetration { get; set; }

        public BombType BombType { get; set; }

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

        public Bomb Build()
        {
            return new()
            {
                Id = Id,
                Index = Index,
                Name = Name,
                ProjectileType = ProjectileType,
                Damage = Damage,
                Penetration = Penetration,
                BombType = BombType,
                AirDrag = AirDrag,
                FuseTimer = FuseTimer,
                ArmingThreshold = ArmingThreshold,
                Caliber = Caliber,
                Krupp = Krupp,
                Mass = Mass,
                RicochetAngle = RicochetAngle,
                AlwaysRicochetAngle = AlwaysRicochetAngle,
                MuzzleVelocity = MuzzleVelocity,
                FireChance = FireChance,
                SplashCoeff = SplashCoeff,
                ExplosionRadius = ExplosionRadius,
                DepthSplashRadius = DepthSplashRadius,
                SplashDamageCoefficient = SplashDamageCoefficient,
            };
        }
    }

    private sealed class RocketBuilder : Projectile
    {
        public float Damage { get; set; }

        public float Penetration { get; set; }

        public RocketType RocketType { get; set; }

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

        public Rocket Build()
        {
            return new()
            {
                Id = Id,
                Index = Index,
                Name = Name,
                ProjectileType = ProjectileType,
                Damage = Damage,
                Penetration = Penetration,
                RocketType = RocketType,
                AirDrag = AirDrag,
                FuseTimer = FuseTimer,
                ArmingThreshold = ArmingThreshold,
                Caliber = Caliber,
                Krupp = Krupp,
                Mass = Mass,
                RicochetAngle = RicochetAngle,
                AlwaysRicochetAngle = AlwaysRicochetAngle,
                MuzzleVelocity = MuzzleVelocity,
                FireChance = FireChance,
                SplashCoeff = SplashCoeff,
                ExplosionRadius = ExplosionRadius,
                DepthSplashRadius = DepthSplashRadius,
                SplashDamageCoefficient = SplashDamageCoefficient,
            };
        }
    }
}
