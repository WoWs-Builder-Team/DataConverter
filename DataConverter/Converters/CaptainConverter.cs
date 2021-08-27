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
        //jsonCaptainInput contains all Captains' informations
        //jsonSkillsInput contains all skills' tiers for all the classes
        public static Dictionary<string, Captain> ConvertCaptain(string captainJsonInput, string skillsJsonInput)
        {
            //create a List of our Objects
            Dictionary<string, Captain> captainList = new Dictionary<string, Captain>();

            //deserialize into an object
            var wgCaptain = JsonConvert.DeserializeObject<List<WGCaptain>>(captainJsonInput) ?? throw new InvalidOperationException();

            var skillsTiers = JsonConvert.DeserializeObject<SkillsTiers>(skillsJsonInput) ?? throw new InvalidOperationException();

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
                //initialize dictionaries for skills, skill's modifiers and skill's tiers
                Dictionary<string, Skill> skills = new Dictionary<string, Skill>();
                Dictionary<string, float> modifiers = new Dictionary<string, float>();
                Dictionary<string, float> conditionalModifiers = new Dictionary<string, float>();
                Dictionary<ShipClass, int> tiers = new Dictionary<ShipClass, int>();

                //iterate all captain's skills
                foreach (var currentWgSkill in currentWgCaptain.Skills)
                {
                    //start mapping
                    skill.CanBeLearned = currentWgSkill.Value.canBeLearned;
                    skill.IsEpic = currentWgSkill.Value.isEpic;
                    skill.SkillNumber = currentWgSkill.Value.skillType;

                    //map skill's tier for each class
                    if (skillsTiers.Cruiser.ContainsKey(skill.SkillNumber))
                    {
                        tiers.Add(ShipClass.Cruiser, skillsTiers.Cruiser[skill.SkillNumber]);
                    }
                    if (skillsTiers.Auxiliary.ContainsKey(skill.SkillNumber))
                    {
                        tiers.Add(ShipClass.Auxiliary, skillsTiers.Auxiliary[skill.SkillNumber]);
                    }
                    if (skillsTiers.Destroyer.ContainsKey(skill.SkillNumber))
                    {
                        tiers.Add(ShipClass.Destroyer, skillsTiers.Destroyer[skill.SkillNumber]);
                    }
                    if (skillsTiers.AirCarrier.ContainsKey(skill.SkillNumber))
                    {
                        tiers.Add(ShipClass.AirCarrier, skillsTiers.AirCarrier[skill.SkillNumber]);
                    }
                    if (skillsTiers.Submarine.ContainsKey(skill.SkillNumber))
                    {
                        tiers.Add(ShipClass.Submarine, skillsTiers.Submarine[skill.SkillNumber]);
                    }
                    if (skillsTiers.Battleship.ContainsKey(skill.SkillNumber))
                    {
                        tiers.Add(ShipClass.Battleship, skillsTiers.Battleship[skill.SkillNumber]);
                    }
                    skill.Tiers = tiers;
                    //list of the classes that can use the skill
                    skill.LearnableOn = new List<ShipClass>(tiers.Keys);

                    //collect all modifiers of the skill
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
