using DataConverter.WGStructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using WoWsShipBuilderDataStructures;

namespace DataConverter.Converters
{
    public static class ConsumableConverter
    {
        //convert the list of consumables from WG to our list of Consumables
        public static Dictionary<string, Consumable> ConvertConsumable(string jsonInput)
        {
            //create a List of our Objects
            Dictionary<string, Consumable> consumableList = new Dictionary<string, Consumable>();

            //deserialize into an object
            var wgConsumable = JsonConvert.DeserializeObject<List<WGConsumable>>(jsonInput) ?? throw new InvalidOperationException();

            //iterate over the entire list to convert everything
            foreach (var currentWgConsumable in wgConsumable)
            {
                Program.TranslationNames.Add(currentWgConsumable.name);
                //collecting consumable variants
                var variant = currentWgConsumable.variants;
                List<string> variantsKeys = new List<string>(variant.Keys);
                Program.TranslationNames.UnionWith(variant.Values.Select(variantValue => variantValue.descIDs));

                foreach (string currentVariantKey in variantsKeys)
                {
                    //mapping all the variants
                    Statistics stats = variant[currentVariantKey];

                    //create our object type
                    var consumable = new Consumable
                    {
                        //start mapping
                        Id = currentWgConsumable.id,
                        Index = currentWgConsumable.index,
                        Name = currentWgConsumable.name,
                        DescId = stats.descIDs,
                        Group = stats.group,
                        IconId = stats.iconIDs,
                        NumConsumables = stats.numConsumables,
                        ReloadTime = stats.reloadTime,
                        WorkTime = stats.workTime,
                        ConsumableVariantName = currentVariantKey,
                        Effects = stats.Effects,
                    };
                    if (consumable.Effects?.Keys != null)
                    {
                        Program.TranslationNames.UnionWith(consumable.Effects.Keys);
                    }

                    //dictionary with consumable name and variant name separated by an empty space as keys
                    var consumableKey = $"{consumable.Name} {currentVariantKey}";
                    consumableList.Add(consumableKey, consumable);
                }
            }

            return consumableList;
        }
    }
}
