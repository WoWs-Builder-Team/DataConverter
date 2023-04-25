namespace WoWsShipBuilder.DataStructures.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable once NotAccessedPositionalProperty.Global
public sealed record Dispersion(double IdealRadius, double MinRadius, double IdealDistance, double TaperDist, double RadiusOnZero, double RadiusOnDelim, double RadiusOnMax, double Delim)
{
    public static Dispersion Default { get; } = new(0, 0, 0, 0, 0, 0, 0, 0);

    public decimal MaximumHorizontalDispersion { get; set; }

    public decimal MaximumVerticalDispersion { get; set; }
}
