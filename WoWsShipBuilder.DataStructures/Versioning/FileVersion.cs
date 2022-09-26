using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Versioning;

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
