using System.Collections.Generic;

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

    public Dictionary<string, float> Modifiers { get; set; } = new(); // modifiers for always on effects

    public Dictionary<string, float> ConditionalModifiers { get; set; } = new(); // modifiers for the skill in SkillTrigger

    public string ConditionalTriggerType { get; set; } = string.Empty; // add stuff from WGCaptain.SkillTrigger if you deem necessary
}
