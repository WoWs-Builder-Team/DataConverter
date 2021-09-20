using System.Collections.Generic;

namespace WoWsShipBuilderDataStructures
{
    public class VersionInfo
    {
        public Dictionary<string, List<FileVersion>> Categories { get; }

        public int CurrentVersionCode { get; }

        public VersionInfo(Dictionary<string, List<FileVersion>> categories, int currentVersionCode = 0)
        {
            Categories = categories;
            CurrentVersionCode = currentVersionCode;
        }
    }

    public class FileVersion
    {
        public string FileName { get; }
        public int Version { get; }

        public FileVersion(string fileName, int version)
        {
            FileName = fileName;
            Version = version;
        }
    }
}
