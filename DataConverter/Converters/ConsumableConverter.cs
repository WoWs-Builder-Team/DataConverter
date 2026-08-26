using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DataConverter.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures.Consumable;
using WoWsShipBuilder.DataStructures.Modifiers;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace DataConverter.Converters
{
    public static class ConsumableConverter
    {
        /// <summary>
        /// The fields of a consumable variant's logic block that name a buff object, in the order they are merged.
        /// </summary>
        private static readonly string[] BuffLogicKeys = { "buff", "buffOnSelf" };

        //convert the list of consumables from WG to our list of Consumables
        public static Dictionary<string, Consumable> ConvertConsumable(IEnumerable<WgConsumable> wgConsumable, Dictionary<string, Modifier> modifierDictionary, IReadOnlyDictionary<string, WgBuff> buffDictionary, ILogger logger)
        {
            //create a List of our Objects
            Dictionary<string, Consumable> consumableList = new Dictionary<string, Consumable>();

            //iterate over the entire list to convert everything
            foreach (var currentWgConsumable in wgConsumable)
            {
                DataCache.TranslationNames.Add(currentWgConsumable.Name);
                //collecting consumable variants
                var variant = currentWgConsumable.Variants;
                List<string> variantsKeys = new List<string>(variant.Keys);
                DataCache.TranslationNames.UnionWith(variant.Values.Select(variantValue => variantValue.DescIDs));

                foreach (string currentVariantKey in variantsKeys)
                {
                    //mapping all the variants
                    WgStatistics stats = variant[currentVariantKey];

                    var isShipFighter = string.Equals(stats.Group, "ship") && string.Equals(stats.ConsumableType, "fighter");
                    //create our object type
                    var consumable = new Consumable
                    {
                        //start mapping
                        Id = currentWgConsumable.Id,
                        Index = currentWgConsumable.Index,
                        Name = currentWgConsumable.Name,
                        DescId = stats.DescIDs,
                        Group = stats.Group,
                        IconId = stats.IconIDs,
                        NumConsumables = stats.NumConsumables,
                        ReloadTime = stats.ReloadTime,
                        WorkTime = stats.WorkTime,
                        ConsumableVariantName = currentVariantKey,
                        PlaneName = isShipFighter && string.IsNullOrEmpty(stats.FightersName) ? "PAAF001_Grumman_F3F" : stats.FightersName,
                        PreparationTime = stats.PreparationTime,
                        IsTimeBased = stats.LifeCycleType == 1,
                        TimeBasedActiveTime = stats.MaxCapacity,
                        Modifiers = ConvertModifiers(currentWgConsumable, stats, modifierDictionary, buffDictionary, logger),
                    };
                    DataCache.TranslationNames.UnionWith(consumable.Modifiers.Select(m => m.Name));

                    //dictionary with consumable name and variant name separated by an empty space as keys
                    var consumableKey = $"{consumable.Name} {currentVariantKey}";
                    consumableList.Add(consumableKey, consumable);
                }
            }

            return consumableList;
        }

        /// <summary>
        /// Resolves the effects of the buff objects a consumable variant references.
        /// </summary>
        /// <remarks>
        /// A variant may name both a buff, applied to whatever the consumable targets, and a weaker buffOnSelf
        /// applied to the ship using it. Both are read so a self-only stat is not lost, but the plain buff wins on
        /// a collision: it is the value the in-game consumable card advertises, and two modifiers sharing a name
        /// would break any consumer that keys them by name.
        /// </remarks>
        private static Dictionary<string, float> ResolveBuffModifiers(WgStatistics consumableStats, IReadOnlyDictionary<string, WgBuff> buffDictionary, ILogger logger)
        {
            var results = new Dictionary<string, float>(StringComparer.Ordinal);
            foreach (string logicKey in BuffLogicKeys)
            {
                if (!consumableStats.Logic.TryGetValue(logicKey, out JToken? buffToken) || buffToken.Type != JTokenType.String)
                {
                    continue;
                }

                var buffName = buffToken.Value<string>();
                if (string.IsNullOrEmpty(buffName))
                {
                    continue;
                }

                if (!buffDictionary.TryGetValue(buffName, out WgBuff? buff))
                {
                    logger.LogWarning("Unable to resolve buff {Buff} referenced by a consumable", buffName);
                    continue;
                }

                foreach ((string key, float value) in buff.RetrieveModifiers())
                {
                    results.TryAdd(key, value);
                }
            }

            return results;
        }

        private static ImmutableList<Modifier> ConvertModifiers(WgConsumable wgConsumable, WgStatistics consumableStats, Dictionary<string, Modifier> modifierDictionary, IReadOnlyDictionary<string, WgBuff> buffDictionary, ILogger logger)
        {
            var results = new List<Modifier>();

            // A variant that also declares a stat inline is the more specific source, so it wins over its buff.
            var modifiers = new Dictionary<string, float>(consumableStats.RetrieveModifiers(logger), StringComparer.Ordinal);
            foreach ((string buffKey, float buffValue) in ResolveBuffModifiers(consumableStats, buffDictionary, logger))
            {
                modifiers.TryAdd(buffKey, buffValue);
            }

            foreach ((string key, float modifierValue) in modifiers)
            {
                Modifier modifier;
                Modifier? modifierData;
                switch (key)
                {
                    case "boostCoeff" when wgConsumable.Index.Equals("PCY022"):
                        modifierData = modifierDictionary.TryGetValue("artilleryReloadCoeff", out modifierData) ? modifierData : null;
                        modifier = new Modifier("artilleryReloadCoeff", modifierValue, wgConsumable.Name, modifierData);
                        results.Add(modifier);
                        break;
                    case "boostCoeff" when wgConsumable.Index.Equals("PCY034"):
                        // Skip boost for plane consumable because it's invisible in UI anyway
                        break;
                    case "preparationTime":
                    case "regenerationHPSpeedUnits":
                        //Skip this modifier, it's value is always 0
                        break;
                    case "workPreparationTime":
                        //Skip: near-always 0 and overlaps the consumable's preparation time; would render as untranslated noise.
                        break;
                    case "GMMaxDistAbsoluteCap":
                        //Skip: a template sentinel every buff carries with the same value. Modifiers.json treats it as a distance,
                        //so it would render as a "10 000 km" main battery range bonus on every consumable that resolves a buff.
                        break;
                    case "aimRange":
                        //Skip: the matching sentinel, always 100 on every buff, and never an effect of the consumable itself.
                        break;
                    case "regenerationHPSpeed":
                        var fixedKey = "consumable_" + key;
                        modifierData = modifierDictionary.TryGetValue(fixedKey, out modifierData) ? modifierData : null;
                        modifier = new Modifier(fixedKey, modifierValue, wgConsumable.Name, modifierData);
                        results.Add(modifier);
                        break;
                    default:
                        modifierData = modifierDictionary.TryGetValue(key, out modifierData) ? modifierData : null;
                        modifier = new Modifier(key, modifierValue, wgConsumable.Name, modifierData);
                        results.Add(modifier);
                        break;
                }
            }

            return results.ToImmutableList();
        }
    }
}
