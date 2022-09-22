using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Captain;

public class WgUniqueSkill
{
    public int MaxTriggerNum { get; set; }

    public ShipClass[] TriggerAllowedShips { get; set; } = Array.Empty<ShipClass>();

    public string TriggerType { get; set; } = string.Empty;

    [JsonExtensionData]
    public Dictionary<string, JToken> SkillEffects { get; set; } = new(); // value is actually Dictionary<string, object>, process in converter
}
