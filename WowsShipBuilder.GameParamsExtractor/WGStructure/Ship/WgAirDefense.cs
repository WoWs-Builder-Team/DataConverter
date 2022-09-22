using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

public class WgAirDefense : WgArmamentModule
{
    public bool IsAa { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JToken> Other { get; set; } = new();

    [JsonIgnore]
    public Dictionary<string, WgAaAura> AntiAirAuras => Other.FindAaAuras();
}

public class WgAaAura
{
    public decimal AreaDamage { get; set; }

    public decimal AreaDamagePeriod { get; set; }

    public decimal BubbleDamage { get; set; }

    [JsonRequired]
    public decimal HitChance { get; set; }

    public int InnerBubbleCount { get; set; }

    public decimal MaxDistance { get; set; }

    public decimal MinDistance { get; set; }

    public string Type { get; set; } = string.Empty;

    public static implicit operator AntiAirAura(WgAaAura aura)
    {
        return new AntiAirAura
        {
            ConstantDps = aura.AreaDamage,
            FlakDamage = aura.BubbleDamage,
            FlakCloudsNumber = aura.InnerBubbleCount,
            HitChance = aura.HitChance,
            MaxRange = aura.MaxDistance,
            MinRange = aura.MinDistance,
        };
    }
}
