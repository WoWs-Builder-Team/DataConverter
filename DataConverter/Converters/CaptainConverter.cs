using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DataConverter.Data;
using GameParamsExtractor.WGStructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures;
using WowsShipBuilder.GameParamsExtractor.WGStructure;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Captain;

namespace DataConverter.Converters;

public static class CaptainConverter
{
    /// <summary>
    /// Converter method that transforms a <see cref="WgCaptain"/> object into a <see cref="Captain"/> object.
    /// </summary>
    /// <param name="wgCaptain">The file content of captain data extracted from game params.</param>
    /// <param name="skillsJsonInput">The file content of the embedded captain data file.</param>
    /// <param name="isCommon">If the file is the Common one.</param>
    /// <returns>A dictionary mapping an ID to a <see cref="Captain"/> object that contains the transformed data based on WGs data.</returns>
    /// <exception cref="InvalidOperationException">Occurs if the provided data cannot be processed.</exception>
    public static Dictionary<string, Captain> ConvertCaptain(IEnumerable<WgCaptain> wgCaptain, string skillsJsonInput, bool isCommon)
    {
        //create a List of our Objects
        Dictionary<string, Captain> captainList = new Dictionary<string, Captain>();

        //deserialize into an object
        var skillsTiers = JsonConvert.DeserializeObject<SkillsTiers>(skillsJsonInput) ?? throw new InvalidOperationException();

        bool addedDefault = false;

        //order the list for safety during common default captain choice
        wgCaptain = wgCaptain.OrderBy(x => x.Index);

        //iterate over the entire list to convert everything

        foreach (var currentWgCaptain in wgCaptain)
        {
            var tags = currentWgCaptain.CrewPersonality.Tags;
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
            if (tags is { Count: > 0 } && !tags.Contains("upperks") && !tags.Contains("talants"))
            {
                continue;
            }
            string name = currentWgCaptain.CrewPersonality.PersonName;
            if (string.IsNullOrEmpty(name))
            {
                name = "Default";
            }

            DataCache.TranslationNames.Add(name);
            //start mapping
            Captain captain = new Captain
            {
                Id = currentWgCaptain.Id,
                Index = currentWgCaptain.Index,
                Name = name,
                HasSpecialSkills = false,
                Nation = Enum.Parse<Nation>(currentWgCaptain.typeinfo.nation.Replace("_", string.Empty), true),
            };

            //create object SKill
            //initialize dictionaries for skills and skill's tiers
            Dictionary<string, Skill> skills = new Dictionary<string, Skill>();
            //iterate all captain's skills
            foreach (var currentWgSkill in currentWgCaptain.Skills)
            {
                var skill = ProcessSkill(currentWgSkill, captain, skillsTiers);
                skills.Add(currentWgSkill.Key, skill);
            }

            //map skills into object captain
            captain.Skills = skills;

            //map captain's talents
            //iterate over the various skills from wg data
            var uniqueSkillDictionary = ProcessUniqueSkills(currentWgCaptain, captain);
            captain.UniqueSkills = uniqueSkillDictionary;
            //dictionary with captain's name as key
            captainList.Add(captain.Name, captain);
        }

        return captainList;
    }

