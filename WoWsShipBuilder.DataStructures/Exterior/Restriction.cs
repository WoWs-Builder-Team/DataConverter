using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Exterior;

public class Restriction
{
    public List<string> ForbiddenShips { get; set; } = new();

    public List<string> Levels { get; set; } = new();

    public List<string> Nations { get; set; } = new();

    public List<string> SpecificShips { get; set; } = new();

    public List<string> Subtype { get; set; } = new();
}
