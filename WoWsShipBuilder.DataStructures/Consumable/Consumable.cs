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

    /// <summary>
    /// Indicates whether this consumable uses the time-based ("capacity") lifecycle instead of discrete charges.
    /// </summary>
    public bool IsTimeBased { get; init; }

    /// <summary>
    /// For time-based consumables, the total usage time pool (in seconds) that can be spent while active
    /// (the game's <c>maxCapacity</c>). Zero for classic charge-based consumables.
    /// </summary>
    public float TimeBasedActiveTime { get; init; }

    public ImmutableList<Modifier> Modifiers { get; init; } = ImmutableList<Modifier>.Empty;

    /// <summary>
    /// Gets the effect this consumable has on the ship using it, where that differs from the effect on its target.
    /// </summary>
    /// <remarks>
    /// Squadron support consumables buff an ally and their own carrier by different amounts under the same stat
    /// names - the support heal restores 50% to its target and 25% to its user - so the two cannot share one list.
    /// Empty for the majority of consumables, which affect only the ship using them and report that in
    /// <see cref="Modifiers"/>.
    /// </remarks>
    public ImmutableList<Modifier> SelfModifiers { get; init; } = ImmutableList<Modifier>.Empty;
}