    private static Dictionary<string, UniqueSkill> ProcessUniqueSkills(WgCaptain currentWgCaptain, Captain captain)
    {
        var skills = new Dictionary<string, UniqueSkill>();
        foreach (var (currentUniqueSkillKey, currentUniqueSkillValue) in currentWgCaptain.UniqueSkills)
        {
            //create our talent data
            UniqueSkill uniqueSkill = new()
            {
                MaxTriggerNum = currentUniqueSkillValue.MaxTriggerNum,
                AllowedShips = currentUniqueSkillValue.TriggerAllowedShips?.ToList(),
                TriggerType = currentUniqueSkillValue.TriggerType,
            };

            //initialize an empty dictionary for effect name and effect modifiers/stats.
            var skillEffectDictionary = new Dictionary<string, UniqueSkillEffect>();

            //uniqueIds for translation key
            var uniqueIds = new List<int>();

            //iterate through the various fields
            foreach (var (currentWgUniqueSkillEffectKey, currentWgUniqueSkillEffectValue) in currentUniqueSkillValue.SkillEffects)
            {
                //create the skill effect object
                var skillEffect = new UniqueSkillEffect();

                // take into account only the properties containing "unique", that are the talent effects.
                if (currentWgUniqueSkillEffectKey.Contains("Unique"))
                {
                    //create a modifiers dictionary for the current effect
                    var effectsModifiers = new Dictionary<string, float>();

                    var jObject = (JObject)currentWgUniqueSkillEffectValue;
                    var values = jObject.ToObject<Dictionary<string, JToken>>()!;

                    //iterate through the entire object fields
                    foreach ((string key, var value) in values)
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
                        else if (value.Type is JTokenType.Float or JTokenType.Integer)
                        {
                            //if it's a float with a value of 1, then it's probably a modifier that keep the value the same.
                            if (value.Type == JTokenType.Float && Math.Abs(value.Value<float>() - 1f) < 0.001)
                            {
                                continue;
                            }

                            effectsModifiers.Add($"{key}", value.Value<float>());
                            DataCache.TranslationNames.Add(key);
                        }
                        else if (value.Type == JTokenType.Object)
                        {
                            JObject jObjectModifier = (JObject)value;
                            var modifiers = jObjectModifier.ToObject<Dictionary<string, float>>();
                            bool allEquals = modifiers!.Values.Distinct().Count() == 1;
                            if (allEquals)
                            {
                                effectsModifiers.Add($"{key}", modifiers.First().Value);
                                DataCache.TranslationNames.Add(key);
                            }
                            else
                            {
                                foreach (var (modifierName, modifierValue) in modifiers)
                                {
                                    effectsModifiers.Add($"{key}_{modifierName}", modifierValue);
                                    DataCache.TranslationNames.Add($"{key}_{modifierName}");
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
            DataCache.TranslationNames.Add(translationId);

            //add the unique skill to the dictionary
            uniqueSkill.SkillEffects = skillEffectDictionary;
            skills.Add(currentUniqueSkillKey, uniqueSkill);
        }

        return skills;
    }

    private static Skill ProcessSkill(KeyValuePair<string, WgSkill> currentWgSkill, Captain captain, SkillsTiers skillsTiers)
    {
        var skill = new Skill
        {
            //start mapping
            CanBeLearned = currentWgSkill.Value.CanBeLearned,
            IsEpic = currentWgSkill.Value.IsEpic,
            SkillNumber = currentWgSkill.Value.SkillType,
        };

        //check if there are skills with special values
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

        //collect all modifiers of the skill
        Dictionary<string, float> modifiers = ProcessSkillModifiers(currentWgSkill.Value.Modifiers);
        skill.Modifiers = modifiers;

        //collect all skill's modifiers with trigger condition
        Dictionary<string, JToken>? wgConditionalModifiers = currentWgSkill.Value.LogicTrigger.Modifiers;
        Dictionary<string, float> conditionalModifiers = ProcessSkillModifiers(wgConditionalModifiers);

        skill.ConditionalModifiers = conditionalModifiers;
        skill.ConditionalTriggerType = currentWgSkill.Value.LogicTrigger.TriggerType;
        DataCache.TranslationNames.Add(skill.ConditionalTriggerType);
        DataCache.TranslationNames.UnionWith(skill.ConditionalModifiers.Keys);
        DataCache.TranslationNames.Add(GetSkillTranslationId(currentWgSkill.Key));
        DataCache.TranslationNames.UnionWith(modifiers.Keys);
        return skill;
    }

    private static Dictionary<string, float> ProcessSkillModifiers(Dictionary<string, JToken> skillModifiers)
    {
        Dictionary<string, float> modifiers = new();
        foreach ((string? s, var token) in skillModifiers)
        {
            if (token.Type is JTokenType.Float or JTokenType.Integer)
            {
                modifiers.Add(s, token.Value<float>());
            }
            else if (token.Type == JTokenType.Object)
            {
                JObject jObject = (JObject)token;
                var values = jObject.ToObject<Dictionary<string, float>>()!;
                bool isEqual = true;
                var first = values.First().Value;
                foreach ((string _, float value) in values)
                {
                    if (Math.Abs(value - first) > 0.001)
                    {
                        isEqual = false;
                    }
                }

                if (isEqual)
                {
                    modifiers.Add(s, first);
                }
                else
                {
                    foreach ((string key, float value) in values)
                    {
                        modifiers.Add($"{s}_{key}", value);
                    }
                }
            }
        }

        return modifiers;
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
