namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
/// <summary>
/// Data for air strikes of ships.
/// </summary>
public class WgAirSupport : WgArmamentModule
{
    public int ChargesNum { get; init; }

    public decimal FlyAwayTime { get; init; }

    public int MaxDist { get; init; }

    public int MaxPlaneFlightDist { get; init; }

    public int MinDist { get; init; }

    public string PlaneName { get; init; } = string.Empty;

    public decimal ReloadTime { get; init; }

    public decimal TimeBetweenShots { get; init; }

    public decimal TimeFromHeaven { get; init; }
}
