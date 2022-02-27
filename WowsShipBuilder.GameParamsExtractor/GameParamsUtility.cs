using System.Collections;
using System.Linq;
using DataConverter.WGStructure;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Razorvine.Pickle;

namespace WowsShipBuilder.GameParamsExtractor
{
    public class GameParamsUtility
    {
        //species_map = {"Main": ("guns", main), "Torpedo": ("torpedoArray", torpedo),
        //   "Secondary": ("antiAirAndSecondaries", secondary), "DCharge": ("depthCharges", dcharge)}

        private static Dictionary<string, string> speciesMap = new()
        {
            { "Main", "guns" },
            { "Torpedo", "torpedoArray" },
            { "Secondary", "antiAirAndSecondaries" },
            { "DCharge", "depthCharges" },
        };

        public static byte[] Decompress(byte[] data)
        {
            var outputStream = new MemoryStream();
            using var compressedStream = new MemoryStream(data);
            using var inputStream = new InflaterInputStream(compressedStream);
            inputStream.CopyTo(outputStream);
            outputStream.Position = 0;
            return outputStream.ToArray();
        }

        public static Hashtable UnpickleGameParams(byte[] decompressedGpBytes)
        {
            using var unpickler = new Unpickler();
            Unpickler.registerConstructor("copy_reg", "_reconstructor", new CustomUnpicklerClass("copy_reg", "_reconstructor"));
            var unpickledObjectTemp = (object[])unpickler.loads(decompressedGpBytes);
            var unpickledGp = (Hashtable)unpickledObjectTemp[0];
            return unpickledGp;
        }

        public static Dictionary<string, object> ConvertDataValue(object objectPass)
        {
            return objectPass switch
            {
                Hashtable hashtable => hashtable.Cast<DictionaryEntry>().ToDictionary(kvp => kvp.Key.ToString()!, kvp => kvp.Value!),
                Dictionary<string, object> dictionary => dictionary,
                _ => throw new ArgumentException("The parameter has an invalid type", nameof(objectPass)),
            };
        }

        public static Type GetWgObjectClassList(string name)
        {
            return name switch
            {
                "Exterior" => typeof(List<WGExterior>),
                "Ability" => typeof(List<WGConsumable>),
                "Modernization" => typeof(List<WGModernization>),
                "Crew" => typeof(List<WGCaptain>),
                "Ship" => typeof(List<WGShip>),
                "Aircraft" => typeof(List<WGAircraft>),
                "Unit" => typeof(List<WGModule>),
                "Projectile" => typeof(List<WGProjectile>),
                _ => throw new ArgumentException("The parameter has an invalid type", nameof(name)),
            };
        }

        public static SortedDictionary<string, object> AggregateGuns(Dictionary<string,object> modulesDict, string gunsName)
        {
            var keysToMove = new SortedDictionary<string, object>();
            //iterate over the ATBAs and process them all
            foreach (var module in modulesDict)
            {
                //get the module data
                var moduleData = ConvertDataValue(module.Value);
                var gunsDictionary = new SortedDictionary<string, object>();
                var ATBAsGuns = new SortedDictionary<string, object>();
                //iterate through all the stats of the module
                foreach (var singleStat in moduleData)
                {
                    if (singleStat.Value is CustomClassDict gunData)
                    {
                        //if it has typeinfo, it's always a gun and not a dictionary fo values.
                        if (gunData.ContainsKey("typeinfo"))
                        {
                            ATBAsGuns.Add(singleStat.Key, singleStat.Value);
                        }
                        else
                        {
                            gunsDictionary.Add(singleStat.Key, singleStat.Value);
                        }
                    }
                    else
                    {
                        gunsDictionary.Add(singleStat.Key, singleStat.Value);
                    }

                }
                //insert the guns with their own key
                gunsDictionary.Add(gunsName, ATBAsGuns);
                keysToMove.Add(module.Key, gunsDictionary);
            }
            return keysToMove;
        }

        public static SortedDictionary<string, object> AggregateGuns(Dictionary<string, object> modulesDict)
        {
            var keysToMove = new SortedDictionary<string, object>();
            //iterate through all the modules
            foreach (var module in modulesDict)
            {
                //get the module data
                var moduleData = ConvertDataValue(module.Value);
                var gunsDictionary = new SortedDictionary<string, object>();
                var ATBAsGuns = new SortedDictionary<string, object>();
                var gunsName = "";
                bool isAA = false;
                //iterate through all the stats of the module
                foreach (var singleStat in moduleData)
                {
                    if (singleStat.Value is CustomClassDict gunData)
                    {
                        //if it has typeinfo, it's always a gun and not a dictionary fo values.
                        if (gunData.ContainsKey("typeinfo"))
                        {
                            if (string.IsNullOrEmpty(gunsName))
                            {
                                var typeInfo = gunData["typeinfo"];
                                var specie = ConvertDataValue(typeInfo)["species"];
                                if (specie != null && speciesMap.ContainsKey(specie.ToString()!))
                                {
                                    gunsName = speciesMap[specie.ToString()!];
                                }
                                else if(!isAA && specie != null && specie.Equals("AAircraft"))
                                {
                                    isAA = true;
                                }
                                else
                                {
                                    gunsDictionary.Add(singleStat.Key, singleStat.Value);
                                    continue;
                                }
                            }
                            ATBAsGuns.Add(singleStat.Key, singleStat.Value);
                        }
                        else
                        {
                            gunsDictionary.Add(singleStat.Key, singleStat.Value);
                        }
                    }
                    else
                    {
                        gunsDictionary.Add(singleStat.Key, singleStat.Value);
                    }

                }
                //insert the guns with their own key
                if (!string.IsNullOrEmpty(gunsName))
                {
                    gunsDictionary.Add(gunsName, ATBAsGuns);
                }
                if (isAA)
                {
                    gunsDictionary.Add("isAA", true);
                }
                keysToMove.Add(module.Key, gunsDictionary);
            }
            return keysToMove;
        }
    }
}
