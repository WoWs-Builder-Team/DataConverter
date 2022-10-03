using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public class Hull
{
    public decimal Health { get; set; }

    public decimal MaxSpeed { get; set; }

    public decimal RudderTime { get; set; }

    public decimal SpeedCoef { get; set; }

    public decimal SteeringGearArmorCoeff { get; set; }

    public decimal SmokeFiringDetection { get; set; }

    public decimal SurfaceDetection { get; set; }

    public decimal AirDetection { get; set; }

    public decimal DetectionBySubPeriscope { get; set; }

    public decimal DetectionBySubOperating { get; set; }

    public AntiAir? AntiAir { get; set; }

    public TurretModule? SecondaryModule { get; set; }

    public DepthChargeArray? DepthChargeArray { get; set; }

    public int FireSpots { get; set; }

    public decimal FireResistance { get; set; }

    public decimal FireTickDamage { get; set; }

    public decimal FireDuration { get; set; }

    public int FloodingSpots { get; set; }

    public decimal FloodingResistance { get; set; }

    public decimal FloodingTickDamage { get; set; }

    public decimal FloodingDuration { get; set; }

    public int TurningRadius { get; set; }

    public ShipSize Sizes { get; set; } = new();

    public int EnginePower { get; set; }

    public int Tonnage { get; set; }

    public List<HitLocation> HitLocations { get; set; } = new();

    public Dictionary<SubsBuoyancyStates, decimal> MaxSpeedAtBuoyancyStateCoeff { get; set; } = new();
}

public class ShipSize
{
    public decimal Length { get; init; }

    public decimal Width { get; init; }

    public decimal Height { get; init; }
}

public class HitLocation
{
    public ShipHitLocation Name { get; init; }

    public string Type { get; init; } = string.Empty;

    public float RepairableDamage { get; init; }

    public int Hp { get; init; }
}
