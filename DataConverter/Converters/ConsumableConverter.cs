using System.Collections.Generic;
using System.Linq;
using DataConverter.Data;
using WoWsShipBuilder.DataStructures.Consumable;
using WoWsShipBuilder.DataStructures.Modifiers;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace DataConverter.Converters
{
    public static class ConsumableConverter
    {
        //convert the list of consumables from WG to our list of Consumables
        public static Dictionary<string, Consumable> ConvertConsumable(IEnumerable<WgConsumable> wgConsumable, Dictionary<string, Modifier> modifierDictionary)
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
                        PreparationTime = stats.PreparationTime,
                        Modifiers = ConvertModifiers(currentWgConsumable, stats, modifierDictionary),
                    };
                    DataCache.TranslationNames.UnionWith(consumable.Modifiers.Select(m => m.Name));

                    //dictionary with consumable name and variant name separated by an empty space as keys
                    var consumableKey = $"{consumable.Name} {currentVariantKey}";
                    consumableList.Add(consumableKey, consumable);
                }
            }

            return consumableList;
        }

        private static List<Modifier> ConvertModifiers(WgConsumable wgConsumable, WgStatistics consumableStats, Dictionary<string, Modifier> modifierDictionary)
        {
            var results = new List<Modifier>();
            foreach ((string key, float modifierValue) in consumableStats.Modifiers)
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

            return results;
        }
    }
}
