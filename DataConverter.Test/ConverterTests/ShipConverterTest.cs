using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataConverter.Converters;
using DataConverter.Data;
using DataConverter.JsonData;
using DataConverter.Services;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Modifiers;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Versioning;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;
using Ship = WoWsShipBuilder.DataStructures.Ship.Ship;

namespace DataConverter.Test.ConverterTests;

/// <summary>
/// Tests for the Aimed Fire anti-air mechanic added in game update 15.7, driven by per-ship fixtures extracted from
/// game build 13015811.
/// </summary>
[TestFixture]
public class ShipConverterTest
{
    /// <summary>Tier 10 German cruiser, an ordinary ship with air defense and all three AA auras.</summary>
    private const string Hindenburg = "PGSC110";

    /// <summary>
    /// One of the three ships whose Aimed Fire multipliers are not the uniform 1.5 / 2.0, and the only one of them
    /// whose values differ between ship classes.
    /// </summary>
    private const string Flint = "PASC707";

    /// <summary>Tier 4 German cruiser with an air defense module that defines no AA aura at all.</summary>
    private const string Karlsruhe = "PGSC104";

    /// <summary>Tier 3 German cruiser without any air defense module.</summary>
    private const string Kolberg = "PGSC103";

    private static readonly string[] ShipsWithAirDefense = [Hindenburg, Flint, Karlsruhe];

