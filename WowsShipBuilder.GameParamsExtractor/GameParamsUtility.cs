using System.Collections;
using System.Diagnostics;
using GameParamsExtractor.WGStructure;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Newtonsoft.Json;
using Razorvine.Pickle;

namespace WowsShipBuilder.GameParamsExtractor;

public static class GameParamsUtility
{
    private const string ModuleContainerName = "ModulesArmaments";

    private static readonly string[] GroupsToProcess = { "Exterior", "Ability", "Modernization", "Crew", "Ship", "Aircraft", "Unit", "Projectile" };

    private static readonly Dictionary<string, string> SpeciesMap = new()
    {
        { "Main", "guns" },
        { "Torpedo", "torpedoArray" },
        { "Secondary", "antiAirAndSecondaries" },
        { "DCharge", "depthCharges" },
    };

    private static readonly string[] RetardedModulesNames = { "AirDefenseDefault", "EngineDefault", "ArtilleryDefault", "HullDefault", "FireControlDefault", "TorpedoesDefault", "ATBADefault", "AirArmamentDefault"};

    public static Dictionary<string, Dictionary<string, List<WGObject>>> ProcessGameParams(string gameParamsPath, bool writeUnfilteredFiles = false, bool writeFilteredFiles = false, string outputPath = default!)
    {
        var stopwatch = new Stopwatch();
        Console.WriteLine("Starting gameparams processing");

        if ((writeFilteredFiles || writeUnfilteredFiles) && !Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        Dictionary<object, Dictionary<string, object>> dict = UnpickleGameParams(gameParamsPath, stopwatch);

        stopwatch.Start();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Start data processing");

        ParallelQuery<IGrouping<object, KeyValuePair<object, Dictionary<string, object>>>> groups = dict
            .AsParallel()
            .Where(x => GroupsToProcess.Contains(ConvertDataValue(x.Value["typeinfo"])["type"]))
            .GroupBy(x => ConvertDataValue(x.Value["typeinfo"])["type"]);

        var data = new Dictionary<string, Dictionary<string, List<WGObject>>>();
        foreach (var group in groups)
        {
            data.Add(group.Key.ToString()!, UnpackType(writeUnfilteredFiles, writeFilteredFiles, outputPath, group));
        }

        stopwatch.Stop();
        Console.WriteLine($"Gameparams processed. Time passed: {stopwatch.Elapsed}");
        Console.ResetColor();
        return data;
    }

    private static Dictionary<string, List<WGObject>> UnpackType(bool writeUnfilteredFiles, bool writeFilteredFiles, string outputPath, IGrouping<object, KeyValuePair<object, Dictionary<string, object>>> group)
    {
        Console.WriteLine($"Start processing: {group.Key}");

        var outputDirectory = Path.Join(outputPath, group.Key.ToString());
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        //get all elements divided by nation and exclude the Event ones
        var nationGroups = group
            .GroupBy(x => ConvertDataValue(x.Value["typeinfo"])["nation"])
            .Where(x => !x.Key.Equals("Events"));

        var nationsDictionary = new Dictionary<string, List<WGObject>>();
        foreach (var nation in nationGroups)
        {
            List<WGObject> nationObjects = UnpackNation(writeUnfilteredFiles, writeFilteredFiles, nation, group.Key.ToString() ?? string.Empty, outputDirectory);
            nationsDictionary.Add(nation.Key.ToString()!, nationObjects);
        }

        return nationsDictionary;
    }

    private static List<WGObject> UnpackNation(bool writeUnfilteredFiles, bool writeFilteredFiles, IGrouping<object, KeyValuePair<object, Dictionary<string, object>>> nationGroup, string groupName, string outputDirectory)
    {
        //we can make this a normal dictionary to reduce overhead. or we can keep it as sorted for easier human reading.
        List<SortedDictionary<string, object>> nationEntries = nationGroup.Select(x => new SortedDictionary<string, object>(x.Value)).ToList();

        Console.WriteLine($"Number of element for {groupName} - {nationGroup.Key}: {nationEntries.Count}");

        if (writeUnfilteredFiles)
        {
            using StreamWriter file = File.CreateText(Path.Join(outputDirectory, $"{nationGroup.Key}.json"));
            JsonSerializer serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented,
            };
            serializer.Serialize(file, nationEntries);
        }

        // process in here the single stuff we improved. Example is joining all the ships armament in one single dictionary

        List<SortedDictionary<string, object>> filteredEntries = groupName switch
        {
            "Ship" => CustomShipProcessing(nationEntries),
            "Ability" => CustomConsumableProcessing(nationEntries),
            "Exterior" => nationEntries.Where(x => !ConvertDataValue(x["typeinfo"])["species"].Equals("Ensign")).ToList(),
            _ => nationEntries,
        };

        string jsonData = JsonConvert.SerializeObject(filteredEntries);
        var objectList = JsonConvert.DeserializeObject<List<WGObject>>(jsonData);
        if (writeFilteredFiles)
        {
            using StreamWriter file = File.CreateText(Path.Join(outputDirectory, $"filtered_{nationGroup.Key}.json"));
            JsonSerializer serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented,
            };
            serializer.Serialize(file, objectList);
        }

