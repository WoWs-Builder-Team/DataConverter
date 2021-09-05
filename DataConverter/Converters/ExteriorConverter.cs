using DataConverter.WGStructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
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
                    SortOrder = currentWgExterior.sortOrder,
                    };
                
                //converting one dictionary to another 
                Dictionary<string, double> modifiers = new Dictionary<string, double>();

                foreach (var currentWgExteriorModifier in currentWgExterior.modifiers)
                {
                    JToken jtoken = currentWgExteriorModifier.Value;
                    if (jtoken.Type == JTokenType.Float || jtoken.Type == JTokenType.Integer)
                    {
                        modifiers.Add(currentWgExteriorModifier.Key, jtoken.Value<float>());
                    }
                    else if (jtoken.Type == JTokenType.Object)
                    {
                        JObject jObject = (JObject)jtoken;
                        var values = jObject.ToObject<Dictionary<string, double>>();
                        bool isEqual = true;
                        var first = values.First().Value;
                        foreach ((string key, double value) in values)
                        {
                            if (value != first)
                            {
                                isEqual = false;
                            }
                        }

                        if (isEqual)
                        {
                            modifiers.Add(currentWgExteriorModifier.Key, first);
                        }
                        else
                        {
                            foreach ((string key, double value) in values)
                            {
                                modifiers.Add($"{currentWgExteriorModifier.Key}_{key}", value);
                            }
                        }
                    }
                }

                exterior.Modifiers = modifiers;
                


                
                //Exterior Type parsing
                if (currentWgExterior.typeinfo.species.Equals("Flags", StringComparison.InvariantCultureIgnoreCase))
                {
                    exterior.Type = ExteriorType.Flags;
                }
                else if (currentWgExterior.typeinfo.species.Equals("Camouflage", StringComparison.InvariantCultureIgnoreCase))
                {
                    exterior.Type = ExteriorType.Camouflage;
                } 
                else if (currentWgExterior.typeinfo.species.Equals("Permoflage", StringComparison.InvariantCultureIgnoreCase))
                {
                    exterior.Type = ExteriorType.Permoflage;
                }




                Restriction restriction = new Restriction();
                //{
                //    ForbiddenShips = currentWgExterior.restrictions.forbiddenShips.ToList(),
                //    Levels = currentWgExterior.restrictions.levels.ToList(),
                //    Nations = currentWgExterior.restrictions.nations.ToList(),
                //    SpecificShips = currentWgExterior.restrictions.specificShips.ToList(),
                //    Subtype = currentWgExterior.restrictions.subtype.ToList()
                //};
                if (currentWgExterior.restrictions != null)
                {
                    if (currentWgExterior.restrictions.forbiddenShips != null)
                    {
                        restriction.ForbiddenShips = currentWgExterior.restrictions.forbiddenShips.ToList();
                    }
                    else if (currentWgExterior.restrictions.levels != null)
                    {
                        restriction.Levels = currentWgExterior.restrictions.levels.ToList();
                    }
                    else if (currentWgExterior.restrictions.nations != null)
                    {
                        restriction.Nations = currentWgExterior.restrictions.nations.ToList();
                    }
                    else if (currentWgExterior.restrictions.nations != null)
                    {
                        restriction.SpecificShips = currentWgExterior.restrictions.specificShips.ToList();
                    }
                    else if (currentWgExterior.restrictions.subtype != null)
                    {
                        restriction.Subtype = currentWgExterior.restrictions.subtype.ToList();
                    }
                    exterior.Restrictions = restriction;
                }
                
                
                
                exteriorlist.Add(currentWgExterior.index, exterior);
            }

            return exteriorlist;
        }
    }
}