// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

public class WgHull : WgArmamentModule
{
    public decimal Health { get; set; }

    public decimal MaxSpeed { get; set; }

    public decimal RudderTime { get; set; }

    public WgSteeringGear Sg { get; set; } = new();

    public decimal SpeedCoef { get; set; }

    public decimal VisibilityCoefGkInSmoke { get; set; }

    public decimal VisibilityFactor { get; set; }

    public decimal VisibilityFactorByPlane { get; set; }

    public Dictionary<string, decimal> VisibilityFactorsBySubmarine { get; set; } = new();

    public decimal[][] BurnNodes { get; set; } = Array.Empty<decimal[]>(); // Format: Fire resistance coeff, damage per second in %, burn time in s

    public decimal[][] FloodNodes { get; set; } = Array.Empty<decimal[]>(); // Format: Torpedo belt reduction, damage per second in %, flood time in s

    public int TurningRadius { get; set; }

    public decimal[] Size { get; set; } = Array.Empty<decimal>();

    public int EnginePower { get; set; }

    public int Tonnage { get; set; }

    public WgHitLocation Cas { get; set; } = new();

    public WgHitLocation Bow { get; set; } = new();

    public WgHitLocation Ss { get; set; } = new();

    public WgHitLocation St { get; set; } = new();

    public WgHitLocation Ssc { get; set; } = new();

    public WgHitLocation Hull { get; set; } = new();

    public WgHitLocation Cit { get; set; } = new();

}

public class WgSteeringGear
{
    public decimal ArmorCoeff { get; set; }
}

public class WgHitLocation
{
    public string HlType { get; set; } = string.Empty;
    public float RegeneratedHpPart { get; set; }
    public int MaxHp { get; set; }
}
