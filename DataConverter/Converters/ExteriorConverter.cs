using System;
using System.Collections.Generic;
using System.Linq;
using DataConverter.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace DataConverter.Converters
{
    public class ExteriorConverter
    {
        public static Dictionary<string, Exterior> ConvertExterior(IEnumerable<WgExterior> wgExterior, ILogger? logger)
        {
            //create a List of our Objects
            Dictionary<string, Exterior> exteriorList = new Dictionary<string, Exterior>();

            //iterate over the entire list to convert everything
            foreach (var currentWgExterior in wgExterior)
            {
                DataCache.TranslationNames.Add(currentWgExterior.Name);
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

                    if (jtoken.Type is JTokenType.Float or JTokenType.Integer)
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
                DataCache.TranslationNames.UnionWith(modifiers.Keys);
                try
                {
                    exterior.Type = Enum.Parse<ExteriorType>(currentWgExterior.TypeInfo.Species);
                }
                catch (ArgumentException)
                {
                    var currentWgExteriorType = currentWgExterior.TypeInfo.Species;
                    if (currentWgExteriorType != null)
                    {
                        if (currentWgExteriorType.Equals("Skin") || currentWgExteriorType.Equals("MSkin"))
                        {
                            exterior.Type = ExteriorType.Permoflage;
                        }
                        else
                        {
                            logger?.LogWarning("Found an unknown exterior type. Type: {}, ID: {}", currentWgExteriorType, exterior.Id);
                            // throw new ArgumentException($"Type: {currentWgExteriorType}, ID: {exterior.Id}");
                            continue;
                        }
                    }
                }

                if (exterior.Type != ExteriorType.Flags)
                {
                    continue;
                }

                exterior.Restrictions = new()
                {
                    ForbiddenShips = currentWgExterior.Restrictions?.ForbiddenShips?.ToList(),
                    Levels = currentWgExterior.Restrictions?.Levels?.ToList(),
                    Nations = currentWgExterior.Restrictions?.Nations?.ToList(),
                    SpecificShips = currentWgExterior.Restrictions?.SpecificShips?.ToList(),
                    Subtype = currentWgExterior.Restrictions?.Subtype?.ToList(),
                };

                //dictionary with Index as key
                exteriorList.Add(exterior.Index, exterior);
            }

            return exteriorList;
        }
    }
}
