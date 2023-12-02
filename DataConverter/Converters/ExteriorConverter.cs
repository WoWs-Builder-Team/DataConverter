using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DataConverter.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Exterior;
using WoWsShipBuilder.DataStructures.Modifiers;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace DataConverter.Converters;

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
            var exterior = new ExteriorBuilder
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
                    // fix for regenerationHPSpeed being used as name for 3 different things: this exterior buff, the flat regen from lutjens and the actual hp regen.
                        if (currentWgExterior.Index.Equals("PCEF007"))
                        {
                            modifiersDictionary.TryGetValue("exterior_" + currentWgExteriorModifier.Key, out Modifier? modifierData);
                            modifiers.Add(new Modifier("exterior_" + currentWgExteriorModifier.Key, token.Value<float>(), "Exterior", modifierData));
                        }
                        else
                        {
                            modifiersDictionary.TryGetValue(currentWgExteriorModifier.Key, out Modifier? modifierData);

                            modifiers.Add(new Modifier(currentWgExteriorModifier.Key, token.Value<float>(), "Exterior", modifierData));
                        }
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

            exterior.Modifiers = modifiers.ToImmutableList();
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
                ForbiddenShips = currentWgExterior.Restrictions.ForbiddenShips.ToImmutableArray(),
                Levels = currentWgExterior.Restrictions.Levels.ToImmutableArray(),
                Nations = currentWgExterior.Restrictions.Nations.ToImmutableArray(),
                SpecificShips = currentWgExterior.Restrictions.SpecificShips.ToImmutableArray(),
                Subtype = currentWgExterior.Restrictions.Subtype.ToImmutableArray(),
            };

            //dictionary with Index as key
            exteriorList.Add(exterior.Index, exterior.ToExterior());
        }

        return exteriorList;
    }

    private sealed class ExteriorBuilder
    {
        public long Id { get; set; }

        public string Index { get; set; } = string.Empty;

        public ImmutableList<Modifier> Modifiers { get; set; } = ImmutableList<Modifier>.Empty;

        public string Name { get; set; } = string.Empty;

        public ExteriorType Type { get; set; }

        public int SortOrder { get; set; }

        public Restriction Restrictions { get; set; } = new();

        public int Group { get; set; }

        public Exterior ToExterior()
        {
            return new()
            {
                Id = Id,
                Index = Index,
                Modifiers = Modifiers,
                Name = Name,
                Type = Type,
                SortOrder = SortOrder,
                Restrictions = Restrictions,
                Group = Group,
            };
        }
    }
}
