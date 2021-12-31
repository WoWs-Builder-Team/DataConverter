using System.Collections.Generic;

namespace WoWsShipBuilder.DataStructures
{
    public sealed record Exterior
    {
        public long Id { get; init; }
        public string Index { get; init; }  = string.Empty;
        public Dictionary<string,double>? Modifiers { get; set; }
        public string Name { get; init; } = string.Empty;
        public ExteriorType Type { get; set; }
        public int SortOrder { get; init; }
        public Restriction Restrictions { get; set; } = new();
        public int Group { get; init; }
    }


    public sealed record Restriction
    {
        public List<string>? ForbiddenShips { get; init; }
        public List<string>? Levels { get; init; }
        public List<string>? Nations { get; init; }
        public List<string>? SpecificShips { get; init; }
        public List<string>? Subtype { get; init; }
    }
}
