using System.Collections.Immutable;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using WoWsShipBuilder.DataStructures.Modifiers;

namespace DataConverter.Test.StructureTests;

[TestFixture]
public class ModifierDisplayTest
{
    /// <summary>
    /// ToDisplayValue formats with the current culture, so the decimal separator varies by machine. Pin it here so
    /// the assertions describe the rounding rather than the host's locale.
    /// </summary>
    [SetUp]
    public void PinCulture()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    /// <summary>
    /// Value is a float, so a coefficient that is not exactly representable produced a long decimal tail such as
    /// "+10.000002384185791". PositivePercentage was the only percentage kind missing the rounding its siblings apply.
    /// </summary>
    [TestCase(1.1f, "+10")]
    [TestCase(1.3f, "+30")]
    [TestCase(1.05f, "+5")]
    [TestCase(1.25f, "+25")]
    [TestCase(1.075f, "+7.5")]
    public void ToDisplayValue_PositivePercentage_HasNoFloatingPointTail(float value, string expected)
    {
        var template = new Modifier("test", 0f, null, null, Unit.Percent, ImmutableHashSet<string>.Empty, DisplayValueProcessingKind.PositivePercentage, ValueProcessingKind.Multiplier);

        var modifier = new Modifier("test", value, "test", template);

        modifier.ToDisplayValue().Should().Be(expected);
    }
}
