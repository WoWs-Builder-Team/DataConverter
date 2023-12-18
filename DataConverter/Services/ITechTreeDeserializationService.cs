using System.Collections.Generic;

namespace DataConverter.Services;

public interface ITechTreeDeserializationService
{
    /// <summary>
    /// Reads the tech tree file at the provided path.
    /// </summary>
    /// <param name="path">The path to the tech tree file.</param>
    /// <returns>The dictionary of ship ids and their horizontal positions in the tech tree.</returns>
    public Dictionary<long, int> ReadTechTreeFile(string path);
}
