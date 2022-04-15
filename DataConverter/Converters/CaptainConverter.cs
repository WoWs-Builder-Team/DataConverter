using GameParamsExtractor.WGStructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using WoWsShipBuilder.DataStructures;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace DataConverter.Converters
{
    public static class CaptainConverter
    {
        /// <summary>
        /// Converter method that transforms a <see cref="WGCaptain"/> object into a <see cref="Captain"/> object.
        /// </summary>
        /// <param name="wgCaptain">The file content of captain data extracted from game params.</param>
        /// <param name="skillsJsonInput">The file content of the embedded captain data file.</param>
        /// <param name="isCommon">If the file is the Common one.</param>
        /// <returns>A dictionary mapping an ID to a <see cref="Captain"/> object that contains the transformed data based on WGs data.</returns>
        /// <exception cref="InvalidOperationException">Occurs if the provided data cannot be processed.</exception>
        public static Dictionary<string, Captain> ConvertCaptain(IEnumerable<WGCaptain> wgCaptain, string skillsJsonInput, bool isCommon)
        {
            //create a List of our Objects
            Dictionary<string, Captain> captainList = new Dictionary<string, Captain>();

            //deserialize into an object
            var skillsTiers = JsonConvert.DeserializeObject<SkillsTiers>(skillsJsonInput) ?? throw new InvalidOperationException();

            bool addedDefault = false;

            //order the list for safety during common default captain choice
            wgCaptain = wgCaptain.ToList().OrderBy(x => x.index);

            //iterate over the entire list to convert everything

            foreach (var currentWgCaptain in wgCaptain)
            {
                var tags = currentWgCaptain.CrewPersonality.tags;
                // if no tags and we are not processing Common, skip the captain. A captain with no tags is the default captain of the nation, a copy of the one in Common.
                if ((tags == null || tags.Count == 0) && !isCommon)
                {
                    continue;
                }
                // if no tags and we already added the default captain in Common, skip all the next. They are copy of each other.
                if (isCommon && (tags == null || tags.Count == 0) && addedDefault)
                {
                    continue;
                }
                // if no tags and we haven't added the default captain in Common, set the flag to true to skip future ones.
                if (isCommon && (tags == null || tags.Count == 0) && !addedDefault)
                {
                    addedDefault = true;
                }
                // finally, if the tags are not null or empty and are different from upperks (boosted skill) or talants (wg doesn't know how to spell talent, it's the legendary captain)
                // then skip them. No need to have all the cosmetic captain that are the same of the default one displayed.
                if ((tags != null && tags.Count > 0) && (!tags.Contains("upperks") && !tags.Contains("talants")))
                {
                    continue;
                }
                string name = currentWgCaptain.CrewPersonality.personName;
                if (string.IsNullOrEmpty(name))
                {
                    name = "Default";
                }

                Program.TranslationNames.Add(name);
                //start mapping
                Captain captain = new Captain
                {
                    Id = currentWgCaptain.id,
                    Index = currentWgCaptain.index,
                    Name = name,
                    HasSpecialSkills = false,
                    Nation = ConvertNationString(currentWgCaptain.typeinfo.nation),
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
                            var values = jObject.ToObject<Dictionary<string, float>>()!;
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

                //Create dictionary for unique skills, aka talents
                var uniqueSkillDictionary = new Dictionary<string, UniqueSkill>();
                //iterate over the various skills from wg data
                foreach ((var currentUniqueSkillKey, var currentUniqueSkillValue) in currentWgCaptain.uniqueSkills)
                {
                    //create our talent data
                    UniqueSkill uniqueSkill = new()
                    {
                        MaxTriggerNum = currentUniqueSkillValue.maxTriggerNum,
                        AllowedShips = currentUniqueSkillValue.triggerAllowedShips?.ToList<ShipClass>(),
                        TriggerType = currentUniqueSkillValue.triggerType,
                    };

                    //initialize an empty dictionary for effect name and effect modifiers/stats.
                    var skillEffectDictionary = new Dictionary<string, UniqueSkillEffect>();

                    //uniqueIds for translation key
                    var uniqueIds = new List<int>();

                    //iterate through the various fields
                    foreach ((var currentWgUniqueSkillEffectKey, var currentWgUniqueSkillEffectValue) in currentUniqueSkillValue.skillEffects)
                    {
                        //create the skill effect object
                        var skillEffect = new UniqueSkillEffect();

                        // take into account only the properties containing "unique", that are the talent effects.
                        if (currentWgUniqueSkillEffectKey.Contains("Unique"))
                        {
                            //create a modifiers dictionary for the current effect
                            var effectsModifiers = new Dictionary<string, float>();

                            JObject jObject = (JObject)currentWgUniqueSkillEffectValue;
                            var values = jObject.ToObject<Dictionary<string, JToken>>()!;
                            //iterate through the entire object fields
                            foreach ((string key, JToken value) in values)
                            {
                                //if the field is "uniqueType", i'll save it in the skillEffect, it's not a modifier
                                if (key.Contains("uniqueType"))
                                {
                                    skillEffect.UniqueType = value.Value<int>();
                                    uniqueIds.Add(value.Value<int>());
                                }
                                //else if the field is "percentTalent", i'll save it in the skillEffect, it's not a modifier
                                else if (key.Contains("percentTalent"))
                                {
                                    skillEffect.IsPercent = value.Value<bool>();
                                }
                                //else if the field is a number, it's a modifier. save it in the dictionary of modifiers
                                else if (value.Type == JTokenType.Float || value.Type == JTokenType.Integer)
                                {
                                    //if it's a float with a value of 1, then it's probably a modifier that keep the value the same.
                                    if (value.Type == JTokenType.Float && value.Value<float>() == 1f)
                                    {
                                        continue;
                                    }
                                    effectsModifiers.Add($"{key}", value.Value<float>());
                                    Program.TranslationNames.Add(key);
                                }
                                else if (value.Type == JTokenType.Object)
                                {
                                    JObject jObjectModifier = (JObject)value;
                                    var modifiers = jObjectModifier.ToObject<Dictionary<string, float>>();
                                    bool allEquals = modifiers!.Values.Distinct().Count() == 1;
                                    if (allEquals)
                                    {
                                        effectsModifiers.Add($"{key}", modifiers.First().Value);
                                        Program.TranslationNames.Add(key);
                                    }
                                    else
                                    {
                                        foreach ((var modifierName, var modifierValue) in modifiers)
                                        {
                                            effectsModifiers.Add($"{key}_{modifierName}", modifierValue);
                                            Program.TranslationNames.Add($"{key}_{modifierName}");
                                        }
                                    }
                                }
                            }
                            //after iterating through the entire thing, put the modifiers in the skill effect
                            skillEffect.Modifiers = effectsModifiers;
                        }
                        //value is not an actual modifier/effect, skip it
                        else
                        {
                            continue;
                        }
                        //add the current skill effect name and data t the dictionary
                        skillEffectDictionary.Add(currentWgUniqueSkillEffectKey, skillEffect);
                    }
                    //calculate the localization string
                    uniqueIds.Sort();
                    var uniqueIdsString = string.Join("_", uniqueIds);
                    var translationId = $"TALENT_{captain.Index}_{uniqueSkill.TriggerType}_{uniqueIdsString}";
                    uniqueSkill.TranslationId = translationId;
                    Program.TranslationNames.Add(translationId);

                    //add the unique skill to the dictionary
                    uniqueSkill.SkillEffects = skillEffectDictionary;
                    uniqueSkillDictionary.Add(currentUniqueSkillKey, uniqueSkill);
                }
                captain.UniqueSkills = uniqueSkillDictionary;
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

        private static Nation ConvertNationString(string wgNation)
        {
            return wgNation.Replace("_", "") switch
            {
                "USA" => Nation.Usa,
                { } any => Enum.Parse<Nation>(any),
            };
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
