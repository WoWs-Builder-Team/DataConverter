using System.IO;
using DataConverter.WGStructure;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataConverter.Test.ShipTests
{
    public class WgShipDeserialization
    {
        private string GetFilePath(string fileName)
        {
            return Path.Combine("ShipTests", "TestData", fileName);
        }
        
        [Test]
        public void DeserializeWgShip_Success()
        {
            var filePath = GetFilePath("SingleShip.json");
            var fileContent = File.ReadAllText(filePath);

            var ship = JsonConvert.DeserializeObject<WGShip>(fileContent);
            Assert.NotNull(ship);
        }
        
        [Test]
        public void DeserializeWgShip_SuccessUpgradeInfoProcessing()
        {
            var filePath = GetFilePath("SingleShip.json");
            var fileContent = File.ReadAllText(filePath);

            var ship = JsonConvert.DeserializeObject<WGShip>(fileContent);
            Assert.NotNull(ship);
            Assert.AreEqual(9, ship.ShipUpgradeInfo.ConvertedUpgrades.Count);
        }
    }
}