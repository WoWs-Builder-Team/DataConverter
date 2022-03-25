using GameParamsExtractor.WGStructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WowsShipBuilder.GameParamsExtractor.WGStructure;

public static class WgStructureHelper
{
    public static T? ToObjectOrNull<T>(this JToken token) where T : class
    {
        try
        {
            return token.ToObject<T>();
        }
        catch (JsonSerializationException)
        {
            return null;
        }
    }

    public static Dictionary<string, AaAura> FindAaAuras(this Dictionary<string, JToken> dictionary)
    {
        return dictionary.Where(entry => entry.Value.Type == JTokenType.Object)
            .Where(entry => entry.Value["hitChance"] != null || entry.Value["HitChance"] != null)
            .Select(entry => new KeyValuePair<string, AaAura?>(entry.Key, entry.Value.ToObjectOrNull<AaAura>()))
            .Where(entry => entry.Value is not null)
            .ToDictionary(entry => entry.Key, entry => entry.Value!);
    }
}
