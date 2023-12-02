using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures.Modifiers;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Upgrade;

public sealed class Modernization
{
    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public ImmutableList<Modifier> Modifiers { get; init; } = ImmutableList<Modifier>.Empty;

    public string Name { get; init; } = string.Empty;

    public ImmutableArray<Nation> AllowedNations { get; init; } = ImmutableArray<Nation>.Empty;

    public ImmutableArray<int> ShipLevel { get; init; } = ImmutableArray<int>.Empty;

    public ImmutableList<string> AdditionalShips { get; init; } = ImmutableList<string>.Empty;

    public ImmutableArray<ShipClass> ShipClasses { get; init; } = ImmutableArray<ShipClass>.Empty;

    public int Slot { get; init; }

    public ImmutableList<string> BlacklistedShips { get; init; } = ImmutableList<string>.Empty;

    public ModernizationType Type { get; init; }
}
