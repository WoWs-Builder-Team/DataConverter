using System;
using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Versioning;

public sealed record VersionInfo(Dictionary<string, List<FileVersion>> Categories, int CurrentVersionCode, GameVersion CurrentVersion, GameVersion? LastVersion = null)
{
    public static readonly VersionInfo Default = new(new(), 0, GameVersion.Default);

    public Version DataStructuresVersion { get; init; } = new("0.0.0");
}
