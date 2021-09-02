using System.IO;
using DataConverter.Converters;
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

        [Test]
        public void ConvertShip()
        {
            var filePath = GetFilePath("ConverterTest.json");
            var fileContent = File.ReadAllText(filePath);
            var result = ShipConverter.ConvertShips(fileContent);
            Assert.AreEqual(1, result.Count);
            File.WriteAllText("output.json", JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        [Test]
        public void DeserializeWgAircraft_Succes()
        {
            var filePath = GetFilePath("SingleAircraft.json");
            var fileContent = File.ReadAllText(filePath);
            
            var aircraft = JsonConvert.DeserializeObject<WGAircraft>(fileContent);
            Assert.NotNull(aircraft);
        }

        [Test]
        public void DeserializeAircraft_Succes()
        {
            var filePath = GetFilePath("filtered_USA_Aircraft.json");
            var fileContent = File.ReadAllText(filePath);

            object dict = AircraftConverter.ConvertAircraft(fileContent);
            string test = JsonConvert.SerializeObject(dict);
            Assert.NotNull(test);
        }

        [Test]
        public void DeserializeWgCaptain_Succes()
        {
            var filePath = GetFilePath("SingleCrew.json");
            var fileContent = File.ReadAllText(filePath);

            var test = JsonConvert.DeserializeObject<WGCaptain>(fileContent);
            Assert.NotNull(test);
        }

        [Test]
        public void DeserializeCaptain_Succes()
        {
            var filePath = GetFilePath("filtered_USA_Crew.json");
            var fileContent = File.ReadAllText(filePath);

            var skillsJsonPath = GetFilePath("SKILLS_BY_TIER.json");
            var skillsJsonContent = File.ReadAllText(skillsJsonPath);

            object dict = CaptainConverter.ConvertCaptain(fileContent, skillsJsonContent);
            string test = JsonConvert.SerializeObject(dict);
            Assert.NotNull(test);
        }

    }
}