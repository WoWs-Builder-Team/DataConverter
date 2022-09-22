using GameParamsExtractor.WGStructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

//this is AA and secondaries too. smallGun i think indicates if it's a secondary
public class WgAtba : WgArmamentModule
{
    public Dictionary<string, WgAntiAirAndSecondaries> AntiAirAndSecondaries { get; set; } = new();

    public decimal MaxDist { get; set; }

    public decimal SigmaCount { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JToken> Other { get; set; } = new();

    [JsonIgnore]
    public Dictionary<string, WgAaAura> AntiAirAuras => Other.FindAaAuras();
}

public class WgAntiAirAndSecondaries
{
    public string[] AmmoList { get; set; } = Array.Empty<string>();

    public long Id { get; set; }

    public string Index { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal BarrelDiameter { get; set; }

    public int NumBarrels { get; set; }

    public decimal[] RotationSpeed { get; set; } = Array.Empty<decimal>();

    public decimal ShotDelay { get; set; }

    public bool SmallGun { get; set; }

    public TypeInfo TypeInfo { get; set; } = new();

    public static explicit operator Gun(WgAntiAirAndSecondaries wgSecondary)
    {
        return new Gun
        {
            AmmoList = wgSecondary.AmmoList.ToList(),
            BarrelDiameter = wgSecondary.BarrelDiameter,
            Id = wgSecondary.Id,
            Index = wgSecondary.Index,
            Name = wgSecondary.Name,
            NumBarrels = wgSecondary.NumBarrels,
            HorizontalRotationSpeed = wgSecondary.RotationSpeed[0],
            VerticalRotationSpeed = wgSecondary.RotationSpeed[1],
            Reload = wgSecondary.ShotDelay,
        };
    }
}
