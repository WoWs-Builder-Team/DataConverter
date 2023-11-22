using System;
using System.Collections.Generic;
using System.Linq;
using DataConverter.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Exterior;
using WoWsShipBuilder.DataStructures.Modifiers;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace DataConverter.Converters
{
    public class ExteriorConverter
    {
        public static Dictionary<string, Exterior> ConvertExterior(IEnumerable<WgExterior> wgExterior, ILogger? logger, Dictionary<string, Modifier> modifiersDictionary)
        {
            //create a List of our Objects
            Dictionary<string, Exterior> exteriorList = new Dictionary<string, Exterior>();

            //iterate over the entire list to convert everything
            foreach (var currentWgExterior in wgExterior)
            {
                DataCache.TranslationNames.Add(currentWgExterior.Name);
                //create our object type
                var exterior = new Exterior
                {
                    //start mapping
                    Id = currentWgExterior.Id,
                    Index = currentWgExterior.Index,
                    Name = currentWgExterior.Name,
                    SortOrder = currentWgExterior.SortOrder,
                    Group = currentWgExterior.Group,
                };

                var modifiers = new List<Modifier>();
                foreach (var currentWgExteriorModifier in currentWgExterior.Modifiers)
                {
                    var token = currentWgExteriorModifier.Value;

                    if (token.Type is JTokenType.Float or JTokenType.Integer)
                    {
                        modifiersDictionary.TryGetValue(currentWgExteriorModifier.Key, out Modifier? modifierData);
                        modifiers.Add(new Modifier(currentWgExteriorModifier.Key, token.Value<float>(), "Exterior", modifierData));
                    }
                    else
                    {
                        JObject jObject = (JObject)token;
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
                            modifiersDictionary.TryGetValue(currentWgExteriorModifier.Key, out Modifier? modifierData);
                            modifiers.Add(new Modifier(currentWgExteriorModifier.Key, first, "Exterior", modifierData));
                        }
                        else
                        {
                            foreach ((string key, float value) in values)
                            {
                                // exclude auxiliary ship type modifiers, since they are unused
                                if (!key.Equals("auxiliary", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    modifiersDictionary.TryGetValue($"{currentWgExteriorModifier.Key}_{key}", out Modifier? modifierData);
                                    modifiers.Add(new Modifier($"{currentWgExteriorModifier.Key}_{key}", value, "Exterior", modifierData));
                                }
                            }
                        }
                    }
                }

                exterior.Modifiers = modifiers;
                DataCache.TranslationNames.UnionWith(modifiers.Select(m => m.Name));
                try
                {
                    exterior.Type = Enum.Parse<ExteriorType>(currentWgExterior.TypeInfo.Species);
                }
                catch (ArgumentException)
                {
                    var currentWgExteriorType = currentWgExterior.TypeInfo.Species;
                    if (currentWgExteriorType.Equals("Skin") || currentWgExteriorType.Equals("MSkin"))
                    {
                        exterior.Type = ExteriorType.Permoflage;
                    }
                    else
                    {
                        logger?.LogWarning("Found an unknown exterior type. Type: {}, ID: {}", currentWgExteriorType, exterior.Id);
                        continue;
                    }
                }

                if (exterior.Type != ExteriorType.Flags)
                {
                    continue;
                }

                exterior.Restrictions = new()
                {
                    ForbiddenShips = currentWgExterior.Restrictions.ForbiddenShips.ToList(),
                    Levels = currentWgExterior.Restrictions.Levels.ToList(),
                    Nations = currentWgExterior.Restrictions.Nations.ToList(),
                    SpecificShips = currentWgExterior.Restrictions.SpecificShips.ToList(),
                    Subtype = currentWgExterior.Restrictions.Subtype.ToList(),
                };

                //dictionary with Index as key
                exteriorList.Add(exterior.Index, exterior);
            }

            return exteriorList;
        }
    }
}
