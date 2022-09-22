// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

public class WgShipAbility
{
    public string[][] Abils { get; set; } = Array.Empty<string[]>();

    public int Slot { get; set; }
}
