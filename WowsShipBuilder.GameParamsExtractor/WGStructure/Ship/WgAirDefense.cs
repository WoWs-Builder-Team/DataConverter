using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

public class WgAirDefense : WgArmamentModule
{
    public bool IsAa { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JToken> Other { get; init; } = new();

    [JsonIgnore]
    public Dictionary<string, WgAaAura> AntiAirAuras => Other.FindAaAuras();
}

public class WgAaAura
{
    public decimal AreaDamage { get; init; }

    public decimal AreaDamagePeriod { get; init; }

    public decimal BubbleDamage { get; init; }

    [JsonRequired]
    public decimal HitChance { get; init; }

    public int InnerBubbleCount { get; init; }

    public int OuterBubbleCount { get; init; }

    public decimal MaxDistance { get; init; }

    public decimal MinDistance { get; init; }

    public string Type { get; init; } = string.Empty;
}
