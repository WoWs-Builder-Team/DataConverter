using Newtonsoft.Json.Linq;

namespace WowsShipBuilder.GameParamsExtractor.WGStructure;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgAircraft : WgObject
{
    public Dictionary<string, AircraftAbility> PlaneAbilities { get; init; } = new();

    public double AttackCooldown { get; init; }

    public double AttackInterval { get; init; }

    public double AttackSpeedMultiplier { get; init; }

    public double AttackSpeedMultiplierApplyTime { get; init; }

    public double AttackerDamageTakenMultiplier { get; init; }

    public int AttackerSize { get; init; }

    public double BombFallingTime { get; init; }

    public string BombName { get; init; } = string.Empty;

    public double VisibilityFactor { get; init; }

    public double VisibilityFactorByPlane { get; init; }

    public double VisionToPlane { get; init; }

    public double VisionToShip { get; init; }

    public double VisionToTorpedo { get; init; }

    public double FlightHeight { get; init; }

    public double FlightRadius { get; init; }

    public HangarSettings HangarSettings { get; init; } = new(0, 0, 0, 0);

    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public double InnerBombsPercentage { get; init; }

    public List<double> InnerSalvoSize { get; init; } = new();

    public List<double> OuterSalvoSize { get; init; } = new();

    public JToken MinSpread { get; init; } = default!;

    public JToken MaxSpread { get; init; } = default!;

    public bool? IsAirSupportPlane { get; init; } // deprecated by WG with 0.11.1

    public bool? IsConsumablePlane { get; init; } // deprecated by WG with 0.11.1

    public string[] PlaneSubtype { get; init; } = Array.Empty<string>();

    public bool IsJatoBoosterDetachable { get; init; }

    public double JatoDuration { get; init; }

    public double JatoSpeedMultiplier { get; init; }

    public float MaxHealth { get; init; }

    public string Name { get; init; } = string.Empty;

    public double NaturalAcceleration { get; init; }

    public double NaturalDeceleration { get; init; }

    public int NumPlanesInSquadron { get; init; }

    public float ReturnHeight { get; init; }

    public float SpeedMax { get; init; }

    public float SpeedMin { get; init; }

    public float SpeedMoveWithBomb { get; init; }

    public float MaxForsageAmount { get; init; }

    public decimal AimingAccuracyDecreaseRate { get; init; }

    public decimal AimingAccuracyIncreaseRate { get; init; }

    public decimal AimingTime { get; init; }

    public decimal PostAttackInvulnerabilityDuration { get; init; }

    public decimal PreparationAccuracyDecreaseRate { get; init; }

    public decimal PreparationAccuracyIncreaseRate { get; init; }

    public decimal PreparationTime { get; init; }

    public int ProjectilesPerAttack { get; init; }

    public int AttackCount { get; init; }
}

public sealed record HangarSettings(int MaxValue, int RestoreAmount, int StartValue, float TimeToRestore);

public sealed record AircraftAbility(string[][] Abils, int Slot);
