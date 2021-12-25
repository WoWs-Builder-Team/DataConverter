using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global

#nullable enable

namespace WoWsShipBuilder.DataStructures.Ship
{
    //small abuse, but we reuse this for secondaries.
    public sealed record TurretModule(decimal Sigma, decimal MaxRange, List<Gun> Guns)
    {
        public AntiAirAura? AntiAir { get; set; }

        public Dispersion? DispersionValues { get; set; }
    }

    public sealed record Gun(
        List<string> AmmoList,
        decimal BarrelDiameter,
        long Id,
        string Index,
        string Name,
        int NumBarrels,
        decimal HorizontalRotationSpeed,
        decimal VerticalRotationSpeed,
        decimal Reload)
    {
        public TurretOrientation TurretOrientation { get; set; }

        public double[]? HorizontalSector { get; set; }

        public double[][]? HorizontalDeadZones { get; set; }

        public double HorizontalPosition { get; set; }

        public double VerticalPosition { get; set; }

        public decimal SmokeDetectionWhenFiring { get; set; }

        public string? WgGunIndex { get; set; }
    }

    public sealed record Dispersion(
        double IdealRadius,
        double MinRadius,
        double IdealDistance,
        double TaperDist,
        double RadiusOnZero,
        double RadiusOnDelim,
        double RadiusOnMax,
        double Delim)
    {
        public decimal MaximumHorizontalDispersion { get; set; }

        public decimal MaximumVerticalDispersion { get; set; }

        public double CalculateHorizontalDispersion(double range)
        {
            // Calculation according to https://www.reddit.com/r/WorldOfWarships/comments/l1dpzt/reverse_engineered_dispersion_ellipse_including/ 
            double x = range / 30;
            double effectiveTaperDist = TaperDist / 30;
            if (x <= effectiveTaperDist)
            {
                return (x * (IdealRadius - MinRadius) / IdealDistance + MinRadius * (x / effectiveTaperDist)) * 30;
            }

            return (x * (IdealRadius - MinRadius) / IdealDistance + MinRadius) * 30;
        }

        public double CalculateVerticalDispersion(double maxRange, double range = -1)
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

            return CalculateHorizontalDispersion(x * 30) * verticalCoeff;
        }
    }
}

#nullable restore