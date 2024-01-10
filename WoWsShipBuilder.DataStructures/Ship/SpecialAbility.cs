using System.Collections.Immutable;
using WoWsShipBuilder.DataStructures.Modifiers;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public sealed class SpecialAbility
{
    public ImmutableList<Modifier> Modifiers { get; init; } = ImmutableList<Modifier>.Empty;
    public string Name { get; init; } = string.Empty;
    public double DecrementPeriod { get; init; }
    public double Duration { get; init; }
    public double DecrementCount { get; init; }
    public double DecrementDelay { get; init; }
    public double ProgressPerAction { get; init; }
    public string ActivatorName { get; init; } = string.Empty;
    public double ActivatorRadius { get; init; }
}

public sealed class BurstModeAbility
{
    public decimal ReloadDuringBurst { get; init; }

    public decimal ReloadAfterBurst { get; init; }

    public ImmutableList<Modifier> Modifiers { get; init; } = ImmutableList<Modifier>.Empty;

    public int ShotInBurst { get; init; }
}
