using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WoWsShipBuilder.DataStructures.Versioning;

namespace DataConverter;

public static class Extensions
{
    public static string CapitalizeFirstChar(this string input) =>
        input switch
        {
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => string.Concat(input[0].ToString().ToUpperInvariant(), input.AsSpan(1)),
        };

    public static void UnionWith<T>(this ConcurrentBag<T> bag, IEnumerable<T> toAdd)
    {
        foreach (var element in toAdd)
        {
            bag.Add(element);
        }
    }

    public static List<FileVersion>? GetCategoryVersions(this VersionInfo versionInfo, string category)
    {
        versionInfo.Categories.TryGetValue(category, out List<FileVersion>? versions);
        return versions;
    }
}
