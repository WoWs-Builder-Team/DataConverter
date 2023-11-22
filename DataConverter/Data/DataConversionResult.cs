using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WoWsShipBuilder.DataStructures.Modifiers;

namespace DataConverter.Data;

public sealed record DataConversionResult(List<ResultFileContainer> Files, List<Modifier> ModifiersList);

public sealed record ResultFileContainer(string Content, string Category, string Filename)
{
    public async Task WriteFileAsync(string baseDirectory)
    {
        string directory = Path.Join(baseDirectory, Category);
        Directory.CreateDirectory(directory);
        string path = Path.Join(directory, Filename);
        await File.WriteAllTextAsync(path, Content);
    }
}
