using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DataConverter.Data;
using DataConverter.Services;
using FluentAssertions;
using NUnit.Framework;
using WoWsShipBuilder.DataStructures.Modifiers;

namespace DataConverter.Test.StructureTests;

/// <summary>
/// Guards the metadata file that drives how every modifier is displayed and applied.
/// </summary>
[TestFixture]
public class ModifiersFileTest
{
    /// <summary>
    /// Modifier.ToDisplayValue and Modifier.ApplyModifier both throw on NotAssigned, so an incomplete entry is a
    /// crash in the consuming application rather than a missing label.
    /// </summary>
    [Test]
    public void EmbeddedModifiers_HaveNoUnassignedProcessingKinds()
    {
        var modifiers = ModifierProcessingService.LoadEmbeddedModifiers();

        modifiers.Values.Where(modifier => modifier.DisplayValueProcessingKind == DisplayValueProcessingKind.NotAssigned)
            .Select(modifier => modifier.Name).Should().BeEmpty();
        modifiers.Values.Where(modifier => modifier.ValueProcessingKind == ValueProcessingKind.NotAssigned)
            .Select(modifier => modifier.Name).Should().BeEmpty();
        modifiers.Values.Where(modifier => modifier.Unit == Unit.NotAssigned)
            .Select(modifier => modifier.Name).Should().BeEmpty();
    }

    /// <summary>
    /// A modifier with neither localization key renders as a blank row. An empty game key is the explicit marker for
    /// "known to have no game string", so only null counts as missing.
    /// </summary>
    [Test]
    public void EmbeddedModifiers_AllHaveALocalizationKey()
    {
        var modifiers = ModifierProcessingService.LoadEmbeddedModifiers();

        modifiers.Values.Where(modifier => modifier.GameLocalizationKey is null && modifier.AppLocalizationKey is null)
            .Select(modifier => modifier.Name).Should().BeEmpty();
    }

    /// <summary>
    /// A modifier that is actually applied has to say what it applies to, or ApplyModifiers silently matches nothing.
    /// </summary>
    [Test]
    public void EmbeddedModifiers_ThatAreAppliedDeclareAffectedProperties()
    {
        var modifiers = ModifierProcessingService.LoadEmbeddedModifiers();

        modifiers.Values
            .Where(modifier => modifier.ValueProcessingKind != ValueProcessingKind.None && modifier.AffectedProperties.IsEmpty)
            .Select(modifier => modifier.Name).Should().BeEmpty();
    }

    /// <summary>
    /// LoadEmbeddedModifiers keys the file by name, so a duplicate throws there rather than failing an assertion.
    /// Assert on the raw list instead, and report the offending name rather than an ArgumentException.
    /// </summary>
    [Test]
    public void EmbeddedModifiers_HaveNoDuplicateNames()
    {
        using var stream = typeof(ModifierProcessingService).Assembly.GetManifestResourceStream("DataConverter.JsonData.Modifiers.json")!;
        using var reader = new StreamReader(stream);
        var modifiers = JsonSerializer.Deserialize<List<Modifier>>(reader.ReadToEnd(), Constants.ModifierSerializerOptions)!;

        modifiers.Should().NotBeEmpty();
        modifiers.Select(modifier => modifier.Name).Should().OnlyHaveUniqueItems();
    }
}
