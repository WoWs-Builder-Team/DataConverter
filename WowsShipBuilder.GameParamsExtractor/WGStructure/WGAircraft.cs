using GameParamsExtractor.WGStructure;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure;

public class WgAircraft : WGObject
{
    public Dictionary<string, AircraftAbility> PlaneAbilities { get; set; } = new();

    public double AttackCooldown { get; set; }

    public double AttackInterval { get; set; }

    public double AttackSpeedMultiplier { get; set; }

    public double AttackSpeedMultiplierApplyTime { get; set; }

    public double AttackerDamageTakenMultiplier { get; set; }

    public int AttackerSize { get; set; }

    public double BombFallingTime { get; set; }

    public string BombName { get; set; } = string.Empty;

    public double FlightHeight { get; set; }

    public double FlightRadius { get; set; }

    public Hangarsettings HangarSettings { get; set; } = new();

    public long Id { get; set; }

    public string Index { get; set; } = string.Empty;

    public double InnerBombsPercentage { get; set; }

    public List<double> InnerSalvoSize { get; set; } = new();

    public bool? IsAirSupportPlane { get; set; } // deprecated by WG with 0.11.1

    public bool? IsConsumablePlane { get; set; } // deprecated by WG with 0.11.1

    public string[] PlaneSubtype { get; set; } = Array.Empty<string>();

    public bool IsJatoBoosterDetachable { get; set; }

    public double JatoDuration { get; set; }

    public double JatoSpeedMultiplier { get; set; }

    public float MaxHealth { get; set; }

    public string Name { get; set; } = string.Empty;

    public double NaturalAcceleration { get; set; }

    public double NaturalDeceleration { get; set; }

    public int NumPlanesInSquadron { get; set; }

    public float ReturnHeight { get; set; }

    public float SpeedMax { get; set; }

    public float SpeedMin { get; set; }

    public float SpeedMoveWithBomb { get; set; }

    public decimal AimingAccuracyDecreaseRate { get; set; }

    public decimal AimingAccuracyIncreaseRate { get; set; }

    public decimal AimingTime { get; set; }

    public decimal PostAttackInvulnerabilityDuration { get; set; }

    public decimal PreparationAccuracyDecreaseRate { get; set; }

    public decimal PreparationAccuracyIncreaseRate { get; set; }

    public decimal PreparationTime { get; set; }

    public int ProjectilesPerAttack { get; set; }
}

public record Hangarsettings
{
    public int MaxValue { get; init; }

    public int RestoreAmount { get; init; }

    public int StartValue { get; init; }

    public float TimeToRestore { get; init; }
}

public record AircraftAbility(string[][] Abils, int Slot);
