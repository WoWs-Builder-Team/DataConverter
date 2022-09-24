using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

//this is AA and secondaries too. smallGun i think indicates if it's a secondary
public class WgAtba : WgArmamentModule
{
    public Dictionary<string, WgGun> AntiAirAndSecondaries { get; init; } = new();

    public decimal MaxDist { get; init; }

    public decimal SigmaCount { get; init; }

    public double TaperDist { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JToken> Other { get; init; } = new();

    [JsonIgnore]
    public Dictionary<string, WgAaAura> AntiAirAuras => Other.FindAaAuras();
}
