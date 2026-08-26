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
