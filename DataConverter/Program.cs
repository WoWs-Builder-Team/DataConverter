using DataConverter.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace DataConverter
{
    class Program
    {
        public static string InputFolder;
        public static string OutputFolder;

        static void Main(string[] args)
        {
            Console.WriteLine("Insert Input folder");
            InputFolder = Console.ReadLine();
            Console.WriteLine("Insert Output folder");
            OutputFolder = Console.ReadLine();
            ConvertData();
        }

        public static void ConvertData()
        {
            string[] categories = Directory.GetDirectories(InputFolder);

            foreach (var category in categories)
            {
                string[] files = Directory.GetFiles(category);
                string categoryName = Path.GetFileName(Path.GetDirectoryName(category));
                foreach (string file in files)
                {
                    string wgList = File.ReadAllText(file);
                    string fileName = Path.GetFileName(file);
                    string ownStructure;
                    object dict;
                    switch (categoryName)
                    {
                        case "Ability":
                            dict = ConsumableConverter.ConvertConsumable(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(Path.Join(OutputFolder,category,fileName), ownStructure);
                            break;
                        case "Aircraft":
                            dict = AircraftConverter.ConvertAircraft(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(Path.Join(OutputFolder, category, fileName), ownStructure);
                            break;
                        case "Crew":
                            string skillsList = CaptainConverter.LoadEmbeddedSkillData();
                            dict = CaptainConverter.ConvertCaptain(wgList, skillsList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(Path.Join(OutputFolder, category, fileName), ownStructure);
                            break;
                        case "Modernization":
                            dict = ModernizationConverter.ConvertModernization(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(Path.Join(OutputFolder, category, fileName), ownStructure);
                            break;
                        case "Projectile":
                            dict = ProjectileConverter.ConvertProjectile(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(Path.Join(OutputFolder, category, fileName), ownStructure);
                            break;
                        case "Ship":
                            dict = ShipConverter.ConvertShips(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(Path.Join(OutputFolder, category, fileName), ownStructure);
                            break;
                        case "Unit":
                            dict = ModuleConverter.ConvertModule(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(Path.Join(OutputFolder, category, fileName), ownStructure);
                            break;
                      case "Exterior":
                          dict = ExteriorConverter.ConvertExterior(wgList);
                          ownStructure = JsonConvert.SerializeObject(dict);
                          File.WriteAllText(Path.Join(OutputFolder, category, fileName), ownStructure);
                          break;
                        default:
                            throw new KeyNotFoundException();
                    }
                }
            }
        }
    }
}
