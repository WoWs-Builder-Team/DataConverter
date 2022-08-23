using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WoWsShipBuilder.DataStructures;

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

    public static async Task<T?> GetJsonAsync<T>(this HttpClient client, string requestUri)
    {
        await using var stream = await client.GetStreamAsync(requestUri);
        using var streamReader = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(streamReader);
        var jsonSerializer = new JsonSerializer();
        return jsonSerializer.Deserialize<T>(jsonReader);
    }

    public static List<FileVersion>? GetCategoryVersions(this VersionInfo versionInfo, string category)
    {
        versionInfo.Categories.TryGetValue(category, out List<FileVersion>? versions);
        return versions;
    }
}
