#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace WoWsShipBuilder.DataStructures;

public sealed record VersionInfo(Dictionary<string, List<FileVersion>> Categories, int CurrentVersionCode, GameVersion CurrentVersion, GameVersion? LastVersion = null)
{
    public static readonly VersionInfo Default = new(new(), 0, GameVersion.Default);

    public Version DataStructuresVersion { get; init; } = new("0.0.0");
}

public sealed record FileVersion(string FileName, int Version, string Checksum = "")
{
    public static string ComputeChecksum(Stream stream)
    {
        using var sha256 = SHA256.Create();
        return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
    }

    public static string ComputeChecksum(string data)
    {
        using var sha256 = SHA256.Create();
        return BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(data))).Replace("-", "").ToLowerInvariant();
    }
}

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
#nullable restore
