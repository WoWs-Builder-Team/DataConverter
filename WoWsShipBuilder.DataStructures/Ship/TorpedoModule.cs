using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public class TorpedoModule
{
    public Dictionary<SubTorpLauncherLoaderPosition, List<string>> TorpedoLoaders { get; set; } = new();

    public List<TorpedoLauncher> TorpedoLaunchers { get; set; } = new();
}
