using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DataConverter;
using DataConverter.WGStructure;
using Newtonsoft.Json;
using SecondLanguage;
using WowsShipBuilder.GameParamsExtractor;

namespace WowsShipBuilder.GameParamsExtractorConsole
{
    internal class Program
    {
        private const string GameParamsPath = "GameParams.data";
        private const string BaseDir = "output/";
        private const string translationPath = "global.mo";

        private static bool translation = false;

        public static void Main(string[] args)
        {
            if (translation)
            {
                var lines = File.ReadAllLines("translation.csv");
                var filter = new HashSet<string>(lines);
                TranslatorUtility.ProcessTranslationFiles(translationPath, BaseDir, filter);
            }
            else
            {
                var data = GameParamsUtility.ProcessGameParams(GameParamsPath, writeFilteredFiles: true, outputPath: BaseDir);
                Debug.WriteLine(data.Count);
            }
        }
    }
}
