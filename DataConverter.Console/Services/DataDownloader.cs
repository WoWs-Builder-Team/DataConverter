using System.IO.Compression;
using Microsoft.Extensions.Logging;

namespace DataConverter.Console.Services;

/// <summary>
/// Helper service to download gamedata.
/// </summary>
public class DataDownloader
{
    private readonly HttpClient httpClient;
    private readonly ILogger<DataDownloader> logger;

    public DataDownloader(HttpClient httpClient, ILogger<DataDownloader> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
    }

    public async Task DownloadGameData(string dataUrl, string targetDirectory)
    {
        logger.LogInformation("Preparing game data download from {dataUrl} to {targetDirectory}", dataUrl, targetDirectory);
        PrepareTargetDirectory(targetDirectory);
        logger.LogDebug("Directory prepared, downloading data");
        var zipStream = await httpClient.GetStreamAsync(dataUrl);
        var archive = new ZipArchive(zipStream);
        archive.ExtractToDirectory(targetDirectory);
        logger.LogInformation("Game data downloaded and extracted to {targetDirectory}", targetDirectory);
    }

    private void PrepareTargetDirectory(string targetDirectory)
    {
        var di = new DirectoryInfo(targetDirectory);
        if (!di.Exists)
        {
            logger.LogInformation("Target directory does not exist, creating it.");
            Directory.CreateDirectory(targetDirectory);
            return;
        }

        logger.LogInformation("Target directory exists, deleting all files and subdirectories.");
        foreach (var directoryInfo in di.GetDirectories())
        {
            logger.LogDebug("Deleting directory {}", directoryInfo.FullName);
            directoryInfo.Delete(true);
        }
        foreach (var fileInfo in di.GetFiles())
        {
            logger.LogDebug("Deleting file {}", fileInfo.FullName);
            fileInfo.Delete();
        }
    }
}
