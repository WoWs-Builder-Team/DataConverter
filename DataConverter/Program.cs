using DataConverter.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using WoWsShipBuilderDataStructures;

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
            ConvertData();
        }

        public static void ConvertData()
        {
            // TODO: fix with correct paths
            string fileName = $"{inputFolder}/Modernization/Common.json";
            string wgList = File.ReadAllText(fileName);
            //insert here the other methods converting the stuff into Lists of our data structures
            Dictionary<string,Modernization> modernizations = ModernizationConverter.ConvertModernization(wgList);
        }

        

    }
}
