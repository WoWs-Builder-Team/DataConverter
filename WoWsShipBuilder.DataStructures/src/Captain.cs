using System.Collections.Generic;

namespace WoWsShipBuilder.DataStructures
{
    public sealed record Captain
    {
        public long Id { get; init; }
        public string Index { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public bool HasSpecialSkills { get; set; }
        public Dictionary<string, Skill> Skills { get; set; } = new();
        public Dictionary<string, UniqueSkill> UniqueSkills { get; set; } = new();
        public Nation Nation { get; init; }
    }

    public sealed record Skill
    {
        public bool CanBeLearned { get; init; }
        public bool IsEpic { get; init; } //true if the skill has buffed modifier
        public int SkillNumber { get; init; }
        public List<ShipClass> LearnableOn { get; set; } = new();
        public List<SkillPosition> Tiers { get; set; } = new(); //contains the tier of the skill for all the classes that can use it
        //modifiers for always on effects
        public Dictionary<string, float> Modifiers { get; set; } = new();
        //modifiers for the skill in SkillTrigger
        public Dictionary<string, float> ConditionalModifiers { get; set; } = new();
        //add stuff from WGCaptain.SkillTrigger if you deem necessary
        public string ConditionalTriggerType { get; set; }  = string.Empty;
    }

    public sealed record SkillPosition(int Tier, int XPosition, ShipClass ShipClass);
    
    public sealed record UniqueSkill
    {
        public Dictionary<string, double> SkillEffects { get; set; } = new(); // value is actually Dictionary<string, object>, process in converter
        public int MaxTriggerNum { get; init; }
        public List<ShipClass>? AllowedShips { get; init; }
        public string TriggerType { get; init; } = string.Empty;
    }
}