using WoWsShipBuilder.DataStructures.Modifiers;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace WowsShipBuilder.GameParamsExtractor.Data;

public sealed record GameParamsExtractionResult(
    Dictionary<string, Dictionary<string, List<WgObject>>> FilteredData,
    Dictionary<string, Dictionary<string, List<SortedDictionary<string, object>>>>? UnfilteredData
);
