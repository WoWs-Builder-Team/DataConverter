#nullable enable
using System;
using System.Collections.Generic;

namespace WoWsShipBuilder.DataStructures
{
    public sealed record VersionInfo(Dictionary<string, List<FileVersion>> Categories, int CurrentVersionCode = 0, string VersionName = "", string LastVersionName = "")
    {
        public Version DataStructuresVersion { get; init; } = new("0.0.0");
    }

    public sealed record FileVersion(string FileName, int Version);
}
#nullable restore
