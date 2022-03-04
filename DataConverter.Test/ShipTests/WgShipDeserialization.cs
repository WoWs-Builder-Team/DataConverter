using System.Collections.Generic;
using System.IO;
using DataConverter.Converters;
using FluentAssertions;
using GameParamsExtractor.WGStructure;
using Newtonsoft.Json;
using NUnit.Framework;
using WoWsShipBuilder.DataStructures;

namespace DataConverter.Test.ShipTests
{
    // [Ignore("Temporarily disabled due to data structure adjustments")]
    public class WgShipDeserialization
    {
        private string GetFilePath(string fileName)
        {
            return Path.Combine("ShipTests", "TestData", fileName);
        }

        #region Ship

        [Test]
        public void DeserializeWgShip_Success()
        {
            var filePath = GetFilePath("filtered_USA_Ship.json");
            var fileContent = File.ReadAllText(filePath);

            var ship = JsonConvert.DeserializeObject<List<WgShip>>(fileContent);
            Assert.NotNull(ship);
        }

        [Test]
        public void DeserializeShip_Succes()
        {
            var filePath = GetFilePath("filtered_USA_Ship.json");
            var fileContent = File.ReadAllText(filePath);

            var dict = ShipConverter.ConvertShips(fileContent);
            string test = JsonConvert.SerializeObject(dict);
            Assert.NotNull(test);
        }

