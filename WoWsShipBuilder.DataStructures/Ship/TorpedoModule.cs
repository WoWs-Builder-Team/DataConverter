using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public class TorpedoModule
{
    public decimal TimeToChangeAmmo { get; set; }

    public List<TorpedoLauncher> TorpedoLaunchers { get; set; } = new();
}
