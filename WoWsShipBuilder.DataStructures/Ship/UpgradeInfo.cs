using System.Collections.Immutable;
using System.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public class UpgradeInfo
{
    public ImmutableList<ShipUpgrade> ShipUpgrades { get; init; } = ImmutableList<ShipUpgrade>.Empty;

    public int CostCredits { get; init; }

    public int CostGold { get; init; }

    public int CostSaleGold { get; init; }

    public int CostXp { get; init; }

    public int Value { get; init; }

    /// <summary>
    /// Helper method to easily filter all upgrade configurations of a specific type.
    /// </summary>
    /// <param name="componentType">The type of the component to look for.</param>
    /// <returns>A list of all ship upgrades with the specified type.</returns>
    public ImmutableList<ShipUpgrade> FindUpgradesOfType(ComponentType componentType)
    {
        return ShipUpgrades.Where(upgrade => upgrade.UcType == componentType).ToImmutableList();
    }

    /// <summary>
    /// Helper method to group all available ship upgrades by their type.
    /// </summary>
    /// <returns>A dictionary mapping the <see cref="ComponentType"/> of a <see cref="ShipUpgrade"/> to all upgrades available with that type.
    /// Stock upgrades appear first.</returns>
    public ImmutableDictionary<ComponentType, ImmutableList<ShipUpgrade>> GroupUpgradesByType()
    {
        return ShipUpgrades.GroupBy(upgrade => upgrade.UcType)
            .ToImmutableDictionary(group => group.Key, group => group.OrderByDescending(upgrade => string.IsNullOrEmpty(upgrade.Prev)).ToImmutableList());
    }
}

public class ShipUpgrade
{
    public ImmutableDictionary<ComponentType, ImmutableArray<string>> Components { get; init; } = ImmutableDictionary<ComponentType, ImmutableArray<string>>.Empty;

    public string Name { get; init; } = string.Empty;

    public ImmutableArray<string> NextShips { get; init; } = ImmutableArray<string>.Empty;

    public string? Prev { get; init; }

    public ComponentType UcType { get; init; }

    public bool CanBuy { get; init; }
}
