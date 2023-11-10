using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataConverter.JsonData;

public class ShiptoolData
{
    public List<ShiptoolShip> Ship { get; set; } = new();
}

public class ShiptoolShip
{
    public string Index { get; set; } = string.Empty;

    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalData { get; set; } = new();

    public ShiptoolHullModule? GetHullModule(string key)
    {
        return AdditionalData.TryGetValue(key, out var element) ? element.Deserialize<ShiptoolHullModule>() : null;
    }
}

public class ShiptoolHullModule
{
    [JsonPropertyName("ANGLES")]
    public Dictionary<string, decimal> Angles { get; set; } = new();
}
