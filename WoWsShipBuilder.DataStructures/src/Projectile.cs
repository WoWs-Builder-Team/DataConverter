using System.Collections.Generic;
using JsonSubTypes;
using Newtonsoft.Json;
using WoWsShipBuilder.DataStructures.Common;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace WoWsShipBuilder.DataStructures
{
    [JsonConverter(typeof(JsonSubtypes), "ProjectileType")]
    [JsonSubtypes.KnownSubType(typeof(ArtilleryShell), ProjectileType.Artillery)]
    [JsonSubtypes.KnownSubType(typeof(Bomb), ProjectileType.Bomb)]
    [JsonSubtypes.KnownSubType(typeof(Bomb), ProjectileType.SkipBomb)]
    [JsonSubtypes.KnownSubType(typeof(Rocket), ProjectileType.Rocket)]
    [JsonSubtypes.KnownSubType(typeof(DepthCharge), ProjectileType.DepthCharge)]
    [JsonSubtypes.KnownSubType(typeof(Torpedo), ProjectileType.Torpedo)]
    public record Projectile
    {
        public long Id { get; set; }
        public string Index { get; set; } = default!;
        public string Name { get; set; } = default!;
        public ProjectileType ProjectileType { get; set; }
    }

    public record ArtilleryShell : Projectile
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
    }

    public record Bomb : Projectile
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
    }

    public record Rocket : Projectile
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
    }

    public record DepthCharge : Projectile
    {
        public float Damage { get; set; }
        public float FireChance { get; set; }
        public float FloodChance { get; set; }
        public float DetonationTimer { get; set; }
        public float SinkingSpeed { get; set; }
        public float ExplosionRadius { get; set; }
    }

    public record Torpedo : Projectile
    {
        public float SpottingRange { get; set; } //It's visibilityFactor
        public float Damage { get; set; } //(alphaDamage/3)+damage 
        public TorpedoType TorpedoType { get; set; }
        public float Caliber { get; set; }
        public float MaxRange { get; set; }//MaxDist*30
        public List<ShipClass>? IgnoreClasses { get; set; }
        public float Speed { get; set; }
        public float ArmingTime { get; set; }
        public float FloodChance { get; set; } // It's uwCritical
        public float SplashCoeff { get; set; }
        public float ExplosionRadius { get; set; }
    }
}