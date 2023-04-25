using System;
using NUnit.Framework;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;

namespace DataConverter.Test
{
    [TestFixture]
    public class DispersionTest
    {
        // Dispersion data for Zao
        private readonly Dispersion dispersion = new(8.0, 0.5, 1000, 4000, 0.2, 0.5, 0.6, 0.5);

        [Test]
        public void HorizontalDispersionCalculation()
        {
            double range = 16230;
            var horizontalDispersion = dispersion.CalculateHorizontalDispersion(range, 1);
            Assert.True(Math.Abs(horizontalDispersion - 136.7) < 0.1);
        }

        [Test]
        public void HorizontalDispersionCalculation_ShortRange()
        {
            double range = 3500;
            var horizontalDispersion = dispersion.CalculateHorizontalDispersion(range, 1);
            Assert.True(Math.Abs(horizontalDispersion - 39.3) < 0.1);
        }

        [Test]
        public void VerticalDispersionCalculation()
        {
            double range = 16230;
            var verticalDispersion = dispersion.CalculateVerticalDispersion(range, dispersion.CalculateHorizontalDispersion(range, 1));
            Assert.True(Math.Abs(verticalDispersion - 82) < 0.1);
        }

        [Test]
        public void VerticalDispersionCalculation_ShortRange()
        {
            double maxRange = 16230;
            double range = 3500;
            var verticalDispersion = dispersion.CalculateVerticalDispersion(maxRange, dispersion.CalculateHorizontalDispersion(range, 1), range);
            Assert.True(Math.Abs(verticalDispersion - 13) < 0.1);
        }
    }
}
