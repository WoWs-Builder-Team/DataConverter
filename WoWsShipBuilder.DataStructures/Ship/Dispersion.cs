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

    public double CalculateHorizontalDispersion(double range, double modifier = 1)
    {
        // Calculation according to https://www.reddit.com/r/WorldOfWarships/comments/l1dpzt/reverse_engineered_dispersion_ellipse_including/
        double x = range / 30;
        double effectiveTaperDist = TaperDist / 30;
        if (x <= effectiveTaperDist)
        {
            return (x * (IdealRadius - MinRadius) / IdealDistance + MinRadius * (x / effectiveTaperDist)) * 30 * modifier;
        }

        return (x * (IdealRadius - MinRadius) / IdealDistance + MinRadius) * 30 * modifier;
    }

    public double CalculateVerticalDispersion(double maxRange, double horizontalDispersion, double range = -1)
    {
        // Calculation according to https://www.reddit.com/r/WorldOfWarships/comments/l1dpzt/reverse_engineered_dispersion_ellipse_including/
        maxRange /= 30;
        double x = range >= 0 ? range / 30 : maxRange;
        double delimDist = maxRange * Delim;

        double verticalCoeff;
        if (x < delimDist)
        {
            verticalCoeff = RadiusOnZero + (RadiusOnDelim - RadiusOnZero) * (x / delimDist);
        }
        else
        {
            verticalCoeff = RadiusOnDelim + (RadiusOnMax - RadiusOnDelim) * (x - delimDist) / (maxRange - delimDist);
        }

        return horizontalDispersion * verticalCoeff;
    }

    public DispersionContainer CalculateDispersion(double maxRange, double modifier = 1, double range = -1)
    {
        var horizontalDisp = CalculateHorizontalDispersion(range >= 0 ? range : maxRange, modifier);
        var verticalDisp = CalculateVerticalDispersion(maxRange, horizontalDisp, range);

        return new DispersionContainer(horizontalDisp, verticalDisp);
    }

    public sealed record DispersionContainer(double Horizontal, double Vertical);
}
