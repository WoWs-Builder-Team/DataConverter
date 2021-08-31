using System.Collections.Generic;

namespace WoWsShipBuilderDataStructures
{
    public class Projectile
    {
        public long Id { get; set; }
        public string Index { get; set; }
        public string Name { get; set; }
        public ProjectileType ProjectileType { get; set; }
    }

    public class ArtilleryShell : Projectile
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
        public float MuzzleVelocity { get; set; }
        public float FireChance { get; set; }
    }

    public class Bomb : Projectile
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
        public float MuzzleVelocity { get; set; }
        public float FireChance { get; set; }

    }

    public class Rocket : Projectile
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
        public float MuzzleVelocity { get; set; }
        public float FireChance { get; set; }
    }

    public class DepthCharge : Projectile
    {
        public float Damage { get; set; }
        public float FireChance { get; set; }
        public float FloodChance { get; set; }
        public float DetonationTimer { get; set; }
        public float SinkingSpeed { get; set; }
    }

    public class Torpedo : Projectile
    {
        public float SpottingRange { get; set; } //It's visibilityFactor
        public float Damage { get; set; } //(alphaDamage/3)+damage 
        public TorpedoType TorpedoType { get; set; }
        public float Caliber { get; set; }
        public float MaxRange { get; set; }//MaxDist*30
        public List<ShipClass> IgnoreClasses { get; set; }
        public float Speed { get; set; }
        public float ArmingTime { get; set; }
        public float FloodChance { get; set; } // It's uwCritical
    }
}