    private Dictionary<string, Modifier> modifiers = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        DataCache.CurrentVersion = new GameVersion(new Version(0, 15, 7), GameVersionType.Live, 1);
        this.modifiers = ModifierProcessingService.LoadEmbeddedModifiers();
    }

    [TestCaseSource(nameof(ShipsWithAirDefense))]
    public void ConvertShip_ShipWithAirDefense_EveryHullCarriesAimedFire(string shipIndex)
    {
        var ship = this.ConvertSingle(shipIndex);

        ship.Hulls.Should().NotBeEmpty();
        ship.Hulls.Values.Should().OnlyContain(hull => hull.AntiAir!.AimedFire != null);
    }

    [Test]
    public void ConvertShip_AimedFire_MatchesTheGameValues()
    {
        var aimedFire = AimedFireOfFirstHull(this.ConvertSingle(Hindenburg));

        aimedFire.RequiredCharge.Should().Be(100m);
        aimedFire.ChargeSpendingRate.Should().Be(2.5m);
        aimedFire.DecrementDelay.Should().Be(130m);
        aimedFire.DecrementRate.Should().Be(5m);
        aimedFire.InstantDamageCooldown.Should().Be(5m);
        aimedFire.AuraDamageMultiplier.Should().Be(1.5m);
        aimedFire.BubbleDamageMultiplier.Should().Be(2m);
    }

    /// <summary>
    /// The game states the charge gain as an increment on a repeating timer rather than as a rate. 2.5 per second
    /// against a required charge of 100 means 40 seconds to a full bar.
    /// </summary>
    [Test]
    public void ConvertShip_ChargeGainRate_IsTheIncrementDividedByTheTimerPeriod()
    {
        var aimedFire = AimedFireOfFirstHull(this.ConvertSingle(Hindenburg));

        aimedFire.ChargeGainRate.Should().Be(2.5m);
        (aimedFire.RequiredCharge / aimedFire.ChargeGainRate).Should().Be(40m);
    }

    /// <summary>
    /// The instant damage share is keyed by the class of the ship being shot at, not by the class of the ship that
    /// owns the module: an ordinary cruiser still reports the higher destroyer value.
    /// </summary>
    [Test]
    public void ConvertShip_InstantDamagePercentage_StaysAPerClassMap()
    {
        var aimedFire = AimedFireOfFirstHull(this.ConvertSingle(Hindenburg));

        aimedFire.InstantDamagePercentage.Should().HaveCount(6);
        aimedFire.InstantDamagePercentage[ShipClass.Destroyer].Should().Be(0.05m);
        aimedFire.InstantDamagePercentage[ShipClass.Cruiser].Should().Be(0.035m);
        aimedFire.InstantDamagePercentage[ShipClass.Battleship].Should().Be(0.035m);
    }

    /// <summary>
    /// Flint's multipliers differ per class of the owning ship (1.75 / 4.0 for a cruiser, 1.25 / 2.0 otherwise), so
    /// picking the wrong key is visible. Flint is a cruiser and must get the cruiser values.
    /// </summary>
    [Test]
    public void ConvertShip_DamageMultipliers_AreResolvedForTheOwningShipClass()
    {
        var ship = this.ConvertSingle(Flint);
        var aimedFire = AimedFireOfFirstHull(ship);

        ship.ShipClass.Should().Be(ShipClass.Cruiser);
        aimedFire.AuraDamageMultiplier.Should().Be(1.75m);
        aimedFire.BubbleDamageMultiplier.Should().Be(4m);

        // Flint is also tuned differently on the timings, which rules out the fixture matching by accident.
        aimedFire.DecrementDelay.Should().Be(70m);
        aimedFire.InstantDamagePercentage[ShipClass.Destroyer].Should().Be(0.035m);
    }

    /// <summary>
    /// Six ships carry an air defense module that defines Aimed Fire but not a single AA aura. Aimed Fire hangs off
    /// the same object as the auras, so it must not be tied to their presence.
    /// </summary>
    [Test]
    public void ConvertShip_AirDefenseWithoutAura_StillCarriesAimedFire()
    {
        var airDefenseModules = LoadFixture(Karlsruhe).Single().ModulesArmaments.Values.OfType<WgAirDefense>().ToList();

        airDefenseModules.Should().NotBeEmpty();
        airDefenseModules.Should().OnlyContain(module => module.AntiAirAuras.Count == 0);

        this.ConvertSingle(Karlsruhe).Hulls.Values.Should().OnlyContain(hull => hull.AntiAir!.AimedFire != null);
    }

    [Test]
    public void ConvertShip_ShipWithoutAirDefense_HasNoAimedFire()
    {
        var ship = this.ConvertSingle(Kolberg);

        ship.Hulls.Should().NotBeEmpty();
        ship.Hulls.Values.Should().OnlyContain(hull => hull.AntiAir!.AimedFire == null);
    }

    /// <summary>
    /// The AA auras are found by scanning the air defense module's extension data for objects carrying a hitChance.
    /// Aimed Fire is a nested object without one, so it has to be read as a typed property instead of being left in
    /// the extension data where a looser aura filter would pick it up.
    /// </summary>
    [Test]
    public void DeserializeShip_AimedFire_IsNotPartOfTheExtensionData()
    {
        var airDefenseModules = LoadFixture(Hindenburg).Single().ModulesArmaments.Values.OfType<WgAirDefense>().ToList();

        airDefenseModules.Should().NotBeEmpty();
        airDefenseModules.Should().OnlyContain(module => module.AimedFire != null);
        airDefenseModules.SelectMany(module => module.Other.Keys).Should().NotContain("AimedFire");
        airDefenseModules.SelectMany(module => module.AntiAirAuras.Keys).Should().NotContain("AimedFire");
    }

    /// <summary>
    /// Guards the aura conversion the Aimed Fire change sits next to: the numbers every ship's AA is built from must
    /// survive untouched.
    /// </summary>
    [Test]
    public void ConvertShip_AntiAirAuras_AreStillConverted()
    {
        var antiAir = this.ConvertSingle(Hindenburg).Hulls.Values.Select(hull => hull.AntiAir!).Last();

        antiAir.LongRangeAura!.ConstantDps.Should().BeGreaterThan(0m);
        antiAir.LongRangeAura.FlakCloudsNumber.Should().BeGreaterThan(0);
        antiAir.MediumRangeAura!.ConstantDps.Should().BeGreaterThan(0m);
        antiAir.ShortRangeAura!.ConstantDps.Should().BeGreaterThan(0m);
    }

    private static AntiAirAimedFire AimedFireOfFirstHull(Ship ship)
    {
        return ship.Hulls.Values.First().AntiAir!.AimedFire!;
    }

    private static List<WgShip> LoadFixture(string shipIndex)
    {
        string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "Ships", shipIndex + ".json");
        return JsonConvert.DeserializeObject<List<WgShip>>(File.ReadAllText(path)) ?? throw new InvalidOperationException($"Unable to read fixture for {shipIndex}");
    }

    private Ship ConvertSingle(string shipIndex)
    {
        var result = ShipConverter.ConvertShips(LoadFixture(shipIndex), "Test", new ShiptoolData(), null, this.modifiers, new Dictionary<long, int>());
        result.Should().HaveCount(1, "the fixture contains exactly one ship");
        return result.Values.Single();
    }
}
