namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgShipAbility
{
    public string[][] Abils { get; init; } = Array.Empty<string[]>();

    public int Slot { get; init; }
}
