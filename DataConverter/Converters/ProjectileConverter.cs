using System;
using System.Collections.Generic;
using System.Linq;
using DataConverter.Data;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.DataStructures;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Projectile;

namespace DataConverter.Converters
{
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
                        //create our object type
                        ArtilleryShell shell = new ArtilleryShell
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
                        shell.SplashDamageCoefficient = currentWgShell.PointsOfDamage.First().Last();

                        projectileList.Add(shell.Name, shell);
                        break;

                    case ProjectileType.Bomb:
                    case ProjectileType.SkipBomb:
                        //create our object type
                        Bomb bomb = new Bomb
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
                        bomb.SplashDamageCoefficient = currentWgBomb.PointsOfDamage.First().Last();

                        projectileList.Add(bomb.Name, bomb);
                        break;

                    case ProjectileType.Torpedo:
                        //create our object type
                        Torpedo torpedo = new Torpedo
                        {
                            Id = currentWgProjectile.Id,
                            Index = currentWgProjectile.Index,
                            Name = currentWgProjectile.Name,
                            ProjectileType = currentWgProjectileType,
                        };

                        //cast WGProjectile object into a WGTorpedo object
                        WgTorpedo currentWgTorpedo = (WgTorpedo)currentWgProjectile;

                        torpedo.Damage = (currentWgTorpedo.AlphaDamage / 3) + currentWgTorpedo.Damage;
                        torpedo.SpottingRange = currentWgTorpedo.VisibilityFactor;
                        torpedo.ArmingTime = currentWgTorpedo.ArmingTime;
                        torpedo.Caliber = currentWgTorpedo.BulletDiametr;
                        torpedo.MaxRange = currentWgTorpedo.MaxDist * 30;
                        torpedo.Speed = currentWgTorpedo.Speed;
                        torpedo.FloodChance = currentWgTorpedo.UwCritical;
                        torpedo.SplashCoeff = currentWgTorpedo.SplashArmorCoeff;
                        torpedo.ExplosionRadius = currentWgTorpedo.SplashCubeSize * 30 / 2;
                        torpedo.IgnoreClasses = currentWgTorpedo.IgnoreClasses.Select(Enum.Parse<ShipClass>).ToList();

                        //change WGTorpedo ammoType to our TorpedoType
                        torpedo.TorpedoType = TorpedoType.Standard;
                        if (currentWgTorpedo.AmmoType.Equals("torpedo_deepwater", StringComparison.OrdinalIgnoreCase))
                        {
                            torpedo.TorpedoType = TorpedoType.DeepWater;
                        }
                        if (currentWgTorpedo.CustomUiPostfix.Equals("_subDefault"))
                        {
                            torpedo.TorpedoType = TorpedoType.Magnetic;
                        }

                        projectileList.Add(torpedo.Name, torpedo);
                        break;

                    case ProjectileType.DepthCharge:
                        //create our object type
                        DepthCharge depthCharge = new DepthCharge
                        {
                            Id = currentWgProjectile.Id,
                            Index = currentWgProjectile.Index,
                            Name = currentWgProjectile.Name,
                            ProjectileType = currentWgProjectileType,
                        };

                        //cast WGProjectile object into a WGDepthCharge object
                        WgDepthCharge currentWgDepthCharge = (WgDepthCharge)currentWgProjectile;

                        depthCharge.Damage = currentWgDepthCharge.AlphaDamage;
                        depthCharge.FireChance = currentWgDepthCharge.BurnProb;
                        depthCharge.FloodChance = currentWgDepthCharge.UwCritical;
                        depthCharge.DetonationTimer = currentWgDepthCharge.Timer;
                        depthCharge.DetonationTimerRng = currentWgDepthCharge.TimerDeltaAbsolute;
                        depthCharge.SinkingSpeed = currentWgDepthCharge.Speed;
                        depthCharge.SinkingSpeedRng = currentWgDepthCharge.SpeedDeltaRelative;
                        depthCharge.ExplosionRadius = currentWgDepthCharge.DepthSplashRadius * 30;

                        var pointsOfDamage = new Dictionary<float, List<float>>();
                        foreach (float[] data in currentWgDepthCharge.PointsOfDamage.Reverse())
                        {
                            float range = data[0];
                            float dmgCoeff = data[1];
                            if (pointsOfDamage.ContainsKey(dmgCoeff))
                            {
                                pointsOfDamage[dmgCoeff].Add(range);
                                pointsOfDamage[dmgCoeff].Sort();
                                pointsOfDamage[dmgCoeff].Reverse();
                            }
                            else
                            {
                                pointsOfDamage.Add(dmgCoeff, new List<float> { range });
                            }
                        }

                        depthCharge.PointsOfDamage = pointsOfDamage;

                        projectileList.Add(depthCharge.Name, depthCharge);
                        break;

                    case ProjectileType.Rocket:
                        //create our object type
                        Rocket rocket = new Rocket
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
                        rocket.SplashDamageCoefficient = currentWgRocket.PointsOfDamage.First().Last();

                        projectileList.Add(rocket.Name, rocket);
                        break;

                    default:
                        throw new InvalidOperationException($"Nation: {currentWgProjectile.TypeInfo.Nation}, ID: {currentWgProjectile.Id}");
                }
            }

            //dictionary containing all the projectiles
            return projectileList;
        }
    }
}
