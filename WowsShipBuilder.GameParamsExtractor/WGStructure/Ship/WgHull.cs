namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgHull : WgArmamentModule
{
    public decimal Health { get; init; }

    public decimal MaxSpeed { get; init; }

    public decimal RudderTime { get; init; }

    public WgSteeringGear Sg { get; init; } = new();

    public decimal SpeedCoef { get; init; }

    public decimal VisibilityCoefGkInSmoke { get; init; }

    public decimal VisibilityFactor { get; init; }

    public decimal VisibilityFactorByPlane { get; init; }

    public Dictionary<string, decimal> VisibilityFactorsBySubmarine { get; init; } = new();

    public decimal[][] BurnNodes { get; init; } = Array.Empty<decimal[]>(); // Format: Fire resistance coeff, damage per second in %, burn time in s

    public decimal[][] FloodNodes { get; init; } = Array.Empty<decimal[]>(); // Format: Torpedo belt reduction, damage per second in %, flood time in s

    public int TurningRadius { get; init; }

    public decimal[] Size { get; init; } = Array.Empty<decimal>();

    public int EnginePower { get; init; }

    public int Tonnage { get; init; }

    public WgHitLocation Cas { get; } = new();

    public WgHitLocation Bow { get; } = new();

    public WgHitLocation Ss { get; } = new();

    public WgHitLocation St { get; } = new();

    public WgHitLocation Ssc { get; } = new();

    public WgHitLocation Hull { get; } = new();

    public WgHitLocation Cit { get; } = new();

    public Dictionary<string, object[]> BuoyancyStates { get; } = new();
}

public class WgSteeringGear
{
    public decimal ArmorCoeff { get; init; }
}

public class WgHitLocation
{
    public string HlType { get; init; } = string.Empty;

    public float RegeneratedHpPart { get; init; }

    public int MaxHp { get; init; }
}
