using System;
using System.IO;
using DataConverter.WGStructure;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataConverter.Test.ShipTests
{
    public class WgShipDeserialization
    {
        [Test]
        public void DeserializeWgShip_Success()
        {
            var filePath = @"ShipTests\TestData\SingleShip.json";
            var fileContent = File.ReadAllText(filePath);

            var ship = JsonConvert.DeserializeObject<WGShip>(fileContent);
            Assert.NotNull(ship);
        }
        
        [Test]
        public void DeserializeWgShip_SuccessUpgradeInfoProcessing()
        {
            var filePath = @"ShipTests\TestData\SingleShip.json";
            var fileContent = File.ReadAllText(filePath);

            var ship = JsonConvert.DeserializeObject<WGShip>(fileContent);
            Assert.NotNull(ship);
            Assert.AreEqual(9, ship.ShipUpgradeInfo.ConvertedUpgrades.Count);
        }
    }
}