        public void ConvertShip()
        {
            var filePath = GetFilePath("ConverterTest.json");
            var fileContent = File.ReadAllText(filePath);
            var result = ShipConverter.ConvertShips(fileContent);
            Assert.AreEqual(1, result.Count);
            File.WriteAllText("output.json", JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        #endregion

        #region Aircraft

        [Test]
        public void DeserializeWgAircraft_Succes()
        {
            var filePath = GetFilePath("filtered_USA_Aircraft.json");
            var fileContent = File.ReadAllText(filePath);

            var test = JsonConvert.DeserializeObject<List<WGAircraft>>(fileContent);
            Assert.NotNull(test);
        }

        [Test]
        public void DeserializeAircraft_Succes()
        {
            var filePath = GetFilePath("filtered_USA_Aircraft.json");
            var fileContent = File.ReadAllText(filePath);

            var dict = AircraftConverter.ConvertAircraft(fileContent);
            string test = JsonConvert.SerializeObject(dict);
            Assert.NotNull(test);
        }

        #endregion

        #region Captain

        [Test]
        public void DeserializeWgCaptain_Succes()
        {
            var filePath = GetFilePath("filtered_Germany_Crew.json");
            var fileContent = File.ReadAllText(filePath);

            var test = JsonConvert.DeserializeObject<List<WGCaptain>>(fileContent);
            Assert.NotNull(test);
        }

        [Test]
        public void DeserializeCaptain_Succes()
        {
            var filePath = GetFilePath("filtered_USA_Crew.json");
            var fileContent = File.ReadAllText(filePath);

            var skillsJsonPath = GetFilePath("SKILLS_BY_TIER.json");
            var skillsJsonContent = File.ReadAllText(skillsJsonPath);

            var dict = CaptainConverter.ConvertCaptain(fileContent, skillsJsonContent, false);
            string test = JsonConvert.SerializeObject(dict);
            Assert.NotNull(test);
        }

        #endregion

        #region Projectile

        [Test]
        public void DeserializeWgProjectile_Succes()
        {
            var filePath = GetFilePath("filtered_USA_Projectile.json");
            var fileContent = File.ReadAllText(filePath);

            var test = JsonConvert.DeserializeObject<List<WGProjectile>>(fileContent);
            Assert.NotNull(test);
        }

        [Test]
        public void DeserializeProjectile_Succes()
        {
            var filePath = GetFilePath("filtered_USA_Projectile.json");
            var fileContent = File.ReadAllText(filePath);

            var dict = ProjectileConverter.ConvertProjectile(fileContent);
            string test = JsonConvert.SerializeObject(dict);
            Assert.NotNull(test);
        }

        #endregion

        #region Modernization

        [Test]
        public void DeserializeWgModernization_Succes()
        {
            var filePath = GetFilePath("filtered_Modernization.json");
            var fileContent = File.ReadAllText(filePath);

            var test = JsonConvert.DeserializeObject<List<WGModernization>>(fileContent);
            Assert.NotNull(test);
        }

        [Test]
        public void DeserializeModernization_Succes()
        {
            var filePath = GetFilePath("filtered_Modernization.json");
            var fileContent = File.ReadAllText(filePath);

            var dict = ModernizationConverter.ConvertModernization(fileContent);
            string test = JsonConvert.SerializeObject(dict);
            Assert.NotNull(test);
        }

        #endregion

        #region Consumable

        [Test]
        public void DeserializeWgConsumable_Succes()
        {
            var filePath = GetFilePath("filtered_Consumable.json");
            var fileContent = File.ReadAllText(filePath);

            var test = JsonConvert.DeserializeObject<List<WGConsumable>>(fileContent);
            Assert.NotNull(test);
        }

        [Test]
        public void DeserializeConsumable_Succes()
        {
            var filePath = GetFilePath("filtered_Consumable.json");
            var fileContent = File.ReadAllText(filePath);

            var dict = ConsumableConverter.ConvertConsumable(fileContent);
            string test = JsonConvert.SerializeObject(dict);
            Assert.NotNull(test);
        }

        #endregion

        #region Module

        [Test]
        public void DeserializeWgModule_Succes()
        {
            var filePath = GetFilePath("filtered_USA_Module.json");
            var fileContent = File.ReadAllText(filePath);

            var test = JsonConvert.DeserializeObject<List<WGModule>>(fileContent);
            Assert.NotNull(test);
        }

        [Test]
        public void DeserializeModule_Succes()
        {
            var filePath = GetFilePath("filtered_USA_Module.json");
            var fileContent = File.ReadAllText(filePath);

            var dict = ModuleConverter.ConvertModule(fileContent);
            string test = JsonConvert.SerializeObject(dict);
            Assert.NotNull(test);
        }

        #endregion

        #region Exterior

        [Test]
        public void DeserializeWgExterior_Succes()
        {
            var filePath = GetFilePath("filtered_USA_Exterior.json");
            var fileContent = File.ReadAllText(filePath);

            var test = JsonConvert.DeserializeObject<List<WGExterior>>(fileContent);
            Assert.NotNull(test);

            var filePath2 = GetFilePath("filtered_Common_Exterior.json");
            var fileContent2 = File.ReadAllText(filePath2);

            var test2 = JsonConvert.DeserializeObject<List<WGExterior>>(fileContent2);
            Assert.NotNull(test2);
        }

        [Test]
        public void DeserializeConsumablePermacamos_Succes()
        {
            // TODO: enable after adding exterior converter. Disabled to allow compilation.
             var filePath = GetFilePath("filtered_USA_Exterior.json");
             var fileContent = File.ReadAllText(filePath);

             var dict = ExteriorConverter.ConvertExterior(fileContent);
             string test = JsonConvert.SerializeObject(dict);
             Assert.NotNull(test);

             var filePath2 = GetFilePath("filtered_Common_Exterior.json");
             var fileContent2 = File.ReadAllText(filePath2);

             var dict2 = ExteriorConverter.ConvertExterior(fileContent2);
             string test2 = JsonConvert.SerializeObject(dict2);
             Assert.NotNull(test2);
        }
        #endregion

        #region VersionInfo

        [Test]
        public void DeserializeVersionInfo()
        {
            var filePath = GetFilePath("VersionInfo.json");
            var fileContent = File.ReadAllText(filePath);

            var versionInfo = JsonConvert.DeserializeObject<VersionInfo>(fileContent);

            Assert.NotNull(versionInfo);
            versionInfo.CurrentVersionCode.Should().Be(1);
            versionInfo.Categories.Should().NotBeEmpty();
        }

        #endregion

        #region Summary

        [Test]
        public void DeserializeSummary()
        {
            var filePath = GetFilePath("Summary_Common.json");
            var fileContent = File.ReadAllText(filePath);

            var summaryList = JsonConvert.DeserializeObject<List<ShipSummary>>(fileContent);

            Assert.NotNull(summaryList);
            summaryList.Should().HaveCountGreaterThan(0);
        }

        #endregion
    }
}
