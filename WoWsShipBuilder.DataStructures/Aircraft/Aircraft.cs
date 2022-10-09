using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Aircraft;

public class Aircraft
{
    public long Id { get; set; }

    public string Index { get; set; } = string.Empty;

    public float MaxHealth { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsTactical { get; set; }

    public int NumPlanesInSquadron { get; set; }

    public float ReturnHeight { get; set; }

    public float SpeedMaxModifier { get; set; }

    public float SpeedMinModifier { get; set; }

    public float Speed { get; set; }

    public float MaxEngineBoostDuration { get; set; }

    public double NaturalAcceleration { get; set; }

    public double NaturalDeceleration { get; set; }

    public double ConcealmentFromShips { get; set; }

    public double ConcealmentFromPlanes { get; set; }

    public double SpottingOnShips { get; set; }

    public double SpottingOnPlanes { get; set; }

    public double SpottingOnTorps { get; set; }

    public int MaxPlaneInHangar { get; set; }

    public int StartingPlanes { get; set; }

    public float RestorationTime { get; set; }

    public double BombFallingTime { get; set; }

    public string BombName { get; set; } = string.Empty;

    public double DamageTakenMultiplier { get; set; }

    public double FlightHeight { get; set; }

    public double FlightRadius { get; set; }

    public double InnerBombsPercentage { get; set; }

    public List<double> InnerSalvoSize { get; set; } = new();

    public PlaneCategory PlaneCategory { get; set; }

    public PlaneAttackData AttackData { get; set; } = new();

    public JatoData JatoData { get; set; } = new(default, default);

    public List<AircraftConsumable> AircraftConsumable { get; set; } = new();

    public decimal AimingAccuracyDecreaseRate { get; set; }

    public decimal AimingAccuracyIncreaseRate { get; set; }

    public decimal AimingTime { get; set; }

    public decimal PostAttackInvulnerabilityDuration { get; set; }

    public decimal PreparationAccuracyDecreaseRate { get; set; }

    public decimal PreparationAccuracyIncreaseRate { get; set; }

    public decimal PreparationTime { get; set; }
}
