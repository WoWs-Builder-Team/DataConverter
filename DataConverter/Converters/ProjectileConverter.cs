using DataConverter.WGStructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Common;

namespace DataConverter.Converters
{
    public static class ProjectileConverter
    {
        private static readonly HashSet<string> ReportedProjectileTypes = new();

        /// <summary>
        /// Converter method that transforms a <see cref="WGProjectile"/> object into a <see cref="Projectile"/> object
        /// </summary>
        /// <param name="jsonInput">The file content of projectile data extracted from game params.</param>
        /// <returns>A dictionary mapping an ID to a <see cref="Projectile"/> object that contains the transformed data based on WGs data.</returns>
        /// <exception cref="InvalidOperationException">Occurs if the provided data cannot be processed.</exception>
        public static Dictionary<string, Projectile> ConvertProjectile(string jsonInput)
        {
            //Dictionary containing all projectiles
             Dictionary<string, Projectile> projectileList = new Dictionary<string, Projectile>();

            //deserialize into an object
            var wgProjectile = JsonConvert.DeserializeObject<List<WGProjectile>>(jsonInput) ?? throw new InvalidOperationException();

            //iterate over the entire list to convert and sort everything
            foreach (var currentWgProjectile in wgProjectile)
            {
                Program.TranslationNames.Add(currentWgProjectile.name);
                ProjectileType currentWgProjectileType;
                try
                {
                    currentWgProjectileType = Enum.Parse<ProjectileType>(currentWgProjectile.typeinfo.species);
                }
                catch (Exception)
                {
                    if (ReportedProjectileTypes.Add(currentWgProjectile.typeinfo.species))
                    {
                        Console.WriteLine("Projectile type not recognized: " + currentWgProjectile.typeinfo.species);
                    }
                    continue;
                }
                switch (currentWgProjectileType)
                {
                    case ProjectileType.Artillery:
                        //create our object type
                        ArtilleryShell shell = new ArtilleryShell
                        {
                            Id = currentWgProjectile.id,
                            Index = currentWgProjectile.index,
                            Name = currentWgProjectile.name,
                            ProjectileType = currentWgProjectileType
                        };

                        //cast WGProjectile object into a WGShell object
                        WGShell currentWgShell = (WGShell)currentWgProjectile;

                        ShellType shellType;
                        //change SAP shell ammoType form CS to SAP
                        if (currentWgShell.ammoType.Equals("CS"))
                        {
                            shellType = ShellType.SAP;
                            shell.ShellType = shellType;

                            shell.Penetration = currentWgShell.alphaPiercingCS;
                            shell.RicochetAngle = currentWgShell.bulletRicochetAt;
                            shell.AlwaysRicochetAngle = currentWgShell.bulletAlwaysRicochetAt;
                            //SAP FireChance = 0 => not relevant => shell.FireChance is set to default value
                            //SAP Krupp = 3 => not relevant => shell.Krupp is set to default value
                        }
                        else
                        {
                            shellType = Enum.Parse<ShellType>(currentWgShell.ammoType);
                            shell.ShellType = shellType;

                            if (shellType == ShellType.HE)
                            {
                                shell.Penetration = currentWgShell.alphaPiercingHE;
                                //HE RicochetAngle = 91 => not relevant => shell.RicochetAngle is set to default value
                                shell.FireChance = currentWgShell.burnProb;
                                //HE Krupp = 3 => not relevant => shell.Krupp is set to default value
                                shell.SplashCoeff = currentWgShell.splashArmorCoeff;
                                shell.ExplosionRadius = currentWgShell.splashCubeSize * 30 / 2;
                            }
                            else
                            {
                                //AP Penetration is not a fixed value => shell.Penetration is set to default value
                                shell.RicochetAngle = currentWgShell.bulletRicochetAt;
                                shell.AlwaysRicochetAngle = currentWgShell.bulletAlwaysRicochetAt;
                                //AP FirceChance = 0 => not relevant => shell.FireChance is set to default value
                                shell.Krupp = currentWgShell.bulletKrupp;
                            }
                        }

                        shell.Damage = currentWgShell.alphaDamage;
                        shell.AirDrag = currentWgShell.bulletAirDrag;
                        shell.FuseTimer = currentWgShell.bulletDetonator;
                        shell.ArmingThreshold = currentWgShell.bulletDetonatorThreshold;
                        shell.Caliber = currentWgShell.bulletDiametr;
                        shell.Mass = currentWgShell.bulletMass;
                        shell.MuzzleVelocity = currentWgShell.bulletSpeed;

                        projectileList.Add(shell.Name, shell);
                        break;

                    case ProjectileType.Bomb: 
                    case ProjectileType.SkipBomb:
                        //create our object type
                        Bomb bomb = new Bomb
                        {
                            Id = currentWgProjectile.id,
                            Index = currentWgProjectile.index,
                            Name = currentWgProjectile.name,
                            ProjectileType = currentWgProjectileType
                        };

                        //cast WGProjectile object into a WGBomb object
                        WGBomb currentWgBomb = (WGBomb)currentWgProjectile;

                        BombType bombType = Enum.Parse<BombType>(currentWgBomb.ammoType);
                        bomb.BombType = bombType;

                        if (bombType == BombType.HE)
                        {
                            bomb.Penetration = currentWgBomb.alphaPiercingHE;
                            //HE RicochetAngle = 91 => not relevant => shell.RicochetAngle is set to default value
                            bomb.FireChance = currentWgBomb.burnProb;
                            //HE Krupp = 3 => not relevant => shell.Krupp is set to default value
                            bomb.SplashCoeff = currentWgBomb.splashArmorCoeff;
                            bomb.ExplosionRadius = currentWgBomb.splashCubeSize * 30 / 2;
                        }
                        else
                        {
                            //For bombs, AP Penetration is a fixed value. Don't know how to calculate it tho. => shell.Penetration is set to default value
                            bomb.RicochetAngle = currentWgBomb.bulletRicochetAt;
                            bomb.AlwaysRicochetAngle = currentWgBomb.bulletAlwayRiccochetAt;
                            //AP FirceChance = 0 => not relevant => shell.FireChance is set to default value
                            bomb.Krupp = currentWgBomb.bulletKrupp;
                        }

                        bomb.Damage = currentWgBomb.alphaDamage;
                        bomb.AirDrag = currentWgBomb.bulletAirDrag;
                        bomb.FuseTimer = currentWgBomb.bulletDetonator;
                        bomb.ArmingThreshold = currentWgBomb.bulletDetonatorThreshold;
                        bomb.Caliber = currentWgBomb.bulletDiametr;
                        bomb.Mass = currentWgBomb.bulletMass;
                        bomb.MuzzleVelocity = currentWgBomb.bulletSpeed;

                        projectileList.Add(bomb.Name, bomb);
                        break;

                    case ProjectileType.Torpedo:
                        //create our object type
                        Torpedo torpedo = new Torpedo
                        {
                            Id = currentWgProjectile.id,
                            Index = currentWgProjectile.index,
                            Name = currentWgProjectile.name,
                            ProjectileType = currentWgProjectileType
                        };

                        //cast WGProjectile object into a WGTorpedo object
                        WGTorpedo currentWgTorpedo = (WGTorpedo)currentWgProjectile;

                        //change WGTorpedo ammoType to our TorpedoType
                        if (currentWgTorpedo.ammoType.Equals("torpedo_deepwater"))
                        {
                            torpedo.TorpedoType = TorpedoType.DeepWater;
                        }
                        else
                        {
                            torpedo.TorpedoType = TorpedoType.Normal;
                        }

                        torpedo.Damage = (currentWgTorpedo.alphaDamage / 3) + currentWgTorpedo.damage;
                        torpedo.SpottingRange = currentWgTorpedo.visibilityFactor;
                        torpedo.ArmingTime = currentWgTorpedo.armingTime;
                        torpedo.Caliber = currentWgTorpedo.bulletDiametr;
                        torpedo.MaxRange = currentWgTorpedo.maxDist * 30;
                        torpedo.Speed = currentWgTorpedo.speed;
                        torpedo.FloodChance = currentWgTorpedo.uwCritical;
                        torpedo.SplashCoeff = currentWgTorpedo.splashArmorCoeff;
                        torpedo.ExplosionRadius = currentWgTorpedo.splashCubeSize * 30 / 2;

                        List<ShipClass> ignoreClasses = new List<ShipClass>();
                        foreach (var shipClass in currentWgTorpedo.ignoreClasses)
                        {
                            ignoreClasses.Add(Enum.Parse<ShipClass>(shipClass));
                        }
                        torpedo.IgnoreClasses = ignoreClasses;

                        projectileList.Add(torpedo.Name, torpedo);
                        break;

                    case ProjectileType.DepthCharge:
                        //create our object type
                        DepthCharge depthCharge = new DepthCharge
                        {
                            Id = currentWgProjectile.id,
                            Index = currentWgProjectile.index,
                            Name = currentWgProjectile.name,
                            ProjectileType = currentWgProjectileType
                        };

                        //cast WGProjectile object into a WGDepthCharge object
                        WGDepthCharge currentWgDepthCharge = (WGDepthCharge)currentWgProjectile;    

                        depthCharge.Damage = currentWgDepthCharge.alphaDamage;
                        depthCharge.FireChance = currentWgDepthCharge.burnProb;
                        depthCharge.FloodChance = currentWgDepthCharge.uwCritical;
                        depthCharge.DetonationTimer = currentWgDepthCharge.timer;
                        depthCharge.SinkingSpeed = currentWgDepthCharge.speed;
                        depthCharge.ExplosionRadius = currentWgDepthCharge.splashCubeSize * 30 / 2;

                        projectileList.Add(depthCharge.Name, depthCharge);
                        break;

                    case ProjectileType.Rocket:
                        //create our object type
                        Rocket rocket = new Rocket
                        {
                            Id = currentWgProjectile.id,
                            Index = currentWgProjectile.index,
                            Name = currentWgProjectile.name,
                            ProjectileType = currentWgProjectileType
                        };

                        //cast WGProjectile object into a WGBomb object
                        WGRocket currentWgRocket = (WGRocket)currentWgProjectile;

                        RocketType rocketType = Enum.Parse<RocketType>(currentWgRocket.ammoType);
                        rocket.RocketType = rocketType;

                        if (rocketType == RocketType.HE)
                        {
                            rocket.Penetration = currentWgRocket.alphaPiercingHE;
                            //HE RicochetAngle = 91 => not relevant => shell.RicochetAngle is set to default value
                            rocket.FireChance = currentWgRocket.burnProb;
                            //HE Krupp = 3 => not relevant => shell.Krupp is set to default value
                            rocket.SplashCoeff = currentWgRocket.splashArmorCoeff;
                            rocket.ExplosionRadius = currentWgRocket.splashCubeSize * 30 / 2;
                        }
                        else
                        {
                            //AP Penetration is not a fixed value => shell.Penetration is set to default value
                            rocket.AlwaysRicochetAngle = currentWgRocket.bulletAlwayRiccochetAt;
                            rocket.RicochetAngle = currentWgRocket.bulletRicochetAt;
                            //AP FirceChance = 0 => not relevant => shell.FireChance is set to default value
                            rocket.Krupp = currentWgRocket.bulletKrupp;
                        }

                        rocket.Damage = currentWgRocket.alphaDamage;
                        rocket.AirDrag = currentWgRocket.bulletAirDrag;
                        rocket.FuseTimer = currentWgRocket.bulletDetonator;
                        rocket.ArmingThreshold = currentWgRocket.bulletDetonatorThreshold;
                        rocket.Caliber = currentWgRocket.bulletDiametr;          
                        rocket.Mass = currentWgRocket.bulletMass;
                        rocket.MuzzleVelocity = currentWgRocket.bulletSpeed;

                        projectileList.Add(rocket.Name, rocket);
                        break;

                    default:
                        throw new InvalidOperationException($"Nation: {currentWgProjectile.typeinfo.nation}, ID: {currentWgProjectile.id}");
                }
            }
            //dictionary containing all the projectiles
            return projectileList;
        }
    }
}
