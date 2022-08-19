// <copyright file="ILocalizationExtractor.cs" company="PlaceholderCompany">Copyright (c) PlaceholderCompany. All rights reserved.</copyright>

namespace WowsShipBuilder.GameParamsExtractor.Services;

public interface ILocalizationExtractor
{
    IEnumerable<LocalizationExtractionResult> ExtractLocalizations(LocalizationExtractionOptions options);

    Task WriteLocalizationFiles(IEnumerable<LocalizationExtractionResult> localizationFiles, string outputBasePath, bool writeUnfiltered, string? debugOutputPath);
}
