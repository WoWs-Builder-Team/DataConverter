using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using DataConverter.Data;
using DataConverter.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Versioning;

namespace DataConverter.Test.Services;

[TestFixture]
public class VersionCheckServiceTest
{
    [Test]
    public void CompareVersions_SameVersion_OldVersionReturned()
    {
        const string testContent = "123456789";
        const string testCategory = "Ship";
        const string testFile = "USA.json";
        const int currentVersion = 2;
        string contentHash = FileVersion.ComputeChecksum(testContent);
        var oldFileVersion = new FileVersion(testFile, currentVersion - 1, contentHash);
        var oldVersions = new List<FileVersion>
        {
            oldFileVersion,
        }.ToImmutableList();
        var fileContainer = new ResultFileContainer(testContent, testCategory, testFile);

        var result = VersionCheckService.CompareVersions(fileContainer, oldVersions, currentVersion);

        result.Should().Be(oldFileVersion);
    }

    [Test]
    public void CompareVersions_OldVersionNull_NewVersionReturned()
    {
        const string testContent = "123456789";
        const string testCategory = "Ship";
        const string testFile = "USA.json";
        const int currentVersion = 2;
        var oldVersions = ImmutableList<FileVersion>.Empty;
        var fileContainer = new ResultFileContainer(testContent, testCategory, testFile);

        var result = VersionCheckService.CompareVersions(fileContainer, oldVersions, currentVersion);

        result.FileName.Should().Be(testFile);
        result.Version.Should().Be(currentVersion);
    }

    [Test]
    public void CompareVersions_DifferentFileContent_NewVersionReturned()
    {
        const string oldTestContent = "987654321";
        const string testContent = "123456789";
        const string testCategory = "Ship";
        const string testFile = "USA.json";
        const int currentVersion = 2;
        string oldContentHash = FileVersion.ComputeChecksum(oldTestContent);
        string contentHash = FileVersion.ComputeChecksum(testContent);
        var oldFileVersion = new FileVersion(testFile, currentVersion - 1, oldContentHash);
        var oldVersions = new List<FileVersion>
        {
            oldFileVersion,
        }.ToImmutableList();
        var fileContainer = new ResultFileContainer(testContent, testCategory, testFile);

        var result = VersionCheckService.CompareVersions(fileContainer, oldVersions, currentVersion);

        result.FileName.Should().Be(testFile);
        result.Version.Should().Be(currentVersion);
        result.Checksum.Should().Be(contentHash);
    }

    [Test]
    public void CheckFileVersions_NoFilesSameMainVersion_IterationIncreased()
    {
        var client = new HttpClient();
        var versionCheckService = new VersionCheckService(NullLogger<VersionCheckService>.Instance, client);
        var conversionResult = new DataConversionResult(new(), new());
        var oldGameVersion = new GameVersion(new(0, 11, 7), GameVersionType.Live,1);
        var oldVersionInfo = new VersionInfo(ImmutableDictionary<string, ImmutableList<FileVersion>>.Empty, 1, oldGameVersion);
        var expectedVersion = oldGameVersion with { DataIteration = 2 };

        var result = versionCheckService.CheckFileVersions(conversionResult, oldGameVersion, oldVersionInfo);

        result.Categories.Should().BeEmpty();
        result.CurrentVersion.Should().Be(expectedVersion);
        result.CurrentVersionCode.Should().Be(2);
    }
}
