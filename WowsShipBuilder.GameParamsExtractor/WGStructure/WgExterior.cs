using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure;

public class WgExterior : WgObject
{
    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public Dictionary<string, JToken> Modifiers { get; init; } = new();

    public string Name { get; init; } = string.Empty;

    public int SortOrder { get; init; }

    public WgRestriction Restrictions { get; init; } = new();

    public int Group { get; init; }
}

//check if they are actually string, couldn't find one example
public class WgRestriction
{
    public string[] ForbiddenShips { get; init; } = Array.Empty<string>();

    public string[] Levels { get; init; } = Array.Empty<string>();

    public string[] Nations { get; init; } = Array.Empty<string>();

    public string[] SpecificShips { get; init; } = Array.Empty<string>();

    public string[] Subtype { get; init; } = Array.Empty<string>();
}
