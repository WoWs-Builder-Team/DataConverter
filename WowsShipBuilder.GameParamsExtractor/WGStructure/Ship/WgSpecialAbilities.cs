namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgBurstArtilleryModule
{
    public decimal BurstReloadTime { get; init; }

    public decimal FullReloadTime { get; init; }

    public Dictionary<string, float> Modifiers { get; init; } = new();

    public int ShotsCount { get; init; }
}

public class WgSpecialAbility : WgArmamentModule
{
    public WgRageMode RageMode { get; init; } = new();

    public int BuffsShiftMaxLevel { get; init; }

    public double BuffsShiftSpeed { get; init; }

    public int BuffsStartPool { get; init; }

    public Dictionary<string, float> Modifiers { get; init; } = new();
}

public class WgRageMode
{
    public double BoostDuration { get; init; }

    public int DecrementCount { get; init; }

    public double DecrementDelay { get; init; }

    public double DecrementPeriod { get; init; }

    public int GunsForSalvo { get; init; }

    public Dictionary<string, float> Modifiers { get; init; } = new();

    public double Radius { get; init; }

    public string RageModeName { get; init; } = string.Empty;

    public int RequiredHits { get; init; }
}
