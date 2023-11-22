using System.Collections.Generic;
using WoWsShipBuilder.DataStructures.Modifiers;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global
namespace WoWsShipBuilder.DataStructures.Captain;

public class Skill
{
    public bool CanBeLearned { get; set; }

    public bool IsEpic { get; set; } // true if the skill has buffed modifier

    public int SkillNumber { get; set; }

    public List<ShipClass> LearnableOn { get; set; } = new();

    public List<SkillPosition> Tiers { get; set; } = new(); // contains the tier of the skill for all the classes that can use it

    public List<Modifier> Modifiers { get; set; } = new();

    public List<ConditionalModifierGroup> ConditionalModifierGroups { get; set; } = new();
}
