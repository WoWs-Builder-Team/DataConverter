using System.Collections.Concurrent;

namespace DataConverter.Data;

public class DataCache
{
    public static readonly ConcurrentBag<string> TranslationNames = new();
}
