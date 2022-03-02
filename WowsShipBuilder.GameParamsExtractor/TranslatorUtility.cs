using System.Collections.Generic;
using Newtonsoft.Json;
using SecondLanguage;

namespace WowsShipBuilder.GameParamsExtractor
{
    public static class TranslatorUtility
    {
        /// <summary>
        /// Process all the .mo file in a folder, applies the filter and write a json for each input file in the output folder.
        /// </summary>
        /// <param name="inputPath">The folder containing the .mo files.</param>
        /// <param name="outputPath">The folder in which to write the file.</param>
        /// <param name="translationFilter">The filter HashShet.</param>
        public static void ProcessTranslationFiles(string inputPath, string outputPath, HashSet<string> translationFilter)
        {
            Console.WriteLine("Start processing translation files");
            var input = new DirectoryInfo(inputPath);
            Parallel.ForEach(input.GetFiles(), file =>
            {
                Console.WriteLine("Processing file: " + file);
                FileStream stream = file.OpenRead();
                var moTranslation = new GettextMOTranslation();
                moTranslation.Load(stream);
                var dict = moTranslation.GetGettextKeys();
                var dictionary = dict.ToDictionary(x => x.Key.ID, x => x.Value);
                //remove first and emtpy key, contains gibberish
                dictionary.Remove("");

                dictionary = dictionary.Where(x => translationFilter.Any(s => x.Key.Contains(s, StringComparison.OrdinalIgnoreCase))).ToDictionary(x => x.Key.Substring(4), x => x.Value);

                var data = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
                File.WriteAllText(@$"{outputPath}{file.Name}", data);

            });

            Console.WriteLine("All translation file processed");
        }
    }
}
