﻿using FluentAssertions;
using NUnit.Framework;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;

namespace DataConverter.Test;

[TestFixture]
public class DispersionTest
{
    // Dispersion data for Zao
    private readonly Dispersion dispersion = new()
    {
        IdealRadius = 8.0,
        MinRadius = 0.5,
        IdealDistance = 1000,
        TaperDist = 4000,
        RadiusOnZero = 0.2,
        RadiusOnDelim = 0.5,
        RadiusOnMax = 0.6,
        Delim = 0.5,
    };

    [Test]
    public void HorizontalDispersionCalculation()
    {
        double range = 16230;
        var horizontalDispersion = dispersion.CalculateHorizontalDispersion(range, 1);
        horizontalDispersion.Should().BeApproximately(136.7, 0.1);
    }

    [Test]
    public void HorizontalDispersionCalculation_ShortRange()
    {
        double range = 3500;
        var horizontalDispersion = dispersion.CalculateHorizontalDispersion(range, 1);
        horizontalDispersion.Should().BeApproximately(39.3, 0.1);
    }

    [Test]
    public void VerticalDispersionCalculation()
    {
        double range = 16230;
        var verticalDispersion = dispersion.CalculateVerticalDispersion(range, dispersion.CalculateHorizontalDispersion(range, 1));
        verticalDispersion.Should().BeApproximately(82, 0.1);
    }

    [Test]
    public void VerticalDispersionCalculation_ShortRange()
    {
        double maxRange = 16230;
        double range = 3500;
        var verticalDispersion = dispersion.CalculateVerticalDispersion(maxRange, dispersion.CalculateHorizontalDispersion(range, 1), range);
        verticalDispersion.Should().BeApproximately(13, 0.1);
    }
}
