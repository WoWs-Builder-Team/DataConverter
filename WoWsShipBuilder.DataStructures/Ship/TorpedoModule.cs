using System.Collections.Immutable;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Ship;

public sealed class TorpedoModule
{
    public ImmutableDictionary<TorpedoLauncherLoaderPosition, ImmutableArray<string>> TorpedoLoaders { get; init; } = ImmutableDictionary<TorpedoLauncherLoaderPosition, ImmutableArray<string>>.Empty;

    public ImmutableArray<TorpedoLauncher> TorpedoLaunchers { get; init; } = ImmutableArray<TorpedoLauncher>.Empty;
}
