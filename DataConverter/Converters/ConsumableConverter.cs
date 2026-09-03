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
        private const string TargetBuffKey = "buff";

        private const string SelfBuffKey = "buffOnSelf";

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
                    (var modifiers, var selfModifiers) = ConvertModifiers(currentWgConsumable, stats, modifierDictionary, buffDictionary, logger);
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
                        Modifiers = modifiers,
                        SelfModifiers = selfModifiers,
                    };
                    DataCache.TranslationNames.UnionWith(consumable.Modifiers.Concat(consumable.SelfModifiers).Select(m => m.Name));

                    //dictionary with consumable name and variant name separated by an empty space as keys
                    var consumableKey = $"{consumable.Name} {currentVariantKey}";
                    consumableList.Add(consumableKey, consumable);
                }
            }

            return consumableList;
        }

        /// <summary>
        /// Reads the effect a consumable references by name rather than declaring inline.
        /// </summary>
        /// <remarks>
        /// A squadron support consumable names two buffs: one applied to its target and one to the ship using it.
        /// They share stat names but not values - the support heal restores 50% to its target and 25% to its user -
        /// so merging them into one map by name silently reports the ally's figure as the user's.
        /// </remarks>
        private static (ImmutableSortedDictionary<string, float> Target, ImmutableSortedDictionary<string, float> Self) ResolveBuffModifiers(
            WgStatistics consumableStats,
            IReadOnlyDictionary<string, WgBuff> buffDictionary,
            ILogger logger)
        {
            var target = ResolveBuff(consumableStats, TargetBuffKey, buffDictionary, logger);
            var self = ResolveBuff(consumableStats, SelfBuffKey, buffDictionary, logger);

            // The fire extinguishing consumable names the same buff on both sides, meaning it treats its user like any
            // other target. Reporting that as a separate self effect would claim a difference that does not exist.
            return (target, self.SequenceEqual(target) ? ImmutableSortedDictionary<string, float>.Empty : self);
        }

        private static ImmutableSortedDictionary<string, float> ResolveBuff(WgStatistics consumableStats, string logicKey, IReadOnlyDictionary<string, WgBuff> buffDictionary, ILogger logger)
        {
            if (!consumableStats.Logic.TryGetValue(logicKey, out JToken? buffToken) || buffToken.Type != JTokenType.String)
            {
                return ImmutableSortedDictionary<string, float>.Empty;
            }

            var buffName = buffToken.Value<string>();
            if (string.IsNullOrEmpty(buffName))
            {
                return ImmutableSortedDictionary<string, float>.Empty;
            }

            if (!buffDictionary.TryGetValue(buffName, out WgBuff? buff))
            {
                logger.LogWarning("Unable to resolve buff {Buff} referenced by a consumable", buffName);
                return ImmutableSortedDictionary<string, float>.Empty;
            }

            return buff.RetrieveModifiers();
        }

        private static (ImmutableList<Modifier> Modifiers, ImmutableList<Modifier> SelfModifiers) ConvertModifiers(
            WgConsumable wgConsumable,
            WgStatistics consumableStats,
            Dictionary<string, Modifier> modifierDictionary,
            IReadOnlyDictionary<string, WgBuff> buffDictionary,
            ILogger logger)
        {
            (var targetBuff, var selfBuff) = ResolveBuffModifiers(consumableStats, buffDictionary, logger);

            // A variant that also declares a stat inline is the more specific source, so it wins over its buff.
            // Sorted because the inline stats arrive hash-ordered, and that order would otherwise reach the output
            // and change the file's checksum on every run.
            var modifiers = new SortedDictionary<string, float>(consumableStats.RetrieveModifiers(logger), StringComparer.Ordinal);
            foreach ((string buffKey, float buffValue) in targetBuff)
            {
                modifiers.TryAdd(buffKey, buffValue);
            }

            return (BuildModifiers(modifiers, wgConsumable, modifierDictionary), BuildModifiers(selfBuff, wgConsumable, modifierDictionary));
        }

        private static ImmutableList<Modifier> BuildModifiers(IEnumerable<KeyValuePair<string, float>> modifiers, WgConsumable wgConsumable, Dictionary<string, Modifier> modifierDictionary)
        {
            var results = new List<Modifier>();

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
