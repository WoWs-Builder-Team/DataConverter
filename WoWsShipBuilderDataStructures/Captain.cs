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
        public Dictionary<string, UniqueSkill> UniqueSkills { get; set; }
    }

    public class Skill
    {
        public bool CanBeLearned { get; set; }
        public bool IsEpic { get; set; } //true if the skill has buffed modifier
        public int SkillNumber { get; set; }
        public List<ShipClass> LearnableOn { get; set; }
        public List<SkillPosition> Tiers { get; set; } //contains the tier of the skill for all the classes that can use it
        //modifiers for always on effects
        public Dictionary<string, float> Modifiers { get; set; }
        //modifiers for the skill in SkillTrigger
        public Dictionary<string, float> ConditionalModifiers { get; set; }
        //add stuff from WGCaptain.SkillTrigger if you deem necessary
        public string ConditionalTriggerType { get; set; }
    }

    public class SkillPosition 
    {
        public int Tier { get; set; }
        public int XPosition { get; set; }
        public ShipClass ShipClass { get; set; }
    }
    public class UniqueSkill
    {
        public Dictionary<string, double> SkillEffects { get; set; } // value is actually Dictionary<string, object>, process in converter
        public int MaxTriggerNum { get; set; }
        public List<ShipClass> AllowedShips { get; set; }
        public string TriggerType { get; set; }
    }
}