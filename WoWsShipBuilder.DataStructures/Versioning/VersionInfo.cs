using System;
using System.Collections.Immutable;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Versioning;

public sealed record VersionInfo(ImmutableDictionary<string, ImmutableList<FileVersion>> Categories, int CurrentVersionCode, GameVersion CurrentVersion, GameVersion? LastVersion = null)
{
    public static readonly VersionInfo Default = new(ImmutableDictionary<string, ImmutableList<FileVersion>>.Empty, 0, GameVersion.Default);

    public Version DataStructuresVersion { get; init; } = new("0.0.0");
}
