using System.Collections.Immutable;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Aircraft;

public sealed class Aircraft
{
    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public float MaxHealth { get; init; }

    public string Name { get; init; } = string.Empty;

    public int NumPlanesInSquadron { get; init; }

    public float ReturnHeight { get; init; }

    public float SpeedMaxModifier { get; init; }

    public float SpeedMinModifier { get; init; }

    public float Speed { get; init; }

    public float MaxEngineBoostDuration { get; init; }

    public double NaturalAcceleration { get; init; }

    public double NaturalDeceleration { get; init; }

    public double ConcealmentFromShips { get; init; }

    public double ConcealmentFromPlanes { get; init; }

    public double SpottingOnShips { get; init; }

    public double SpottingOnPlanes { get; init; }

    public double SpottingOnTorps { get; init; }

    public int MaxPlaneInHangar { get; init; }

    public int StartingPlanes { get; init; }

    public float RestorationTime { get; init; }

    public double BombFallingTime { get; init; }

    public string BombName { get; init; } = string.Empty;

    public double DamageTakenMultiplier { get; init; }

    public double FlightHeight { get; init; }

    public double FlightRadius { get; init; }

    public double InnerBombsPercentage { get; init; }

    public ImmutableArray<double> InnerSalvoSize { get; init; } = ImmutableArray<double>.Empty;

    public ImmutableArray<double> OuterSalvoSize { get; init; } = ImmutableArray<double>.Empty;

    public ImmutableArray<float> MinSpreadCoeff { get; init; } = ImmutableArray<float>.Empty;

    public ImmutableArray<float> MaxSpreadCoeff { get; init; } = ImmutableArray<float>.Empty;

    public PlaneCategory PlaneCategory { get; init; }

    public PlaneAttackData AttackData { get; init; } = new();

    public JatoData JatoData { get; init; } = new(default, default);

    public ImmutableArray<AircraftConsumable> AircraftConsumable { get; init; } = ImmutableArray<AircraftConsumable>.Empty;

    public decimal AimingAccuracyDecreaseRate { get; init; }

    public decimal AimingAccuracyIncreaseRate { get; init; }

    public decimal AimingTime { get; init; }

    public decimal PostAttackInvulnerabilityDuration { get; init; }

    public decimal PreparationAccuracyDecreaseRate { get; init; }

    public decimal PreparationAccuracyIncreaseRate { get; init; }

    public decimal PreparationTime { get; init; }

    public PlaneType PlaneType { get; init; }
}
