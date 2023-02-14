using System;
using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Projectile;

public class Torpedo : Projectile
{
    public float SpottingRange { get; set; } // It's visibilityFactor

    public float Damage { get; set; } // (alphaDamage/3)+damage

    public TorpedoType TorpedoType { get; set; }

    public float Caliber { get; set; }

    public float MaxRange { get; set; } // MaxDist*30

    public List<ShipClass> IgnoreClasses { get; set; } = new();

    public float Speed { get; set; }

    public float ArmingTime { get; set; }

    public float FloodChance { get; set; } // It's uwCritical

    public float SplashCoeff { get; set; }

    public float ExplosionRadius { get; set; }

    public MagneticTorpedoParams MagneticTorpedoParams { get; set; } = new();
}

public class MagneticTorpedoParams
{
    public Dictionary<ShipClass, float[]> DropTargetAtDistance { get; set; } = new();

    public float[] MaxTurningSpeed { get; set; } = Array.Empty<float>();

    public float[] TurningAcceleration { get; set; } = Array.Empty<float>();

    public float[] MaxVerticalSpeed { get; set; } = Array.Empty<float>();

    public float[] VerticalAcceleration { get; set; } = Array.Empty<float>();

    public float[] SearchRadius { get; set; } = Array.Empty<float>();

    public float[] SearchAngle { get; set; } = Array.Empty<float>();
}
