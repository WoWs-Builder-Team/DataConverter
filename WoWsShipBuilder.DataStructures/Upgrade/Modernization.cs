using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Upgrade;

public sealed record Modernization
{
    public long Id { get; set; }

    public string Index { get; set; } = string.Empty;

    public Dictionary<string, double> Effect { get; set; } = new();

    public string Name { get; set; } = string.Empty;

    public List<Nation> AllowedNations { get; set; } = new();

    public List<int> ShipLevel { get; set; } = new();

    public List<string> AdditionalShips { get; set; } = new();

    public List<ShipClass> ShipClasses { get; set; } = new();

    public int Slot { get; set; }

    public List<string> BlacklistedShips { get; set; } = new();

    public ModernizationType Type { get; set; }
}
