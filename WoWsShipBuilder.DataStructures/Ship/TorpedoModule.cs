using System.Collections.Immutable;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public class TorpedoModule
{
    public ImmutableDictionary<SubTorpLauncherLoaderPosition, ImmutableArray<string>> TorpedoLoaders { get; init; } = ImmutableDictionary<SubTorpLauncherLoaderPosition, ImmutableArray<string>>.Empty;

    public ImmutableArray<TorpedoLauncher> TorpedoLaunchers { get; init; } = ImmutableArray<TorpedoLauncher>.Empty;
}
