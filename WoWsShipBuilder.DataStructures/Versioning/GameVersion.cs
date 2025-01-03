﻿using System;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Versioning;

[SuppressMessage("Design", "CA1036", Justification = "comparison operators can be ignored for now")]
public sealed record GameVersion(Version MainVersion, GameVersionType VersionType, int DataIteration) : IComparable<GameVersion>
{
    public static readonly GameVersion Default = new(new(0, 0), GameVersionType.Live, 0);

    public int CompareTo(GameVersion? other)
    {
        if (other == null)
        {
            return 1;
        }

        int versionDiff = MainVersion.CompareTo(other.MainVersion);
        return versionDiff == 0 ? DataIteration.CompareTo(other.DataIteration) : versionDiff;
    }
}
