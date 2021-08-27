using DataConverter.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DataConverter
{
    class Program
    {
        public static string inputFolder;
        public static string outputFolder;

        static void Main(string[] args)
        {
            Console.WriteLine("Insert Input folder");
            inputFolder = Console.ReadLine();
            Console.WriteLine("Insert Output folder");
            outputFolder = Console.ReadLine();
            //ConvertData();
        }

        public static void ConvertData()
        {
            string[] categories = Directory.GetDirectories(inputFolder);

            foreach (var category in categories)
            {
                string[] files = Directory.GetFiles(category);
                var categoryName = Path.GetFileName(Path.GetDirectoryName(category));
                foreach (var file in files)
                {
                    string wgList = File.ReadAllText(file);
                    var fileName = Path.GetFileName(file);
                    string ownStructure;
                    object dict;
                    switch (categoryName)
                    {
                        case "Ability":
                            dict = ConsumableConverter.ConvertConsumable(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(Path.Join(outputFolder,category,fileName), ownStructure);
                            break;
                        case "Aircraft":
                            dict = AircraftConverter.ConvertAircraft(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(Path.Join(outputFolder, category, fileName), ownStructure);
                            break;
                        case "Crew":
                            var skillsList = CaptainConverter.LoadEmbeddedSkillData();
                            dict = CaptainConverter.ConvertCaptain(wgList, skillsList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(Path.Join(outputFolder, category, fileName), ownStructure);
                            break;
                        case "Modernization":
                            dict = ModernizationConverter.ConvertModernization(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(Path.Join(outputFolder, category, fileName), ownStructure);
                            break;
                        // case "Projectile":
                        //     dict = ProjectileConverter.ConvertProjectile(wgList);
                        //     ownStructure = JsonConvert.SerializeObject(dict);
                        //     File.WriteAllText(Path.Join(outputFolder, category, fileName), ownStructure);
                        //     break;
                        // case "Ship":
                        //     dict = ShipConverter.ConvertShip(wgList);
                        //     ownStructure = JsonConvert.SerializeObject(dict);
                        //     File.WriteAllText(Path.Join(outputFolder, category, fileName), ownStructure);
                        //     break;
                        case "Unit":
                            dict = ModuleConverter.ConvertModule(wgList);
                            ownStructure = JsonConvert.SerializeObject(dict);
                            File.WriteAllText(Path.Join(outputFolder, category, fileName), ownStructure);
                            break;
                        default:
                            throw new KeyNotFoundException();
                    }
                }
            }


        }


    }
}
