using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameParamsExtractor.WGStructure;

public static class WgStructureHelper
{
    public static T? ToObjectSafe<T>(this JToken token) where T : class
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
}
