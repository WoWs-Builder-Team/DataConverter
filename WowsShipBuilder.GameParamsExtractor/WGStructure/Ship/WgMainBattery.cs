using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

public class WgMainBattery : WgArmamentModule
{
    public Dictionary<string, WgGun> Guns { get; init; } = new();

    public WgBurstArtilleryModule? SwitchableModeArtilleryModule { get; init; }

    public decimal MaxDist { get; init; }

    public decimal SigmaCount { get; init; }

    public double TaperDist { get; init; }

    public bool NormalDistribution { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JToken> Other { get; init; } = new();

    [JsonIgnore]
    public Dictionary<string, WgAaAura> AntiAirAuras => Other.FindAaAuras();
}
