using DataConverter.Console.Model;
using Microsoft.Extensions.Logging;
using WowsShipBuilder.GameParamsExtractor.Services;

namespace DataConverter.Console.Services;

internal class DataUnpackHelper
{
    private readonly IGameDataUnpackService unpackService;

    private readonly ILocalizationExtractor localizationExtractor;

    private readonly ILogger<DataUnpackHelper> logger;

    public DataUnpackHelper(IGameDataUnpackService unpackService, ILocalizationExtractor localizationExtractor, ILogger<DataUnpackHelper> logger)
    {
        this.unpackService = unpackService;
        this.localizationExtractor = localizationExtractor;
        this.logger = logger;
    }

    public async Task UnpackData(ExtractOptions options)
    {
        if (Directory.Exists(options.OutputDirectory) && Directory.GetFiles(options.OutputDirectory).Any())
        {
            logger.LogWarning("Specified output directory is not empty, old files may get mixed into the unpacked data.");
        }

        var unpackResult = unpackService.ExtractAndRefineGameParams(new(options.GameParamsFile, true));
        Directory.CreateDirectory(options.OutputDirectory);
        unpackService.WriteUnfilteredFiles(unpackResult.UnfilteredData!, options.OutputDirectory);

        if (options.LocalizationInputDirectory is not null)
        {
            IEnumerable<LocalizationExtractionResult> localizations = localizationExtractor.ExtractRawLocalizations(options.LocalizationInputDirectory);
            await localizationExtractor.WriteRawLocalizationFiles(localizations, options.OutputDirectory);
        }
    }
}
