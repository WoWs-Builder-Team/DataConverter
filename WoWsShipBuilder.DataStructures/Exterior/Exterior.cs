using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures.Modifiers;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Exterior;

public sealed class Exterior
{
    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public ImmutableList<Modifier> Modifiers { get; init; } = ImmutableList<Modifier>.Empty;

    public string Name { get; init; } = string.Empty;

    public ExteriorType Type { get; init; }

    public int SortOrder { get; init; }

    public Restriction Restrictions { get; init; } = new();

    public int Group { get; init; }
}
