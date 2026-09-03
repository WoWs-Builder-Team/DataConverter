using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataConverter.Converters;
using DataConverter.Data;
using DataConverter.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Consumable;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Versioning;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace DataConverter.Test.ConverterTests;

/// <summary>
/// Tests for the buff references consumables started using in game update 15.7, driven by fixtures extracted from
/// game build 13015811.
/// </summary>
[TestFixture]
public class ConsumableConverterTest
{
    /// <summary>Auxiliary Armament Booster: five variants, three different buffs, no numbers of its own.</summary>
    private const string AuxiliaryTorpedoArmamentBooster = "PCY087";

    /// <summary>Fire extinguishing support: its only effect is a boolean flag.</summary>
    private const string SupportFireExtinguishing = "PCY085";

    /// <summary>Support heal: names both a buff and a weaker buffOnSelf that set the same stat.</summary>
    private const string SupportHeal = "PCY086";

    /// <summary>Airstrike countermeasures: its buffs use the tiered level shape instead of a modifier block.</summary>
    private const string AirstrikeCountermeasures = "PCY080";

    /// <summary>Auxiliary armament support: the only one of the fixtures whose buff has a surviving per-class map.</summary>
    private const string SupportAux = "PCY083";

    private static readonly string[] BuffBackedConsumables = [AuxiliaryTorpedoArmamentBooster, SupportFireExtinguishing, SupportHeal, AirstrikeCountermeasures, SupportAux];

    /// <summary>
    /// The battle entities a buff's nested maps are keyed by that are not ship classes. None of them may ever end
    /// up in a modifier name.
    /// </summary>
    private static readonly string[] NonShipClassEntities =
    [
        "Filth", "CoastalArtillery", "Airfield", "Complex", "Generator", "SpaceStation", "Portal", "RayTower",
        "AntiAircraft", "Military", "Tower", "BattleEntity", "SensorTower", "Fake", "AirBase", "Minefield",
    ];

    /// <summary>
    /// Sentinels every buff repeats with the same value. They say nothing about the consumable, and
    /// GMMaxDistAbsoluteCap is registered as a distance, so it would render as a huge range bonus on every card.
    /// </summary>
    private static readonly string[] TemplateSentinels = ["GMMaxDistAbsoluteCap", "aimRange"];

    private Dictionary<string, Modifier> modifiers = null!;

