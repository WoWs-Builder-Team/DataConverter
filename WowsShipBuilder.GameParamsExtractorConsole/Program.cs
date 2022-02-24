using System.Collections;
using System.Diagnostics;
using Newtonsoft.Json;
using WowsShipBuilder.GameParamsExtractor;

namespace GameParamsFilter
{
    internal class Program
    {
        private static string GameparamsPath = @"D:\Desktop\GameParamsFilter\GameParamsFilter\GameParamsFilter\Data\GameParams.data";
        private static string baseDir = @"D:\Desktop\GameParamsFilter\output\";
        private static string[] groupsToProcess = new string[] { "Gun", "Exterior", "Ability", "Modernization", "Crew", "Ship", "Aircraft", "Unit", "Projectile" };

        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var gpBytes = File.ReadAllBytes(GameparamsPath);
            Array.Reverse(gpBytes);
            var decompressedGpBytes = GameParamsUtility.Decompress(gpBytes);
            Hashtable UnpickledGP = GameParamsUtility.UnpickleGameParams(decompressedGpBytes);

            Dictionary<object, Dictionary<object, object>> dict = new();
            foreach (DictionaryEntry UnpickledJSONEntry in UnpickledGP)
            {
                string UnpickledJSONEntry_Key = UnpickledJSONEntry.Key.ToString()!;
                Dictionary<object, object> UnpickledJSONEntry_Value = GameParamsUtility.ConvertDataValue(UnpickledJSONEntry.Value!);
                dict.Add(UnpickledJSONEntry_Key, UnpickledJSONEntry_Value);
            }
            stopwatch.Stop();
            Console.WriteLine("Time in ms for pickling: " + stopwatch.ElapsedMilliseconds);

            GC.Collect();

            stopwatch.Reset();
            Console.WriteLine("start json");
            stopwatch.Start();
            var groups = dict.GroupBy(x => GameParamsUtility.ConvertDataValue(x.Value["typeinfo"])["type"]).Where(x => groupsToProcess.Contains(x.Key));
            Parallel.ForEach(groups, group =>
            {
                Debug.WriteLine(group.Key);
                var dir = baseDir + group.Key;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var nationGroups = group.GroupBy(x => GameParamsUtility.ConvertDataValue(x.Value["typeinfo"])["nation"]);

                foreach (var nation in nationGroups)
                {
                    //we can make this a normal dictionary to reduce overhead. or we can keep it as sorted for easier human reading.
                    var groupDict = nation.Select(x => new SortedDictionary<object,object>(x.Value));
                    // process in here the single stuff we improved. Example is joining all the ships armament in one single dictionary
                    var data = JsonConvert.SerializeObject(groupDict, Formatting.Indented);
                    File.WriteAllText(baseDir + group.Key + @"\" + nation.Key + ".json", data);
                }
            });
            stopwatch.Stop();
            Console.WriteLine("Time in ms for json: " + stopwatch.ElapsedMilliseconds);
            Console.WriteLine("end");
        }

    }
}
