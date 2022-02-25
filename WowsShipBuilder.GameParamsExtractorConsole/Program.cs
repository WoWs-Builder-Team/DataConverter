using System.Collections;
using System.Diagnostics;
using Newtonsoft.Json;
using WowsShipBuilder.GameParamsExtractor;

namespace WowsShipBuilder.GameParamsExtractorConsole
{
    internal class Program
    {
        private const string GameParamsPath = "GameParams.data";
        private const string BaseDir = "output/";

        private static readonly string[] GroupsToProcess = { "Gun", "Exterior", "Ability", "Modernization", "Crew", "Ship", "Aircraft", "Unit", "Projectile" };

        private static readonly string[] ArmamentsNames = {
            "_Artillery", "_FireControl", "_Torpedoes", "_ATBA", "_AirArmament", "_AirDefense",
            "_DepthChargeGuns", "_TorpedoBomber", "_DiveBomber", "_Fighter", "_SkipBomber", "_Engine", "_Hull"
        };

        private static string moduleContainerName = "ModuleArmaments";

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

                var nationGroups = group.GroupBy(x => GameParamsUtility.ConvertDataValue(x.Value["typeinfo"])["nation"]);

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
                            Dictionary<string, object> keysToMove = shipData
                                .Where(dataPair => ArmamentsNames.Any(s => dataPair.Key.Contains(s, StringComparison.OrdinalIgnoreCase)))
                                .ToDictionary(x => x.Key, x => x.Value);
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

                    //var dict = JsonConvert.DeserializeObject<List<WGAircraft>>(data);
                    //data = JsonConvert.SerializeObject(dict, Formatting.Indented);
                    File.WriteAllText(@$"{BaseDir}{group.Key}\{nation.Key}.json", data);
                }
            });
            stopwatch.Stop();
            Console.WriteLine("Time in ms for json: " + stopwatch.ElapsedMilliseconds);
            Console.WriteLine("end");
        }
    }
}
