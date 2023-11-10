using System.Collections.Concurrent;
using WoWsShipBuilder.DataStructures.Versioning;

namespace DataConverter.Data;

public class DataCache
{
    public static readonly ConcurrentBag<string> TranslationNames = new();

    public static GameVersion CurrentVersion { get; set; } = default!;
}
