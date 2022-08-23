namespace WowsShipBuilder.GameParamsExtractor.Services;

public interface ILocalizationExtractor
{
    IEnumerable<LocalizationExtractionResult> ExtractLocalizations(LocalizationExtractionOptions options);

    IEnumerable<LocalizationExtractionResult> ExtractRawLocalizations(string inputDirectory);

    Task WriteLocalizationFiles(IEnumerable<LocalizationExtractionResult> localizationFiles, string outputBasePath, bool writeUnfiltered, string? debugOutputPath);

    Task WriteRawLocalizationFiles(IEnumerable<LocalizationExtractionResult> localizationFiles, string outputBasePath);
}
