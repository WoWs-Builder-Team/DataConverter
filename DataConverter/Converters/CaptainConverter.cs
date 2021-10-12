using DataConverter.WGStructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
                Program.TranslationNames.Add(currentWgCaptain.name);
                //start mapping
                Captain captain = new Captain
                {
                    Id = currentWgCaptain.id,
                    Index = currentWgCaptain.index,
                    Name = currentWgCaptain.name,
                    HasSpecialSkills = false
                };

                //create object SKill
                //initialize dictionaries for skills and skill's tiers
                Dictionary<string, Skill> skills = new Dictionary<string, Skill>();
                //iterate all captain's skills
                foreach (var currentWgSkill in currentWgCaptain.Skills)
                {
                    Skill skill = new Skill();
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

                    List<SkillPosition> skillPositions = FindSkillPosition(skillsTiers, skill.SkillNumber);
                    tiers.AddRange(skillPositions);
                    classes.AddRange(skillPositions.Select(pos => pos.ShipClass).Distinct());

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
                        if (jtoken.Type == JTokenType.Float || jtoken.Type == JTokenType.Integer)
                        {
                            modifiers.Add(currentWgModifier.Key, jtoken.Value<float>());
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
                    skill.ConditionalTriggerType = currentWgSkill.Value.LogicTrigger.triggerType;
                    Program.TranslationNames.Add(skill.ConditionalTriggerType);
                    Program.TranslationNames.Add(GetSkillTranslationId(currentWgSkill.Key));
                    Program.TranslationNames.UnionWith(modifiers.Keys);
                    skills.Add(currentWgSkill.Key, skill);
                }

                //map skills into object captain
                captain.Skills = skills;

                //map captain's talents
                var uniqueSkills = new Dictionary<string, UniqueSkill>();
                foreach ((var currentUniqueSkillKey, var currentUniqueSkillValue) in currentWgCaptain.uniqueSkills)
                {
                    UniqueSkill uniqueSkill = new()
                    {
                        MaxTriggerNum = currentUniqueSkillValue.maxTriggerNum,
                        AllowedShips = currentUniqueSkillValue.triggerAllowedShips?.ToList<ShipClass>(),
                        TriggerType = currentUniqueSkillValue.triggerType
                    };

                    var skillEffects = new Dictionary<string, double>();
                    foreach ((var currentWgUniqueSkillEffectKey, var currentWgUniqueSkillEffectValue) in currentUniqueSkillValue.skillEffects)
                    {
                        if (currentWgUniqueSkillEffectKey.ToString().Contains("Unique"))
                        {
                            JObject jObject = (JObject)currentWgUniqueSkillEffectValue;
                            var values = jObject.ToObject<Dictionary<string, JToken>>();
                            foreach ((string key, JToken value) in values)
                            {
                                if (value.Type == JTokenType.Float || value.Type == JTokenType.Integer)
                                {
                                    skillEffects.Add($"{currentWgUniqueSkillEffectKey}_{key}", value.Value<double>());
                                }
                                else if (value.Type == JTokenType.Object)
                                {
                                    JObject anotherJObject = (JObject)value;
                                    var moreValues = anotherJObject.ToObject<Dictionary<string, double>>();
                                    bool isEqual = true;
                                    var first = moreValues.First().Value;
                                    foreach ((string anotherKey, double anotherValue) in moreValues)
                                    {
                                        if (anotherValue != first)
                                        {
                                            isEqual = false;
                                        }
                                    }
                                    if (isEqual)
                                    {
                                        skillEffects.Add($"{currentWgUniqueSkillEffectKey}_{key}", first);
                                    }
                                    else
                                    {
                                        foreach ((string anotherKey, double anotherValue) in moreValues)
                                        {
                                            skillEffects.Add($"{currentWgUniqueSkillEffectKey}_{key}_{anotherKey}", anotherValue);
                                        }
                                    }
                                }
                            }
                        }
                        uniqueSkill.SkillEffects = skillEffects;
                    }
                    uniqueSkills.Add(currentUniqueSkillKey, uniqueSkill);
                }
                captain.UniqueSkills = uniqueSkills;            
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

        private static List<SkillPosition> FindSkillPosition(SkillsTiers skillsTiers, int skillNumber)
        {
            var positions = new List<SkillPosition>();
            foreach ((ShipClass shipClass, List<SkillRow> skillRow) in skillsTiers.PositionsByClass)
            {
                for (var skillTier = 0; skillTier < skillRow.Count; skillTier++)
                {
                    List<int> skillsInRow = skillRow[skillTier].SkillGroups.SelectMany(group => group).ToList();
                    int skillIndex = skillsInRow.IndexOf(skillNumber);
                    if (skillIndex >= 0)
                    {
                        var position = new SkillPosition
                        {
                            ShipClass = shipClass,
                            XPosition = skillIndex,
                            Tier = skillTier,
                        };
                        positions.Add(position);
                    }
                }
            }

            return positions;
        }

        private static string GetSkillTranslationId(string skillName)
        {
            if (skillName == null)
            {
                throw new ArgumentNullException(nameof(skillName));
            }
            if (skillName.Length < 2)
            {
                return skillName;
            }
            var sb = new StringBuilder();
            sb.Append(char.ToLowerInvariant(skillName[0]));
            for (int i = 1; i < skillName.Length; ++i)
            {
                char c = skillName[i];
                if (char.IsUpper(c))
                {
                    sb.Append('_');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}