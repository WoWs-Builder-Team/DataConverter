namespace WoWsShipBuilder.DataStructures.Ship;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
public sealed class ShipConsumable
{
    public int Slot { get; init; }

    public string ConsumableName { get; init; } = string.Empty;

    public string ConsumableVariantName { get; init; } = string.Empty;
}
