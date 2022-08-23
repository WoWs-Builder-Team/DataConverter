using System;
using DataConverter.Converters;
using FluentAssertions;
using NUnit.Framework;
using WoWsShipBuilder.DataStructures;

namespace DataConverter.Test.StructureTests;

public class GameVersionConverterTest
{
    [Test]
    public void FromVersionString_ValidString_LiveNoSuffix()
    {
        const string testString = "0.11.0#1";
        var expectedVersion = new GameVersion(new(0, 11, 0), GameVersionType.Live, 1);

        var gameVersion = GameVersionConverter.FromVersionString(testString);

        gameVersion.Should().Be(expectedVersion);
    }

    [Test]
    public void FromVersionString_ValidString_PtsNoSuffix()
    {
        const string testString = "0.11.0#2-pts";
        var expectedVersion = new GameVersion(new(0, 11, 0), GameVersionType.Pts, 2);

        var gameVersion = GameVersionConverter.FromVersionString(testString);

        gameVersion.Should().Be(expectedVersion);
    }

    [Test]
    public void FromVersionString_ValidString_LiveDev1Suffix()
    {
        const string testString = "0.11.0#1-dev1";
        var expectedVersion = new GameVersion(new(0, 11, 0), GameVersionType.Dev1, 1);

        var gameVersion = GameVersionConverter.FromVersionString(testString);

        gameVersion.Should().Be(expectedVersion);
    }

    [Test]
    public void FromVersionString_InvalidString_ThrowsException()
    {
        const string testString = "0.11.0#2#3-pts";

        Action action = () => _ = GameVersionConverter.FromVersionString(testString);

        action.Should().Throw<FormatException>();
    }
}
