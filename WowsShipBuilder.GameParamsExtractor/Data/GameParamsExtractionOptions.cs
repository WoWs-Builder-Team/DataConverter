namespace WowsShipBuilder.GameParamsExtractor.Data;

public sealed record GameParamsExtractionOptions(
    string GameParamsFilePath,
    bool ReturnUnfilteredFiles = false
);
