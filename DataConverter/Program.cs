using DataConverter.Converters;
using System;
using System.Collections.Generic;
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
            //insert here the other methods converting the stuff into Lists of our data structures
            List<Modernization> modernizations = ModernizationConverter.ConvertModernization();
        }

        

    }
}
