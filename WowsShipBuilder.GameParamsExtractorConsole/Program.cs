using System.Diagnostics;
using WowsShipBuilder.GameParamsExtractor;

namespace WowsShipBuilder.GameParamsExtractorConsole
{
    internal class Program
    {
        private const string GameParamsPath = "GameParams.data";
        private const string BaseDir = "output/";
        private const string TranslationPath = "texts";

        private static bool translation = true;

        public static void Main(string[] args)
        {
            if (translation)
            {
                var lines = File.ReadAllLines("TranslationNames.csv").ToList();
                TranslatorUtility.ProcessTranslationFiles(TranslationPath, Path.Combine(BaseDir, "Localization"), lines);
            }
            else
            {
                var data = GameParamsUtility.ProcessGameParams(GameParamsPath, writeFilteredFiles: true, outputPath: BaseDir);
                Debug.WriteLine(data.Count);
            }
        }
    }
}
