using System.Collections.Generic;
// ReSharper disable NotAccessedPositionalProperty.Global

namespace WoWsShipBuilder.DataStructures
{
    public sealed record VersionInfo(Dictionary<string, List<FileVersion>> Categories, int CurrentVersionCode = 0, string VersionName = "", string LastVersionName = "");

    public sealed record FileVersion(string FileName, int Version);
}
