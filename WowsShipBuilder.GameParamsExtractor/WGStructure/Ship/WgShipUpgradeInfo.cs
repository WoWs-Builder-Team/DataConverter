using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

public class WgShipUpgradeInfo
{
    public int CostCr { get; init; }

    public int CostGold { get; init; }

    public int CostSaleGold { get; init; }

    public int CostXp { get; init; }

    public int Value { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JToken> Other { get; init; } = new();

    [JsonIgnore]
    public Dictionary<string, WgShipUpgrade> ConvertedUpgrades =>
        Other.Select(entry => (entry.Key, entry.Value.ToObjectOrNull<WgShipUpgrade>()))
            .Where(entry => entry.Item2 is not null)
            .ToDictionary(entry => entry.Key, entry => entry.Item2!);
}

public class WgShipUpgrade
{
    public bool CanBuy { get; init; }

    public Dictionary<string, string[]> Components { get; init; } = new();

    public string[] NextShips { get; init; } = Array.Empty<string>();

    public string? Prev { get; init; }

    public string UcType { get; init; } = string.Empty;
}
