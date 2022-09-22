namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
/// <summary>
/// Data for air strikes of ships.
/// </summary>
public class WgAirSupport : WgArmamentModule
{
    public int ChargesNum { get; set; }

    public decimal FlyAwayTime { get; set; }

    public int MaxDist { get; set; }

    public int MaxPlaneFlightDist { get; set; }

    public int MinDist { get; set; }

    public string PlaneName { get; set; } = string.Empty;

    public decimal ReloadTime { get; set; }

    public decimal TimeBetweenShots { get; set; }

    public decimal TimeFromHeaven { get; set; }
}
