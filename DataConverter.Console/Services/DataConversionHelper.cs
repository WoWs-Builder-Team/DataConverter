using System.Diagnostics;
using DataConverter.Console.Model;
using DataConverter.Converters;
using DataConverter.Services;
using Microsoft.Extensions.Logging;
using WowsShipBuilder.GameParamsExtractor.Services;

namespace DataConverter.Console.Services;

public class DataConversionHelper
{
    private readonly IGameDataUnpackService unpackService;

    private readonly IDataConverterService dataConverterService;

    private readonly IVersionCheckService versionCheckService;

    private readonly ILocalizationExtractor localizationExtractor;

    private readonly ILogger<DataConversionHelper> logger;

    public DataConversionHelper(IGameDataUnpackService unpackService, IDataConverterService dataConverterService, IVersionCheckService versionCheckService, ILocalizationExtractor localizationExtractor, ILogger<DataConversionHelper> logger)
    {
        this.unpackService = unpackService;
        this.dataConverterService = dataConverterService;
        this.versionCheckService = versionCheckService;
        this.localizationExtractor = localizationExtractor;
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
        logger.LogInformation("Starting data conversion with game version {} and version type {}", gameVersion.MainVersion, gameVersion.VersionType);

        var extractionResult = unpackService.ExtractAndRefineGameParams(options.ToExtractOptions());
        var convertedData = await Task.Run(() => dataConverterService.ConvertRefinedData(extractionResult.FilteredData));
        var versionInfo = await versionCheckService.CheckFileVersions(convertedData, gameVersion, "cdn.wowssb.com");

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

        if (options.LocalizationInputDirectory is not null)
        {
            IEnumerable<string> filteredTranslations = DataConverter.Program.TranslationNames.Where(x => !string.IsNullOrWhiteSpace(x));
            IEnumerable<LocalizationExtractionResult> localizations = localizationExtractor.ExtractLocalizations(new(options.LocalizationInputDirectory, filteredTranslations, options.WriteUnfiltered));
            await localizationExtractor.WriteLocalizationFiles(localizations, options.OutputDirectory, options.WriteUnfiltered, options.DebugOutputDirectory);
        }

        sw.Stop();
        logger.LogInformation("Operation finished. Time passed: {}", sw.Elapsed);
    }
}
