namespace WoWsShipBuilder.DataStructures.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class Dispersion
{
    public double IdealRadius { get; init; }

    public double MinRadius { get; init; }

    public double IdealDistance { get; init; }

    public double TaperDist { get; init; }

    public double RadiusOnZero { get; init; }

    public double RadiusOnDelim { get; init; }

    public double RadiusOnMax { get; init; }

    public double Delim { get; init; }

    public decimal MaximumHorizontalDispersion { get; set; }

    public decimal MaximumVerticalDispersion { get; set; }
}
