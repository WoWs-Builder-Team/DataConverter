using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DataConverter.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WoWsShipBuilder.DataStructures;

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
        var oldVersionInfo = await client.GetJsonAsync<VersionInfo>($"https://{cdnHost}/api/{gameVersion.VersionType.ToString().ToLowerInvariant()}/VersionInfo.json");
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
        string fileContent = JsonConvert.SerializeObject(versionInfo);
        await File.WriteAllTextAsync(Path.Join(outputBasePath, VersionInfoFileName), fileContent);
    }

    internal static FileVersion CompareVersions(ResultFileContainer fileContainer, List<FileVersion> oldVersions, int currentVersionCode)
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

        Dictionary<string, List<FileVersion>> newFileVersions = new();
        foreach (var resultCategory in resultCategories)
        {
            List<FileVersion> oldVersions = oldVersionInfo.GetCategoryVersions(resultCategory.Key) ?? new();
            List<FileVersion> fileVersions = resultCategory.Value
                .Select(f => CompareVersions(f, oldVersions, oldVersionInfo.CurrentVersionCode + 1))
                .ToList();
            newFileVersions[resultCategory.Key] = fileVersions;
        }

        var newGameVersion = gameVersion with { DataIteration = gameVersion.DataIteration + 1 };
        logger.LogInformation("Creating version info for current version {}", newGameVersion);
        var dataStructureVersion = Assembly.GetAssembly(typeof(Ship))!.GetName().Version!;
        return new(newFileVersions, oldVersionInfo.CurrentVersionCode + 1, newGameVersion, oldVersionInfo.CurrentVersion)
        {
            DataStructuresVersion = dataStructureVersion,
        };
    }
}
