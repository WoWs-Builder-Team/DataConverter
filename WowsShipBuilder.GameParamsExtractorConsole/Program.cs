using System.Collections;
using System.Diagnostics;
using DataConverter.WGStructure;
using Newtonsoft.Json;
using WowsShipBuilder.GameParamsExtractor;

namespace WowsShipBuilder.GameParamsExtractorConsole
{
    internal class Program
    {
        private const string GameParamsPath = "GameParams.data";
        private const string BaseDir = "output/";

        private static readonly string[] GroupsToProcess = { "Exterior", "Ability", "Modernization", "Crew", "Ship", "Aircraft", "Unit", "Projectile" };

        private static readonly string[] ArmamentsNames = {
            "_FireControl", "_Torpedoes", "_AirArmament", "PingerGun",
            "_DepthChargeGuns", "_TorpedoBomber", "_DiveBomber", "_Fighter", "_SkipBomber", "_Engine", "_Hull"
        };
        private static readonly string artilleryName = "_Artillery";
        private static readonly string burstFireModuleName = "BurstArtilleryModule";

        private static string moduleContainerName = "ModulesArmaments";

        public static void Main(string[] args)
        {
            Console.WriteLine("Start");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            byte[] gpBytes = File.ReadAllBytes(GameParamsPath);
            Array.Reverse(gpBytes);
            byte[] decompressedGpBytes = GameParamsUtility.Decompress(gpBytes);
            var unpickledGp = GameParamsUtility.UnpickleGameParams(decompressedGpBytes);

            Dictionary<object, Dictionary<string, object>> dict = new();
            foreach (DictionaryEntry unpickledJsonEntry in unpickledGp)
            {
                var unpickledJsonEntryKey = unpickledJsonEntry.Key.ToString()!;
                Dictionary<string, object> unpickledJsonEntryValue = GameParamsUtility.ConvertDataValue(unpickledJsonEntry.Value!);
                dict.Add(unpickledJsonEntryKey, unpickledJsonEntryValue);
            }

            stopwatch.Stop();
            Console.WriteLine("Time in ms for pickling: " + stopwatch.ElapsedMilliseconds);

            stopwatch.Reset();
            Console.WriteLine("start json");
            stopwatch.Start();
            var groups = dict.GroupBy(x => GameParamsUtility.ConvertDataValue(x.Value["typeinfo"])["type"]).Where(x => GroupsToProcess.Contains(x.Key));
            Parallel.ForEach(groups, group =>
            {
                Debug.WriteLine(group.Key);
                var dir = BaseDir + group.Key;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                //get all elements divided by nation and exclude the Event ones
                var nationGroups = group.GroupBy(x => GameParamsUtility.ConvertDataValue(x.Value["typeinfo"])["nation"])
                    .Where(x => !x.Key.Equals("Event"));

                foreach (var nation in nationGroups)
                {
                    //we can make this a normal dictionary to reduce overhead. or we can keep it as sorted for easier human reading.
                    IEnumerable<SortedDictionary<string, object>> nationEntries = nation.Select(x => new SortedDictionary<string, object>(x.Value));

                    // process in here the single stuff we improved. Example is joining all the ships armament in one single dictionary

                    var filteredEntries = new List<SortedDictionary<string, object>>();

                    if (group.Key.Equals("Ship"))
                    {
                        foreach (SortedDictionary<string, object> shipData in nationEntries)
                        {
                            var shipType = shipData["group"];
                            var shipClass = GameParamsUtility.ConvertDataValue(shipData["typeinfo"])["species"];
                            //skip completely the ship that are not relevant
                            if (shipClass.Equals("Auxiliary") || shipType.Equals("clan") || shipType.Equals("disabled") ||
                                 shipType.Equals("preserved") || shipType.Equals("unavailable"))
                            {
                                continue;
                            }

                            var keysToMove = new Dictionary<string, object>();

                            //ATBA special processing
                            //get all the ATBAs
                            var ATBAsModules = shipData.Where(dataPair => dataPair.Key.Contains("_ATBA", StringComparison.OrdinalIgnoreCase))
                                .ToDictionary(x => x.Key, x => x.Value);
                            if (ATBAsModules.Count > 0)
                            {
                                var atbas = GameParamsUtility.AggregateGuns(ATBAsModules, "antiAirAndSecondaries");
                                keysToMove = keysToMove.Union(atbas).ToDictionary(x => x.Key, x => x.Value);
                            }

                            //Artillery special processing. need to extract the ArtilleryBurstModule
                            var artilleriesModules = shipData.Where(dataPair => dataPair.Key.Contains("_Artillery", StringComparison.OrdinalIgnoreCase))
                               .ToDictionary(x => x.Key, x => x.Value);
                            if (artilleriesModules.Count > 0)
                            {
                                var artilleries = GameParamsUtility.AggregateGuns(artilleriesModules, "guns");
                                keysToMove = keysToMove.Union(artilleries).ToDictionary(x => x.Key, x => x.Value);
                            }

                            //move BurstArtilleryFire in the root need to fix
                            if (keysToMove.Any(x => x.Key.Contains(artilleryName)))
                            {
                                var artilleryPair = keysToMove.Where(x => x.Key.Contains(artilleryName)).First();

                                var artilleryKey = artilleryPair.Key;
                                var artilleryDataDict = GameParamsUtility.ConvertDataValue(artilleryPair.Value);
                                if (artilleryDataDict.ContainsKey(burstFireModuleName))
                                {
                                    var burstFireModule = artilleryDataDict[burstFireModuleName];
                                    GameParamsUtility.ConvertDataValue(keysToMove[artilleryKey]).Remove(burstFireModuleName);
                                    shipData.Add(burstFireModuleName, burstFireModule);
                                }
                            }


                            //Add isAA to the AirDefense
                            var airDefenseModules = shipData.Where(dataPair => dataPair.Key.Contains("_AirDefense", StringComparison.OrdinalIgnoreCase))
                                .ToDictionary(x => x.Key, x => x.Value);
                            if (airDefenseModules.Count > 0)
                            {
                                var aaModuleLists = new SortedDictionary<string, object>();
                                foreach (var module in airDefenseModules)
                                {
                                    var aaStats = GameParamsUtility.ConvertDataValue(module.Value);
                                    aaStats.Add("isAA", true);
                                    aaModuleLists.Add(module.Key, aaStats);
                                }
                                keysToMove = keysToMove.Union(aaModuleLists).ToDictionary(x => x.Key, x => x.Value);
                            }

                            //add the rest of the keys
                            var otherKeysToMove = shipData
                                .Where(dataPair => ArmamentsNames.Any(s => dataPair.Key.Contains(s, StringComparison.OrdinalIgnoreCase)))
                                .ToDictionary(x => x.Key, x => x.Value);

                            keysToMove = keysToMove = keysToMove.Union(otherKeysToMove).ToDictionary(x => x.Key, x => x.Value);

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

                    string data = JsonConvert.SerializeObject(filteredEntries, Formatting.Indented);

                    var type = GameParamsUtility.GetWgObjectClassList(group.Key.ToString()!);
                    var dict = JsonConvert.DeserializeObject(data, type);
                    data = JsonConvert.SerializeObject(dict, Formatting.Indented);
                    File.WriteAllText(@$"{BaseDir}{group.Key}\{nation.Key}.json", data);
                }
            });
            stopwatch.Stop();
            Console.WriteLine("Time in ms for json: " + stopwatch.ElapsedMilliseconds);
            Console.WriteLine("end");
        }
    }
}
