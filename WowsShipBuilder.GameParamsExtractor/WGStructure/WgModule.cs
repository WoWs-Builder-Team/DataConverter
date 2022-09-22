// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global

namespace WowsShipBuilder.GameParamsExtractor.WGStructure;

public class WgModule : WgObject
{
    public int CostCr { get; set; }

    public int CostXp { get; set; }

    public long Id { get; set; }

    public string Index { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
