using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure;

public class WgModernization : WgObject
{
    public string[] Excludes { get; init; } = Array.Empty<string>();

    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public Dictionary<string, JToken> Modifiers { get; init; } = new();

    public string Name { get; init; } = string.Empty;

    public string[] Nation { get; init; } = Array.Empty<string>();

    public int[] Shiplevel { get; init; } = Array.Empty<int>();

    public string[] Ships { get; init; } = Array.Empty<string>();

    public string[] Shiptype { get; init; } = Array.Empty<string>();

    public int Slot { get; init; }

    public int Type { get; init; }
}
