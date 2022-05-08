using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using WoWsShipBuilder.DataStructures;

namespace DataConverter.Test.ChecksumTests;

[TestFixture]
public class ChecksumTest
{
    [Test]
    public void ComputeChecksum_SimpleData_FileAndMemoryGenerationMatches_Success()
    {
        const string testData = "1234567890";
        const string fileName = "testfile.txt";
        string expectedHash = CreateInMemoryHash(testData);
        File.WriteAllText(fileName, testData);

        using var fs = File.OpenRead(fileName);
        string hash = FileVersion.ComputeChecksum(fs);

        hash.Should().Be(expectedHash);
    }

    [Test]
    public void ComputeChecksum_RealisticData_FileAndMemoryGenerationMatches_Success()
    {
        string testData = JsonConvert.SerializeObject(GenerateTestDictionary(50));
        const string fileName = "testfile.txt";
        string expectedHash = CreateInMemoryHash(testData);
        File.WriteAllText(fileName, testData);

        using var fs = File.OpenRead(fileName);
        string hash = FileVersion.ComputeChecksum(fs);

        hash.Should().Be(expectedHash);
    }

    [Test]
    public void ComputeChecksum_RealisticData_FileAndMemoryGenerationMatches_NoChecksumMatch()
    {
        string testData = JsonConvert.SerializeObject(GenerateTestDictionary(50));
        const string fileName = "testfile.txt";
        string expectedHash = CreateInMemoryHash(testData);
        File.WriteAllText(fileName, testData);
        File.AppendAllText(fileName, "testtext");

        using var fs = File.OpenRead(fileName);
        string hash = FileVersion.ComputeChecksum(fs);

        hash.Should().NotBe(expectedHash);
    }

    private static Dictionary<string, Ship> GenerateTestDictionary(int targetSize)
    {
        var shipDictionary = new Dictionary<string, Ship>();
        for (var i = 0; i < targetSize; i++)
        {
            shipDictionary.Add($"PASA{i}", new() { Id = targetSize, Index = $"PASA{i}", Tier = 9, ShipCategory = ShipCategory.TechTree, ShipClass = ShipClass.Battleship });
        }

        return shipDictionary;
    }

    private static string CreateInMemoryHash(string content)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream);
        writer.Write(content);
        writer.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);
        return FileVersion.ComputeChecksum(memoryStream);
    }
}
