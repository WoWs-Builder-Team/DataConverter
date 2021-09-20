using System;
using System.Collections.Generic;

namespace WoWsShipBuilderDataStructures
{
    public class Versioner
    {
        public Dictionary<string, List<FileVersion>> Categories { get; set; }

        public Versioner(Dictionary<string, List<FileVersion>> categoriesVersions)
        {
            Categories = categoriesVersions;
        }

    }

    public class FileVersion
    {
        public string FileName { get; set; }
        public int Version { get; set; }

        public FileVersion(string fileName, int version)
        {
            FileName = fileName;
            Version = version;
        }
    }
}