    private Dictionary<string, WgBuff> buffs = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        DataCache.CurrentVersion = new GameVersion(new Version(0, 15, 7), GameVersionType.Live, 1);
        this.modifiers = ModifierProcessingService.LoadEmbeddedModifiers();
        this.buffs = LoadBuffFixture().ToDictionary(buff => buff.Name, StringComparer.Ordinal);
    }

    [TestCaseSource(nameof(BuffBackedConsumables))]
    public void ConvertConsumable_BuffBackedConsumable_EveryVariantCarriesModifiers(string consumableIndex)
    {
        var variants = this.ConvertVariants(consumableIndex);

        variants.Should().NotBeEmpty();
        variants.Values.Should().OnlyContain(variant => variant.Modifiers.Count > 0);
    }

    /// <summary>
    /// The five variants of the booster share a reload time and a charge count but not their buff. Collapsing the
    /// three distinct buffs into one modifier set would make three of the five variants indistinguishable.
    /// </summary>
    [Test]
    public void ConvertConsumable_VariantsPointingAtDifferentBuffs_KeepDifferentModifiers()
    {
        var variants = this.ConvertVariants(AuxiliaryTorpedoArmamentBooster);

        ModifierValues(variants["Default_Aux"]).Should().BeEquivalentTo(new Dictionary<string, float>
        {
            ["GTShotDelay"] = 0.8f,
            ["GSAlphaFactor"] = 1.1f,
            ["aimedFireProgressBonus"] = 1.2f,
            ["asReloadTimeCoeff"] = 0.8f,
        });
        ModifierValues(variants["Aux_Improved"])["aimedFireProgressBonus"].Should().Be(1.67f);
        ModifierValues(variants["Aux_Airstrike"])["asReloadTimeCoeff"].Should().Be(0.9f);
    }

    [Test]
    public void ConvertConsumable_TemplateSentinels_AreNeverEmitted()
    {
        var modifierNames = BuffBackedConsumables.SelectMany(index => this.ConvertVariants(index).Values)
            .SelectMany(variant => variant.Modifiers)
            .Select(modifier => modifier.Name)
            .ToList();

        modifierNames.Should().NotBeEmpty().And.NotIntersectWith(TemplateSentinels);
    }

    /// <summary>
    /// A buff nests its per-target maps under 22 battle entity types, only six of which are ship classes. A name
    /// built from any of the other 16 would be a modifier that can never apply to a ship.
    /// </summary>
    [Test]
    public void RetrieveModifiers_PerEntityMaps_NeverProduceNonShipClassNames()
    {
        var perEntityMaps = this.buffs.Values.SelectMany(buff => buff.RawData.Values).SelectMany(block => block.Children<JProperty>())
            .Select(property => property.Value).OfType<JObject>()
            .SelectMany(map => map.Properties().Select(property => property.Name))
            .ToList();
        var modifierNames = this.buffs.Values.SelectMany(buff => buff.RetrieveModifiers().Keys).ToList();

        perEntityMaps.Should().Contain(NonShipClassEntities, "the fixture would not exercise the filter otherwise");
        modifierNames.Should().NotBeEmpty();
        modifierNames.Should().NotContain(name => NonShipClassEntities.Any(entity => name.EndsWith("_" + entity, StringComparison.Ordinal)));
    }

    /// <summary>
    /// A stat the game merely repeats once per ship class is a single stat, not six. Splitting it would produce six
    /// modifier names that carry no metadata in place of the one that does.
    /// </summary>
    [Test]
    public void RetrieveModifiers_UniformPerShipClassMap_CollapsesToOneModifier()
    {
        var buff = this.buffs["PCOM911_SupportAuxBuff"];

        buff.RawData["modifier"]["AAAuraDamageAbsolute"]!.Should().HaveCount(6);
        buff.RetrieveModifiers().Should().Contain(new KeyValuePair<string, float>("AAAuraDamageAbsolute", 1.15f));
        buff.RetrieveModifiers().Keys.Should().NotContain(name => name.StartsWith("AAAuraDamageAbsolute_", StringComparison.Ordinal));
        ModifierValues(this.ConvertVariants(SupportAux)["Default"])["AAAuraDamageAbsolute"].Should().Be(1.15f);
    }

    /// <summary>
    /// A buff enumerates every stat the game knows and leaves the untouched ones at their identity value. Only the
    /// handful that actually change something may become modifiers.
    /// </summary>
    [Test]
    public void RetrieveModifiers_NeutralEntries_AreDropped()
    {
        var buff = this.buffs["PCOM915_AuxiliaryTorpedoArmamentBooster"];

        buff.RawData["modifier"].Children().Should().HaveCountGreaterThan(400);
        buff.RetrieveModifiers().Keys.Should().BeEquivalentTo(["GTShotDelay", "GSAlphaFactor", "aimedFireProgressBonus", "asReloadTimeCoeff", .. TemplateSentinels]);
    }

    /// <summary>
    /// Four of the reachable buffs have no modifier block at all: they carry a neutral "level" template plus one
    /// "level_N" block per tier. Reading the template instead of the tier resolves them to nothing.
    /// </summary>
    [Test]
    public void RetrieveModifiers_BuffWithoutModifierBlock_ReadsTheTier()
    {
        var buff = this.buffs["PCOM905_AirstrikeCountermeasuresDDBuff"];

        buff.RawData.Should().NotContainKey("modifier");
        buff.RawData.Should().ContainKeys("level", "level_1");
        buff.RetrieveModifiers().Should().Contain(new KeyValuePair<string, float>("vulnerabilityTorpedoShips", 0.9f))
            .And.Contain(new KeyValuePair<string, float>("vulnerabilityBombAvia", 0.7f));
    }

    /// <summary>
    /// Fire immunity is the only effect of its buff and the game stores it as a boolean, while a modifier value is
    /// a float. Skipping non-numeric entries would leave the consumable with no effect at all.
    /// </summary>
    [Test]
    public void ConvertConsumable_BooleanEffect_BecomesOne()
    {
        var variant = this.ConvertVariants(SupportFireExtinguishing)["GER_CV_10"];

        ModifierValues(variant)["fireImmunityEnabled"].Should().Be(1f);
    }

    /// <summary>
    /// The support heal restores 50% to its target and 25% to the ship using it, through two separate buffs that set
    /// the same stat. Merging them into one list by name would report the ally's figure as the user's.
    /// </summary>
    [Test]
    public void ConvertConsumable_BuffAndBuffOnSelf_ReportBothEffects()
    {
        var variant = this.ConvertVariants(SupportHeal)["BR_CV_6"];

        variant.Modifiers.Where(modifier => modifier.Name.Equals("healthRegenPercentAbsolute", StringComparison.Ordinal))
            .Should().ContainSingle().Which.Value.Should().Be(0.5f);
        variant.SelfModifiers.Where(modifier => modifier.Name.Equals("healthRegenPercentAbsolute", StringComparison.Ordinal))
            .Should().ContainSingle().Which.Value.Should().Be(0.25f);
    }

    /// <summary>
    /// A consumable that affects only the ship using it states that in its own effect list, so nothing should end up
    /// duplicated into the caster-side one.
    /// </summary>
    [Test]
    public void ConvertConsumable_WithoutABuffOnSelf_ReportsNoSelfEffects()
    {
        this.ConvertVariants(AuxiliaryTorpedoArmamentBooster)["Default_Aux"].SelfModifiers.Should().BeEmpty();
    }

    /// <summary>
    /// The emitted order has to follow the ordinal order of the stat names the game states, not the order a hash set
    /// happens to enumerate them in. Otherwise every conversion rewrites the file with a new checksum.
    /// </summary>
    /// <remarks>
    /// Asserted as an exact sequence rather than as "ascending", because the conversion renames some stats on the way
    /// out - <c>regenerationHPSpeed</c> becomes <c>consumable_regenerationHPSpeed</c> - so the emitted names are not
    /// sorted among themselves. It is the source names that are.
    /// </remarks>
    [Test]
    public void ConvertConsumable_Modifiers_FollowTheOrdinalOrderOfTheirSourceNames()
    {
        var names = this.ConvertVariants(AuxiliaryTorpedoArmamentBooster)["Default_Aux"].Modifiers.Select(modifier => modifier.Name);

        names.Should().Equal("GSAlphaFactor", "GTShotDelay", "aimedFireProgressBonus", "asReloadTimeCoeff");
    }

    /// <summary>
    /// Guards the inline path the buff resolution was added next to: a consumable that declares numbers in its own
    /// logic block must keep them.
    /// </summary>
    [Test]
    public void ConvertConsumable_InlineLogicValues_AreStillConverted()
    {
        var values = ModifierValues(this.ConvertVariants(SupportFireExtinguishing)["JAP_CV_6"]);

        values["flyAwayTime"].Should().Be(6f);
        values["timeFromHeaven"].Should().Be(2f);
        values["climbAngle"].Should().Be(15f);
    }

    /// <summary>
    /// The buff type also covers hundreds of objects that are not buffs, so the fixture doubles as a reminder that
    /// the species is what the extraction has to filter on.
    /// </summary>
    [Test]
    public void DeserializeBuff_Fixture_CarriesTheBuffTypeInfo()
    {
        var loadedBuffs = LoadBuffFixture();

        loadedBuffs.Should().NotBeEmpty();
        loadedBuffs.Should().OnlyContain(buff => buff.TypeInfo.Type == WgBuff.GameParamsType && buff.TypeInfo.Species == WgBuff.GameParamsSpecies);
    }

    private static Dictionary<string, float> ModifierValues(Consumable consumable)
    {
        return consumable.Modifiers.ToDictionary(modifier => modifier.Name, modifier => modifier.Value, StringComparer.Ordinal);
    }

    private static List<WgBuff> LoadBuffFixture()
    {
        string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "Buffs", "Common.json");
        return JsonConvert.DeserializeObject<List<WgBuff>>(File.ReadAllText(path)) ?? throw new InvalidOperationException("Unable to read the buff fixture");
    }

    private static List<WgConsumable> LoadConsumableFixture(string consumableIndex)
    {
        string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "Consumables", consumableIndex + ".json");
        return JsonConvert.DeserializeObject<List<WgConsumable>>(File.ReadAllText(path)) ?? throw new InvalidOperationException($"Unable to read fixture for {consumableIndex}");
    }

    /// <summary>
    /// Converts a single consumable and re-keys the result by variant name, dropping the consumable name the
    /// converter prefixes so that every variant of every consumable fits into one dictionary.
    /// </summary>
    private Dictionary<string, Consumable> ConvertVariants(string consumableIndex)
    {
        var result = ConsumableConverter.ConvertConsumable(LoadConsumableFixture(consumableIndex), this.modifiers, this.buffs, NullLogger.Instance);
        return result.Values.ToDictionary(consumable => consumable.ConsumableVariantName, StringComparer.Ordinal);
    }
}
