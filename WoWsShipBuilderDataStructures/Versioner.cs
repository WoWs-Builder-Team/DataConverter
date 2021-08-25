using System;
using System.Collections.Generic;

namespace WoWsShipBuilderDataStructures
{
    class Versioner
    {
        public Dictionary<string,FileVersion> Categories { get; set; }

        public bool AreCategoryFileUpdated(Versioner secondVersioner, string category)
        {
           return Categories.GetValueOrDefault(category).Equals(secondVersioner.Categories.GetValueOrDefault(category));
        }
    }

    class FileVersion : IEquatable<FileVersion>
    {
        public string FileName { get; set; }
        public int Version { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as FileVersion);
        }

        public bool Equals(FileVersion other)
        {
            return other != null &&
                   FileName == other.FileName &&
                   Version == other.Version;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FileName, Version);
        }
    }
}
