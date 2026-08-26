using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DataConverter.Converters;
using DataConverter.Data;
using DataConverter.Services;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Captain;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Versioning;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Captain;

namespace DataConverter.Test.ConverterTests;

/// <summary>
/// Tests for talent (unique skill) conversion, driven by per-captain fixtures extracted from game build 13015811.
/// </summary>
[TestFixture]
public partial class CaptainConverterTest
{
    /// <summary>
    /// Keys that belong to a talent's trigger definition rather than its effect. None of them may ever become a
    /// modifier: they describe when the talent fires, and several of them hold non-numeric values.
    /// </summary>
    private static readonly string[] TriggerMetadataKeys =
    [
        "type", "requiredCount", "maxActivations", "numberOfTargets", "separateTracking", "timeLimit",
        "damageAmountPerMaxHealth", "damageAmount", "isRepeating", "startEnabled", "progressIncrement",
        "triggerRibbonsNum", "sortIndex", "battleGroup", "isUnlimited",
    ];

    private static readonly string[] LegendaryCaptains = ["PIW101", "PAW102", "PJW018", "PSW100", "PBW100"];

    private Dictionary<string, Modifier> modifiers = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        DataCache.CurrentVersion = new GameVersion(new Version(0, 15, 7), GameVersionType.Live, 1);
        this.modifiers = ModifierProcessingService.LoadEmbeddedModifiers();
    }

    [TestCaseSource(nameof(LegendaryCaptains))]
    public void ConvertCaptain_CaptainWithTalents_DoesNotThrow(string captainIndex)
    {
        Action action = () => _ = this.Convert(captainIndex);

        action.Should().NotThrow();
    }

    [TestCaseSource(nameof(LegendaryCaptains))]
    public void ConvertCaptain_TalentTriggerObjects_AreNotConvertedToSkillEffects(string captainIndex)
    {
        var captain = this.ConvertSingle(captainIndex);

        var effectNames = captain.UniqueSkills.Values.SelectMany(talent => talent.SkillEffects.Keys).ToList();

        effectNames.Should().NotBeEmpty();
        effectNames.Should().NotContain(name => name.Contains("GameLogicTrigger", StringComparison.Ordinal));
    }

    [TestCaseSource(nameof(LegendaryCaptains))]
    public void ConvertCaptain_TalentTriggerMetadata_DoesNotLeakIntoModifiers(string captainIndex)
    {
        var captain = this.ConvertSingle(captainIndex);

        var modifierNames = AllTalentModifiers(captain).Select(modifier => modifier.Name).ToList();

        modifierNames.Should().NotIntersectWith(TriggerMetadataKeys);
    }

    [TestCaseSource(nameof(LegendaryCaptains))]
    public void ConvertCaptain_EveryTalent_HasAtLeastOneSkillEffect(string captainIndex)
    {
        var captain = this.ConvertSingle(captainIndex);

        captain.UniqueSkills.Should().NotBeEmpty();
        captain.UniqueSkills.Values.Should().OnlyContain(talent => talent.SkillEffects.Count > 0);
    }

    /// <summary>
    /// Regression test for the consumable-reload talent handling added for game update 15.5: the nested object mixes
    /// a float reloadFactor with an excludedConsumables string array and must not be read as a plain modifier map.
    /// </summary>
    [Test]
    public void ConvertCaptain_ConsumableReloadTalent_EmitsInvisibleReloadModifiers()
    {
        var captain = this.ConvertSingle("PSW100");

        var modifierNames = AllTalentModifiers(captain).Select(modifier => modifier.Name).ToList();

        modifierNames.Should().Contain("consumableSpecialistReloadTime");
        modifierNames.Should().Contain(name => name.StartsWith("invisible_", StringComparison.Ordinal) && name.EndsWith("ReloadCoeff", StringComparison.Ordinal));
        modifierNames.Should().NotContain("excludedConsumables");
    }

    /// <summary>
    /// Wrappers such as "modifiers" and "level_1" group several distinct stats, unlike the per-ship-class maps the
    /// all-equal collapse is meant for. Collapsing them emitted a modifier named after the wrapper itself - a name
    /// with no metadata, so the effect vanished from the app - and discarded every stat but the first.
    /// </summary>
    [TestCaseSource(nameof(LegendaryCaptains))]
    public void ConvertCaptain_StatWrapper_IsNeverEmittedAsAModifierName(string captainIndex)
    {
        var captain = this.ConvertSingle(captainIndex);

        var modifierNames = AllTalentModifiers(captain).Select(modifier => modifier.Name).ToList();

        modifierNames.Should().NotContain("modifiers");

        // "level_1" is a wrapper; "level_1_speedCoef" is a real stat inside it.
        modifierNames.Should().NotContain(name => BareLevelWrapper().IsMatch(name));
    }

    [Test]
    public void ConvertCaptain_ModifierWrapperWithEqualValues_KeepsEveryStat()
    {
        var captain = this.ConvertSingle("PSW100");

        var modifierNames = AllTalentModifiers(captain).Select(modifier => modifier.Name).ToList();

        // Both stats carry the same value (0.97), which previously collapsed them into a single entry.
        modifierNames.Should().Contain("regenCrewReloadCoeff");
        modifierNames.Should().Contain("crashCrewReloadCoeff");
    }

    [Test]
    public void ConvertCaptain_ConsumableCapacityTalent_IsNotDiscarded()
    {
        var captain = this.ConvertSingle("PIW101");

        var modifiers = AllTalentModifiers(captain).ToList();

        modifiers.Should().ContainSingle(modifier => modifier.Name.Equals("shipConsumableCapacityCoeff", StringComparison.Ordinal))
            .Which.Value.Should().BeApproximately(1.025f, 0.0001f);
    }

    /// <summary>
    /// The game keys a talent by its position in the captain's talent list, not by trigger type. Since update 15.7
    /// every talent reports triggerType "ribbons", so the old scheme generated keys that match no game string.
    /// </summary>
    [Test]
    public void ConvertCaptain_TranslationId_UsesCaptainSortIndexAndUniqueTypes()
    {
        var captain = this.ConvertSingle("PIW101");

        var translationIds = captain.UniqueSkills.Values.Select(talent => talent.TranslationId).Distinct().ToList();

        translationIds.Should().BeEquivalentTo("TALENT_PIW101_1_13", "TALENT_PIW101_2_9", "TALENT_PIW101_3_10_21");
    }

    [TestCaseSource(nameof(LegendaryCaptains))]
    public void ConvertCaptain_TranslationId_DoesNotContainTriggerType(string captainIndex)
    {
        var captain = this.ConvertSingle(captainIndex);

        captain.UniqueSkills.Values.Select(talent => talent.TranslationId)
            .Should().OnlyContain(id => !id.Contains("ribbons", StringComparison.Ordinal));
    }

    [Test]
    public void ConvertCaptain_BattleGroups_AreMapped()
    {
        var captain = this.ConvertSingle("PIW101");

        var byGroup = captain.UniqueSkills.Values.GroupBy(talent => talent.BattleGroup).ToDictionary(group => group.Key, group => group.Count());

        byGroup.Should().Contain(new KeyValuePair<TalentBattleGroup, int>(TalentBattleGroup.Regular, 1));
        byGroup.Should().Contain(new KeyValuePair<TalentBattleGroup, int>(TalentBattleGroup.Operations, 1));
        byGroup.Should().Contain(new KeyValuePair<TalentBattleGroup, int>(TalentBattleGroup.Every, 2));
    }

    /// <summary>
    /// A talent tuned differently for operations ships as two entries that share a translation id. Consumers rely on
    /// the battle group to pick one, so the pairing must survive conversion.
    /// </summary>
    [Test]
    public void ConvertCaptain_PairedTalent_SharesTranslationIdAcrossBattleGroups()
    {
        var captain = this.ConvertSingle("PIW101");

        var paired = captain.UniqueSkills.Values
            .GroupBy(talent => talent.TranslationId)
            .Where(group => group.Count() > 1)
            .ToList();

        paired.Should().ContainSingle().Which.Select(talent => talent.BattleGroup)
            .Should().BeEquivalentTo([TalentBattleGroup.Regular, TalentBattleGroup.Operations]);
    }

    /// <summary>
    /// Tiers arrive out of order in the game data (Yamamoto's are level_2, level_3, level_1) and must be sorted.
    /// </summary>
    [Test]
    public void ConvertCaptain_TieredEffect_LevelsAreOrdered()
    {
        var effect = TieredEffect("PJW018", TalentBattleGroup.Regular, "UniqueMainReloadBooster1");

        effect.Levels.Select(level => level.Level).Should().Equal(1, 2, 3);
    }

    /// <summary>
    /// For a multiplicative stat the game ships the per-tier increment under the bare name and the effective value
    /// under a "...UI" twin. The twin is the running product and is the number shown in game, so both must survive.
    /// </summary>
    [Test]
    public void ConvertCaptain_TieredEffect_CumulativeValuesAreTheRunningProduct()
    {
        var effect = TieredEffect("PJW018", TalentBattleGroup.Regular, "UniqueMainReloadBooster1");

        Increment(effect, 1, "GMShotDelay").Should().BeApproximately(0.95f, 0.0001f);
        Increment(effect, 2, "GMShotDelay").Should().BeApproximately(0.78947f, 0.0001f);
        Increment(effect, 3, "GMShotDelay").Should().BeApproximately(0.8f, 0.0001f);

        Cumulative(effect, 1, "GMShotDelay").Should().BeApproximately(0.95f, 0.0001f);
        Cumulative(effect, 2, "GMShotDelay").Should().BeApproximately(0.75f, 0.0001f);
        Cumulative(effect, 3, "GMShotDelay").Should().BeApproximately(0.6f, 0.0001f);

        // Sanity-check the relationship the twin encodes rather than just the literals.
        (Cumulative(effect, 1, "GMShotDelay") * Increment(effect, 2, "GMShotDelay"))
            .Should().BeApproximately(Cumulative(effect, 2, "GMShotDelay"), 0.0001f);
    }

    /// <summary>
    /// An absolute stat has no "...UI" twin, so a blanket "recompute the cumulative value as a product" rule would
    /// be wrong. Its effective value is simply the tier's own.
    /// </summary>
    [Test]
    public void ConvertCaptain_TieredEffectWithoutUiTwin_CumulativeEqualsIncrement()
    {
        var effect = TieredEffect("PJW018", TalentBattleGroup.Regular, "UniqueRegen1");

        foreach (var level in effect.Levels)
        {
            level.CumulativeModifiers.Should().BeEquivalentTo(level.Modifiers);
        }
    }

    /// <summary>
    /// Consumers that predate tiers read the flat modifier list, so it must describe the fully escalated talent.
    /// </summary>
    [Test]
    public void ConvertCaptain_TieredEffect_FlatModifiersHoldTopTierCumulativeValues()
    {
        var effect = TieredEffect("PJW018", TalentBattleGroup.Regular, "UniqueMainReloadBooster1");

        effect.Modifiers.Should().BeEquivalentTo(effect.Levels[^1].CumulativeModifiers);
    }

    /// <summary>
    /// A stat is often declared both at effect level, as a zero placeholder, and again per tier. The flat list must
    /// report it once, with the tier value, or a consumer keying modifiers by name throws.
    /// </summary>
    [TestCaseSource(nameof(LegendaryCaptains))]
    public void ConvertCaptain_TieredEffect_FlatModifiersHaveNoDuplicateNames(string captainIndex)
    {
        var captain = this.ConvertSingle(captainIndex);

        foreach (var effect in captain.UniqueSkills.Values.SelectMany(talent => talent.SkillEffects.Values))
        {
            effect.Modifiers.Select(modifier => modifier.Name).Should().OnlyHaveUniqueItems();
        }
    }

    [Test]
    public void ConvertCaptain_TieredEffect_FlatModifiersPreferTheTierValueOverThePlaceholder()
    {
        var effect = TieredEffect("PJW018", TalentBattleGroup.Regular, "UniqueRegen1");

        effect.Modifiers.Single(modifier => modifier.Name.Equals("workTime", StringComparison.Ordinal))
            .Value.Should().Be(120f);
    }

    [Test]
    public void ConvertCaptain_Trigger_IsParsedFromTheGameLogicTriggerSibling()
    {
        var talent = Talent("PJW018", TalentBattleGroup.Regular, hasLevels: true);

        talent.Trigger.Should().NotBeNull();
        talent.Trigger!.ActivatorType.Should().Be("RibbonActivator");
        talent.Trigger.MaxActivations.Should().Be(3);
        talent.Trigger.Levels.Select(level => level.Thresholds["requiredCount"]).Should().Equal(2m, 5m, 7m);
    }

    /// <summary>
    /// Topete escalates the trigger without escalating the effect, and the threshold key differs per activator type,
    /// so tier thresholds cannot be modelled as a single requiredCount field.
    /// </summary>
    [Test]
    public void ConvertCaptain_TieredTriggerWithoutTieredEffect_StillCarriesThresholds()
    {
        var captain = this.ConvertSingle("PSW100");

        var talent = captain.UniqueSkills.Values.Single(t => t.Trigger?.Levels.IsEmpty == false);

        talent.Trigger!.ActivatorType.Should().Be("RemainingHealthActivator");
        talent.Trigger.Levels.Select(level => level.Thresholds["thresholdPerMaxHealth"]).Should().Equal(0.75m, 0.5m, 0.25m);
        talent.SkillEffects.Values.Should().OnlyContain(effect => effect.Levels.IsEmpty);
    }

    /// <summary>
    /// regenerationHPSpeed is tuned per captain and has per-captain metadata, so the tiered path must namespace it
    /// exactly like the flat one does.
    /// </summary>
    [Test]
    public void ConvertCaptain_TieredRegenerationHpSpeed_IsNamespacedPerCaptain()
    {
        var effect = TieredEffect("PJW018", TalentBattleGroup.Regular, "UniqueRegen1");

        effect.Levels[0].Modifiers.Select(modifier => modifier.Name)
            .Should().Contain("captain_PJW018_Yamamoto_regenerationHPSpeed").And.NotContain("regenerationHPSpeed");
    }

    private static float Increment(UniqueSkillEffect effect, int level, string statName)
    {
        return effect.Levels.Single(l => l.Level == level).Modifiers.Single(m => m.Name.Equals(statName, StringComparison.Ordinal)).Value;
    }

    private static float Cumulative(UniqueSkillEffect effect, int level, string statName)
    {
        return effect.Levels.Single(l => l.Level == level).CumulativeModifiers.Single(m => m.Name.Equals(statName, StringComparison.Ordinal)).Value;
    }

    [GeneratedRegex(@"^level_\d+$")]
    private static partial Regex BareLevelWrapper();

    private static IEnumerable<Modifier> AllTalentModifiers(Captain captain)
    {
        return captain.UniqueSkills.Values
            .SelectMany(talent => talent.SkillEffects.Values)
            .SelectMany(effect => effect.Modifiers);
    }

    private static List<WgCaptain> LoadFixture(string captainIndex)
    {
        string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "Captains", captainIndex + ".json");
        return JsonConvert.DeserializeObject<List<WgCaptain>>(File.ReadAllText(path)) ?? throw new InvalidOperationException($"Unable to read fixture for {captainIndex}");
    }

    private UniqueSkill Talent(string captainIndex, TalentBattleGroup battleGroup, bool hasLevels)
    {
        return this.ConvertSingle(captainIndex).UniqueSkills.Values
            .Single(talent => talent.BattleGroup == battleGroup && talent.SkillEffects.Values.Any(effect => !effect.Levels.IsEmpty) == hasLevels && talent.Trigger is not null);
    }

    private UniqueSkillEffect TieredEffect(string captainIndex, TalentBattleGroup battleGroup, string effectName)
    {
        return this.ConvertSingle(captainIndex).UniqueSkills.Values
            .Where(talent => talent.BattleGroup == battleGroup)
            .SelectMany(talent => talent.SkillEffects)
            .Single(effect => effect.Key.Equals(effectName, StringComparison.Ordinal) && !effect.Value.Levels.IsEmpty)
            .Value;
    }

    private Dictionary<string, Captain> Convert(string captainIndex)
    {
        return CaptainConverter.ConvertCaptain(LoadFixture(captainIndex), CaptainConverter.LoadEmbeddedSkillData(), false, this.modifiers);
    }

    private Captain ConvertSingle(string captainIndex)
    {
        var result = this.Convert(captainIndex);
        result.Should().HaveCount(1, "the fixture contains exactly one captain");
        return result.Values.Single();
    }
}
