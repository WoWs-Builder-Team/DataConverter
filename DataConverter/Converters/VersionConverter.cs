using System;
using WoWsShipBuilder.DataStructures;

namespace DataConverter.Converters
{
    public static class VersionConverter
    {
        public static GameVersion FromVersionString(string? versionString)
        {
            if (versionString == null)
            {
                return new(new(0, 0), GameVersionType.Dev, 0);
            }

            GameVersionType versionType = GameVersionType.Live;
            string? versionSuffix = null;
            if (versionString.Contains("-"))
            {
                int suffixIndex = versionString.IndexOf("-", StringComparison.InvariantCulture);
                string versionTypeString = versionString[suffixIndex..];
                versionString = versionString[..suffixIndex];
                if (versionTypeString.EndsWith("pts", StringComparison.InvariantCultureIgnoreCase))
                {
                    versionType = GameVersionType.Pts;
                }
                else
                {
                    versionType = GameVersionType.Dev;
                    versionSuffix = versionTypeString.TrimStart('-', ' ');
                }
            }

            var dataIteration = 0;
            if (versionString.Contains("#"))
            {
                int iterationIndex = versionString.IndexOf("#", StringComparison.InvariantCulture);
                string iteration = versionString[(iterationIndex + 1)..];
                dataIteration = int.Parse(iteration);
                versionString = versionString[..iterationIndex];
            }

            var mainVersion = Version.Parse(versionString);

            return new(mainVersion, versionType, dataIteration, versionSuffix);
        }
    }
}
