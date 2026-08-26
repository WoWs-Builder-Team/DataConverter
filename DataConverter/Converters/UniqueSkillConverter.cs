using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DataConverter.Data;
using Newtonsoft.Json.Linq;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Captain;
using WoWsShipBuilder.DataStructures.Modifiers;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Captain;

namespace DataConverter.Converters;

internal static class UniqueSkillConverter
{
    internal static Dictionary<string, UniqueSkill> ProcessUniqueSkills(WgCaptain currentWgCaptain, string captainIndex, Dictionary<string, Modifier> modifierDictionary)
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

                // Talent effects are the objects carrying a "uniqueType" discriminator. Sibling "GameLogicTrigger*"
                // objects describe what fires the talent, not its effect, and also match a name-based "Unique" check
                // because the talent itself may be named "...TriggerUniqueDamage1". Their nested activators hold
                // strings such as "TargetsDamagedActivator", which cannot be read as modifiers.
                if (currentWgUniqueSkillEffectValue is JObject jObject && jObject.ContainsKey("uniqueType"))
                {
                    //create a modifiers dictionary for the current effect
                    var effectsModifiers = new List<Modifier>();

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
                            effectsModifiers.Add(new Modifier(fixedKey, value.Value<float>(), $"Skill_{captainIndex}_{currentUniqueSkillKey}", modifierData));
                            DataCache.TranslationNames.Add(key);
                        }
                        else if (value.Type == JTokenType.Object)
                        {
                            JObject jObjectModifier = (JObject)value;

                            // Consumable-reload talents (e.g. PSW100 Topete) nest a "modifiers" object that mixes a
                            // float reloadFactor with an excludedConsumables string array. Mirror the regular-skill
                            // handling instead of force-deserializing the whole object to Dictionary<string, float>.
                            if (jObjectModifier.ContainsKey("excludedConsumables"))
                            {
                                var reloadModifiers = ComputeConsumableReloadModifiers(jObjectModifier.ToObject<Dictionary<string, JToken>>()!);
                                foreach (var (modifierName, modifierValue) in reloadModifiers)
                                {
                                    modifierDictionary.TryGetValue(modifierName, out Modifier? reloadModifierData);
                                    effectsModifiers.Add(new Modifier(modifierName, modifierValue, $"Skill_{captainIndex}_{currentUniqueSkillKey}", reloadModifierData));
                                    DataCache.TranslationNames.Add(modifierName);
                                }

                                continue;
                            }

                            // Only numeric leaves are modifiers. Nested objects can also carry strings, arrays and
                            // booleans (activator descriptors, flags), which must never be coerced to float.
                            var modifiers = jObjectModifier.ToObject<Dictionary<string, JToken>>()!
                                .Where(entry => entry.Value.Type is JTokenType.Float or JTokenType.Integer)
                                .ToDictionary(entry => entry.Key, entry => entry.Value.Value<float>());
                            if (modifiers.Count == 0)
                            {
                                continue;
                            }

                            // The collapse below exists for per-ship-class maps: one stat listed once per class, which
                            // becomes a single modifier when every class shares a value. Other wrappers ("modifiers",
                            // "level_1", ...) group *distinct stats*, so collapsing them would emit one modifier named
                            // after the wrapper - a name with no metadata - and discard every stat but the first.
                            // Decide by looking at the keys rather than by listing wrapper names.
                            bool isPerShipClassMap = modifiers.Keys.All(name => Enum.TryParse<ShipClass>(name, out _));
                            bool isModifierWrapper = key.Equals("modifiers", StringComparison.Ordinal);
                            bool allEquals = isPerShipClassMap && modifiers.Values.Distinct().Count() == 1;
                            if (allEquals)
                            {
                                modifierDictionary.TryGetValue(key, out Modifier? modifierData);
                                effectsModifiers.Add(new Modifier(key, modifiers.First().Value, $"Skill_{captainIndex}_{currentUniqueSkillKey}", modifierData));
                                DataCache.TranslationNames.Add(key);
                            }
                            else
                            {
                                foreach (var (modifierName, modifierValue) in modifiers)
                                {
                                    string name = isModifierWrapper ? $"{modifierName}" : $"{key}_{modifierName}";
                                    modifierDictionary.TryGetValue(name, out Modifier? modifierData);
                                    effectsModifiers.Add(new Modifier(name, modifierValue, $"Skill_{captainIndex}_{currentUniqueSkillKey}", modifierData));
                                    DataCache.TranslationNames.Add(name);
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

            // Calculate the localization string. The game keys talents by their position in the captain's talent
            // list, not by trigger type: IDS_TALENT_<captain>_<sortIndex>_<sorted uniqueTypes>. Since update 15.7
            // every talent reports triggerType "ribbons", so the old scheme produced keys that match nothing.
            uniqueIds.Sort();
            var uniqueIdsString = string.Join("_", uniqueIds);
            var translationId = $"TALENT_{captainIndex}_{currentUniqueSkillValue.SortIndex}_{uniqueIdsString}";
            DataCache.TranslationNames.Add(translationId);

            // Descriptions are split per battle group. Registering them explicitly is redundant with the
            // substring-based translation filter, but keeps the keys this converter relies on discoverable.
            DataCache.TranslationNames.Add($"{translationId}_DESCRIPTION_BATTLE_GROUP_REGULAR");
            DataCache.TranslationNames.Add($"{translationId}_DESCRIPTION_BATTLE_GROUP_OPERATIONS");

            //create our talent data
            UniqueSkill uniqueSkill = new()
            {
                MaxTriggerNum = currentUniqueSkillValue.MaxTriggerNum,
                AllowedShips = currentUniqueSkillValue.TriggerAllowedShips.ToImmutableArray(),
                TriggerType = currentUniqueSkillValue.TriggerType,
                TranslationId = translationId,
                BattleGroup = ParseBattleGroup(currentUniqueSkillValue.BattleGroup),
                SkillEffects = skillEffectDictionary.ToImmutableDictionary(),
            };

            skills.Add(currentUniqueSkillKey, uniqueSkill);
        }

        return skills;
    }

    /// <summary>
    /// Maps the game's battle-group constant onto <see cref="TalentBattleGroup"/>. An unset or unrecognised value
    /// means the talent is not split per battle type.
    /// </summary>
    private static TalentBattleGroup ParseBattleGroup(string battleGroup) => battleGroup switch
    {
        "BATTLE_GROUP_REGULAR" => TalentBattleGroup.Regular,
        "BATTLE_GROUP_OPERATIONS" => TalentBattleGroup.Operations,
        _ => TalentBattleGroup.Every,
    };

    internal static Dictionary<string, float> ComputeConsumableReloadModifiers(Dictionary<string, JToken> skillModifiers)
    {
        // Callers reach this after spotting either key, but the game does not guarantee both are present.
        if (!skillModifiers.TryGetValue("reloadFactor", out JToken? reloadFactor))
        {
            return new();
        }

        var reloadCoeff = reloadFactor.Value<float>();
        IEnumerable<string?> excludedConsumables = skillModifiers.TryGetValue("excludedConsumables", out JToken? excluded)
            ? excluded.Values<string>()
            : [];
        var availableConsumables = ImmutableArray.Create("airDefenseDisp", "scout", "regenCrew", "sonar", "rls", "crashCrew", "smokeGenerator", "speedBoosters", "artilleryBoosters", "fighter", "torpedoReloader");
        return availableConsumables.Except(excludedConsumables).Select(c => $"invisible_{c}ReloadCoeff").Select(c => (c, reloadCoeff)).Append(("consumableSpecialistReloadTime", reloadCoeff)).ToDictionary(x => x.Item1, x => x.reloadCoeff);
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
}
