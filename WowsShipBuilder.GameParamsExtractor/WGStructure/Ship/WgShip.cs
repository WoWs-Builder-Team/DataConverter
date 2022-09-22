using GameParamsExtractor.WGStructure;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

public class WgShip : WGObject
{
    public Dictionary<string, WgArmamentModule> ModulesArmaments { get; set; } = new();

    public Dictionary<string, WgShipAbility> ShipAbilities { get; set; } = new();

    public WgShipUpgradeInfo ShipUpgradeInfo { get; set; } = new();

    public long Id { get; set; }

    public string Index { get; set; } = string.Empty;

    public int Level { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Group { get; set; } = string.Empty;

    public List<string> Permoflages { get; set; } = new();
}
