using System.Collections.Generic;

namespace WoWsShipBuilder.DataStructures
{
    public class Captain
    {
        public long Id { get; set; }
        public string Index { get; set; }
        public string Name { get; set; }
        public bool HasSpecialSkills { get; set; }
        public Dictionary<string, Skill> Skills { get; set; }
        public Dictionary<string, UniqueSkill> UniqueSkills { get; set; }
        public Nation Nation { get; set; }
    }

    public class Skill
    {
        public bool CanBeLearned { get; set; }
        public bool IsEpic { get; set; } // true if the skill has buffed modifier
        public int SkillNumber { get; set; }
        public List<ShipClass> LearnableOn { get; set; }
        public List<SkillPosition> Tiers { get; set; } // contains the tier of the skill for all the classes that can use it
        public Dictionary<string, float> Modifiers { get; set; } // modifiers for always on effects
        public Dictionary<string, float> ConditionalModifiers { get; set; } // modifiers for the skill in SkillTrigger
        public string ConditionalTriggerType { get; set; } // add stuff from WGCaptain.SkillTrigger if you deem necessary
    }

    public class SkillPosition
    {
        public int Tier { get; set; }
        public int XPosition { get; set; }
        public ShipClass ShipClass { get; set; }
    }

    public class UniqueSkill
    {
        public Dictionary<string, UniqueSkillEffect> SkillEffects { get; set; } // dictionary of the effects and their names
        public int MaxTriggerNum { get; set; }
        public List<ShipClass> AllowedShips { get; set; }
        public string TriggerType { get; set; }
        public string TranslationId { get; set; }
    }

    public class UniqueSkillEffect
    {
        public bool IsPercent { get; set; }
        public int UniqueType { get; set; }
        public Dictionary<string, double> Modifiers { get; set; }
    }
}
