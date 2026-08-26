using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
public class CaptainConverterTest
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
    /// A "modifiers" wrapper groups several distinct stats. Collapsing it into one entry named after the wrapper
    /// produced a modifier literally called "modifiers", whose metadata is Discard, so the talent effect was
    /// silently dropped from the app - and every stat but the first was lost outright.
    /// </summary>
    [TestCaseSource(nameof(LegendaryCaptains))]
    public void ConvertCaptain_ModifierWrapper_IsNeverEmittedAsAModifierName(string captainIndex)
    {
        var captain = this.ConvertSingle(captainIndex);

        var modifierNames = AllTalentModifiers(captain).Select(modifier => modifier.Name).ToList();

        modifierNames.Should().NotContain("modifiers");
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
