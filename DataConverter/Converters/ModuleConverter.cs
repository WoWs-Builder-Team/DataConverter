using System;
using System.Collections.Generic;
using DataConverter.Data;
using WoWsShipBuilder.DataStructures;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace DataConverter.Converters
{
    public static class ModuleConverter
    {
        //convert the list of modules from WG to our list of Modules
        public static Dictionary<string, Module> ConvertModule(IEnumerable<WgModule> wgModules)
        {
            //create a List of our Objects
            Dictionary<string, Module> moduleList = new Dictionary<string, Module>();

            //iterate over the entire list to convert everything
            foreach (var currentWgModule in wgModules)
            {
                DataCache.TranslationNames.Add(currentWgModule.Name);
                //create our object type
                Module module = new Module
                {
                    //start mapping
                    CostCr = currentWgModule.CostCr,
                    CostXp = currentWgModule.CostXp,
                    Id = currentWgModule.Id,
                    Index = currentWgModule.Index,
                    Name = currentWgModule.Name,
                };
                //for Type enum
                ModuleType type = Enum.Parse<ModuleType>(currentWgModule.TypeInfo.Species);
                module.Type = type;
                //dictionary with Index as key
                moduleList.Add(module.Index, module);
            }

            return moduleList;
        }
    }
}
