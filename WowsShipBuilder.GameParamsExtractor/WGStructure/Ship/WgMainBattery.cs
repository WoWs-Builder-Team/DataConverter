using GameParamsExtractor.WGStructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

public class WgMainBattery : WgArmamentModule
{
    public Dictionary<string, WgMainBatteryGun> Guns { get; set; } = new();

    public WgBurstArtilleryModule? BurstArtilleryModule { get; set; }

    public decimal MaxDist { get; set; }

    public decimal SigmaCount { get; set; }

    public double TaperDist { get; set; }

    public bool NormalDistribution { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JToken> Other { get; set; } = new();

    [JsonIgnore]
    public Dictionary<string, WgAaAura> AntiAirAuras => Other.FindAaAuras();
}

public class WgMainBatteryGun
{
    public string[] AmmoList { get; set; } = Array.Empty<string>();

    public decimal BarrelDiameter { get; set; }

    public decimal[] HorizSector { get; set; } = Array.Empty<decimal>();

    public long Id { get; set; }

    public string Index { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int NumBarrels { get; set; }

    public decimal[] Position { get; set; } = Array.Empty<decimal>();

    public decimal[] RotationSpeed { get; set; } = Array.Empty<decimal>();

    public decimal[][] DeadZone { get; set; } = Array.Empty<decimal[]>();

    public decimal ShotDelay { get; set; }

    public decimal SmokePenalty { get; set; }

    public double IdealRadius { get; set; }

    public double MinRadius { get; set; }

    public double IdealDistance { get; set; }

    public double RadiusOnZero { get; set; }

    public double RadiusOnDelim { get; set; }

    public double RadiusOnMax { get; set; }

    public double Delim { get; set; }

    public TypeInfo TypeInfo { get; set; } = new();
}
