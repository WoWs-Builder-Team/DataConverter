using System.Collections.Generic;
using WoWsShipBuilder.DataStructures.Modifiers;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Captain;

public class UniqueSkillEffect
{
    public bool IsPercent { get; set; }

    public int UniqueType { get; set; }

    public List<Modifier> Modifiers { get; set; } = new();
}
