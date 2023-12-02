using System.Diagnostics;
using DataConverter.Console.Model;
using DataConverter.Converters;
using DataConverter.Data;
using DataConverter.Services;
using Microsoft.Extensions.Logging;
using WowsShipBuilder.GameParamsExtractor.Services;

namespace DataConverter.Console.Services;

internal sealed class DataConversionHelper
{
    private readonly IGameDataUnpackService unpackService;

    private readonly IDataConverterService dataConverterService;

    private readonly IVersionCheckService versionCheckService;

    private readonly ILocalizationExtractor localizationExtractor;

    private readonly IModifierProcessingService modifierProcessingService;

    private readonly ILogger<DataConversionHelper> logger;

    public DataConversionHelper(IGameDataUnpackService unpackService, IDataConverterService dataConverterService, IVersionCheckService versionCheckService, ILocalizationExtractor localizationExtractor, IModifierProcessingService modifierProcessingService, ILogger<DataConversionHelper> logger)
    {
        this.unpackService = unpackService;
        this.dataConverterService = dataConverterService;
        this.versionCheckService = versionCheckService;
        this.localizationExtractor = localizationExtractor;
        this.modifierProcessingService = modifierProcessingService;
        this.logger = logger;
    }

    public async Task ExtractAndConvertData(ConvertOptions options)
    {
        var sw = Stopwatch.StartNew();
        var gameVersion = GameVersionConverter.FromVersionString(options.Version);
        if (options.VersionType is not null)
        {
            gameVersion = gameVersion with { VersionType = options.VersionType.Value };
            logger.LogInformation("Overriding version type with type {}", gameVersion.VersionType);
        }

        DataCache.CurrentVersion = gameVersion;
        logger.LogInformation("Starting data conversion with game version {} and version type {}", gameVersion.MainVersion, gameVersion.VersionType);

        if (Directory.Exists(options.OutputDirectory) && Directory.GetFiles(options.OutputDirectory).Any())
        {
            logger.LogWarning("Specified output directory is not empty, old files may get mixed into the conversion results");
        }

        var gameParamsFile = new FileInfo(options.GameParamsFile);
        var modifierDictionary = modifierProcessingService.ReadModifiersFile(gameParamsFile.Directory!.FullName + "Modifiers.json");

        var extractionResult = unpackService.ExtractAndRefineGameParams(options.ToExtractionOptions());
        var convertedData = await dataConverterService.ConvertRefinedData(extractionResult.FilteredData, options.WriteModifierDebugOutput, modifierDictionary);
        var versionInfo = await versionCheckService.CheckFileVersionsAsync(convertedData, gameVersion, Constants.CdnHost);

        Directory.CreateDirectory(options.OutputDirectory);

        string debugOutput = options.DebugOutputDirectory ?? options.OutputDirectory;
        if (options.WriteUnfiltered)
        {
            unpackService.WriteUnfilteredFiles(extractionResult.UnfilteredData!, debugOutput);
        }

        if (options.WriteFiltered)
        {
            unpackService.WriteFilteredFiles(extractionResult.FilteredData, debugOutput);
        }

        await dataConverterService.WriteConvertedData(convertedData, options.OutputDirectory);
        await versionCheckService.WriteVersionInfo(versionInfo, options.OutputDirectory);

        IEnumerable<LocalizationExtractionResult>? localizations = null;
        if (options.LocalizationInputDirectory is not null)
        {
            IEnumerable<string> filteredTranslations = DataCache.TranslationNames.Where(x => !string.IsNullOrWhiteSpace(x));
            localizations = localizationExtractor.ExtractLocalizations(new(options.LocalizationInputDirectory, filteredTranslations, options.WriteUnfiltered)).ToList();
            await localizationExtractor.WriteLocalizationFiles(localizations, options.OutputDirectory, options.WriteUnfiltered, options.DebugOutputDirectory);
        }

        if (options.WriteModifierDebugOutput && localizations is not null)
        {
            await modifierProcessingService.WriteDebugModifierFiles(convertedData.ModifiersList, modifierDictionary, localizations.First().FilteredLocalizations.Keys.ToList(), debugOutput);
        }

        sw.Stop();
        logger.LogInformation("Operation finished. Time passed: {}", sw.Elapsed);
    }
}
