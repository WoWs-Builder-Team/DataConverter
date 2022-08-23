using System.Collections.Generic;
using System.Linq;
using DataConverter.Data;
using WoWsShipBuilder.DataStructures;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace DataConverter.Converters
{
    public static class ConsumableConverter
    {
        //convert the list of consumables from WG to our list of Consumables
        public static Dictionary<string, Consumable> ConvertConsumable(IEnumerable<WgConsumable> wgConsumable)
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
                    Statistics stats = variant[currentVariantKey];

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
                        PlaneName = stats.FightersName,
                        Modifiers = ConvertModifiers(currentWgConsumable, stats),
                    };
                    if (consumable.Modifiers?.Keys != null)
                    {
                        DataCache.TranslationNames.UnionWith(consumable.Modifiers.Keys);
                    }

                    //dictionary with consumable name and variant name separated by an empty space as keys
                    var consumableKey = $"{consumable.Name} {currentVariantKey}";
                    consumableList.Add(consumableKey, consumable);
                }
            }

            return consumableList;
        }

        private static Dictionary<string, float> ConvertModifiers(WgConsumable wgConsumable, Statistics consumableStats)
        {
            var results = new Dictionary<string, float>();
            foreach ((string key, float modifier) in consumableStats.Modifiers ?? new Dictionary<string, float>())
            {
                switch (key)
                {
                    case "boostCoeff" when wgConsumable.Index.Equals("PCY022"):
                        results["artilleryReloadCoeff"] = modifier;
                        break;
                    case "boostCoeff" when wgConsumable.Index.Equals("PCY034"):
                        // Skip boost for plane consumable because it's invisible in UI anyway
                        break;
                    case "preparationTime":
                    case "regenerationHPSpeedUnits":
                        //Skip this modifier, it's value is always 0
                        break;
                    default:
                        results[key] = modifier;
                        break;
                }
            }

            return results;
        }
    }
}
