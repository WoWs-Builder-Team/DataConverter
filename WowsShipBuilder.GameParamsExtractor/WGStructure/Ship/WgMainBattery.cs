using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

public class WgMainBattery : WgArmamentModule
{
    public Dictionary<string, WgMainBatteryGun> Guns { get; init; } = new();

    public WgBurstArtilleryModule? BurstArtilleryModule { get; init; }

    public decimal MaxDist { get; init; }

    public decimal SigmaCount { get; init; }

    public double TaperDist { get; init; }

    public bool NormalDistribution { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JToken> Other { get; init; } = new();

    [JsonIgnore]
    public Dictionary<string, WgAaAura> AntiAirAuras => Other.FindAaAuras();
}

public class WgMainBatteryGun
{
    public string[] AmmoList { get; init; } = Array.Empty<string>();

    public decimal BarrelDiameter { get; init; }

    public decimal[] HorizSector { get; init; } = Array.Empty<decimal>();

    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public int NumBarrels { get; init; }

    public decimal[] Position { get; init; } = Array.Empty<decimal>();

    public decimal[] RotationSpeed { get; init; } = Array.Empty<decimal>();

    public decimal[][] DeadZone { get; init; } = Array.Empty<decimal[]>();

    public decimal ShotDelay { get; init; }

    public decimal SmokePenalty { get; init; }

    public double IdealRadius { get; init; }

    public double MinRadius { get; init; }

    public double IdealDistance { get; init; }

    public double RadiusOnZero { get; init; }

    public double RadiusOnDelim { get; init; }

    public double RadiusOnMax { get; init; }

    public double Delim { get; init; }

    public TypeInfo TypeInfo { get; init; } = new();
}
