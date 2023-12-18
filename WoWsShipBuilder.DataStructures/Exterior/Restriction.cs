using System.Collections.Immutable;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Exterior;

public sealed class Restriction
{
    public ImmutableArray<string> ForbiddenShips { get; set; } = ImmutableArray<string>.Empty;

    public ImmutableArray<string> Levels { get; set; } = ImmutableArray<string>.Empty;

    public ImmutableArray<string> Nations { get; set; } = ImmutableArray<string>.Empty;

    public ImmutableArray<string> SpecificShips { get; set; } = ImmutableArray<string>.Empty;

    public ImmutableArray<string> Subtype { get; set; } = ImmutableArray<string>.Empty;
}
