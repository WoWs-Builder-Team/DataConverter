using System.Collections;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Razorvine.Pickle;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace WowsShipBuilder.GameParamsExtractor;

internal static class GameParamsUtility
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

    public static FilterAndConvertResult FilterAndConvertGameParams(Dictionary<object, Dictionary<string, object>> rawGameParams, bool returnUnfiltered, ILogger? logger = null)
    {
        logger?.LogInformation("Filtering extracted gameparams and converting to WGObject structure");
        ParallelQuery<IGrouping<object, KeyValuePair<object, Dictionary<string, object>>>> groups = rawGameParams
            .AsParallel()
            .Where(x => GroupsToProcess.Contains(ConvertDataValue(x.Value["typeinfo"])["type"]))
            .GroupBy(x => ConvertDataValue(x.Value["typeinfo"])["type"]);

        var data = new Dictionary<string, Dictionary<string, List<WgObject>>>();
        var unfilteredData = new Dictionary<string, Dictionary<string, List<SortedDictionary<string, object>>>>();
        foreach (var group in groups)
        {
            var result = UnpackType(group, returnUnfiltered, logger);
            data.Add(group.Key.ToString()!, result.RefinedData);
            if (returnUnfiltered)
            {
                unfilteredData.Add(group.Key.ToString()!, result.UnfilteredData!);
            }
        }

        logger?.LogInformation("Finished filtering extracted gameparams");
        return new(data, returnUnfiltered ? unfilteredData : null);
    }

    internal static Dictionary<object, Dictionary<string, object>> UnpickleGameParams(string gameParamsPath)
    {
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

        GC.Collect();
        return dict;
    }

    private static TypeUnpackResult UnpackType(IGrouping<object, KeyValuePair<object, Dictionary<string, object>>> group, bool returnUnfiltered, ILogger? logger = null)
    {
        logger?.LogInformation("Unpacking type: {}", group.Key);

        //get all elements divided by nation and exclude the Event ones
        var nationGroups = group
            .GroupBy(x => ConvertDataValue(x.Value["typeinfo"])["nation"])
            .Where(x => !x.Key.Equals("Events"));

        var nationsDictionary = new Dictionary<string, List<WgObject>>();
        var unfilteredNationsDictionary = new Dictionary<string, List<SortedDictionary<string, object>>>();
        foreach (var nation in nationGroups)
        {
            var nationObjects = UnpackNation(nation, group.Key.ToString() ?? string.Empty, returnUnfiltered);
            nationsDictionary.Add(nation.Key.ToString()!, nationObjects.RefinedData);
            if (returnUnfiltered)
            {
                unfilteredNationsDictionary.Add(nation.Key.ToString()!, nationObjects.UnfilteredData!);
            }
        }

        return new(nationsDictionary, returnUnfiltered ? unfilteredNationsDictionary : null);
    }

    private static NationUnpackResult UnpackNation(IGrouping<object, KeyValuePair<object, Dictionary<string, object>>> nationGroup, string groupName, bool returnUnfiltered, ILogger? logger = null)
    {
        //we can make this a normal dictionary to reduce overhead. or we can keep it as sorted for easier human reading.
        List<SortedDictionary<string, object>> nationEntries = nationGroup.Select(x => new SortedDictionary<string, object>(x.Value)).ToList();

        logger?.LogInformation("Number of element for {GroupName} - {Nation}: {Entries}", groupName, nationGroup.Key, nationEntries.Count);

        // process in here the single stuff we improved. Example is joining all the ships armament in one single dictionary

        List<SortedDictionary<string, object>> filteredEntries = groupName switch
        {
            "Ship" => CustomShipProcessing(nationEntries),
            "Ability" => CustomConsumableProcessing(nationEntries),
            "Exterior" => nationEntries.Where(x => !ConvertDataValue(x["typeinfo"])["species"].Equals("Ensign")).ToList(),
            _ => nationEntries,
        };

        string jsonData = JsonConvert.SerializeObject(filteredEntries);
        var objectList = JsonConvert.DeserializeObject<List<WgObject>>(jsonData);

        logger?.LogInformation("End processing for {GroupName} - {Nation}", groupName, nationGroup.Key);
        return new(objectList ?? throw new InvalidOperationException("Object list was null"), returnUnfiltered ? nationEntries : null);
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
                    if (gunData.TryGetValue("typeinfo", out object? typeInfo))
                    {
                        if (string.IsNullOrEmpty(gunsName))
                        {
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
                shipType.Equals("preserved") || shipType.Equals("unavailable") || shipType.Equals("event"))
            {
                continue;
            }

            var keysToMove = new Dictionary<string, object>();

            var upgradeInfo = shipData["ShipUpgradeInfo"] as PythonDictionary ?? throw new InvalidOperationException();
            var upgradeDetailEntries = upgradeInfo.Values.Where(value => value is PythonDictionary).Cast<PythonDictionary>().ToList();
            List<string> components = upgradeDetailEntries
                .Select(detailEntry => ConvertDataValue(detailEntry["components"]))
                .SelectMany(componentDict => componentDict.Values)
                .Cast<ArrayList>()
                .SelectMany(x => x.Cast<string>())
                .Distinct().ToList();

            //WARNING: this works on the assumptions that only modules contains "_" as character. It does seems to be always the case, but you never know with WG.
            // A more solid way could be by checking some of the inner dictionaries and such, but it would means checking all the stats for every key.
            // UPDATE: WG is obviously dumb, so for now, there is RetardedModulesNames where to put the naming exceptions wg used in the past. Let's hope they decided on a standard naming convention now.
            // UPDATE2: WG does use even more naming exceptions so now modules are extracted directly from the ship upgrade info property.
            var modules = shipData.Where(dataPair => components.Contains(dataPair.Key)).ToDictionary(x => x.Key, x => x.Value);
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

    private sealed record TypeUnpackResult(Dictionary<string, List<WgObject>> RefinedData, Dictionary<string, List<SortedDictionary<string, object>>>? UnfilteredData);

    private sealed record NationUnpackResult(List<WgObject> RefinedData, List<SortedDictionary<string, object>>? UnfilteredData);

    public sealed record FilterAndConvertResult(Dictionary<string, Dictionary<string, List<WgObject>>> RefinedData, Dictionary<string, Dictionary<string, List<SortedDictionary<string, object>>>>? UnfilteredData);
}
