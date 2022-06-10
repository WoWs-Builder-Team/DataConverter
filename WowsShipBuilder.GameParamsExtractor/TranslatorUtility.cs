using System.Diagnostics;
using GetText;
using GetText.Loaders;
using Newtonsoft.Json;

namespace WowsShipBuilder.GameParamsExtractor;

public static class TranslatorUtility
{
    /// <summary>
    /// Process all the .mo file in a folder, applies the filter and write a json for each input file in the output folder.
    /// </summary>
    /// <param name="inputPath">The folder containing the .mo files.</param>
    /// <param name="outputPath">The folder in which to write the file.</param>
    /// <param name="translationFilter">The filter HashSet.</param>
    /// <param name="writeUnfilteredFiles">Should write an unfiltered translation file</param>
    /// <param name="debugFilesBasePath">Base Path for the unfiltered translation file</param>
    public static void ProcessTranslationFiles(string inputPath, string outputPath, IEnumerable<string> translationFilter, bool writeUnfilteredFiles, string debugFilesBasePath)
    {
        Console.WriteLine("Start processing translation files");
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var attributes = File.GetAttributes(inputPath);
        DirectoryInfo inputDirectory = new(inputPath);
        if ((attributes & FileAttributes.Directory) != FileAttributes.Directory)
        {
            inputDirectory = inputDirectory.Parent ?? throw new InvalidOperationException();
        }

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        FileInfo[] localizationFiles = inputDirectory.GetFiles("*.mo", SearchOption.AllDirectories);
        var initialFileStream = localizationFiles.First().OpenRead();
        var tmpCatalog = new Catalog(new MoAstPluralLoader(initialFileStream));
        HashSet<string> filteredKeys = FilterTranslationKeys(tmpCatalog.Translations.Keys, translationFilter);

        Parallel.ForEach(localizationFiles, file =>
        {
            Console.WriteLine("Processing file: " + file);
            var fileStream = file.OpenRead();
            var catalog = new Catalog(fileStream);

            //remove first and emtpy key, contains gibberish
            catalog.Translations.Remove("");

            Dictionary<string, string?> filteredTranslations = catalog.Translations
                .Where(entry => filteredKeys.Contains(entry.Key))
                .ToDictionary(x => x.Key[4..], x => x.Value.FirstOrDefault());

            string data = JsonConvert.SerializeObject(filteredTranslations, Formatting.Indented);
            string languageName = file.Directory!.Parent!.Name;

            if (writeUnfilteredFiles && languageName.Contains("en"))
            {
                var dict = catalog.Translations;
                string debugData = JsonConvert.SerializeObject(dict, Formatting.Indented);
                File.WriteAllText(Path.Combine(debugFilesBasePath, "test.json"), debugData);
            }

            string outputFile = Path.Combine(outputPath, languageName + ".json");
            File.WriteAllText(outputFile, data);
        });

        stopwatch.Stop();
        Console.WriteLine($"All translation file processed. Time passed: {stopwatch.Elapsed}");
    }

    private static HashSet<string> FilterTranslationKeys(IEnumerable<string> rawKeys, IEnumerable<string> filter)
    {
        return rawKeys
            .AsParallel()
            .Where(key => filter.Any(s => key.Contains(s, StringComparison.OrdinalIgnoreCase)))
            .ToHashSet();
    }
}
