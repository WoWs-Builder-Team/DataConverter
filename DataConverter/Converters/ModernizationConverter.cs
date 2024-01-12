using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DataConverter.Data;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Upgrade;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace DataConverter.Converters;

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

            var modifiers = ConvertEffects(currentWgMod, modifiersDictionary);
            DataCache.TranslationNames.UnionWith(modifiers.Select(m => m.Name));

            var nationList = currentWgMod.Nation;
            var allowedNations = nationList.Select(nation => Enum.Parse<Nation>(nation.Replace("_", string.Empty), true)).ToImmutableArray();

            string[] classList = currentWgMod.Shiptype;
            var allowedClasses = classList.Select(Enum.Parse<ShipClass>).ToImmutableArray();

            // create our object type
            var mod = new Modernization
            {
                //start mapping
                Id = currentWgMod.Id,
                Index = currentWgMod.Index,
                Name = currentWgMod.Name,
                Type = ConvertModernizationType(currentWgMod.Type),
                Modifiers = modifiers,
                AllowedNations = allowedNations,
                ShipLevel = currentWgMod.Shiplevel.ToImmutableArray(),
                AdditionalShips = currentWgMod.Ships.ToImmutableList(),
                ShipClasses = allowedClasses,
                Slot = currentWgMod.Slot,
                BlacklistedShips = currentWgMod.Excludes.ToImmutableList(),
            };

            // dictionary with index as key, for easier search
            modList.Add(currentWgMod.Index, mod);
        }

        return modList;
    }

    private static ImmutableList<Modifier> ConvertEffects(WgModernization currentWgMod, Dictionary<string, Modifier> modifiersDictionary)
    {
        var modifiers = new List<Modifier>();
        foreach (var currentWgModModifier in currentWgMod.Modifiers)
        {
            JToken jtoken = currentWgModModifier.Value;

            if (jtoken.Type is JTokenType.Float or JTokenType.Integer)
            {
                modifiersDictionary.TryGetValue(currentWgModModifier.Key, out Modifier? modifierData);
                modifiers.Add(new(currentWgModModifier.Key, jtoken.Value<float>(), "Modernization", modifierData));
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
                    modifiers.Add(new(currentWgModModifier.Key, first, "Modernization", modifierData));
                }
                else
                {
                    foreach ((string key, float value) in values)
                    {
                        // exclude auxiliary ship type modifiers, since they are unused
                        if (!key.Equals("auxiliary", StringComparison.InvariantCultureIgnoreCase))
                        {
                            modifiersDictionary.TryGetValue($"{currentWgModModifier.Key}", out Modifier? modifierData);
                            modifiers.Add(new($"{currentWgModModifier.Key}", value, "Modernization", ConverterUtils.ProcessShipClass(key), modifierData));
                        }
                    }
                }
            }
        }

        return modifiers.ToImmutableList();
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
