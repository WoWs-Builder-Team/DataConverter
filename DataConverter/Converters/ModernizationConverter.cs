using DataConverter.WGStructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WoWsShipBuilderDataStructures;

namespace DataConverter.Converters
{
    public static class ModernizationConverter
    {
        //convert the list of modernizations from WG to our list of Modernizations
        public static Dictionary<string, Modernization> ConvertModernization(string jsonInput)
        {
            //create a List of our Objects
            Dictionary<string, Modernization> modList = new Dictionary<string, Modernization>();

            //deserialize into an object
            var wgModernizations = JsonConvert.DeserializeObject<List<WGModernization>>(jsonInput) ?? throw new InvalidOperationException();

            //iterate over the entire list to convert everything
            foreach (var currentWgMod in wgModernizations)
            {
                //create our object type
                Modernization mod = new Modernization
                {
                    //start mapping
                    Id = currentWgMod.id,
                    Index = currentWgMod.index,
                    Effect = currentWgMod.modifiers,
                    Name = currentWgMod.name
                };

                //for List of Enums
                List<Nation> allowedNations = new List<Nation>();
                var nationList = currentWgMod.nation;
                foreach (var nation in nationList)
                {
                    //this will get the respective Enum value
                    Nation value = Enum.Parse<Nation>(nation);
                    allowedNations.Add(value);
                }

                mod.AllowedNations = allowedNations;

                //array into List
                mod.ShipLevel = new List<int>(currentWgMod.shiplevel);
                mod.AdditionalShips = new List<string>(currentWgMod.ships);

                List<ShipClass> allowedClasses = new List<ShipClass>();
                var classList = currentWgMod.shiptype;
                foreach (var shipClass in classList)
                {
                    ShipClass value = Enum.Parse<ShipClass>(shipClass);
                    allowedClasses.Add(value);
                }

                mod.ShipClasses = allowedClasses;

                mod.Slot = currentWgMod.slot;
                mod.BlacklistedShips = new List<string>(currentWgMod.excludes);
                // dictionary with index as key, for easier search
                modList.Add(currentWgMod.index, mod);
            }

            return modList;
        }
    }
}