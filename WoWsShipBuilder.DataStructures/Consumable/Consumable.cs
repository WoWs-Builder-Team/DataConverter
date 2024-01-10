using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures.Modifiers;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Consumable;

public sealed class Consumable
{
    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string DescId { get; init; } = string.Empty;

    public string Group { get; init; } = string.Empty;

    public string IconId { get; init; } = string.Empty;

    public int NumConsumables { get; init; }

    public float ReloadTime { get; init; }

    public float WorkTime { get; init; }

    public string ConsumableVariantName { get; init; } = string.Empty;

    public string PlaneName { get; init; } = string.Empty;

    public float PreparationTime { get; init; }

    public ImmutableList<Modifier> Modifiers { get; init; } = ImmutableList<Modifier>.Empty;
}
