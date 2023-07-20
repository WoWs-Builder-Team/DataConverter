using Newtonsoft.Json.Linq;

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
}

public class WgRageMode
{
    public Dictionary<string, JToken> Modifiers { get; init; } = new();
    public string RageModeName { get; init; } = string.Empty;
    public double DecrementPeriod { get; init; }
    public double BoostDuration { get; init; }
    public double DecrementCount { get; init; }
    public WgGameLogicTrigger GameLogicTrigger { get; set; } = new();
    public double DecrementDelay { get; init; }

}

public class WgGameLogicTrigger
{
    public WgAction Action { get; set; } = new();
    public WgActivator Activator { get; set; } = new();
}

public class WgAction
{
    public double Progress { get; set; }
}

public class WgActivator
{
    public string Type { get; set; } = string.Empty;
    public double Radius { get; set; }

}

