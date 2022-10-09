using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Projectile;

public class DepthCharge : Projectile
{
    public float Damage { get; set; }

    public float FireChance { get; set; }

    public float FloodChance { get; set; }

    public float DetonationTimer { get; set; }

    public float SinkingSpeed { get; set; }

    public float ExplosionRadius { get; set; }

    public float SinkingSpeedRng { get; set; }

    public float DetonationTimerRng { get; set; }

    public Dictionary<float, List<float>> PointsOfDamage { get; set; } = new();
}
