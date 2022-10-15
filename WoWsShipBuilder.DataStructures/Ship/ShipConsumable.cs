namespace WoWsShipBuilder.DataStructures.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public class ShipConsumable
{
    public int Slot { get; set; }

    public string ConsumableName { get; set; } = string.Empty;

    public string ConsumableVariantName { get; set; } = string.Empty;
}