        Console.WriteLine($"End processing for {groupName} - {nationGroup.Key}");
        return objectList ?? throw new InvalidOperationException("Object list was empty");
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
        Console.ForegroundColor = ConsoleColor.Yellow;
        stopwatch.Start();
        Console.WriteLine("Start unpickling");

        byte[] gpBytes = File.ReadAllBytes(gameParamsPath);
        Array.Reverse(gpBytes);
        byte[] decompressedGpBytes = Decompress(gpBytes);
        using var unpickler = new Unpickler();
        Unpickler.registerConstructor("copy_reg", "_reconstructor", new PythonDictionaryConstructor());
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
        Console.ResetColor();
        GC.Collect();
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
            var atbaGuns = new SortedDictionary<string, object>();
            var gunsName = "";
            bool isAntiAir = false;

            //iterate through all the stats of the module
            foreach (var singleStat in moduleData)
            {
                if (singleStat.Value is PythonDictionary gunData)
                {
                    //if it has typeinfo, it's always a gun and not a dictionary of values.
                    if (gunData.ContainsKey("typeinfo"))
                    {
                        if (string.IsNullOrEmpty(gunsName))
                        {
                            var typeInfo = gunData["typeinfo"];
                            var species = ConvertDataValue(typeInfo)["species"];
                            if (species != null && SpeciesMap.ContainsKey(species.ToString()!))
                            {
                                gunsName = SpeciesMap[species.ToString()!];
                            }
                            else if (!isAntiAir && species != null && species.Equals("AAircraft"))
                            {
                                isAntiAir = true;
                            }
                            else
                            {
                                gunsDictionary.Add(singleStat.Key, singleStat.Value);
                                continue;
                            }
                        }

                        atbaGuns.Add(singleStat.Key, singleStat.Value);
                    }
                    else
                    {
                        gunsDictionary.Add(singleStat.Key, singleStat.Value);
                    }
                }
                else
                {
                    gunsDictionary.Add(singleStat.Key, singleStat.Value);

                    // this is needed because there are AA modules with no AA. WG WHY?!
                    if (!isAntiAir && singleStat.Key.Equals("prioritySectorPhases"))
                    {
                        isAntiAir = true;
                    }
                }
            }

            //insert the guns with their own key
            if (!string.IsNullOrEmpty(gunsName))
            {
                gunsDictionary.Add(gunsName, atbaGuns);
            }

            if (isAntiAir)
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
            var variants = consumableData.Where(dataPair => dataPair.Value is PythonDictionary)
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

            //WARNING: this works on the assumptions that only modules contains "_" as character. It does seems to be always the case, but you never know with WG.
            // A more solid way could be by checking some of the inner dictionaries and such, but it would means checking all the stats for every key.
            // UPDATE: WG is obviously dumb, so for now, there is RetardedModulesNames where to put the naming exceptions wg used in the past. Let's hope they decided on a standard naming convention now.
            var modules = shipData.Where(dataPair => dataPair.Key.Contains("_", StringComparison.OrdinalIgnoreCase) || RetardedModulesNames.Contains(dataPair.Key))
                .ToDictionary(x => x.Key, x => x.Value);
            if (modules.Count > 0)
            {
                var aggregatedModules = AggregateGuns(modules);
                keysToMove = keysToMove.Union(aggregatedModules).ToDictionary(x => x.Key, x => x.Value);
            }

            SortedDictionary<string, object> moduleArmaments = new(keysToMove);
            shipData.Add(ModuleContainerName, moduleArmaments);

            foreach (string keyToRemove in keysToMove.Keys)
            {
                shipData.Remove(keyToRemove);
            }

            filteredEntries.Add(shipData);
        }

        return filteredEntries;
    }
}
