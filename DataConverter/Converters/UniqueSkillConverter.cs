using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
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
    /// <summary>
    /// Suffix the game uses for the effective-value twin of a tiered multiplicative stat.
    /// </summary>
    private const string CumulativeSuffix = "UI";

    /// <summary>
    /// Key under which a talent effect groups several distinct stats, as opposed to one stat listed per entity.
    /// </summary>
    private const string ModifierWrapperKey = "modifiers";

    /// <summary>
    /// Prefix of the per-tier blocks of an escalating talent.
    /// </summary>
    private const string LevelPrefix = "level_";

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

                    // Tier definitions of an escalating talent, keyed level_1..level_N.
                    var levelObjects = new Dictionary<string, JObject>();

                    //iterate through the entire object fields
                    foreach ((string key, var value) in values)
                    {
                        // A "level_N" object holds this tier's stat values, not a modifier of its own.
                        if (key.StartsWith(LevelPrefix, StringComparison.Ordinal) && value is JObject levelObject)
                        {
                            levelObjects[key] = levelObject;
                            continue;
                        }

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

                            var fixedKey = ResolveModifierName(key, currentWgCaptain.Name);
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

                            // A map listing one stat once per entity collapses to a single modifier when every entry
                            // agrees. A "modifiers" wrapper is the opposite shape - distinct stats under one name - so
                            // collapsing it would emit a modifier named after the wrapper, which has no metadata, and
                            // discard every stat but the first. Tier wrappers never reach here; they are taken above.
                            bool isModifierWrapper = key.Equals(ModifierWrapperKey, StringComparison.Ordinal);
                            float first = modifiers.Values.First();
                            bool allEquals = !isModifierWrapper && modifiers.Values.All(value => Math.Abs(value - first) < Constants.Tolerance);
                            if (allEquals)
                            {
                                modifierDictionary.TryGetValue(key, out Modifier? modifierData);
                                effectsModifiers.Add(new Modifier(key, first, $"Skill_{captainIndex}_{currentUniqueSkillKey}", modifierData));
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
                    skillEffect.Levels = BuildEffectLevels(levelObjects, $"Skill_{captainIndex}_{currentUniqueSkillKey}", currentWgCaptain.Name, modifierDictionary);
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

            var trigger = BuildTrigger(currentUniqueSkillValue);
            if (skillEffectDictionary.Values.Any(effect => !effect.Levels.IsEmpty) || trigger?.Levels.IsEmpty == false)
            {
                // Only escalating talents carry these. LEVEL_ACTIVATION is a gettext plural array of one sentence,
                // not one entry per tier, so only its singular form survives translation extraction.
                DataCache.TranslationNames.Add($"{translationId}_LEVEL_ACTIVATION");
                DataCache.TranslationNames.Add($"{translationId}_PROGRESSION_DESC");
            }

            //create our talent data
            UniqueSkill uniqueSkill = new()
            {
                MaxTriggerNum = currentUniqueSkillValue.MaxTriggerNum,
                AllowedShips = currentUniqueSkillValue.TriggerAllowedShips.ToImmutableArray(),
                TriggerType = currentUniqueSkillValue.TriggerType,
                TranslationId = translationId,
                BattleGroup = ParseBattleGroup(currentUniqueSkillValue.BattleGroup),
                Trigger = trigger,
                SkillEffects = skillEffectDictionary.ToImmutableDictionary(),
            };

            skills.Add(currentUniqueSkillKey, uniqueSkill);
        }

        return skills;
    }

    /// <summary>
    /// Builds the escalation steps of a tiered talent effect.
    /// </summary>
    /// <remarks>
    /// The game ships two numbers per multiplicative stat: the bare name is the increment applied when the tier is
    /// reached, and a "...UI" twin is the effective value once it is active. Yamamoto's main battery reload, for
    /// example, runs 0.95 / 0.78947 / 0.8 as increments and 0.95 / 0.75 / 0.6 as effective values - the latter being
    /// the running product, and the number shown in game. Absolute stats such as workTime have no twin.
    /// </remarks>
    private static ImmutableList<UniqueSkillEffectLevel> BuildEffectLevels(Dictionary<string, JObject> levelObjects, string location, string captainName, Dictionary<string, Modifier> modifierDictionary)
    {
        var levels = new List<UniqueSkillEffectLevel>();
        foreach ((string levelKey, JObject levelObject) in levelObjects)
        {
            var stats = NumericEntries(levelObject);
            var increments = new List<Modifier>();
            var cumulative = new List<Modifier>();

            foreach ((string statName, float statValue) in stats)
            {
                if (statName.EndsWith(CumulativeSuffix, StringComparison.Ordinal))
                {
                    continue;
                }

                // The flat path drops a multiplier of one as "this stat is unchanged"; a tier that has not started
                // improving a stat yet says the same thing, and would otherwise render as a "+0%" row.
                float effectiveValue = stats.TryGetValue(statName + CumulativeSuffix, out float uiValue) ? uiValue : statValue;
                if (Math.Abs(statValue - 1f) < Constants.Tolerance && Math.Abs(effectiveValue - 1f) < Constants.Tolerance)
                {
                    continue;
                }

                string modifierName = ResolveModifierName(statName, captainName);
                modifierDictionary.TryGetValue(modifierName, out Modifier? modifierData);
                increments.Add(new(modifierName, statValue, location, modifierData));
                cumulative.Add(new(modifierName, effectiveValue, location, modifierData));
                DataCache.TranslationNames.Add(statName);
            }

            levels.Add(new(ParseLevelNumber(levelKey), increments.ToImmutableList(), cumulative.ToImmutableList()));
        }

        return levels.OrderBy(level => level.Level).ToImmutableList();
    }

    /// <summary>
    /// Reads the talent's trigger definition, which lives in a "GameLogicTrigger..." sibling of the effect objects.
    /// </summary>
    private static TalentTrigger? BuildTrigger(WgUniqueSkill talent)
    {
        JObject? triggerObject = talent.SkillEffects
            .Where(entry => entry.Key.StartsWith("GameLogicTrigger", StringComparison.Ordinal))
            .Select(entry => entry.Value as JObject)
            .FirstOrDefault(value => value is not null);

        // The container is Activator1 on most talents but Activator2 on at least one, so match on the prefix.
        JObject? activator = triggerObject?.Properties()
            .Where(property => property.Name.StartsWith("Activator", StringComparison.Ordinal))
            .Select(property => property.Value as JObject)
            .FirstOrDefault(value => value is not null);

        if (activator is null)
        {
            return null;
        }

        var entries = activator.ToObject<Dictionary<string, JToken>>()!;

        var levels = entries
            .Where(entry => entry.Key.StartsWith(LevelPrefix, StringComparison.Ordinal) && entry.Value is JObject)
            .Select(entry => new TalentTriggerLevel(
                ParseLevelNumber(entry.Key),
                NumericEntries((JObject)entry.Value).ToImmutableDictionary(stat => stat.Key, stat => (decimal)stat.Value)))
            .OrderBy(level => level.Level)
            .ToImmutableList();

        // An escalating activator states a placeholder for the stat it escalates and the real figures per tier:
        // Yamamoto reports requiredCount 1 next to tiers of 2/5/7, Topete thresholdPerMaxHealth 0.1 next to
        // 0.75/0.5/0.25. Drop any parameter a tier overrides, so the two collections cannot disagree.
        var tieredKeys = levels.SelectMany(level => level.Thresholds.Keys).ToHashSet(StringComparer.Ordinal);
        var parameters = entries
            .Where(entry => entry.Value.Type is JTokenType.Float or JTokenType.Integer)
            .Where(entry => !tieredKeys.Contains(entry.Key))
            .ToDictionary(entry => entry.Key, entry => entry.Value.Value<decimal>());

        // Every activator in build 13015811 states this, but default to the "unlimited" sentinel rather than 0,
        // which would read as "never fires".
        int maxActivations = parameters.TryGetValue("maxActivations", out decimal stated) ? (int)stated : -1;
        return new(activator.Value<string>("type") ?? string.Empty, parameters.ToImmutableDictionary(), maxActivations, levels);
    }

    /// <summary>
    /// Some stats are tuned per captain and carry per-captain metadata, so their modifier name is namespaced.
    /// </summary>
    private static string ResolveModifierName(string statName, string captainName)
    {
        return statName.Equals("regenerationHPSpeed", StringComparison.Ordinal) ? $"captain_{captainName}_{statName}" : statName;
    }

    private static Dictionary<string, float> NumericEntries(JObject source)
    {
        return source.ToObject<Dictionary<string, JToken>>()!
            .Where(entry => entry.Value.Type is JTokenType.Float or JTokenType.Integer)
            .ToDictionary(entry => entry.Key, entry => entry.Value.Value<float>());
    }

    private static int ParseLevelNumber(string levelKey)
    {
        return int.TryParse(levelKey.AsSpan(LevelPrefix.Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out int level) ? level : 0;
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
        // Hardcoded because the game lists only the exclusions. "auxTorpBooster" is the slot PCY087 took over from
        // the dropped defensive AA fire consumable in update 15.7, so a talent that reloads consumables must reach it.
        var availableConsumables = ImmutableArray.Create("airDefenseDisp", "scout", "regenCrew", "sonar", "rls", "crashCrew", "smokeGenerator", "speedBoosters", "artilleryBoosters", "fighter", "torpedoReloader", "auxTorpBooster");
        return availableConsumables.Except(excludedConsumables).Select(c => $"invisible_{c}ReloadCoeff").Select(c => (c, reloadCoeff)).Append(("consumableSpecialistReloadTime", reloadCoeff)).ToDictionary(x => x.Item1, x => x.reloadCoeff);
    }

    private sealed class UniqueSkillEffectBuilder
    {
        public bool IsPercent { get; set; }

        public int UniqueType { get; set; }

        public List<Modifier> Modifiers { get; set; } = new();

        public ImmutableList<UniqueSkillEffectLevel> Levels { get; set; } = ImmutableList<UniqueSkillEffectLevel>.Empty;

        public UniqueSkillEffect ToUniqueSkillEffect()
        {
            // Consumers that do not model tiers read Modifiers, so for an escalating talent report the fully
            // escalated values rather than nothing. A stat is often declared both at effect level - as a zero
            // placeholder - and again per tier; the tier value wins, otherwise the list would carry the same name
            // twice and a consumer keying by name would throw or read the placeholder.
            ImmutableList<Modifier> modifiers;
            if (Levels.IsEmpty)
            {
                modifiers = Modifiers.ToImmutableList();
            }
            else
            {
                var topTier = Levels[^1].CumulativeModifiers;
                var tieredNames = topTier.Select(modifier => modifier.Name).ToHashSet(StringComparer.Ordinal);
                modifiers = Modifiers.Where(modifier => !tieredNames.Contains(modifier.Name)).Concat(topTier).ToImmutableList();
            }

            return new(IsPercent, UniqueType, modifiers) { Levels = Levels };
        }
    }
}
