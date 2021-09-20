using DataConverter.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using WoWsShipBuilderDataStructures;

namespace DataConverter
{
    class Program
    {
        private static HashSet<string> reportedTypes = new();
        public static string InputFolder;
        public static string OutputFolder;

        private const string Host = "https://d2nzlaerr9l5k3.cloudfront.net";
        public static HttpClient Client = new HttpClient();

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Insert Input folder");
                InputFolder = Console.ReadLine();
                Console.WriteLine("Insert Output folder");
                OutputFolder = Console.ReadLine();
            }
            else if (args.Length == 2)
            {
                InputFolder = args[0];
                OutputFolder = args[1];
            }
            else
            {
                throw new InvalidOperationException();
            }

            ConvertData();
            Console.WriteLine("Conversion finished.");
        }

        public static void ConvertData()
        {
            string[] categories = Directory.GetDirectories(InputFolder);

            Dictionary<string, List<FileVersion>> versions = new ();

            using Stream stream = Client.GetStreamAsync($"{Host}/api/Versioner.json").Result;
            using var streamReader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(streamReader);
            var jsonSerializer = new JsonSerializer();
            Versioner oldVersioner = jsonSerializer.Deserialize<Versioner>(jsonReader);

            var counter = 0;
            foreach (var category in categories)
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
                    string fileName = Path.GetFileName(file);
                    string nation = Path.GetFileNameWithoutExtension(file);
                    string ownStructure;
                    object dict;

                    string oldData;
                    List<FileVersion> oldCategoryVersions;

                    var outputPath = Path.Join(OutputFolder, categoryName, fileName);
                    new FileInfo(outputPath).Directory?.Create();
                    switch (categoryName)
                    {
                        case "Ability":
                            dict = ConsumableConverter.ConvertConsumable(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            // Get the old json
                            oldData = Client.GetStringAsync($"{Host}/api/{categoryName}/{fileName}").Result;
                            oldCategoryVersions = oldVersioner.Categories[category];

                            // Check if the old and the new Json are equal. If they are, don't write any new File and keep the old version.
                            // Otherwise, update the version and write the new file
                            if (!oldData.Equals(ownStructure))
                            {                             
                                foreach (var oldFileVersion in oldCategoryVersions)
                                {
                                    if (oldFileVersion.FileName.Equals(nation))
                                    {
                                        FileVersion newFileVersion = new FileVersion(nation, oldFileVersion.Version + 1);
                                        fileVersionList.Add(newFileVersion);
                                        File.WriteAllText(outputPath, ownStructure);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var oldFileVersion in oldCategoryVersions)
                                {
                                    if (oldFileVersion.FileName.Equals(nation))
                                    {
                                        fileVersionList.Add(oldFileVersion);
                                    }
                                }
                            }
                            break;
                        case "Aircraft":
                            dict = AircraftConverter.ConvertAircraft(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            oldData = Client.GetStringAsync($"{Host}/api/{categoryName}/{fileName}").Result;
                            oldCategoryVersions = oldVersioner.Categories[category];
                            if (!oldData.Equals(ownStructure))
                            {
                                foreach (var oldFileVersion in oldCategoryVersions)
                                {
                                    if (oldFileVersion.FileName.Equals(nation))
                                    {
                                        FileVersion newFileVersion = new FileVersion(nation, oldFileVersion.Version + 1);
                                        fileVersionList.Add(newFileVersion);
                                        File.WriteAllText(outputPath, ownStructure);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var oldFileVersion in oldCategoryVersions)
                                {
                                    if (oldFileVersion.FileName.Equals(nation))
                                    {
                                        fileVersionList.Add(oldFileVersion);
                                    }
                                }
                            }
                            break;
                        case "Crew":
                            string skillsList = CaptainConverter.LoadEmbeddedSkillData();
                            dict = CaptainConverter.ConvertCaptain(wgList, skillsList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            oldData = Client.GetStringAsync($"{Host}/api/{categoryName}/{fileName}").Result;
                            oldCategoryVersions = oldVersioner.Categories[category];
                            if (!oldData.Equals(ownStructure))
                            {
                                foreach (var oldFileVersion in oldCategoryVersions)
                                {
                                    if (oldFileVersion.FileName.Equals(nation))
                                    {
                                        FileVersion newFileVersion = new FileVersion(nation, oldFileVersion.Version + 1);
                                        fileVersionList.Add(newFileVersion);
                                        File.WriteAllText(outputPath, ownStructure);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var oldFileVersion in oldCategoryVersions)
                                {
                                    if (oldFileVersion.FileName.Equals(nation))
                                    {
                                        fileVersionList.Add(oldFileVersion);
                                    }
                                }
                            }
                            break;
                        case "Modernization":
                            dict = ModernizationConverter.ConvertModernization(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            oldData = Client.GetStringAsync($"{Host}/api/{categoryName}/{fileName}").Result;
                            oldCategoryVersions = oldVersioner.Categories[category];
                            if (!oldData.Equals(ownStructure))
                            {
                                foreach (var oldFileVersion in oldCategoryVersions)
                                {
                                    if (oldFileVersion.FileName.Equals(nation))
                                    {
                                        FileVersion newFileVersion = new FileVersion(nation, oldFileVersion.Version + 1);
                                        fileVersionList.Add(newFileVersion);
                                        File.WriteAllText(outputPath, ownStructure);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var oldFileVersion in oldCategoryVersions)
                                {
                                    if (oldFileVersion.FileName.Equals(nation))
                                    {
                                        fileVersionList.Add(oldFileVersion);
                                    }
                                }
                            }
                            break;
                        case "Projectile":
                            dict = ProjectileConverter.ConvertProjectile(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            oldData = Client.GetStringAsync($"{Host}/api/{categoryName}/{fileName}").Result;
                            oldCategoryVersions = oldVersioner.Categories[category];
                            if (!oldData.Equals(ownStructure))
                            {
                                foreach (var oldFileVersion in oldCategoryVersions)
                                {
                                    if (oldFileVersion.FileName.Equals(nation))
                                    {
                                        FileVersion newFileVersion = new FileVersion(nation, oldFileVersion.Version + 1);
                                        fileVersionList.Add(newFileVersion);
                                        File.WriteAllText(outputPath, ownStructure);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var oldFileVersion in oldCategoryVersions)
                                {
                                    if (oldFileVersion.FileName.Equals(nation))
                                    {
                                        fileVersionList.Add(oldFileVersion);
                                    }
                                }
                            }
                            break;
                        case "Ship":
                            dict = ShipConverter.ConvertShips(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            oldData = Client.GetStringAsync($"{Host}/api/{categoryName}/{fileName}").Result;
                            oldCategoryVersions = oldVersioner.Categories[category];
                            if (!oldData.Equals(ownStructure))
                            {
                                foreach (var oldFileVersion in oldCategoryVersions)
                                {
                                    if (oldFileVersion.FileName.Equals(nation))
                                    {
                                        FileVersion newFileVersion = new FileVersion(nation, oldFileVersion.Version + 1);
                                        fileVersionList.Add(newFileVersion);
                                        File.WriteAllText(outputPath, ownStructure);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var oldFileVersion in oldCategoryVersions)
                                {
                                    if (oldFileVersion.FileName.Equals(nation))
                                    {
                                        fileVersionList.Add(oldFileVersion);
                                    }
                                }
                            }
                            break;
                        case "Unit":
                            dict = ModuleConverter.ConvertModule(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            oldData = Client.GetStringAsync($"{Host}/api/{categoryName}/{fileName}").Result;
                            oldCategoryVersions = oldVersioner.Categories[category];
                            if (!oldData.Equals(ownStructure))
                            {
                                foreach (var oldFileVersion in oldCategoryVersions)
                                {
                                    if (oldFileVersion.FileName.Equals(nation))
                                    {
                                        FileVersion newFileVersion = new FileVersion(nation, oldFileVersion.Version + 1);
                                        fileVersionList.Add(newFileVersion);
                                        File.WriteAllText(outputPath, ownStructure);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var oldFileVersion in oldCategoryVersions)
                                {
                                    if (oldFileVersion.FileName.Equals(nation))
                                    {
                                        fileVersionList.Add(oldFileVersion);
                                    }
                                }
                            }
                            break;
                        case "Exterior":
                            dict = ExteriorConverter.ConvertExterior(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            oldData = Client.GetStringAsync($"{Host}/api/{categoryName}/{fileName}").Result;
                            oldCategoryVersions = oldVersioner.Categories[category];
                            if (!oldData.Equals(ownStructure))
                            {
                                foreach (var oldFileVersion in oldCategoryVersions)
                                {
                                    if (oldFileVersion.FileName.Equals(nation))
                                    {
                                        FileVersion newFileVersion = new FileVersion(nation, oldFileVersion.Version + 1);
                                        fileVersionList.Add(newFileVersion);
                                        File.WriteAllText(outputPath, ownStructure);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var oldFileVersion in oldCategoryVersions)
                                {
                                    if (oldFileVersion.FileName.Equals(nation))
                                    {
                                        fileVersionList.Add(oldFileVersion);
                                    }
                                }
                            }
                            break;
                        default:
                            if (reportedTypes.Add(categoryName))
                            {
                                Console.WriteLine("Type not found: " + categoryName);
                            }

                            break;
                    }
                }
                // Add the updated file version list of the current category to the dictionary
                versions.Add(category, fileVersionList);
            }
            Versioner newVersioner = new Versioner(versions);
            //write the updated versioning file
            var versionerString = JsonConvert.SerializeObject(newVersioner);
            var output = Path.Join(OutputFolder, "Versioner.json");
            File.WriteAllText(output, versionerString);
        }
    }
}