using System.Collections.Generic;

namespace WoWsShipBuilderDataStructures
{
    public class Captain
    {
        public long Id { get; set; }
        public string Index { get; set; }
        public string Name { get; set; }
        public Dictionary<string, Skill> Skills { get; set; }
    }


    public class Skill
    {
        public bool CanBeLearned { get; set; }
        public bool IsEpic { get; set; }
        public int SkillNumber { get; set; }

        public ShipClass LearnableOn { get; set; }

        //modifiers for always on effects
        public Dictionary<string, float> Modifiers { get; set; }

        //modifiers for the skill in SkillTrigger
        public Dictionary<string, float> ConditionalModifiers { get; set; }
        //add stuff from WGCaptain.SkillTrigger if you deem necessary
    }
}