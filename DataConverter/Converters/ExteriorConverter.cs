using GameParamsExtractor.WGStructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.DataStructures;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace DataConverter.Converters
{
    public class ExteriorConverter
    {
        public static Dictionary<string, Exterior> ConvertExterior(IEnumerable<WgExterior> wgExterior)
        {
            //create a List of our Objects
            Dictionary<string, Exterior> exteriorList = new Dictionary<string, Exterior>();

            //iterate over the entire list to convert everything
            foreach (var currentWgExterior in wgExterior)
            {
                Program.TranslationNames.Add(currentWgExterior.Name);
                //create our object type
                Exterior exterior = new Exterior
                {
                    //start mapping
                    Id = currentWgExterior.Id,
                    Index = currentWgExterior.Index,
                    Name = currentWgExterior.Name,
                    SortOrder = currentWgExterior.SortOrder,
                    Group = currentWgExterior.Group,
                };

                Dictionary<string, double> modifiers = new Dictionary<string, double>();
                foreach (var currentWgExteriorModifier in currentWgExterior.Modifiers)
                {
                    JToken jtoken = currentWgExteriorModifier.Value;

                    if (jtoken.Type == JTokenType.Float || jtoken.Type == JTokenType.Integer)
                    {
                        modifiers.Add(currentWgExteriorModifier.Key, jtoken.Value<double>());
                    }
                    else
                    {
                        JObject jObject = (JObject)jtoken;
                        var values = jObject.ToObject<Dictionary<string, double>>()!;
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
                            modifiers.Add(currentWgExteriorModifier.Key, first);
                        }
                        else
                        {
                            foreach ((string key, double value) in values)
                            {
                                modifiers.Add($"{currentWgExteriorModifier.Key}_{key}", value);
                            }
                        }
                    }
                }

                exterior.Modifiers = modifiers;
                Program.TranslationNames.UnionWith(modifiers.Keys);
                try
                {
                    exterior.Type = Enum.Parse<ExteriorType>(currentWgExterior.typeinfo.species);
                }
                catch (ArgumentException)
                {
                    var currentWgExteriorType = currentWgExterior.typeinfo.species;
                    if (currentWgExteriorType != null)
                    {
                        if (currentWgExteriorType.Equals("Skin") || currentWgExteriorType.Equals("MSkin"))
                        {
                            exterior.Type = ExteriorType.Permoflage;
                        }
                        else
                        {
                            Console.WriteLine($"Found an unknown exterior type. Type: {currentWgExteriorType}, ID: {exterior.Id}");
                            // throw new ArgumentException($"Type: {currentWgExteriorType}, ID: {exterior.Id}");
                            continue;
                        }
                    }
                }

                exterior.Restrictions = new Restriction()
                {
                    ForbiddenShips = currentWgExterior.Restrictions?.ForbiddenShips?.ToList<string>(),
                    Levels = currentWgExterior.Restrictions?.Levels?.ToList<string>(),
                    Nations = currentWgExterior.Restrictions?.Nations?.ToList<string>(),
                    SpecificShips = currentWgExterior.Restrictions?.SpecificShips?.ToList<string>(),
                    Subtype = currentWgExterior.Restrictions?.Subtype?.ToList<string>(),
                };

                //dictionary with Index as key
                exteriorList.Add(exterior.Index, exterior);
            }

            return exteriorList;
        }
    }
}
