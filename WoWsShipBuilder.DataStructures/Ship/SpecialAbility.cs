using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public class SpecialAbility
{
    public double Duration { get; set; }

    public int RequiredHits { get; set; }

    public double RadiusForSuccessfulHits { get; set; }

    public Dictionary<string, float> Modifiers { get; set; } = new();

    public string Name { get; set; } = string.Empty;
}

public class BurstModeAbility
{
    public decimal ReloadDuringBurst { get; set; }

    public decimal ReloadAfterBurst { get; set; }

    public Dictionary<string, float> Modifiers { get; set; } = new();

    public int ShotInBurst { get; set; }
}
