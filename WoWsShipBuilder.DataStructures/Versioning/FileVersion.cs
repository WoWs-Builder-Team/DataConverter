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
        return Convert.ToHexStringLower(sha256.ComputeHash(stream));
    }

    public static string ComputeChecksum(string data)
    {
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(data)));
    }
}
