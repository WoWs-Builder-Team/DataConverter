using DataConverter.WGStructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using WoWsShipBuilderDataStructures;


namespace DataConverter.Converters
{
    public static class ExteriorConverter
    {
        //convert the list of Exteriors from WG to our list of Exteriors
        public static Dictionary<string, Exterior> ConvertExterior(string jsonInput)
        {
            //create a List of our Objects
            Dictionary<string, Exterior> exteriorlist = new Dictionary<string, Exterior>();

            //deserialize into an object
            var wgExterior = JsonConvert.DeserializeObject<List<WGExterior>>(jsonInput) ?? throw new InvalidOperationException();
            
            //iterate over the entire list to convert everything
            foreach (var currentWgExterior in wgExterior)
            {
                //create our object type
                Exterior exterior = new Exterior()
                {
                    Id = currentWgExterior.id,
                    Index = currentWgExterior.index,
                    Name = currentWgExterior.name,
                    SortOrder = currentWgExterior.sortOrder
                };
                
                //converting one dictionary to another using LINQ's ToDictionary
                exterior.Modifiers = currentWgExterior.modifiers.ToDictionary(Item => Item.Key, item => (double)item.Value);
                
                //Exterior Type parsing
                if (currentWgExterior.typeinfo.species == "Flags")
                {
                    exterior.Type = ExteriorType.Flags;
                }
                else if (currentWgExterior.typeinfo.species == "Camouflage")
                {
                    exterior.Type = ExteriorType.Camouflage;
                } 
                else if (currentWgExterior.typeinfo.species == "Permoflage")
                {
                    exterior.Type = ExteriorType.Permoflage;
                }
                



                Restriction restriction = new Restriction()
                {
                    ForbiddenShips = currentWgExterior.restrictions.forbiddenShips.ToList(),
                    Levels = currentWgExterior.restrictions.levels.ToList(),
                    Nations = currentWgExterior.restrictions.nations.ToList(),
                    SpecificShips = currentWgExterior.restrictions.specificShips.ToList(),
                    Subtype = currentWgExterior.restrictions.subtype.ToList()
                };
                exterior.Restrictions = restriction;
                
                exteriorlist.Add(currentWgExterior.index, exterior);
            }

            return exteriorlist;
        }
    }
}