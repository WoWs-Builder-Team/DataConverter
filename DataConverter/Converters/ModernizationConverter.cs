using DataConverter.WGStructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WoWsShipBuilder.DataStructures;

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
                Program.TranslationNames.Add(currentWgMod.name);
                //create our object type
                Modernization mod = new Modernization
                {
                    //start mapping
                    Id = currentWgMod.id,
                    Index = currentWgMod.index,
                    Name = currentWgMod.name,
                    Type = ConvertModernizationType(currentWgMod.type),
                };
                
                Dictionary<string, double> effects = new Dictionary<string, double>();
                foreach (var currentWgModModifier in currentWgMod.modifiers)
                {
                    JToken jtoken = currentWgModModifier.Value;

                    if (jtoken.Type == JTokenType.Float || jtoken.Type == JTokenType.Integer)
                    {
                        effects.Add(currentWgModModifier.Key, jtoken.Value<double>());
                    }
                    else
                    {
                        JObject jObject = (JObject)jtoken;
                        var values = jObject.ToObject<Dictionary<string, double>>();
                        bool isEqual = true;
                        var first = values.First().Value;
                        foreach ((string key, double value) in values)
                        {
                            if (value != first)
                            {
                                isEqual = false;
                            }
                        }
                        if (isEqual)
                        {
                            effects.Add(currentWgModModifier.Key, first);
                        }
                        else
                        {
                            foreach ((string key, double value) in values)
                            {
                                effects.Add($"{currentWgModModifier.Key}_{key}", value);
                            }
                        }
                    }
                }
                Program.TranslationNames.UnionWith(effects.Keys);
                mod.Effect = effects;

                //for List of Enums
                List<Nation> allowedNations = new List<Nation>();
                var nationList = currentWgMod.nation;
                foreach (var nation in nationList)
                {
                    //this will get the respective Enum value
                    Nation value = Enum.Parse<Nation>(nation.Replace("_",""), true);
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

        private static ModernizationType ConvertModernizationType(int wgType)
        {
            return wgType switch
            {
                0 => ModernizationType.Normal,
                1 => ModernizationType.Consumable,
                3 => ModernizationType.Legendary,
                _ => ModernizationType.Other,
            };
        }
    }
}