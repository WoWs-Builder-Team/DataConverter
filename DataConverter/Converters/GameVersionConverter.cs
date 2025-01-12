using System;
using System.Globalization;
using System.Text.RegularExpressions;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Versioning;

namespace DataConverter.Converters;

public static class GameVersionConverter
{
    public static GameVersion FromVersionString(string versionString)
    {
        var versionPattern = new Regex(@"^(?<main>\d+(\.\d+){1,2})(#(?<iteration>\d+))?(-(?<type>[a-z0-9]+))?$");
        var match = versionPattern.Match(versionString);
        if (!match.Success)
        {
            throw new FormatException("The provided version string does not match the version pattern");
        }

        var mainVersionGroup = match.Groups["main"];
        var iterationGroup = match.Groups["iteration"];
        var typeGroup = match.Groups["type"];

        var version = mainVersionGroup.Success ? Version.Parse(mainVersionGroup.Value) : throw new InvalidOperationException("Main version not found in version string");
        int iteration = iterationGroup.Success ? int.Parse(iterationGroup.Value, CultureInfo.InvariantCulture) : 1;
        var type = typeGroup.Success ? Enum.Parse<GameVersionType>(typeGroup.Value, true) : GameVersionType.Live;
        return new(version, type, iteration);
    }
}
