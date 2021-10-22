using System;
using NUnit.Framework;
using WoWsShipBuilderDataStructures;

namespace DataConverter.Test
{
    [TestFixture]
    public class DispersionTest
    {
        // Dispersion data for Zao
        private Dispersion dispersion = new()
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
            var horizontalDispersion = dispersion.CalculateHorizontalDispersion(range);
            Assert.True(Math.Abs(horizontalDispersion - 136.7) < 0.1);
        }
        
        [Test]
        public void VerticalDispersionCalculation()
        {
            double range = 16230;
            var verticalDispersion = dispersion.CalculateVerticalDispersion(range);
            Assert.True(Math.Abs(verticalDispersion - 82) < 0.1);
        }
    }
}