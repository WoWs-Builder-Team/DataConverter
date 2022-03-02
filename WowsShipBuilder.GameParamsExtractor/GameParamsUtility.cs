using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DataConverter.WGStructure;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Newtonsoft.Json;
using Razorvine.Pickle;

namespace WowsShipBuilder.GameParamsExtractor
{
    public static class GameParamsUtility
    {
        private static string moduleContainerName = "ModulesArmaments";

        private static readonly string[] GroupsToProcess = { "Exterior", "Ability", "Modernization", "Crew", "Ship", "Aircraft", "Unit", "Projectile" };

        private static Dictionary<string, string> speciesMap = new()
        {
            { "Main", "guns" },
            { "Torpedo", "torpedoArray" },
            { "Secondary", "antiAirAndSecondaries" },
            { "DCharge", "depthCharges" },
        };

        public static Dictionary<string, Dictionary<string, List<object>>> ProcessGameParams(string gameParamsPath, bool writeUnfilteredFiles = false, bool writeFilteredFiles = false, string outputPath = default!)
        {
            Console.WriteLine("Starting gameparams processing");

            if ((writeFilteredFiles || writeUnfilteredFiles) && !Directory.Exists(outputPath))
            {
                throw new ArgumentException("outputPath needs to be an existing directory if one of writeUnfilteredFiles or writeFilteredFiles is true");
            }

            // Dictionary<string Type, Dictionary<string Nation, List<WgObject>>. Should we have a base class for our WG stuff, or we just use object?
            var data = new Dictionary<string, Dictionary<string, List<object>>>();

            Console.WriteLine("Start unpickling");
            byte[] gpBytes = File.ReadAllBytes(gameParamsPath);
            Array.Reverse(gpBytes);
            byte[] decompressedGpBytes = Decompress(gpBytes);
            var unpickledGp = UnpickleGameParams(decompressedGpBytes);

            Dictionary<object, Dictionary<string, object>> dict = new();
            foreach (DictionaryEntry unpickledJsonEntry in unpickledGp)
            {
                var unpickledJsonEntryKey = unpickledJsonEntry.Key.ToString()!;
                Dictionary<string, object> unpickledJsonEntryValue = ConvertDataValue(unpickledJsonEntry.Value!);
                dict.Add(unpickledJsonEntryKey, unpickledJsonEntryValue);
            }

            Console.WriteLine("End unpickling");

            Console.WriteLine("Start data processing");

            var groups = dict.AsParallel().Where(x => GroupsToProcess.Contains(GameParamsUtility.ConvertDataValue(x.Value["typeinfo"])["type"])).GroupBy(x => GameParamsUtility.ConvertDataValue(x.Value["typeinfo"])["type"]);
            Parallel.ForEach(groups, group =>
            {
                Console.WriteLine($"Start processing: {group.Key}");

                var dir = outputPath + group.Key;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                //get all elements divided by nation and exclude the Event ones
                var nationGroups = group.GroupBy(x => GameParamsUtility.ConvertDataValue(x.Value["typeinfo"])["nation"])
                        .Where(x => !x.Key.Equals("Event"));

                var nationsDictionary = new Dictionary<string, List<object>>();

                foreach (var nation in nationGroups)
                {
                    //we can make this a normal dictionary to reduce overhead. or we can keep it as sorted for easier human reading.
                    IEnumerable<SortedDictionary<string, object>> nationEntries = nation.Select(x => new SortedDictionary<string, object>(x.Value));

                    Console.WriteLine($"Number of element for {group.Key} - {nation.Key}: {nationEntries.Count()}");

                    if (writeUnfilteredFiles)
                    {
                        var unfilteredData = JsonConvert.SerializeObject(nationEntries);
                        File.WriteAllText(@$"{outputPath}{group.Key}\{nation.Key}.json", unfilteredData);
                    }

                    // process in here the single stuff we improved. Example is joining all the ships armament in one single dictionary

                    var filteredEntries = new List<SortedDictionary<string, object>>();

                    if (group.Key.Equals("Ship"))
                    {
                        foreach (SortedDictionary<string, object> shipData in nationEntries)
                        {
                            var shipType = shipData["group"];
                            var shipClass = ConvertDataValue(shipData["typeinfo"])["species"];
                            //skip completely the ship that are not relevant
                            if (shipClass.Equals("Auxiliary") || shipType.Equals("clan") || shipType.Equals("disabled") ||
                                     shipType.Equals("preserved") || shipType.Equals("unavailable"))
                            {
                                continue;
                            }

                            var keysToMove = new Dictionary<string, object>();

                            //WARNING: this works on the assumptions that only modules contains "_" as charachter. It does seems to be always the case, but you never know with WG.
                            // A more solid way could be by checking some of the inner dictionaries and such, but it would means checking all the stats for every key.
                            var modules = shipData.Where(dataPair => dataPair.Key.Contains("_", StringComparison.OrdinalIgnoreCase))
                                    .ToDictionary(x => x.Key, x => x.Value);
                            if (modules.Count > 0)
                            {
                                var aggregatedModules = AggregateGuns(modules);
                                keysToMove = keysToMove.Union(aggregatedModules).ToDictionary(x => x.Key, x => x.Value);
                            }

                            SortedDictionary<string, object> moduleArmaments = new(keysToMove);
                            shipData.Add(moduleContainerName, moduleArmaments);

                            foreach (string keyToRemove in keysToMove.Keys)
                            {
                                shipData.Remove(keyToRemove);
                            }
                            filteredEntries.Add(shipData);
                        }
                    }
                    else
                    {
                        filteredEntries = nationEntries.ToList();
                    }

                    string data = JsonConvert.SerializeObject(filteredEntries);

                    var type = GetWgObjectClassList(group.Key.ToString()!);
                    var dict = JsonConvert.DeserializeObject(data, type);
                    if (writeFilteredFiles)
                    {
                        data = JsonConvert.SerializeObject(dict, Formatting.Indented);
                        File.WriteAllText(@$"{outputPath}{group.Key}\{nation.Key}.json", data);
                    }
                    List<object> list = (List<object>)dict!;
                    nationsDictionary.Add(nation.Key.ToString()!, list);
                }
                lock (data)
                {
                    data.Add(group.Key.ToString()!, nationsDictionary);
                }
            });
            Console.WriteLine("End of gameparams processing");
            return data;
        }


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
