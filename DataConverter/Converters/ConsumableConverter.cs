using DataConverter.WGStructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWsShipBuilderDataStructures;

namespace DataConverter.Converters
{
    public class ConsumableConverter
    {
        //convert the list of modernizations from WG to our list of Modernizations
        public static Dictionary<string, Consumable> ConvertConsumable(string jsonInput)
        {
            //create a List of our Objects
            Dictionary<string, Consumable> consumableList = new Dictionary<string, Consumable>();

            //deserialize into an object
            var wgConsumable = JsonConvert.DeserializeObject<List<WGConsumable>>(jsonInput) ?? throw new InvalidOperationException();

            //iterate over the entire list to convert everything
            foreach (var currentWgConsumable in wgConsumable)
            {
                //create our object type
                Consumable consumable = new Consumable
                {
                    //start mapping
                    Id = currentWgConsumable.id,
                    Index = currentWgConsumable.index,
                    Name = currentWgConsumable.name
                };

                //collecting consumable variants
                var Variant = currentWgConsumable.variants;
                List<string> variantsKeys = new List<string>(Variant.Keys);
                foreach (var currentVairantKey in variantsKeys)
                {
                    //mapping all the variants
                    Statistics stats = Variant[currentVairantKey];
                    consumable.DescId = stats.descIDs;
                    consumable.Group = stats.group;
                    consumable.IconId = stats.iconIDs;
                    consumable.NumConsumables = stats.numConsumables;
                    consumable.ReloadTime = stats.reloadTime;
                    consumable.WorkTime = stats.workTime;
                    consumable.ConsumableVariantName = currentVairantKey;

                    //dictionary with consumable name and variant name separated by an empty space as keys
                    string consumableKey = $"{consumable.Name} {currentVairantKey}";
                    consumableList.Add(consumableKey, consumable);
                }
            }

            return consumableList;
        }
    }
}
