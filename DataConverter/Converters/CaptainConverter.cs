using DataConverter.WGStructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using WoWsShipBuilderDataStructures;

namespace DataConverter.Converters
{
    public static class CaptainConverter
    {
        /// <summary>
        /// Converter method that transforms a <see cref="WGCaptain"/> object into a <see cref="Captain"/> object.
        /// </summary>
        /// <param name="captainJsonInput">The file content of captain data extracted from game params.</param>
        /// <param name="skillsJsonInput">The file content of the embedded captain data file.</param>
        /// <returns>A dictionary mapping an ID to a <see cref="Captain"/> object that contains the transformed data based on WGs data.</returns>
        /// <exception cref="InvalidOperationException">Occurs if the provided data cannot be processed.</exception>
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

                //create object SKill
                Skill skill = new Skill();
                //initialize dictionaries for skills and skill's tiers
                Dictionary<string, Skill> skills = new Dictionary<string, Skill>();
                //iterate all captain's skills
                foreach (var currentWgSkill in currentWgCaptain.Skills)
                {
                    //start mapping
                    skill.CanBeLearned = currentWgSkill.Value.canBeLearned;
                    skill.IsEpic = currentWgSkill.Value.isEpic;
                    skill.SkillNumber = currentWgSkill.Value.skillType;

                    //check if there are skills with special vaules
                    if (skill.IsEpic)
                    {
                        captain.HasSpecialSkills = true;
                    }
                    //initialize lists for skill's tiers and classes 
                    List<SkillPosition> tiers = new();
                    List<ShipClass> classes = new();

                    //map skill's position in the skilltree of each class
                    foreach (var tier in skillsTiers.Cruiser)
                    {
                        if (tier.Value.Contains(skill.SkillNumber))
                        {
                            var position = new SkillPosition
                            {
                                ShipClass = ShipClass.Cruiser,
                                XPosition = tier.Value.IndexOf(skill.SkillNumber),
                                Tier = tier.Key
                            };
                            tiers.Add(position);
                            classes.Add(position.ShipClass);
                        }
                    }

                    foreach (var tier in skillsTiers.Auxiliary)
                    {
                        if (tier.Value.Contains(skill.SkillNumber))
                        {
                            var position = new SkillPosition
                            {
                                ShipClass = ShipClass.Auxiliary,
                                XPosition = tier.Value.IndexOf(skill.SkillNumber),
                                Tier = tier.Key
                            };
                            tiers.Add(position);
                            classes.Add(position.ShipClass);
                        }
                    }

                    foreach (var tier in skillsTiers.Destroyer)
                    {
                        if (tier.Value.Contains(skill.SkillNumber))
                        {
                            var position = new SkillPosition
                            {
                                ShipClass = ShipClass.Destroyer,
                                XPosition = tier.Value.IndexOf(skill.SkillNumber),
                                Tier = tier.Key
                            };
                            tiers.Add(position);
                            classes.Add(position.ShipClass);
                        }
                    }

                    foreach (var tier in skillsTiers.AirCarrier)
                    {
                        if (tier.Value.Contains(skill.SkillNumber))
                        {
                            var position = new SkillPosition
                            {
                                ShipClass = ShipClass.AirCarrier,
                                XPosition = tier.Value.IndexOf(skill.SkillNumber),
                                Tier = tier.Key
                            };
                            tiers.Add(position);
                            classes.Add(position.ShipClass);
                        }
                    }

                    foreach (var tier in skillsTiers.Submarine)
                    {
                        if (tier.Value.Contains(skill.SkillNumber))
                        {
                            var position = new SkillPosition
                            {
                                ShipClass = ShipClass.Submarine,
                                XPosition = tier.Value.IndexOf(skill.SkillNumber),
                                Tier = tier.Key
                            };
                            tiers.Add(position);
                            classes.Add(position.ShipClass);
                        }
                    }

                    foreach (var tier in skillsTiers.Battleship)
                    {
                        if (tier.Value.Contains(skill.SkillNumber))
                        {
                            var position = new SkillPosition
                            {
                                ShipClass = ShipClass.Battleship,
                                XPosition = tier.Value.IndexOf(skill.SkillNumber),
                                Tier = tier.Key
                            };
                            tiers.Add(position);
                            classes.Add(position.ShipClass);
                        }
                    }

                    skill.Tiers = tiers;
                    //list of the classes that can use the skill
                    skill.LearnableOn = classes;
                    //initialize dictionaries skill's modifiers
                    Dictionary<string, float> modifiers = new Dictionary<string, float>();
                    Dictionary<string, float> conditionalModifiers = new Dictionary<string, float>();
                    //collect all modifiers of the skill
                    foreach (var currentWgModifier in currentWgSkill.Value.modifiers)
                    {
                        JToken jtoken = currentWgModifier.Value;
                        if (jtoken.Type == JTokenType.Float)
                        {
                            modifiers.Add(currentWgModifier.Key, jtoken.Value<float>());
                        }
                        else if (jtoken.Type == JTokenType.Integer)
                        {
                            modifiers.Add(currentWgModifier.Key, jtoken.Value<int>());
                        }
                        else if (jtoken.Type == JTokenType.Object)
                        {
                            JObject jObject = (JObject)jtoken;
                            var values = jObject.ToObject<Dictionary<string, float>>();
                            bool isEqual = true;
                            var first = values.First().Value;
                            foreach ((string key, float value) in values)
                            {
                                if (value != first)
                                {
                                    isEqual = false;
                                }
                            }
                            if (isEqual)
                            {
                                modifiers.Add(currentWgModifier.Key, first);
                            }
                            else
                            {
                                foreach ((string key, float value) in values)
                                {
                                    modifiers.Add($"{currentWgModifier.Key}_{key}", value);
                                }
                            }
                        }
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

        /// <summary>
        /// Utility method to load the skill data from the embedded SKILLS_BY_TIER.json file.
        /// </summary>
        /// <returns>The file content of the embedded file.</returns>
        /// <exception cref="FileNotFoundException">Occurs if the embedded resource does not exist.</exception>
        public static string LoadEmbeddedSkillData()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DataConverter.JsonData.SKILLS_BY_TIER.json") ??
                               throw new FileNotFoundException("Unable to locate embedded captain skill data.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}