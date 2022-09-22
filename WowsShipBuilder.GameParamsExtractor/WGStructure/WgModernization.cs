using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure;

public class WgModernization : WgObject
{
    public string[] Excludes { get; set; } = Array.Empty<string>();

    public long Id { get; set; }

    public string Index { get; set; } = string.Empty;

    public Dictionary<string, JToken> Modifiers { get; set; } = new();

    public string Name { get; set; } = string.Empty;

    public string[] Nation { get; set; } = Array.Empty<string>();

    public int[] Shiplevel { get; set; } = Array.Empty<int>();

    public string[] Ships { get; set; } = Array.Empty<string>();

    public string[] Shiptype { get; set; } = Array.Empty<string>();

    public int Slot { get; set; }

    public int Type { get; set; }
}
