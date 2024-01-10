namespace WoWsShipBuilder.DataStructures.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public sealed class AirStrike
{
    public int Charges { get; init; }

    public decimal FlyAwayTime { get; init; }

    public int MaximumDistance { get; init; }

    public int MaximumFlightDistance { get; init; }

    public int MinimumDistance { get; init; }

    public string PlaneName { get; init; } = string.Empty;

    public decimal ReloadTime { get; init; }

    public decimal TimeBetweenShots { get; init; }

    public decimal DropTime { get; init; }
}
