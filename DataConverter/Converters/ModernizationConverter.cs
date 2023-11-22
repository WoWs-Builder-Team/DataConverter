using System;
using System.Collections.Generic;
using System.Linq;
using DataConverter.Data;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Upgrade;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace DataConverter.Converters
{
    public static class ModernizationConverter
    {
        //convert the list of modernizations from WG to our list of Modernizations
        public static Dictionary<string, Modernization> ConvertModernization(IEnumerable<WgModernization> wgModernizations, Dictionary<string, Modifier> modifiersDictionary)
        {
            //create a List of our Objects
            Dictionary<string, Modernization> modList = new Dictionary<string, Modernization>();

            //iterate over the entire list to convert everything
            foreach (var currentWgMod in wgModernizations)
            {
                //skip removed/unused mods
                if (currentWgMod.Slot == -1)
                {
                    continue;
                }

                DataCache.TranslationNames.Add(currentWgMod.Name);
                //create our object type
                Modernization mod = new Modernization
                {
                    //start mapping
                    Id = currentWgMod.Id,
                    Index = currentWgMod.Index,
                    Name = currentWgMod.Name,
                    Type = ConvertModernizationType(currentWgMod.Type),
                };

                var modifiers = new List<Modifier>();
                foreach (var currentWgModModifier in currentWgMod.Modifiers)
                {
                    JToken jtoken = currentWgModModifier.Value;

                    if (jtoken.Type is JTokenType.Float or JTokenType.Integer)
                    {
                        modifiersDictionary.TryGetValue(currentWgModModifier.Key, out Modifier? modifierData);
                        modifiers.Add(new Modifier(currentWgModModifier.Key, jtoken.Value<float>(), "Modernization", modifierData));
                    }
                    else
                    {
                        JObject jObject = (JObject)jtoken;
                        var values = jObject.ToObject<Dictionary<string, float>>()!;
                        bool isEqual = true;
                        var first = values.First().Value;
                        foreach ((string _, float value) in values)
                        {
                            if (Math.Abs(value - first) > Constants.Tolerance)
                            {
                                isEqual = false;
                            }
                        }
                        if (isEqual)
                        {
                            modifiersDictionary.TryGetValue(currentWgModModifier.Key, out Modifier? modifierData);
                            modifiers.Add(new Modifier(currentWgModModifier.Key, first, "Modernization", modifierData));
                        }
                        else
                        {
                            foreach ((string key, float value) in values)
                            {
                                // exclude auxiliary ship type modifiers, since they are unused
                                if (!key.Equals("auxiliary", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    modifiersDictionary.TryGetValue($"{currentWgModModifier.Key}_{key}", out Modifier? modifierData);
                                    modifiers.Add(new Modifier($"{currentWgModModifier.Key}_{key}", value, "Modernization", modifierData));
                                }
                            }
                        }
                    }
                }
                DataCache.TranslationNames.UnionWith(modifiers.Select(m => m.Name));
                mod.Modifiers = modifiers;

                //for List of Enums
                List<Nation> allowedNations = new List<Nation>();
                var nationList = currentWgMod.Nation;
                foreach (var nation in nationList)
                {
                    //this will get the respective Enum value
                    Nation value = Enum.Parse<Nation>(nation.Replace("_", string.Empty), true);
                    allowedNations.Add(value);
                }

                mod.AllowedNations = allowedNations;

                //array into List
                mod.ShipLevel = new List<int>(currentWgMod.Shiplevel);
                mod.AdditionalShips = new List<string>(currentWgMod.Ships);

                List<ShipClass> allowedClasses = new List<ShipClass>();
                var classList = currentWgMod.Shiptype;
                foreach (var shipClass in classList)
                {
                    ShipClass value = Enum.Parse<ShipClass>(shipClass);
                    allowedClasses.Add(value);
                }

                mod.ShipClasses = allowedClasses;

                mod.Slot = currentWgMod.Slot;
                mod.BlacklistedShips = new List<string>(currentWgMod.Excludes);
                // dictionary with index as key, for easier search
                modList.Add(currentWgMod.Index, mod);
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
