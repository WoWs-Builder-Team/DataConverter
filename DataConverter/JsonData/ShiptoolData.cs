using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataConverter.JsonData;

public class ShiptoolData
{
    public List<ShiptoolShip> Ship { get; set; } = new();
}

public class ShiptoolShip
{
    public string Index { get; set; } = string.Empty;

    [JsonExtensionData]
    private Dictionary<string, JToken> AdditionalData { get; set; } = new();

    public ShiptoolArmamentModule? GetArmamentModule(string key)
    {
        return AdditionalData.TryGetValue(key, out var token) ? token.ToObject<ShiptoolArmamentModule>() : null;
    }
}

public class ShiptoolArmamentModule
{
    [JsonExtensionData]
    private Dictionary<string, JToken> Data { get; set; } = new();

    public ShiptoolGunData? GetGunData(string key) => Data[key].ToObject<ShiptoolGunData>();
}

public class ShiptoolGunData
{
    [JsonProperty(PropertyName = "ANGLE")]
    public decimal BaseAngle { get; set; }
}
