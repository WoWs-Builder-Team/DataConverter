using System.IO.Compression;

namespace DataConverter.Console.Services;

/// <summary>
/// Helper service to download gamedata.
/// </summary>
public class DataDownloader
{
    private readonly HttpClient httpClient;

    public DataDownloader(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task DownloadGameData(string dataUrl, string targetDirectory)
    {
        var zipStream = await httpClient.GetStreamAsync(dataUrl);
        var archive = new ZipArchive(zipStream);
        archive.ExtractToDirectory(targetDirectory);
    }
}
