using DataConverter.WGStructure;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using WoWsShipBuilderDataStructures;

namespace DataConverter.Converters
{
    public static class ProjectileConverter
    {
        /// <summary>
        /// Converter method that transforms a <see cref="WGProjectile"/> object into 5 different objects based on projectile type.
        /// </summary>
        /// <param name="jsonInput">The file content of projectile data extracted from game params.</param>
        /// <returns>An Arraylist of dictionaries containing the transformed data based on WGs data.</returns>
        /// <exception cref="InvalidOperationException">Occurs if the provided data cannot be processed.</exception>
        /// <exception cref="ProjectileTypeNotFoundException">Occurs if it's not possible to find the projectile type.</exception>
        public static ArrayList ConvertProjectile(string jsonInput)
        {
            //Arraylist containing 5 dictionaries one for each prjectile type
            //List<Dictionary<string, object>> projectileList = new List<Dictionary<string, object>>();
            ArrayList projectileList = new ArrayList();

            Dictionary<string, Artillery> artilleryList = new Dictionary<string, Artillery>();
            Dictionary<string, Bomb> bombList = new Dictionary<string, Bomb>();
            Dictionary<string, Torpedo> torpedoList = new Dictionary<string, Torpedo>();
            Dictionary<string, DepthCharge> depthChargeList = new Dictionary<string, DepthCharge>();
            Dictionary<string, Rocket> rocketList = new Dictionary<string, Rocket>();

            //deserialize into an object
            var wgProjectile = JsonConvert.DeserializeObject<List<WGProjectile>>(jsonInput) ?? throw new InvalidOperationException();

            //iterate over the entire list to convert and sort everything
            foreach (var currentWgProjectile in wgProjectile)
            {
                ProjectileType currentWgProjectileType = Enum.Parse<ProjectileType>(currentWgProjectile.typeinfo.species);
                switch (currentWgProjectileType)
                {
                    case ProjectileType.Artillery:
                        //create our object type
                        Artillery shell = new Artillery
                        {
                            Id = currentWgProjectile.id,
                            Index = currentWgProjectile.index,
                            Name = currentWgProjectile.name,
                            ProjectileType = currentWgProjectileType
                        };

                        //cast WGProjectile object into a WGShell object
                        WGShell currentWgShell = (WGShell)currentWgProjectile;

                        //change SAP shell ammoType form CS to SAP
                        if (currentWgShell.ammoType.Equals("CS"))
                        {
                            currentWgShell.ammoType = ShellType.SAP.ToString();
                        }

                        ShellType shellType = Enum.Parse<ShellType>(currentWgShell.ammoType);
                        shell.ShellType = shellType;

                        if (shellType == ShellType.SAP)
                        {
                            shell.Penetration = currentWgShell.alphaPiercingCS;
                            shell.RicochetAngle = currentWgShell.bulletRicochetAt;
                            //SAP FireChance = 0 => not relevant => shell.FireChance is set to default value
                            //SAP Krupp = 3 => not relevant => shell.Krupp is set to default value
                        }
                        else if (shellType == ShellType.HE)
                        {
                            shell.Penetration = currentWgShell.alphaPiercingHE;
                            //HE RicochetAngle = 91 => not relevant => shell.RicochetAngle is set to default value
                            shell.FireChance = currentWgShell.burnProb;
                            //HE Krupp = 3 => not relevant => shell.Krupp is set to default value
                        }
                        else
                        {
                            //AP Penetration is not a fixed value => shell.Penetration is set to default value
                            shell.RicochetAngle = currentWgShell.bulletRicochetAt;
                            //AP FirceChance = 0 => not relevant => shell.FireChance is set to default value
                            shell.Krupp = currentWgShell.bulletKrupp;
                        }

                        shell.Damage = currentWgShell.alphaDamage;
                        shell.AirDrag = currentWgShell.bulletAirDrag;
                        shell.FuseTimer = currentWgShell.bulletDetonator;
                        shell.ArmingThreshold = currentWgShell.bulletDetonatorThreshold;
                        shell.Caliber = currentWgShell.bulletDiametr;
                        shell.Mass = currentWgShell.bulletMass;
                        shell.MuzzleVelocity = currentWgShell.bulletSpeed;

                        artilleryList.Add(shell.Name, shell);
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
                        }
                        else
                        {
                            //AP Penetration is not a fixed value => shell.Penetration is set to default value
                            bomb.RicochetAngle = currentWgBomb.bulletRicochetAt;
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

                        bombList.Add(bomb.Name, bomb);
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
                        if (currentWgTorpedo.ammoType.Equals("torpedo_alternative"))
                        {
                            currentWgTorpedo.ammoType = TorpedoType.Homing.ToString();
                        }
                        if (currentWgTorpedo.ammoType.Equals("torpedo_deepwater"))
                        {
                            currentWgTorpedo.ammoType = TorpedoType.DeepWater.ToString();
                        }

                        TorpedoType torpedoType = Enum.Parse<TorpedoType>(currentWgTorpedo.ammoType);
                        torpedo.TorpedoType = torpedoType;

                        torpedo.Damage = (currentWgTorpedo.alphaDamage /3) + currentWgTorpedo.damage;
                        torpedo.SpottingRange = currentWgTorpedo.visibilityFactor;
                        torpedo.ArmingTime = currentWgTorpedo.armingTime;
                        torpedo.Caliber = currentWgTorpedo.bulletDiametr;
                        torpedo.MaxRange = currentWgTorpedo.maxDist *30;
                        torpedo.Speed = currentWgTorpedo.speed;
                        torpedo.FloodChance = currentWgTorpedo.uwCritical;

                        List<ShipClass> ignoreClasses = new List<ShipClass>();
                        foreach (var shipClass in currentWgTorpedo.ignoreClasses)
                        {
                            ignoreClasses.Add(Enum.Parse<ShipClass>(shipClass));
                        }
                        torpedo.IgnoreClasses = ignoreClasses;

                        torpedoList.Add(torpedo.Name, torpedo);
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

                        depthChargeList.Add(depthCharge.Name, depthCharge);
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
                        }
                        else
                        {
                            //AP Penetration is not a fixed value => shell.Penetration is set to default value
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

                        rocketList.Add(rocket.Name, rocket);
                        break;

                    default:
                        throw new ProjectileTypeNotFoundException();
                }
            }
            //add all the dictionaries to the ArrayList
            projectileList.Add(artilleryList);
            projectileList.Add(bombList);
            projectileList.Add(torpedoList);
            projectileList.Add(depthChargeList);
            projectileList.Add(rocketList);

            return projectileList;
        }
    }

    //custom exception to handle cases when the projectile type is not found
    public class ProjectileTypeNotFoundException : Exception
    {
        public ProjectileTypeNotFoundException() { }
        public ProjectileTypeNotFoundException(string message) : base(message) { }
        public ProjectileTypeNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
