using System.Collections;
using System.Collections.Concurrent;
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

        public static Dictionary<string, Dictionary<string, List<WGObject>>> ProcessGameParams(string gameParamsPath, bool writeUnfilteredFiles = false, bool writeFilteredFiles = false, string outputPath = default!)
        {
            var stopwatch = new Stopwatch();
            Console.WriteLine("Starting gameparams processing");

            if ((writeFilteredFiles || writeUnfilteredFiles) && !Directory.Exists(outputPath))
            {
                throw new ArgumentException("outputPath needs to be an existing directory if one of writeUnfilteredFiles or writeFilteredFiles is true");
            }

            // Dictionary<string Type, Dictionary<string Nation, List<WgObject>>. Should we have a base class for our WG stuff, or we just use object?
            var data = new ConcurrentDictionary<string, Dictionary<string, List<WGObject>>>();

            var dict = UnpickleGameParams(gameParamsPath, stopwatch);

            stopwatch.Start();
            Console.WriteLine("Start data processing");

            var groups = dict.AsParallel().Where(x => GroupsToProcess.Contains(ConvertDataValue(x.Value["typeinfo"])["type"])).GroupBy(x => ConvertDataValue(x.Value["typeinfo"])["type"]);
            Parallel.ForEach(groups, group =>
            {
                Console.WriteLine($"Start processing: {group.Key}");

                var dir = outputPath + group.Key;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                //get all elements divided by nation and exclude the Event ones
                var nationGroups = group.GroupBy(x => ConvertDataValue(x.Value["typeinfo"])["nation"])
                        .Where(x => !x.Key.Equals("Events"));

                var nationsDictionary = new Dictionary<string, List<WGObject>>();

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
                        filteredEntries = CustomShipProcessing(nationEntries);
                    }
                    else if (group.Key.Equals("Ability"))
                    {
                        filteredEntries = CustomConsumableProcessing(nationEntries);
                    }
                    else
                    {
                        filteredEntries = nationEntries.ToList();
                    }

                    string data = JsonConvert.SerializeObject(filteredEntries);
                    var objectList = JsonConvert.DeserializeObject<List<WGObject>>(data);
                    if (writeFilteredFiles)
                    {
                        data = JsonConvert.SerializeObject(objectList, Formatting.Indented);
                        File.WriteAllText(@$"{outputPath}{group.Key}\{nation.Key}.json", data);
                    }

                    nationsDictionary.Add(nation.Key.ToString()!, objectList!);
                }

                data.TryAdd(group.Key.ToString()!, nationsDictionary);
                
            });
            stopwatch.Stop();
            Console.WriteLine($"Gameparams processed. Time passed: {stopwatch.Elapsed}");
            return data.ToDictionary(x => x.Key, x => x.Value);
        }

        private static byte[] Decompress(byte[] data)
        {
            var outputStream = new MemoryStream();
            using var compressedStream = new MemoryStream(data);
            using var inputStream = new InflaterInputStream(compressedStream);
            inputStream.CopyTo(outputStream);
            outputStream.Position = 0;
            return outputStream.ToArray();
        }

        private static Dictionary<object, Dictionary<string, object>> UnpickleGameParams(string gameParamsPath, Stopwatch stopwatch)
        {
            stopwatch.Start();
            Console.WriteLine("Start unpickling");

            byte[] gpBytes = File.ReadAllBytes(gameParamsPath);
            Array.Reverse(gpBytes);
            byte[] decompressedGpBytes = Decompress(gpBytes);
            using var unpickler = new Unpickler();
            Unpickler.registerConstructor("copy_reg", "_reconstructor", new CustomUnpicklerClass("copy_reg", "_reconstructor"));
            var unpickledObjectTemp = (object[])unpickler.loads(decompressedGpBytes);
            var unpickledGp = (Hashtable)unpickledObjectTemp[0];

            Dictionary<object, Dictionary<string, object>> dict = new();
            foreach (DictionaryEntry unpickledJsonEntry in unpickledGp)
            {
                var unpickledJsonEntryKey = unpickledJsonEntry.Key.ToString()!;
                Dictionary<string, object> unpickledJsonEntryValue = ConvertDataValue(unpickledJsonEntry.Value!);
                dict.Add(unpickledJsonEntryKey, unpickledJsonEntryValue);
            }

            stopwatch.Stop();
            Console.WriteLine($"Unpickling finished. Time passed: {stopwatch.Elapsed}");
            stopwatch.Reset();
            return dict;
        }

        private static Dictionary<string, object> ConvertDataValue(object objectPass)
        {
            return objectPass switch
            {
                Hashtable hashtable => hashtable.Cast<DictionaryEntry>().ToDictionary(kvp => kvp.Key.ToString()!, kvp => kvp.Value!),
                Dictionary<string, object> dictionary => dictionary,
                _ => throw new ArgumentException("The parameter has an invalid type", nameof(objectPass)),
            };
        }

        private static SortedDictionary<string, object> AggregateGuns(Dictionary<string, object> modulesDict)
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

        private static List<SortedDictionary<string, object>> CustomConsumableProcessing(IEnumerable<SortedDictionary<string, object>> nationEntries)
        {
            var filteredEntries = new List<SortedDictionary<string, object>>();

            foreach (var consumableData in nationEntries)
            {
                var variants = consumableData.Where(dataPair => dataPair.Value is CustomClassDict)
                    .ToDictionary(x => x.Key, x => x.Value);

                var sortedVariants = new SortedDictionary<string, object>(variants);

                consumableData.Add("variants", variants);

                foreach (var keyToRemove in variants.Keys)
                {
                    consumableData.Remove(keyToRemove);
                }
                filteredEntries.Add(consumableData);
            }

            return filteredEntries;
        }

        private static List<SortedDictionary<string, object>> CustomShipProcessing(IEnumerable<SortedDictionary<string, object>> nationEntries)
        {
            var filteredEntries = new List<SortedDictionary<string, object>>();

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

            return filteredEntries;
        }
    }
}
