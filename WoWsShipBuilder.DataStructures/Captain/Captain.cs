using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Captain;

public class Captain
{
    public long Id { get; set; }
    public string Index { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool HasSpecialSkills { get; set; }
    public Dictionary<string, Skill> Skills { get; set; } = new();
    public Dictionary<string, UniqueSkill> UniqueSkills { get; set; } = new();
    public Nation Nation { get; set; }
}
