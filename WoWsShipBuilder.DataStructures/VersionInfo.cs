#nullable enable
using System;
using System.Collections.Generic;

namespace WoWsShipBuilder.DataStructures
{
    public sealed record VersionInfo(Dictionary<string, List<FileVersion>> Categories, int CurrentVersionCode = 0, GameVersion? CurrentVersion = null, GameVersion? LastVersion = null)
    {
        [Obsolete("Replaced by CurrentVersion, only kept for backwards compatibility.")]
        public string? VersionName { get; init; } = null;

        [Obsolete("Replaced by LastVersion, only kept for backwards compatibility.")]
        public string? LastVersionName { get; init; } = null;

        public Version DataStructuresVersion { get; init; } = new("0.0.0");
    }

    public sealed record FileVersion(string FileName, int Version);

    public sealed record GameVersion(Version MainVersion, GameVersionType VersionType, int DataIteration, string? VersionSuffix = null) : IComparable<GameVersion>
    {
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
}
#nullable restore
