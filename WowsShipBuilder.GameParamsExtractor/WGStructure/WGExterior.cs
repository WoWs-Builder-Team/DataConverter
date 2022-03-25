using GameParamsExtractor.WGStructure;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure;

public class WgExterior : WGObject
{
    public long Id { get; set; }

    public string Index { get; set; } = string.Empty;

    public Dictionary<string, JToken> Modifiers { get; set; } = new();

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public WgRestriction Restrictions { get; set; } = new();

    public int Group { get; set; }
}

//check if they are actually string, couldn't find one example
public class WgRestriction
{
    public string[] ForbiddenShips { get; set; } = Array.Empty<string>();

    public string[] Levels { get; set; } = Array.Empty<string>();

    public string[] Nations { get; set; } = Array.Empty<string>();

    public string[] SpecificShips { get; set; } = Array.Empty<string>();

    public string[] Subtype { get; set; } = Array.Empty<string>();
}
