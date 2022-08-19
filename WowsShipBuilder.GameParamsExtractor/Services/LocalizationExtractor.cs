using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WowsShipBuilder.GameParamsExtractor.Services;

public class LocalizationExtractor : ILocalizationExtractor
{
    private readonly ILogger<LocalizationExtractor> logger;

    private readonly JsonSerializer serializer = new()
    {
        Formatting = Formatting.Indented,
    };

    public LocalizationExtractor(ILogger<LocalizationExtractor> logger)
    {
        this.logger = logger;
    }

    public IEnumerable<LocalizationExtractionResult> ExtractLocalizations(LocalizationExtractionOptions options)
    {
        logger.LogInformation("Starting localization file processing");
        var sw = Stopwatch.StartNew();
        FileInfo[] translationFiles = TranslatorUtility.FindTranslationFiles(options.InputDirectory);
        List<TranslatorUtility.RawLocalizationData> rawLocalizations = TranslatorUtility.ExtractRawLocalization(translationFiles, logger).ToList();
        logger.LogInformation("Filtering localization keys");
        HashSet<string> filteredKeys = TranslatorUtility.FilterTranslationKeys(rawLocalizations.First().Translations.Keys, options.TranslationFilter);
        IEnumerable<TranslatorUtility.LocalizationData> localizations = TranslatorUtility.ProcessTranslations(rawLocalizations, filteredKeys, logger);

        var results = new List<LocalizationExtractionResult>();
        foreach (var localization in localizations)
        {
            var unfilteredLocalizations = options.OutputUnfilteredFiles ? rawLocalizations.FirstOrDefault(r => r.Language.Equals(localization.Language)) : null;
            var result = new LocalizationExtractionResult(localization.Language, localization.Translations, unfilteredLocalizations?.Translations);
            results.Add(result);
        }

        sw.Stop();
        logger.LogInformation("Finished localization file processing. Time passed: {}", sw.Elapsed);
        return results;
    }

    public async Task WriteLocalizationFiles(IEnumerable<LocalizationExtractionResult> localizationFiles, string outputBasePath, bool writeUnfiltered, string? debugOutputPath)
    {
        const string localizationDirectory = "Localization";
        string localizationBasePath = Path.Join(outputBasePath, localizationDirectory);
        Directory.CreateDirectory(localizationBasePath);

        string? debugBasePath = null;
        if (writeUnfiltered && debugOutputPath != null)
        {
            debugBasePath = Path.Join(debugOutputPath, localizationDirectory);
            Directory.CreateDirectory(debugBasePath);
        }
        foreach (var localizationFile in localizationFiles)
        {
            string localizationFilePath = Path.Join(localizationBasePath, localizationFile.Language + ".json");
            await using var file = File.CreateText(localizationFilePath);
            serializer.Serialize(file, localizationFile.FilteredLocalizations);

            if (writeUnfiltered && debugOutputPath != null)
            {
                string debugFilePath = Path.Join(debugBasePath, localizationFile.Language + ".json");
                await using var debugFile = File.CreateText(debugFilePath);
                serializer.Serialize(debugFile, localizationFile.UnfilteredLocalizations);
            }
        }
    }
}

public sealed record LocalizationExtractionOptions(string InputDirectory, IEnumerable<string> TranslationFilter, bool OutputUnfilteredFiles);

public sealed record LocalizationExtractionResult(string Language, Dictionary<string, string> FilteredLocalizations, Dictionary<string, string[]>? UnfilteredLocalizations);
