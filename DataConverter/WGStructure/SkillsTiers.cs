using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Common;

namespace DataConverter.WGStructure
{
    public class SkillsTiers
    {
        private Dictionary<ShipClass, List<SkillRow>>? positionsByClass;

        [JsonExtensionData]
        public Dictionary<string, JToken> RawPositionsByClass { get; set; } = null!;

        public Dictionary<ShipClass, List<SkillRow>> PositionsByClass => positionsByClass ??= RawPositionsByClass
            .Select(entry => (Enum.Parse<ShipClass>(entry.Key, true), entry.Value.ToObject<List<SkillRow>>()))
            .Where(entry => entry.Item2 != null)
            .ToDictionary(entry => entry.Item1, entry => entry.Item2!);
    }

    public class SkillRow
    {
        private List<List<int>>? skillGroups;

        [JsonExtensionData]
        public Dictionary<string, JToken> RawSkillGroups { get; set; } = null!;

        [JsonIgnore]
        public List<List<int>> SkillGroups => skillGroups ??= RawSkillGroups.OrderBy(entry => entry.Key)
            .Select(entry => entry.Value.ToObject<List<int>>())
            .Where(entry => entry != null)
            .ToList()!;
    }
}