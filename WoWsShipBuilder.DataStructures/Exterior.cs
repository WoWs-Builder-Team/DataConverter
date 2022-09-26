using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures;

public class Exterior
{
    public long Id { get; set; }
    public string Index { get; set; } = string.Empty;

    public Dictionary<string, double> Modifiers { get; set; } = new();
    public string Name { get; set; } = string.Empty;
    public ExteriorType Type { get; set; }
    public int SortOrder { get; set; }
    public Restriction Restrictions { get; set; } = new();
    public int Group { get; set; }
}

public class Restriction
{
    public List<string> ForbiddenShips { get; set; } = new();
    public List<string> Levels { get; set; } = new();
    public List<string> Nations { get; set; } = new();
    public List<string> SpecificShips { get; set; } = new();
    public List<string> Subtype { get; set; } = new();
}
