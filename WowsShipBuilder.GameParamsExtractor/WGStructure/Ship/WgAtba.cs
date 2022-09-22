using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

//this is AA and secondaries too. smallGun i think indicates if it's a secondary
public class WgAtba : WgArmamentModule
{
    public Dictionary<string, WgAntiAirAndSecondaries> AntiAirAndSecondaries { get; init; } = new();

    public decimal MaxDist { get; init; }

    public decimal SigmaCount { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JToken> Other { get; init; } = new();

    [JsonIgnore]
    public Dictionary<string, WgAaAura> AntiAirAuras => Other.FindAaAuras();
}

public class WgAntiAirAndSecondaries
{
    public string[] AmmoList { get; init; } = Array.Empty<string>();

    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public decimal BarrelDiameter { get; init; }

    public int NumBarrels { get; init; }

    public decimal[] RotationSpeed { get; init; } = Array.Empty<decimal>();

    public decimal ShotDelay { get; init; }

    public bool SmallGun { get; init; }

    public TypeInfo TypeInfo { get; init; } = new();
}
