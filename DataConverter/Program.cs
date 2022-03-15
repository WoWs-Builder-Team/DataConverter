using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DataConverter.Converters;
using GameParamsExtractor.WGStructure;
using Newtonsoft.Json;
using WoWsShipBuilder.DataStructures;
using WowsShipBuilder.GameParamsExtractor;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace DataConverter
{
    internal static class Program
    {
        private const string Host = "https://d2nzlaerr9l5k3.cloudfront.net";
        private const string BaseInputPath = "InputData/";

        private static readonly ConcurrentBag<string> ReportedTypes = new();
        private static readonly HttpClient Client = new();

        private static string serverType = default!;
        private static string outputFolder = default!;
        private static string versionName = string.Empty;
        private static bool writeUnfilteredFiles = false;
        private static bool writeFilteredFiles = false;
        private static string debugFilesBasePath = default!;

        public static readonly ConcurrentBag<string> TranslationNames = new();

        /*
         * Parameter list
         * 1) Server type, with possible values: live, pts.
         * 2) Output folder.
         * 3) Version name.
         * 4) Should write "raw" gameparams files.
         * 5) Should write filtered gameparams files.
         * 6) Output folder for the files of the previous two points.
         *
         * Possible combinations allowed are:
         * 1) no parameters.
         * 2) 2 parameters: server type and output folder.
         * 3) 3 parameters: server type, output folder and version name.
         * 4) 6 parameters: all the one in the previous list.
         *
         * The GameParams File needs to be called "GameParams_[serverType].data", with [serverType] being "live" or "pts"
        */
        static void Main(string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    Console.WriteLine("Insert server type. Allowed values: live or pts.");
                    serverType = Console.ReadLine()!;
                    Console.WriteLine("Insert Output folder");
                    outputFolder = Console.ReadLine()!;
                    Console.WriteLine("Specify Game version name");
                    versionName = Console.ReadLine()!;
                    break;
                case 2:
                    serverType = args[0];
                    outputFolder = args[1];
                    break;
                case 3:
                    versionName = args[2];
                    goto case 2;
                case 6:
                    writeUnfilteredFiles = bool.Parse(args[3]);
                    writeFilteredFiles = bool.Parse(args[4]);
                    debugFilesBasePath = args[5];
                    goto case 3;
                default:
                    throw new InvalidOperationException();
            }

            if (string.IsNullOrEmpty(versionName))
            {
                versionName = Environment.GetEnvironmentVariable("GAME_VERSION") ?? string.Empty;
            }

            Stopwatch stopwatch = new();
            stopwatch.Start();
            var gameparamsData = GameParamsUtility.ProcessGameParams(BaseInputPath + $"GameParams_{serverType}.data", writeUnfilteredFiles, writeFilteredFiles, debugFilesBasePath);

            GC.Collect();

            ConvertData(gameparamsData);
            stopwatch.Stop();
            Console.WriteLine($"Conversion finished. Total Time: {stopwatch.Elapsed}");
        }

        private static void ConvertData(Dictionary<string, Dictionary<string, List<WGObject>>> gameparamsData)
        {
            Console.WriteLine("Start gameparams conversion.");
            Dictionary<string, List<FileVersion>> versions = new();
            VersionInfo oldVersionInfo;

            try
            {
                Console.WriteLine("Trying to retrieve existing version info file.");
                // Use GetAwaiter().GetResult() instead of Result to avoid receiving an aggregate exception.
                using Stream stream = Client.GetStreamAsync($"{Host}/api/{serverType}/VersionInfo.json").GetAwaiter().GetResult();
                using var streamReader = new StreamReader(stream);
                using var jsonReader = new JsonTextReader(streamReader);
                var jsonSerializer = new JsonSerializer();
                oldVersionInfo = jsonSerializer.Deserialize<VersionInfo>(jsonReader) ?? throw new InvalidOperationException("Unable to deserialize version info object.");
                Console.WriteLine("Version info file retrieved.");
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("No version info found. Creating empty version info.");
                oldVersionInfo = new VersionInfo(new Dictionary<string, List<FileVersion>>());
            }

            var serializerSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new ShouldSerializeContractResolver(),
            };

            var counter = 0;
            foreach ((string categoryName, var nationDictionary) in gameparamsData)
            {
                List<FileVersion> fileVersionList = new List<FileVersion>();

                Parallel.ForEach(nationDictionary, pair =>
                {
                    var nation = pair.Key;
                    var data = pair.Value;

                    Console.WriteLine($"Converting category: {categoryName} - nation: {nation}");
                    counter++;
                    if (counter % 10 == 0)
                    {
                        Console.WriteLine($"Processed {counter} dictionaries.");
                    }

                    string fileName = nation + ".json";
                    string ownStructure;
                    object dict;

                    string outputPath = Path.Join(outputFolder, categoryName);
                    Directory.CreateDirectory(outputPath);

                    FileVersion? fileVersion;
                    switch (categoryName)
                    {
                        case "Ability":
                            dict = ConsumableConverter.ConvertConsumable(data.Cast<WgConsumable>());
                            ownStructure = JsonConvert.SerializeObject(dict, serializerSettings);
                            fileVersion = CheckVersionAndSave(ownStructure, categoryName, fileName, oldVersionInfo, serverType);

                            break;
                        case "Aircraft":
                            dict = AircraftConverter.ConvertAircraft(data.Cast<WgAircraft>());
                            ownStructure = JsonConvert.SerializeObject(dict, serializerSettings);

                            fileVersion = CheckVersionAndSave(ownStructure, categoryName, fileName, oldVersionInfo, serverType);

                            break;
                        case "Crew":
                            string skillsList = CaptainConverter.LoadEmbeddedSkillData();
                            dict = CaptainConverter.ConvertCaptain(data.Cast<WGCaptain>(), skillsList, nation.Equals("Common"));
                            ownStructure = JsonConvert.SerializeObject(dict, serializerSettings);
                            fileVersion = CheckVersionAndSave(ownStructure, categoryName, fileName, oldVersionInfo, serverType);

                            break;
                        case "Modernization":
                            dict = ModernizationConverter.ConvertModernization(data.Cast<WGModernization>());
                            ownStructure = JsonConvert.SerializeObject(dict, serializerSettings);
                            fileVersion = CheckVersionAndSave(ownStructure, categoryName, fileName, oldVersionInfo, serverType);

                            break;
                        case "Projectile":
                            var filteredData = data.Where(x => x is WGProjectile).Cast<WGProjectile>();
                            dict = ProjectileConverter.ConvertProjectile(filteredData);
                            ownStructure = JsonConvert.SerializeObject(dict, serializerSettings);
                            fileVersion = CheckVersionAndSave(ownStructure, categoryName, fileName, oldVersionInfo, serverType);

                            break;
                        case "Ship":
                            Console.WriteLine($"Ships to process for {nation}: {data.Count}");
                            dict = ShipConverter.ConvertShips(data.Cast<WgShip>().ToList(), nation);
                            ownStructure = JsonConvert.SerializeObject(dict, serializerSettings);
                            fileVersion = CheckVersionAndSave(ownStructure, categoryName, fileName, oldVersionInfo, serverType);

                            break;
                        case "Unit":
                            dict = ModuleConverter.ConvertModule(data.Cast<WGModule>());
                            ownStructure = JsonConvert.SerializeObject(dict, serializerSettings);
                            fileVersion = CheckVersionAndSave(ownStructure, categoryName, fileName, oldVersionInfo, serverType);

                            break;
                        case "Exterior":
                            dict = ExteriorConverter.ConvertExterior(data.Cast<WgExterior>());
                            ownStructure = JsonConvert.SerializeObject(dict, serializerSettings);
                            fileVersion = CheckVersionAndSave(ownStructure, categoryName, fileName, oldVersionInfo, serverType);

                            break;
                        default:
                            if (!ReportedTypes.Contains(categoryName))
                            {
                                ReportedTypes.Add(categoryName);
                                Console.WriteLine("Type not found: " + categoryName);
                            }

                            fileVersion = null;

                            break;
                    }

                    if (fileVersion != null)
                    {
                        fileVersionList.Add(fileVersion);
                    }
                });

                if (fileVersionList.Any())
                {
                    // Add the updated file version list of the current category to the dictionary
                    versions.Add(categoryName, fileVersionList);
                }
            }

            string summaryString = JsonConvert.SerializeObject(ShipConverter.ShipSummaries);
            new FileInfo(Path.Combine(outputFolder, "Summary", "Common.json")).Directory?.Create();
            FileVersion summaryVersion = CheckVersionAndSave(summaryString, "Summary", "Common.json", oldVersionInfo, serverType);
            versions.Add("Summary", new() { summaryVersion });

            var structureAssembly = Assembly.GetAssembly(typeof(Ship));
            var lastVersion = oldVersionInfo.CurrentVersion ?? VersionConverter.FromVersionString(oldVersionInfo.VersionName);

            var currentVersion = VersionConverter.FromVersionString(versionName);
            var newVersioner = new VersionInfo(versions, oldVersionInfo.CurrentVersionCode + 1, currentVersion, lastVersion)
            {
                DataStructuresVersion = structureAssembly!.GetName().Version!,
                ReplayVersionDetails = GenerateReplayVersionDetails(),
            };

            //write the updated versioning file
            string versionerString = JsonConvert.SerializeObject(newVersioner);
            string versionInfoPath = Path.Join(outputFolder, "VersionInfo.json");
            File.WriteAllText(versionInfoPath, versionerString);

            Console.ForegroundColor = ConsoleColor.Green;
            var filteredTranslations = TranslationNames.Where(x => !string.IsNullOrWhiteSpace(x));
            TranslatorUtility.ProcessTranslationFiles(Path.Combine(BaseInputPath, "texts"), Path.Combine(outputFolder, "Localization"), filteredTranslations);
            Console.ResetColor();
        }

        private static FileVersion CheckVersionAndSave(string newData, string category, string fileName, VersionInfo oldVersioner, string versionType)
        {
            string categoryName = Path.GetFileName(category);
            string outputPath = Path.Join(outputFolder, categoryName, fileName);
            FileVersion fileVersion;
            string oldData;
            try
            {
                oldData = Client.GetStringAsync($"{Host}/api/{versionType}/{categoryName}/{fileName}").GetAwaiter().GetResult();
            }
            catch (HttpRequestException)
            {
                oldData = string.Empty;
            }

            oldVersioner.Categories.TryGetValue(categoryName, out List<FileVersion>? oldCategoryVersions);
            oldCategoryVersions ??= new();

            // Write data always. Even if the file was not changed, the existing remote data will be removed before publishing so the file needs to be recreated.
            File.WriteAllText(outputPath, newData);

            if (!oldData.Equals(newData))
            {
                fileVersion = new(fileName, oldVersioner.CurrentVersionCode + 1);
            }
            else
            {
                fileVersion = oldCategoryVersions.Find(v => v.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase)) ??
                              new FileVersion(fileName, 1);
            }

            return fileVersion;
        }

        private static Dictionary<Version, ReplayVersionDetails> GenerateReplayVersionDetails()
        {
            return new()
            {
                { new(0, 11, 0), new ReplayVersionDetails(122, 124) },
            };
        }
    }
}
