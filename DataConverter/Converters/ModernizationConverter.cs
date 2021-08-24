using DataConverter.WGStructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using WoWsShipBuilderDataStructures;

namespace DataConverter.Converters
{
    public class ModernizationConverter
    {
        //convert the list of modernizations from WG to our list of Modernizations
        public static Dictionary<string,Modernization> ConvertModernization()
        {
            //create a List of our Objects
            Dictionary<string, Modernization> modList = new Dictionary<string, Modernization>();

            //read the json we extracted
            string fileName = $"{Program.inputFolder}/Modernization/Common.json";
            string wgList = File.ReadAllText(fileName);

            //deserialize into an object
            var wgModernizations = JsonConvert.DeserializeObject<List<WGModernization>>(wgList);

            //iterate over the entire list to convert everything
            foreach (var currentWGMod in wgModernizations)
            {
                //create our object type
                Modernization mod = new Modernization();
                //start mapping
                mod.Id = currentWGMod.id;
                mod.Index = currentWGMod.index;
                mod.Effect = currentWGMod.modifiers;
                mod.Name = currentWGMod.name;

                //for List of Enums
                List<Nation> allowedNations = new List<Nation>();
                var nationList = currentWGMod.nation;
                foreach (var nation in nationList)
                {
                    //this will get the respective Enum value
                    Nation value = (Nation)Enum.Parse(typeof(Nation), nation);
                    allowedNations.Add(value);
                }
                mod.AllowedNations = allowedNations;

                //array into List
                mod.ShipLevel = new List<int>(currentWGMod.shiplevel);
                mod.AdditionalShips = new List<string>(currentWGMod.ships);

                List<ShipClass> allowedClasses = new List<ShipClass>();
                var classList = currentWGMod.shiptype;
                foreach (var shipClass in classList)
                {
                    ShipClass value = (ShipClass)Enum.Parse(typeof(ShipClass), shipClass);
                    allowedClasses.Add(value);
                }
                mod.ShipClasses = allowedClasses;

                mod.Slot = currentWGMod.slot;
                mod.BlacklistedShips = new List<string>(currentWGMod.excludes);
                // dictionary with index as key, for easier search
                modList.Add(currentWGMod.index,mod);

            }

            return modList;
        }
    }
}
