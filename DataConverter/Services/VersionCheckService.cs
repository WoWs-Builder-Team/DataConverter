using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using DataConverter.Data;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.DataStructures.Ship;
using WoWsShipBuilder.DataStructures.Versioning;

namespace DataConverter.Services;

internal class VersionCheckService : IVersionCheckService
{
    private const string VersionInfoFileName = "VersionInfo.json";

    private readonly HttpClient client;

    private readonly ILogger<VersionCheckService> logger;

    public VersionCheckService(ILogger<VersionCheckService> logger, HttpClient client)
    {
        this.logger = logger;
        this.client = client;
    }

    public async Task<VersionInfo> CheckFileVersionsAsync(DataConversionResult conversionResult, GameVersion gameVersion, string cdnHost)
    {
        var oldVersionInfo = await client.GetFromJsonAsync<VersionInfo>($"https://{cdnHost}/api/{gameVersion.VersionType.ToString().ToLowerInvariant()}/VersionInfo.json");
        if (oldVersionInfo is null)
        {
            logger.LogWarning("Unable to retrieve version info from cdn host {} for server type {}", cdnHost, gameVersion.VersionType);
            oldVersionInfo = VersionInfo.Default;
        }

        return CheckFileVersions(conversionResult, gameVersion, oldVersionInfo);
    }

    public async Task WriteVersionInfo(VersionInfo versionInfo, string outputBasePath)
    {
        Directory.CreateDirectory(outputBasePath);
        string fileContent = JsonSerializer.Serialize(versionInfo, Constants.SerializerOptions);
        await File.WriteAllTextAsync(Path.Combine(outputBasePath, VersionInfoFileName), fileContent);
    }

    internal static FileVersion CompareVersions(ResultFileContainer fileContainer, ImmutableList<FileVersion> oldVersions, int currentVersionCode)
    {
        string checksum = FileVersion.ComputeChecksum(fileContainer.Content);
        var oldFileVersion = oldVersions.Find(v => v.FileName.Equals(fileContainer.Filename, StringComparison.InvariantCultureIgnoreCase));
        if (oldFileVersion is null || !checksum.Equals(oldFileVersion.Checksum))
        {
            return new(fileContainer.Filename, currentVersionCode, checksum);
        }

        return oldFileVersion;
    }

    internal VersionInfo CheckFileVersions(DataConversionResult conversionResult, GameVersion gameVersion, VersionInfo oldVersionInfo)
    {
        var resultCategories = conversionResult.Files
            .GroupBy(file => file.Category)
            .Select(f => new KeyValuePair<string, List<ResultFileContainer>>(f.Key, f.ToList()));

        Dictionary<string, ImmutableList<FileVersion>> newFileVersions = new();
        foreach (var resultCategory in resultCategories)
        {
            ImmutableList<FileVersion> oldVersions = oldVersionInfo.GetCategoryVersions(resultCategory.Key) ?? ImmutableList<FileVersion>.Empty;
            List<FileVersion> fileVersions = resultCategory.Value
                .Select(f => CompareVersions(f, oldVersions, oldVersionInfo.CurrentVersionCode + 1))
                .ToList();
            newFileVersions[resultCategory.Key] = fileVersions.ToImmutableList();
        }

        int dataIteration = oldVersionInfo.CurrentVersion.MainVersion == gameVersion.MainVersion ? oldVersionInfo.CurrentVersion.DataIteration + 1 : 1;
        var newGameVersion = gameVersion with { DataIteration = dataIteration };
        logger.LogInformation("Creating version info for current version {}", newGameVersion);
        var dataStructureVersion = Assembly.GetAssembly(typeof(Ship))!.GetName().Version!;
        return new(newFileVersions.ToImmutableDictionary(), oldVersionInfo.CurrentVersionCode + 1, newGameVersion, oldVersionInfo.CurrentVersion)
        {
            DataStructuresVersion = dataStructureVersion,
        };
    }
}
