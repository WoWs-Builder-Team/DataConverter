using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using DataConverter.Data;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Captain;
using WoWsShipBuilder.DataStructures.Modifiers;
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
    /// <param name="modifiersDictionary">The dictionary created from the file content of the embedded modifiers file.</param>
    /// <returns>A dictionary mapping an ID to a <see cref="Captain"/> object that contains the transformed data based on WGs data.</returns>
    /// <exception cref="InvalidOperationException">Occurs if the provided data cannot be processed.</exception>
    public static Dictionary<string, Captain> ConvertCaptain(IEnumerable<WgCaptain> wgCaptain, string skillsJsonInput, bool isCommon, Dictionary<string, Modifier> modifiersDictionary)
    {
        //create a List of our Objects
        Dictionary<string, Captain> captainList = new Dictionary<string, Captain>();

        //deserialize into an object
        var skillsTiers = JsonSerializer.Deserialize<SkillsTiers>(skillsJsonInput, Constants.SerializerOptions) ?? throw new InvalidOperationException();

        bool addedDefault = false;

        //order the list for safety during common default captain choice
        wgCaptain = wgCaptain.OrderBy(x => x.Index);

        //iterate over the entire list to convert everything

        foreach (var currentWgCaptain in wgCaptain)
        {
            var tags = currentWgCaptain.CrewPersonality.Tags;

            // if no tags and we are not processing Common, skip the captain. A captain with no tags is the default captain of the nation, a copy of the one in Common.
            if (tags.Count == 0 && !isCommon)
            {
                continue;
            }

            // if no tags and we already added the default captain in Common, skip all the next. They are copy of each other.
            if (isCommon && tags.Count == 0 && addedDefault)
            {
                continue;
            }

            // if no tags and we haven't added the default captain in Common, set the flag to true to skip future ones.
            if (isCommon && tags.Count == 0 && !addedDefault)
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

            //create object SKill
            //initialize dictionaries for skills and skill's tiers
            Dictionary<string, Skill> skills = new Dictionary<string, Skill>();

            //iterate all captain's skills
            foreach (var currentWgSkill in currentWgCaptain.Skills)
            {
                var skill = ProcessSkill(currentWgSkill, skillsTiers, modifiersDictionary);
                skills.Add(currentWgSkill.Key, skill);
            }

            //map captain's talents
            //iterate over the various skills from wg data
            var uniqueSkillDictionary = ProcessUniqueSkills(currentWgCaptain, currentWgCaptain.Index, modifiersDictionary);

            //start mapping
            Captain captain = new Captain
            {
                Id = currentWgCaptain.Id,
                Index = currentWgCaptain.Index,
                Name = name,
                HasSpecialSkills = skills.Any(skillEntry => skillEntry.Value.IsEpic),
                Nation = Enum.Parse<Nation>(currentWgCaptain.TypeInfo.Nation.Replace("_", string.Empty), true),
                Skills = skills.ToImmutableDictionary(),
                UniqueSkills = uniqueSkillDictionary.ToImmutableDictionary(),
            };

            //dictionary with captain's name as key
            captainList.Add(captain.Name, captain);
        }

        return captainList;
    }

    private static Dictionary<string, UniqueSkill> ProcessUniqueSkills(WgCaptain currentWgCaptain, string captainIndex, Dictionary<string, Modifier> modifierDictionary)
    {
        var skills = new Dictionary<string, UniqueSkill>();
        foreach (var (currentUniqueSkillKey, currentUniqueSkillValue) in currentWgCaptain.UniqueSkills)
        {
            //initialize an empty dictionary for effect name and effect modifiers/stats.
            var skillEffectDictionary = new Dictionary<string, UniqueSkillEffect>();

            //uniqueIds for translation key
            var uniqueIds = new List<int>();

            //iterate through the various fields
            foreach (var (currentWgUniqueSkillEffectKey, currentWgUniqueSkillEffectValue) in currentUniqueSkillValue.SkillEffects)
            {
                //create the skill effect object
                var skillEffect = new UniqueSkillEffectBuilder();

                // take into account only the properties containing "unique", that are the talent effects.
                if (currentWgUniqueSkillEffectKey.Contains("Unique"))
                {
                    //create a modifiers dictionary for the current effect
                    var effectsModifiers = new List<Modifier>();

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
                            if (value.Type == JTokenType.Float && Math.Abs(value.Value<float>() - 1f) < Constants.Tolerance)
                            {
                                continue;
                            }

                            var fixedKey = key.Equals("regenerationHPSpeed") ? "captain_" + currentWgCaptain.Name + "_" + key : key;
                            modifierDictionary.TryGetValue(fixedKey, out Modifier? modifierData);
                            effectsModifiers.Add(new Modifier(fixedKey, value.Value<float>(), $"Skill_{currentUniqueSkillKey}", modifierData));
                            DataCache.TranslationNames.Add(key);
                        }
                        else if (value.Type == JTokenType.Object)
                        {
                            JObject jObjectModifier = (JObject)value;
                            var modifiers = jObjectModifier.ToObject<Dictionary<string, float>>();
                            bool allEquals = modifiers!.Values.Distinct().Count() == 1;
                            if (allEquals)
                            {
                                modifierDictionary.TryGetValue(key, out Modifier? modifierData);
                                effectsModifiers.Add(new Modifier(key, modifiers.First().Value, $"Skill_{currentUniqueSkillKey}", modifierData));
                                DataCache.TranslationNames.Add(key);
                            }
                            else
                            {
                                foreach (var (modifierName, modifierValue) in modifiers)
                                {
                                    modifierDictionary.TryGetValue($"{key}_{modifierName}", out Modifier? modifierData);
                                    effectsModifiers.Add(new Modifier($"{key}_{modifierName}", modifierValue, $"Skill_{currentUniqueSkillKey}", modifierData));
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
                skillEffectDictionary.Add(currentWgUniqueSkillEffectKey, skillEffect.ToUniqueSkillEffect());
            }

            //calculate the localization string
            uniqueIds.Sort();
            var uniqueIdsString = string.Join("_", uniqueIds);
            var translationId = $"TALENT_{captainIndex}_{currentUniqueSkillValue.TriggerType}_{uniqueIdsString}";
            DataCache.TranslationNames.Add(translationId);

            //create our talent data
            UniqueSkill uniqueSkill = new()
            {
                MaxTriggerNum = currentUniqueSkillValue.MaxTriggerNum,
                AllowedShips = currentUniqueSkillValue.TriggerAllowedShips.ToImmutableArray(),
                TriggerType = currentUniqueSkillValue.TriggerType,
                TranslationId = translationId,
                SkillEffects = skillEffectDictionary.ToImmutableDictionary(),
            };

            skills.Add(currentUniqueSkillKey, uniqueSkill);
        }

        return skills;
    }

    private static Skill ProcessSkill(KeyValuePair<string, WgSkill> currentWgSkill, SkillsTiers skillsTiers, Dictionary<string, Modifier> modifiersDictionary)
    {
        var skill = new SkillBuilder()
        {
            //start mapping
            CanBeLearned = currentWgSkill.Value.CanBeLearned,
            IsEpic = currentWgSkill.Value.IsEpic,
            SkillNumber = currentWgSkill.Value.SkillType,
        };

        //initialize lists for skill's tiers and classes
        List<SkillPosition> tiers = new();
        List<ShipClass> classes = new();

        List<SkillPosition> skillPositions = FindSkillPosition(skillsTiers, skill.SkillNumber);
        tiers.AddRange(skillPositions);
        classes.AddRange(skillPositions.Select(pos => pos.ShipClass).Distinct());

        skill.Tiers = tiers.ToImmutableArray();

        //list of the classes that can use the skill
        skill.LearnableOn = classes.ToImmutableArray();

        //collect all modifiers of the skill
        List<Modifier> modifiers = ProcessSkillModifiers(currentWgSkill.Value.Modifiers, modifiersDictionary);
        skill.Modifiers = modifiers.ToImmutableList();

        //collect all skill's modifiers with trigger condition, 44 = IRPR, 81 = Furious
        var conditionalModifierGroups = new List<ConditionalModifierGroup>();
        Dictionary<string, JToken> wgConditionalModifiers = currentWgSkill.Value.LogicTrigger.Modifiers;
        if (currentWgSkill.Value.SkillType == 44 && DataCache.CurrentVersion.MainVersion >= Version.Parse("0.12.10"))
        {
            var trigger = currentWgSkill.Value.LogicTrigger;
            ImmutableList<Modifier> repeatableModifiers = ProcessSkillModifiers(wgConditionalModifiers, modifiersDictionary).ToImmutableList();
            var repeatableModifierGroup = new ConditionalModifierGroup(trigger.TriggerType, !string.IsNullOrWhiteSpace(currentWgSkill.Value.LogicTrigger.TriggerDescIds) ? currentWgSkill.Value.LogicTrigger.TriggerDescIds[4..] : string.Empty, repeatableModifiers, LocalizationOverride: string.Empty);
            conditionalModifierGroups.Add(repeatableModifierGroup);

            modifiersDictionary.TryGetValue("regenCrewAdditionalConsumables", out Modifier? modifierData);
            var onetimeModifiers = new List<Modifier>
            {
                new ("regenCrewAdditionalConsumables", 1f, "captainSkill", modifierData),
            }.ToImmutableList();
            var onetimeModifierGroup = new ConditionalModifierGroup(trigger.TriggerType, !string.IsNullOrWhiteSpace(currentWgSkill.Value.LogicTrigger.TriggerDescIds) ? currentWgSkill.Value.LogicTrigger.TriggerDescIds[4..] : string.Empty, onetimeModifiers, ActivationLimit: 1);
            conditionalModifierGroups.Add(onetimeModifierGroup);
        }
        else if (currentWgSkill.Value.SkillType == 81 && DataCache.CurrentVersion.MainVersion >= Version.Parse("0.12.10"))
        {
            var trigger = currentWgSkill.Value.LogicTrigger;
            var firstModifier = trigger.OtherData["BurnFlood_1"].ToObject<Dictionary<string, float>>()!.Single();
            var otherModifiers = trigger.OtherData["BurnFlood_2"].ToObject<Dictionary<string, float>>()!.Single();

            modifiersDictionary.TryGetValue($"repeatable_first_{firstModifier.Key}", out Modifier? firstModifierData);
            modifiersDictionary.TryGetValue($"repeatable_other_{otherModifiers.Key}", out Modifier? secondModifierData);

            var conditionalModifiers = new List<Modifier>
            {
                new ($"repeatable_first_{firstModifier.Key}", firstModifier.Value, "captainSkill", firstModifierData),
                new ($"repeatable_other_{otherModifiers.Key}", otherModifiers.Value, "captainSkill", secondModifierData),
            }.ToImmutableList();
            var modifierGroup = new ConditionalModifierGroup(trigger.TriggerType, !string.IsNullOrWhiteSpace(currentWgSkill.Value.LogicTrigger.TriggerDescIds) ? currentWgSkill.Value.LogicTrigger.TriggerDescIds[4..] : string.Empty, conditionalModifiers, ActivationLimit: 6);
            conditionalModifierGroups.Add(modifierGroup);
        }
        else if (wgConditionalModifiers.Count > 0)
        {
            List<Modifier> conditionalModifiers = ProcessSkillModifiers(wgConditionalModifiers, modifiersDictionary);
            conditionalModifierGroups.Add(new(currentWgSkill.Value.LogicTrigger.TriggerType, !string.IsNullOrWhiteSpace(currentWgSkill.Value.LogicTrigger.TriggerDescIds) ? currentWgSkill.Value.LogicTrigger.TriggerDescIds[4..] : string.Empty, conditionalModifiers.ToImmutableList()));
        }

        skill.ConditionalModifierGroups = conditionalModifierGroups.ToImmutableArray();
        DataCache.TranslationNames.UnionWith(skill.ConditionalModifierGroups.Select(g => g.TriggerType));
        DataCache.TranslationNames.UnionWith(skill.ConditionalModifierGroups.SelectMany(g => g.Modifiers.Select(m => m.Name)));
        DataCache.TranslationNames.Add(GetSkillTranslationId(currentWgSkill.Key));
        DataCache.TranslationNames.UnionWith(modifiers.Select(m => m.Name));
        return skill.ToSkill();
    }

    private static List<Modifier> ProcessSkillModifiers(Dictionary<string, JToken> skillModifiers, Dictionary<string, Modifier> modifierDictionary)
    {
        List<Modifier> modifiers = new();
        var hasConsumableReloadModifiers = false;
        foreach ((string? s, var token) in skillModifiers)
        {
            if (s.Equals("reloadFactor") || s.Equals("excludedConsumables"))
            {
                hasConsumableReloadModifiers = true;
                continue;
            }

            if (token.Type is JTokenType.Float or JTokenType.Integer)
            {
                modifierDictionary.TryGetValue(s, out Modifier? modifierData);
                var modifier = new Modifier(s, token.Value<float>(), "CaptainSkill", modifierData);
                modifiers.Add(modifier);
            }
            else if (token.Type == JTokenType.Object)
            {
                JObject jObject = (JObject)token;
                var values = jObject.ToObject<Dictionary<string, float>>()!;
                bool isEqual = true;
                var first = values.First().Value;
                foreach ((string _, float value) in values)
                {
                    if (Math.Abs(value - first) > Constants.Tolerance)
                    {
                        isEqual = false;
                    }
                }

                if (isEqual)
                {
                    modifierDictionary.TryGetValue(s, out Modifier? modifierData);
                    var modifier = new Modifier(s, first, "CaptainSkill", modifierData);
                    modifiers.Add(modifier);
                }
                else
                {
                    foreach ((string key, float value) in values)
                    {
                        // exclude auxiliary ship type modifiers, since they are unused
                        if (!key.Equals("auxiliary", StringComparison.InvariantCultureIgnoreCase))
                        {
                            modifierDictionary.TryGetValue($"{s}_{key}", out Modifier? modifierData);
                            var modifier = new Modifier($"{s}_{key}", value, "CaptainSkill", modifierData);
                            modifiers.Add(modifier);
                        }
                    }
                }
            }
        }

        if (!hasConsumableReloadModifiers)
        {
            return modifiers;
        }

        var reloadModifiers = ComputeConsumableReloadModifiers(skillModifiers);
        foreach (var modifierEntry in reloadModifiers)
        {
            modifierDictionary.TryGetValue(modifierEntry.Key, out Modifier? modifierData);
            var modifier = new Modifier(modifierEntry.Key, modifierEntry.Value, "CaptainSkill", modifierData);
            modifiers.Add(modifier);
        }

        return modifiers;
    }

    private static Dictionary<string, float> ComputeConsumableReloadModifiers(Dictionary<string, JToken> skillModifiers)
    {
        var reloadCoeff = skillModifiers["reloadFactor"].Value<float>();
        var excludedConsumables = skillModifiers["excludedConsumables"].Values<string>();
        var availableConsumables = ImmutableArray.Create("airDefenseDisp", "scout", "regenCrew", "sonar", "rls", "crashCrew", "smokeGenerator", "speedBoosters", "artilleryBoosters", "fighter", "torpedoReloader");
        return availableConsumables.Except(excludedConsumables).Select(c => $"invisible_{c}ReloadCoeff").Select(c => (c, reloadCoeff)).Append(("consumableSpecialistReloadTime", reloadCoeff)).ToDictionary(x => x.Item1, x => x.reloadCoeff);
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
        foreach ((ShipClass shipClass, List<SkillRow> skillRow) in skillsTiers.GetPositionsByClass())
        {
            for (var skillTier = 0; skillTier < skillRow.Count; skillTier++)
            {
                List<int> skillsInRow = skillRow[skillTier].GetSkillGroups().SelectMany(group => group).ToList();
                int skillIndex = skillsInRow.IndexOf(skillNumber);
                if (skillIndex >= 0)
                {
                    var position = new SkillPosition(skillTier, skillIndex, shipClass);
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

    private sealed class UniqueSkillEffectBuilder
    {
        public bool IsPercent { get; set; }

        public int UniqueType { get; set; }

        public List<Modifier> Modifiers { get; set; } = new();

        public UniqueSkillEffect ToUniqueSkillEffect()
        {
            return new(IsPercent, UniqueType, Modifiers.ToImmutableList());
        }
    }

    private sealed class SkillBuilder
    {
        public bool CanBeLearned { get; set; }

        public bool IsEpic { get; set; } // true if the skill has buffed modifier

        public int SkillNumber { get; set; }

        public ImmutableArray<ShipClass> LearnableOn { get; set; } = ImmutableArray<ShipClass>.Empty;

        public ImmutableArray<SkillPosition> Tiers { get; set; } = ImmutableArray<SkillPosition>.Empty; // contains the tier of the skill for all the classes that can use it

        public ImmutableList<Modifier> Modifiers { get; set; } = ImmutableList<Modifier>.Empty; // modifiers for always on effects

        public ImmutableArray<ConditionalModifierGroup> ConditionalModifierGroups { get; set; } = ImmutableArray<ConditionalModifierGroup>.Empty;

        public Skill ToSkill() => new()
        {
            CanBeLearned = CanBeLearned,
            IsEpic = IsEpic,
            SkillNumber = SkillNumber,
            LearnableOn = LearnableOn,
            Tiers = Tiers,
            Modifiers = Modifiers,
            ConditionalModifierGroups = ConditionalModifierGroups,
        };
    }
}
