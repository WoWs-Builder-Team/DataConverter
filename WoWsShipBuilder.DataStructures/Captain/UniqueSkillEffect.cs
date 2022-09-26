using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Captain;

public class UniqueSkillEffect
{
    public bool IsPercent { get; set; }
    public int UniqueType { get; set; }
    public Dictionary<string, float> Modifiers { get; set; } = new();
}
