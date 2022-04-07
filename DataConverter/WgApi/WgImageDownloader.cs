using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace DataConverter.WgApi;

public static class WgImageDownloader
{
    private const string BaseUrl = @"https://api.worldofwarships.eu/wows/encyclopedia/ships/?application_id=6d563fd75651dbf9de009418d3ee7f56&fields=ship_id_str%2C+images.large&page_no=";

    private static async Task<WgApiResponse> SendShipRequest(HttpClient client, int pageCounter)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        var apiResponse = await client.GetFromJsonAsync<WgApiResponse>(BaseUrl + pageCounter, options) ?? throw new InvalidOperationException("Unable to deserialize version info object.");
        return apiResponse;
    }

    private static async Task<Dictionary<string, string>> GetAllImageLinks(HttpClient client)
    {
        var dict = new Dictionary<string, string>();
        var currentPage = 1;
        int maxPage;

        do
        {
            Console.WriteLine("Getting page: " + currentPage);
            var response = await SendShipRequest(client, currentPage);
            if (response.Status.Equals("ok"))
            {
                maxPage = response.Meta.PageTotal;
                currentPage++;
                var shipDict = response.Data.ToDictionary(x => x.Value.ShipIdStr, x => x.Value.Images.Large);
                dict.AddRange(shipDict);
            }
            else
            {
                Console.WriteLine("Error in getting page: " + currentPage);
                maxPage = -1;
            }
        }
        while (currentPage <= maxPage);

        return dict;
    }

    public static async Task DownloadImages(HttpClient client, List<string> shipId, string outputFolder)
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }
        var dict = await GetAllImageLinks(client);
        dict = dict.Where(x => shipId.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);

        await Parallel.ForEachAsync(dict, async (entry, ct) =>
        {
            (string? index, string? uri) = entry;
            string outputPath = outputFolder + index + ".png";
            byte[] fileBytes = await client.GetByteArrayAsync(uri, ct);
            await File.WriteAllBytesAsync(outputPath, fileBytes, ct);
        });
    }
}
