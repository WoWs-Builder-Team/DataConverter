namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgBurstArtilleryModule
{
    public decimal BurstReloadTime { get; set; }

    public decimal FullReloadTime { get; set; }

    public Dictionary<string, float> Modifiers { get; set; } = new();

    public int ShotsCount { get; set; }
}

public class WgSpecialAbility : WgArmamentModule
{
    public WgRageMode RageMode { get; set; } = new();

    public int BuffsShiftMaxLevel { get; set; }

    public double BuffsShiftSpeed { get; set; }

    public int BuffsStartPool { get; set; }

    public Dictionary<string, float> Modifiers { get; set; } = new();
}

public class WgRageMode
{
    public double BoostDuration { get; set; }

    public int DecrementCount { get; set; }

    public double DecrementDelay { get; set; }

    public double DecrementPeriod { get; set; }

    public int GunsForSalvo { get; set; }

    public Dictionary<string, float> Modifiers { get; set; } = new();

    public double Radius { get; set; }

    public string RageModeName { get; set; } = string.Empty;

    public int RequiredHits { get; set; }
}
