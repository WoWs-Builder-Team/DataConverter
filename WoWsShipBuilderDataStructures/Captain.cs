using System.Collections.Generic;

namespace WoWsShipBuilderDataStructures
{
    public class Captain
    {
        public long Id { get; set; }
        public string Index { get; set; }
        public string Name { get; set; }
        public bool HasSpecialSkills { get; set; }
        public Dictionary<string, Skill> Skills { get; set; }
    }

    public class Skill
    {
        public bool CanBeLearned { get; set; }
        public bool IsEpic { get; set; } //true if the skill has buffed modifier
        public int SkillNumber { get; set; }
        public List<ShipClass> LearnableOn { get; set; }
        public List<SkillPosition> Tiers { get; set; } //contains the tier of the skill for all the classes that can use it
        //modifiers for always on effects
        public Dictionary<string, dynamic> Modifiers { get; set; }
        //modifiers for the skill in SkillTrigger
        public Dictionary<string, float> ConditionalModifiers { get; set; }
        //add stuff from WGCaptain.SkillTrigger if you deem necessary
    }

    public class SkillPosition 
    {
        public int Tier { get; set; }
        public int XPosition { get; set; }
        public ShipClass ShipClass { get; set; }
    }  
}