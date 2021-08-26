using DataConverter.WGStructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WoWsShipBuilderDataStructures;

namespace DataConverter.Converters
{
    public static class CaptainConverter
    {
        //convert the list of captains from WG to our list of Captains
        public static Dictionary<string, Captain> ConvertCaptain(string jsonInput)
        {
            //create a List of our Objects
            Dictionary<string, Captain> captainList = new Dictionary<string, Captain>();

            //deserialize into an object
            var wgCaptain = JsonConvert.DeserializeObject<List<WGCaptain>>(jsonInput) ?? throw new InvalidOperationException();

            //iterate over the entire list to convert everything
            foreach (var currentWgCaptain in wgCaptain)
            {
                //start mapping
                Captain captain = new Captain
                {
                    Id = currentWgCaptain.id,
                    Index = currentWgCaptain.index,
                    Name = currentWgCaptain.name,
                    HasSpecialSkills = false
                };

                //check if there are skills with special vaules 
                if (currentWgCaptain.CrewPersonality.tags.Contains("upperks"))
                {
                    captain.HasSpecialSkills = true;
                }

                //create object SKill
                Skill skill = new Skill();
                //initialize dictionaries for skills and skill's modifiers
                Dictionary<string, Skill> skills = new Dictionary<string, Skill>();
                Dictionary<string, float> modifiers = new Dictionary<string, float>();
                Dictionary<string, float> conditionalModifiers = new Dictionary<string, float>();

                //iterate all captain's skills
                foreach (var currentWgSkill in currentWgCaptain.Skills)
                {
                    //start mapping
                    skill.CanBeLearned = currentWgSkill.Value.canBeLearned;
                    skill.IsEpic = currentWgSkill.Value.isEpic;
                    skill.SkillNumber = currentWgSkill.Value.skillType;

                    //collect all skill's modifiers
                    foreach (var currentWgModifier in currentWgSkill.Value.modifiers)
                    {
                        modifiers.Add(currentWgModifier.Key, (float)currentWgModifier.Value);
                    }
                    skill.Modifiers = modifiers;

                    //collect all skill's modifiers with trigger condition
                    var wgConditionalModifiers = currentWgSkill.Value.LogicTrigger.modifiers;
                    if (wgConditionalModifiers.Count > 0)
                    {
                        foreach (var currentWgConditionalModifier in wgConditionalModifiers)
                        {
                            conditionalModifiers.Add(currentWgConditionalModifier.Key, currentWgConditionalModifier.Value);
                        }
                    }
                    skill.ConditionalModifiers = conditionalModifiers;

                    skills.Add(currentWgSkill.Key, skill);
                }
                //map skills into object captain
                captain.Skills = skills;
                //dictionary with captain's name as key
                captainList.Add(captain.Name, captain);
            }

            return captainList;
        }
    }
}
