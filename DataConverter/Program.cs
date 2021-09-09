using DataConverter.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataConverter
{
    class Program
    {
        private static HashSet<string> reportedTypes = new();
        public static string InputFolder;
        public static string OutputFolder;

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

            var counter = 0;
            foreach (var category in categories)
            {
                IEnumerable<string> files = Directory.GetFiles(category).Where(file => file.Contains("filtered") && !file.Contains("Event"));
                string categoryName = Path.GetFileName(category);
                foreach (string file in files)
                {
                    counter++;
                    if (counter % 10 == 0)
                    {
                        Console.WriteLine($"Processed {counter} files.");
                    }
                    string wgList = File.ReadAllText(file);
                    string fileName = Path.GetFileName(file);
                    string ownStructure;
                    object dict;
                    var outputPath = Path.Join(OutputFolder, categoryName, fileName);
                    new FileInfo(outputPath).Directory?.Create();
                    switch (categoryName)
                    {
                        case "Ability":
                            dict = ConsumableConverter.ConvertConsumable(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(outputPath, ownStructure);
                            break;
                        case "Aircraft":
                            dict = AircraftConverter.ConvertAircraft(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(outputPath, ownStructure);
                            break;
                        case "Crew":
                            string skillsList = CaptainConverter.LoadEmbeddedSkillData();
                            dict = CaptainConverter.ConvertCaptain(wgList, skillsList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(outputPath, ownStructure);
                            break;
                        case "Modernization":
                            dict = ModernizationConverter.ConvertModernization(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(outputPath, ownStructure);
                            break;
                        case "Projectile":
                            dict = ProjectileConverter.ConvertProjectile(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(outputPath, ownStructure);
                            break;
                        case "Ship":
                            dict = ShipConverter.ConvertShips(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(outputPath, ownStructure);
                            break;
                        case "Unit":
                            dict = ModuleConverter.ConvertModule(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(outputPath, ownStructure);
                            break;
                        case "Exterior":
                            dict = ExteriorConverter.ConvertExterior(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(outputPath, ownStructure);
                            break;
                        default:
                            if (reportedTypes.Add(categoryName))
                            {
                                Console.WriteLine("Type not found: " + categoryName);
                            }

                            break;
                    }
                }
            }
        }
    }
}