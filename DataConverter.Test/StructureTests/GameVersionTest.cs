using FluentAssertions;
using NUnit.Framework;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Versioning;

namespace DataConverter.Test.StructureTests;

[TestFixture]
public class GameVersionTest
{
    [Test]
    public void CompareTo_SameMajorNewerIteration_Greater()
    {
        var currentVersion = new GameVersion(new(0, 11, 1), GameVersionType.Live, 2);
        var otherVersion = new GameVersion(new(0, 11, 1), GameVersionType.Live, 1);

        int result = currentVersion.CompareTo(otherVersion);

        result.Should().BeGreaterOrEqualTo(1);
    }

    [Test]
    public void CompareTo_NewerMajorSameIteration_Greater()
    {
        var currentVersion = new GameVersion(new(0, 11, 1), GameVersionType.Live, 1);
        var otherVersion = new GameVersion(new(0, 11, 0), GameVersionType.Live, 1);

        int result = currentVersion.CompareTo(otherVersion);

        result.Should().BeGreaterOrEqualTo(1);
    }

    [Test]
    public void CompareTo_SameMajorOlderIteration_Less()
    {
        var currentVersion = new GameVersion(new(0, 11, 1), GameVersionType.Live, 1);
        var otherVersion = new GameVersion(new(0, 11, 1), GameVersionType.Live, 2);

        int result = currentVersion.CompareTo(otherVersion);

        result.Should().BeLessOrEqualTo(-1);
    }

    [Test]
    public void CompareTo_OlderMajorSameIteration_Less()
    {
        var currentVersion = new GameVersion(new(0, 11, 0), GameVersionType.Live, 1);
        var otherVersion = new GameVersion(new(0, 11, 1), GameVersionType.Live, 1);

        int result = currentVersion.CompareTo(otherVersion);

        result.Should().BeLessOrEqualTo(-1);
    }
}
