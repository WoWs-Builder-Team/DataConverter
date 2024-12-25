using System;
using JetBrains.Annotations;
using WoWsShipBuilder.DataStructures.Ship;

namespace WoWsShipBuilder.DataStructures;

[PublicAPI]
public static class DataExtensions
{
    public static AntiAirAura AddAura(this AntiAirAura first, AntiAirAura second)
    {
        if (first.MaxRange > 0 && (first.MaxRange != second.MaxRange || first.HitChance != second.HitChance))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Cannot combine auras with different ranges or accuracies");
            Console.ResetColor();
            //throw new InvalidOperationException("Cannot combine auras with different ranges or accuracies.");
        }

        decimal minRange = second.FlakDamage > 0 ? first.MinRange : second.MinRange; // Use minimum range of new aura only if it is no flak cloud aura

        return new()
        {
            ConstantDps = first.ConstantDps + second.ConstantDps,
            FlakDamage = second.FlakDamage,
            FlakCloudsNumber = first.FlakCloudsNumber + second.FlakCloudsNumber,
            HitChance = second.HitChance,
            MaxRange = second.MaxRange,
            MinRange = minRange,
        };
    }

    public static double CalculateHorizontalDispersion(this Dispersion dispersion, double range, double dispersionModifier)
    {
        // Calculation according to https://www.reddit.com/r/WorldOfWarships/comments/l1dpzt/reverse_engineered_dispersion_ellipse_including/
        double x = range / 30;
        double effectiveTaperDist = dispersion.TaperDist / 30;
        if (x <= effectiveTaperDist)
        {
            return (x * (dispersion.IdealRadius - dispersion.MinRadius) / dispersion.IdealDistance + dispersion.MinRadius * (x / effectiveTaperDist)) * 30 * dispersionModifier;
        }

        return (x * (dispersion.IdealRadius - dispersion.MinRadius) / dispersion.IdealDistance + dispersion.MinRadius) * 30 * dispersionModifier;
    }

    public static double CalculateVerticalDispersion(this Dispersion dispersion, double maxRange, double horizontalDispersion, double range = -1)
    {
        // Calculation according to https://www.reddit.com/r/WorldOfWarships/comments/l1dpzt/reverse_engineered_dispersion_ellipse_including/
        maxRange /= 30;
        double x = range >= 0 ? range / 30 : maxRange;
        double delimDist = maxRange * dispersion.Delim;

        double verticalCoeff;
        if (x < delimDist)
        {
            verticalCoeff = dispersion.RadiusOnZero + (dispersion.RadiusOnDelim - dispersion.RadiusOnZero) * (x / delimDist);
        }
        else
        {
            verticalCoeff = dispersion.RadiusOnDelim + (dispersion.RadiusOnMax - dispersion.RadiusOnDelim) * (x - delimDist) / (maxRange - delimDist);
        }

        return horizontalDispersion * verticalCoeff;
    }

    public static DispersionValue CalculateDispersion(this Dispersion dispersion, double maxRange, double dispersionModifier, double range = -1)
    {
        var horizontalDisp = CalculateHorizontalDispersion(dispersion, range >= 0 ? range : maxRange, dispersionModifier);
        var verticalDisp = CalculateVerticalDispersion(dispersion, maxRange, horizontalDisp, range);

        return new(horizontalDisp, verticalDisp);
    }
}

public sealed record DispersionValue(double Horizontal, double Vertical);
