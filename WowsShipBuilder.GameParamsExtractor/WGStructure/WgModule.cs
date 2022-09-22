// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global

namespace WowsShipBuilder.GameParamsExtractor.WGStructure;

public class WgModule : WgObject
{
    public int CostCr { get; init; }

    public int CostXp { get; init; }

    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
}
