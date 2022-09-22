using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

public class WgShipUpgradeInfo
{
    public int CostCr { get; set; }

    public int CostGold { get; set; }

    public int CostSaleGold { get; set; }

    public int CostXp { get; set; }

    public int Value { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JToken> Other { get; set; } = new();

    [JsonIgnore]
    public Dictionary<string, WgShipUpgrade> ConvertedUpgrades =>
        Other.Select(entry => (entry.Key, entry.Value.ToObjectOrNull<WgShipUpgrade>()))
            .Where(entry => entry.Item2 is not null)
            .ToDictionary(entry => entry.Key, entry => entry.Item2!);
}

public class WgShipUpgrade
{
    public bool CanBuy { get; set; }

    public Dictionary<string, string[]> Components { get; set; } = new();

    public string[] NextShips { get; set; } = Array.Empty<string>();

    public string? Prev { get; set; }

    public string UcType { get; set; } = string.Empty;
}
