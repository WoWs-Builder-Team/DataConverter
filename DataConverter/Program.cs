using DataConverter.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using WoWsShipBuilder.DataStructures;

namespace DataConverter
{
    internal static class Program
    {
        private const string Host = "https://d2nzlaerr9l5k3.cloudfront.net";

        private static readonly HashSet<string> ReportedTypes = new();
        private static readonly HttpClient Client = new();
        private static string inputFolder = default!;
        private static string outputFolder = default!;
        private static string versionName = string.Empty;

        public static readonly HashSet<string> TranslationNames = new();

        static void Main(string[] args)
        {
            switch (args.Length)
            {
                case 0:
                    Console.WriteLine("Insert Input folder");
                    inputFolder = Console.ReadLine()!;
                    Console.WriteLine("Insert Output folder");
                    outputFolder = Console.ReadLine()!;
                    Console.WriteLine("Specify Game version name");
                    versionName = Console.ReadLine()!;
                    break;
                case 2:
                    inputFolder = args[0];
                    outputFolder = args[1];
                    break;
                case 3:
                    versionName = args[2];
                    goto case 2;
                default:
                    throw new InvalidOperationException();
            }

            if (string.IsNullOrEmpty(versionName))
            {
                versionName = Environment.GetEnvironmentVariable("GAME_VERSION") ?? string.Empty;
            }

            ConvertData();
            Console.WriteLine("Conversion finished.");
        }

        private static void ConvertData()
        {
            string[] categories = Directory.GetDirectories(inputFolder);
            Dictionary<string, List<FileVersion>> versions = new();
            VersionInfo oldVersionInfo;

            string versionType = outputFolder.Contains("live", StringComparison.InvariantCultureIgnoreCase) ? "live" : "pts";
            try
            {
                // Use GetAwaiter().GetResult() instead of Result to avoid receiving an aggregate exception.
                using Stream stream = Client.GetStreamAsync($"{Host}/api/{versionType}/VersionInfo.json").GetAwaiter().GetResult();
                using var streamReader = new StreamReader(stream);
                using var jsonReader = new JsonTextReader(streamReader);
                var jsonSerializer = new JsonSerializer();
                oldVersionInfo = jsonSerializer.Deserialize<VersionInfo>(jsonReader) ?? throw new InvalidOperationException("Unable to deserialize version info object.");
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
            foreach (string category in categories)
            {
                IEnumerable<string> files = Directory.GetFiles(category).Where(file => file.Contains("filtered") && !file.Contains("Event"));
                string categoryName = Path.GetFileName(category);
                List<FileVersion> fileVersionList = new List<FileVersion>();

                foreach (string file in files)
                {
                    counter++;
                    if (counter % 10 == 0)
                    {
                        Console.WriteLine($"Processed {counter} files.");
                    }

                    string wgList = File.ReadAllText(file);
                    string fileName = Path.GetFileName(file).Replace("filtered_", string.Empty);
                    string ownStructure;
                    object dict;

                    string outputPath = Path.Join(outputFolder, categoryName, fileName);
                    new FileInfo(outputPath).Directory?.Create();

                    FileVersion? fileVersion;
                    switch (categoryName)
                    {
                        case "Ability":
                            dict = ConsumableConverter.ConvertConsumable(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict, serializerSettings);
                            fileVersion = CheckVersionAndSave(ownStructure, category, fileName, oldVersionInfo, versionType);

                            break;
                        case "Aircraft":
                            dict = AircraftConverter.ConvertAircraft(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict, serializerSettings);

                            fileVersion = CheckVersionAndSave(ownStructure, category, fileName, oldVersionInfo, versionType);

                            break;
                        case "Crew":
                            string skillsList = CaptainConverter.LoadEmbeddedSkillData();
                            dict = CaptainConverter.ConvertCaptain(wgList, skillsList, fileName.Equals("Common.json"));
                            ownStructure = JsonConvert.SerializeObject(dict, serializerSettings);
                            fileVersion = CheckVersionAndSave(ownStructure, category, fileName, oldVersionInfo, versionType);

                            break;
                        case "Modernization":
                            dict = ModernizationConverter.ConvertModernization(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict, serializerSettings);
                            fileVersion = CheckVersionAndSave(ownStructure, category, fileName, oldVersionInfo, versionType);

                            break;
                        case "Projectile":
                            dict = ProjectileConverter.ConvertProjectile(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict, serializerSettings);
                            fileVersion = CheckVersionAndSave(ownStructure, category, fileName, oldVersionInfo, versionType);

                            break;
                        case "Ship":
                            dict = ShipConverter.ConvertShips(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict, serializerSettings);
                            fileVersion = CheckVersionAndSave(ownStructure, category, fileName, oldVersionInfo, versionType);

                            break;
                        case "Unit":
                            dict = ModuleConverter.ConvertModule(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict, serializerSettings);
                            fileVersion = CheckVersionAndSave(ownStructure, category, fileName, oldVersionInfo, versionType);

                            break;
                        case "Exterior":
                            dict = ExteriorConverter.ConvertExterior(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict, serializerSettings);
                            fileVersion = CheckVersionAndSave(ownStructure, category, fileName, oldVersionInfo, versionType);

                            break;
                        default:
                            if (ReportedTypes.Add(categoryName))
                            {
                                Console.WriteLine("Type not found: " + categoryName);
                            }

                            fileVersion = null;

                            break;
                    }

                    if (fileVersion != null)
                    {
                        fileVersionList.Add(fileVersion);
                    }
                }

                if (fileVersionList.Any())
                {
                    // Add the updated file version list of the current category to the dictionary
                    versions.Add(categoryName, fileVersionList);
                }
            }

            string summaryString = JsonConvert.SerializeObject(ShipConverter.ShipSummaries);
            new FileInfo(Path.Combine(outputFolder, "Summary", "Common.json")).Directory?.Create();
            FileVersion summaryVersion = CheckVersionAndSave(summaryString, "Summary", "Common.json", oldVersionInfo, versionType);
            versions.Add("Summary", new() { summaryVersion });

            var structureAssembly = Assembly.GetAssembly(typeof(Ship));
#pragma warning disable CS0618
            var lastVersion = oldVersionInfo.LastVersion ?? VersionConverter.FromVersionString(oldVersionInfo.LastVersionName);
#pragma warning restore CS0618
            var currentVersion = VersionConverter.FromVersionString(versionName);
            var newVersioner = new VersionInfo(versions, oldVersionInfo.CurrentVersionCode + 1, currentVersion, lastVersion)
            {
                DataStructuresVersion = structureAssembly!.GetName().Version!,
            };

            //write the updated versioning file
            string versionerString = JsonConvert.SerializeObject(newVersioner);
            string versionInfoPath = Path.Join(outputFolder, "VersionInfo.json");
            File.WriteAllText(versionInfoPath, versionerString);

            string translationNamesPath = Path.Join(outputFolder, "TranslationNames.csv");
            File.WriteAllLines(translationNamesPath, TranslationNames.Distinct().Where(translation => !string.IsNullOrEmpty(translation)));
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
    }
}
