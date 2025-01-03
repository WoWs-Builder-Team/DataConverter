﻿using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using DataConverter.Data;
using FluentAssertions;
using NUnit.Framework;
using WoWsShipBuilder.DataStructures;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Versioning;

namespace DataConverter.Test.ChecksumTests;

[TestFixture]
public class ChecksumTest
{
    [Test]
    public void ComputeChecksum_SimpleData_FileAndMemoryGenerationMatches_Success()
    {
        const string testData = "1234567890";
        const string fileName = "testfile.txt";
        string expectedHash = CreateInMemoryStreamHash(testData);
        File.WriteAllText(fileName, testData);

        using var fs = File.OpenRead(fileName);
        string hash = FileVersion.ComputeChecksum(fs);

        hash.Should().Be(expectedHash);
    }

    [Test]
    public void ComputeChecksum_RealisticData_FileAndMemoryGenerationMatches_Success()
    {
        string testData = JsonSerializer.Serialize(GenerateTestDictionary(50), Constants.SerializerOptions);
        const string fileName = "testfile.txt";
        string expectedHash = CreateInMemoryStreamHash(testData);
        File.WriteAllText(fileName, testData);

        using var fs = File.OpenRead(fileName);
        string hash = FileVersion.ComputeChecksum(fs);

        hash.Should().Be(expectedHash);
    }

    [Test]
    public void ComputeChecksum_RealisticData_FileAndMemoryGenerationMatches_NoChecksumMatch()
    {
        string testData = JsonSerializer.Serialize(GenerateTestDictionary(50), Constants.SerializerOptions);
        const string fileName = "testfile.txt";
        string expectedHash = CreateInMemoryStreamHash(testData);
        File.WriteAllText(fileName, testData);
        File.AppendAllText(fileName, "testtext");

        using var fs = File.OpenRead(fileName);
        string hash = FileVersion.ComputeChecksum(fs);

        hash.Should().NotBe(expectedHash);
    }

    [Test]
    public void ComputeChecksum_StringStreamEqual_True()
    {
        const string testData = "1234567890";
        string expectedHash = CreateInMemoryStreamHash(testData);

        string actualHash = FileVersion.ComputeChecksum(testData);

        actualHash.Should().Be(expectedHash);
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

    private static string CreateInMemoryStreamHash(string content)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream);
        writer.Write(content);
        writer.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);
        return FileVersion.ComputeChecksum(memoryStream);
    }
}
