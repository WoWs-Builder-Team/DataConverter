using GameParamsExtractor.WGStructure;

namespace WowsShipBuilder.GameParamsExtractor.Data;

public sealed record GameParamsExtractionResult(
    Dictionary<string, Dictionary<string, List<WGObject>>> FilteredData,
    Dictionary<string, Dictionary<string, List<SortedDictionary<string, object>>>>? UnfilteredData
);
