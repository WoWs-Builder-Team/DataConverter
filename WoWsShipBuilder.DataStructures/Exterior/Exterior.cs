using System.Collections.Generic;
using WoWsShipBuilder.DataStructures.Modifiers;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Exterior;

public class Exterior
{
    public long Id { get; set; }

    public string Index { get; set; } = string.Empty;

    public List<Modifier> Modifiers { get; set; } = new();

    public string Name { get; set; } = string.Empty;

    public ExteriorType Type { get; set; }

    public int SortOrder { get; set; }

    public Restriction Restrictions { get; set; } = new();

    public int Group { get; set; }
}
