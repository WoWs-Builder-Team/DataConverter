using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public class UpgradeInfo
{
    public List<ShipUpgrade> ShipUpgrades { get; set; } = new();

    public int CostCredits { get; set; }

    public int CostGold { get; set; }

    public int CostSaleGold { get; set; }

    public int CostXp { get; set; }

    public int Value { get; set; }

    /// <summary>
    /// Helper method to easily filter all upgrade configurations of a specific type.
    /// </summary>
    /// <param name="componentType">The type of the component to look for.</param>
    /// <returns>A list of all ship upgrades with the specified type.</returns>
    public List<ShipUpgrade> FindUpgradesOfType(ComponentType componentType)
    {
        return ShipUpgrades.Where(upgrade => upgrade.UcType == componentType).ToList();
    }

    /// <summary>
    /// Helper method to group all available ship upgrades by their type.
    /// </summary>
    /// <returns>A dictionary mapping the <see cref="ComponentType"/> of a <see cref="ShipUpgrade"/> to all upgrades available with that type.
    /// Stock upgrades appear first.</returns>
    public Dictionary<ComponentType, List<ShipUpgrade>> GroupUpgradesByType()
    {
        return ShipUpgrades.GroupBy(upgrade => upgrade.UcType)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(upgrade => string.IsNullOrEmpty(upgrade.Prev)).ToList());
    }
}

public class ShipUpgrade
{
    public Dictionary<ComponentType, string[]> Components { get; set; } = new();

    public string Name { get; set; } = string.Empty;

    public string[] NextShips { get; set; } = Array.Empty<string>();

    public string Prev { get; set; } = string.Empty;

    public ComponentType UcType { get; set; }

    public bool CanBuy { get; set; }
}
