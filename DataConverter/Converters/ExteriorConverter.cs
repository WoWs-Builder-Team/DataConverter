using GameParamsExtractor.WGStructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilder.DataStructures;

namespace DataConverter.Converters
{
    public class ExteriorConverter
    {
        public static Dictionary<string, Exterior> ConvertExterior(IEnumerable<WGExterior> wgExterior)
        {
            //create a List of our Objects
            Dictionary<string, Exterior> exteriorList = new Dictionary<string, Exterior>();

            //iterate over the entire list to convert everything
            foreach (var currentWgExterior in wgExterior)
            {
                Program.TranslationNames.Add(currentWgExterior.name);
                //create our object type
                Exterior exterior = new Exterior
                {
                    //start mapping
                    Id = currentWgExterior.id,
                    Index = currentWgExterior.index,
                    Name = currentWgExterior.name,
                    SortOrder = currentWgExterior.sortOrder,
                    Group = currentWgExterior.group,
                };

                Dictionary<string, double> modifiers = new Dictionary<string, double>();
                foreach (var currentWgExteriorModifier in currentWgExterior.modifiers)
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
                            throw new ArgumentException($"Type: {currentWgExteriorType}, ID: {exterior.Id}");
                        }
                    }
                }

                exterior.Restrictions = new Restriction()
                {
                    ForbiddenShips = currentWgExterior.restrictions?.forbiddenShips?.ToList<string>(),
                    Levels = currentWgExterior.restrictions?.levels?.ToList<string>(),
                    Nations = currentWgExterior.restrictions?.nations?.ToList<string>(),
                    SpecificShips = currentWgExterior.restrictions?.specificShips?.ToList<string>(),
                    Subtype = currentWgExterior.restrictions?.subtype?.ToList<string>(),
                };

                //dictionary with Index as key
                exteriorList.Add(exterior.Index, exterior);
            }

            return exteriorList;
        }
    }
}
