using DataConverter.WGStructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WoWsShipBuilderDataStructures;

namespace DataConverter.Converters
{
    public static class ModuleConverter
    {
        //convert the list of modernizations from WG to our list of Modernizations
        public static Dictionary<string, Module> ConvertModule(string jsonInput)
        {
            //create a List of our Objects
            Dictionary<string, Module> moduleList = new Dictionary<string, Module>();

            //deserialize into an object
            var wgModules = JsonConvert.DeserializeObject<List<WGModule>>(jsonInput) ?? throw new InvalidOperationException();

            //iterate over the entire list to convert everything
            foreach (var currentWgModule in wgModules)
            {
                //create our object type
                Module module = new Module
                {
                    //start mapping
                    CostCr = currentWgModule.costCR,
                    CostXp = currentWgModule.costXP,
                    Id = currentWgModule.id,
                    Index = currentWgModule.index,
                    Name = currentWgModule.name
                };
                //for Type enum
                ModuleType type = Enum.Parse<ModuleType>(currentWgModule.typeinfo.species);
                module.Type = type;
                //dictionary with Index as key
                moduleList.Add(module.Index, module);
            }

            return moduleList;
        }
    }
}