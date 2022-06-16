using System.Diagnostics;
using WowsShipBuilder.GameParamsExtractor;

namespace WowsShipBuilder.GameParamsExtractorConsole
{
    internal class Program
    {
        private const string GameParamsPath = "GameParams.data";
        private const string BaseDir = "output/";
        private const string TranslationPath = "texts";

        private const bool Translation = true;

        public static void Main(string[] args)
        {
            if (Translation)
            {
                var lines = File.ReadAllLines("TranslationNames.csv").ToList();
                // TranslatorUtility.ProcessTranslationFiles(TranslationPath, Path.Combine(BaseDir, "Localization"), lines);
            }
            else
            {
                Directory.CreateDirectory(BaseDir);
                var data = GameParamsUtility.ProcessGameParams(GameParamsPath, writeFilteredFiles: true, outputPath: BaseDir);
                Debug.WriteLine(data.Count);
            }
        }
    }
}
