using Newtonsoft.Json;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

public class WgGun
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

    public decimal AmmoSwitchCoeff { get; init; }

    public double IdealRadius { get; init; }

    public double MinRadius { get; init; }

    public double IdealDistance { get; init; }

    public double RadiusOnZero { get; init; }

    public double RadiusOnDelim { get; init; }

    public double RadiusOnMax { get; init; }

    public double Delim { get; init; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool SmallGun { get; init; }

    public TypeInfo TypeInfo { get; init; } = new();
}
