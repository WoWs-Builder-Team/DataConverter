using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using WoWsShipBuilder.DataStructures;

namespace DataConverter.WgApi;

public static class WgImageDownloader
{
    private static string baseUrl = @"https://api.worldofwarships.eu/wows/encyclopedia/ships/?application_id=6d563fd75651dbf9de009418d3ee7f56&fields=ship_id_str%2C+images.large&page_no=";

    private static WgApiResponse SendShipRequest(HttpClient client,  int pageCounter)
    {
        using Stream stream = client.GetStreamAsync(baseUrl + pageCounter).GetAwaiter().GetResult();
        using var streamReader = new StreamReader(stream);
        using var jsonReader = new JsonTextReader(streamReader);
        var jsonSerializer = new JsonSerializer();
        var response = jsonSerializer.Deserialize<WgApiResponse>(jsonReader) ?? throw new InvalidOperationException("Unable to deserialize version info object.");
        return response;
    }

    private static Dictionary<string, string> GetAllImageLinks(HttpClient client)
    {
        var dict = new Dictionary<string, string>();
        int currentPage = 1;
        int maxPage = 10;

        while (currentPage <= maxPage)
        {
            Console.WriteLine("Getting page: " + currentPage);
            var response = SendShipRequest(client, currentPage);
            if (response.status.Equals("ok"))
            {
                maxPage = response.meta.page_total;
                currentPage++;
                var shipDict = response.data.ToDictionary(x => x.Value.ship_id_str, x => x.Value.images.large);
                dict.AddRange(shipDict);
            }
            else
            {
                Console.WriteLine("Error in getting page: " + currentPage);
            }
        }

        return dict;
    }

    public static void DownloadImages(HttpClient client, List<string> shipId, string outputFolder)
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }
        var dict = GetAllImageLinks(client);
        dict = dict.Where(x => shipId.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);

        foreach (var (index, uri) in dict)
        {
            var outputPath = outputFolder + index + ".png";
            byte[] fileBytes = client.GetByteArrayAsync(uri).GetAwaiter().GetResult();
            File.WriteAllBytes(outputPath, fileBytes);
        }
    }
}
