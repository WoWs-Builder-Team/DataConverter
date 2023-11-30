using System.Collections.Immutable;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public class Hull
{
    public decimal Health { get; init; }

    public decimal MaxSpeed { get; init; }

    public decimal RudderTime { get; init; }

    public decimal SpeedCoef { get; init; }

    public decimal SteeringGearArmorCoeff { get; init; }

    public decimal SmokeFiringDetection { get; init; }

    public decimal SurfaceDetection { get; init; }

    public decimal AirDetection { get; init; }

    public decimal DetectionBySubPeriscope { get; init; }

    public decimal DetectionBySubOperating { get; init; }

    public AntiAir? AntiAir { get; init; }

    public TurretModule? SecondaryModule { get; init; }

    public DepthChargeArray? DepthChargeArray { get; init; }

    public int FireSpots { get; init; }

    public decimal FireResistance { get; init; }

    public decimal FireTickDamage { get; init; }

    public decimal FireDuration { get; init; }

    public int FloodingSpots { get; init; }

    public decimal FloodingResistance { get; init; }

    public decimal FloodingTickDamage { get; init; }

    public decimal FloodingDuration { get; init; }

    public int TurningRadius { get; init; }

    public ShipSize Sizes { get; init; } = new(default, default, default);

    public int EnginePower { get; init; }

    public int Tonnage { get; init; }

    public ImmutableList<HitLocation> HitLocations { get; init; } = ImmutableList<HitLocation>.Empty;

    public ImmutableDictionary<SubsBuoyancyStates, decimal> MaxSpeedAtBuoyancyStateCoeff { get; init; } = ImmutableDictionary<SubsBuoyancyStates, decimal>.Empty;

    public decimal SubBatteryCapacity { get; init; }

    public decimal SubBatteryRegenRate { get; init; }

    public decimal DiveSpeed { get; init; }

    public decimal DivingPlaneShiftTime { get; init; }
}

public record ShipSize(decimal Length, decimal Width, decimal Height);

public record HitLocation(ShipHitLocation Name, string Type, float RepairableDamage, int Hp);
