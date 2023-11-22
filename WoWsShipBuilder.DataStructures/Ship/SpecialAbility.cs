using System.Collections.Generic;
using WoWsShipBuilder.DataStructures.Modifiers;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public class SpecialAbility
{
    public List<Modifier> Modifiers { get; set; } = new();
    public string Name { get; set; } = string.Empty;
    public double DecrementPeriod { get; set; }
    public double Duration { get; set; }
    public double DecrementCount { get; init; }
    public double DecrementDelay { get; init; }
    public double ProgressPerAction { get; set; }
    public string ActivatorName { get; set; } = string.Empty;
    public double ActivatorRadius { get; set; }
}

public class BurstModeAbility
{
    public decimal ReloadDuringBurst { get; set; }

    public decimal ReloadAfterBurst { get; set; }

    public List<Modifier> Modifiers { get; set; } = new();

    public int ShotInBurst { get; set; }
}
