namespace WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class WgShip : WgObject
{
    public Dictionary<string, WgArmamentModule> ModulesArmaments { get; init; } = new();

    public Dictionary<string, WgShipAbility> ShipAbilities { get; init; } = new();

    public WgShipUpgradeInfo ShipUpgradeInfo { get; init; } = new();

    public long Id { get; init; }

    public string Index { get; init; } = string.Empty;

    public int Level { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Group { get; init; } = string.Empty;

    public List<string> Permoflages { get; init; } = new();
}
