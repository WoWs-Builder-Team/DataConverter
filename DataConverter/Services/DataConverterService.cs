using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DataConverter.Converters;
using DataConverter.Data;
using DataConverter.JsonData;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WowsShipBuilder.GameParamsExtractor.WGStructure;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Captain;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Projectile;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

namespace DataConverter.Services;

internal class DataConverterService : IDataConverterService
{
    private readonly ILogger<DataConverterService> logger;

    private readonly HttpClient client;

    private readonly JsonSerializerSettings serializerSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new ShouldSerializeContractResolver(),
    };

    private readonly ConcurrentBag<string> reportedTypes = new();

    public DataConverterService(ILogger<DataConverterService> logger, HttpClient client)
    {
        this.logger = logger;
        this.client = client;
    }

    public async Task<DataConversionResult> ConvertRefinedData(Dictionary<string, Dictionary<string, List<WgObject>>> refinedData)
    {
        var resultFiles = new List<ResultFileContainer>();
        var counter = 0;
        Task<ShiptoolData> shipToolDataTask = LoadShiptoolData();
        foreach ((string categoryName, Dictionary<string, List<WgObject>> nationDictionary) in refinedData)
        {
            await Parallel.ForEachAsync(nationDictionary, async (nationDataPair, _) =>
            {
                (string? nation, List<WgObject>? data) = nationDataPair;

                logger.LogInformation("Converting category: {Category} - nation: {Nation}", categoryName, nation);
                counter++;
                if (counter % 10 == 0)
                {
                    logger.LogInformation("Processed {Counter} dictionaries", counter);
                }

                string fileName = nation + ".json";
                string? convertedFileContent;
                object convertedData;

                switch (categoryName)
                {
                    case "Ability":
                        convertedData = ConsumableConverter.ConvertConsumable(data.Cast<WgConsumable>());
                        convertedFileContent = JsonConvert.SerializeObject(convertedData, serializerSettings);

                        break;
                    case "Aircraft":
                        convertedData = AircraftConverter.ConvertAircraft(data.Cast<WgAircraft>());
                        convertedFileContent = JsonConvert.SerializeObject(convertedData, serializerSettings);

                        break;
                    case "Crew":
                        string skillsList = CaptainConverter.LoadEmbeddedSkillData();
                        convertedData = CaptainConverter.ConvertCaptain(data.Cast<WgCaptain>(), skillsList, nation.Equals("Common"));
                        convertedFileContent = JsonConvert.SerializeObject(convertedData, serializerSettings);

                        break;
                    case "Modernization":
                        convertedData = ModernizationConverter.ConvertModernization(data.Cast<WgModernization>());
                        convertedFileContent = JsonConvert.SerializeObject(convertedData, serializerSettings);

                        break;
                    case "Projectile":
                        var filteredData = data.OfType<WgProjectile>();
                        convertedData = ProjectileConverter.ConvertProjectile(filteredData, logger);
                        convertedFileContent = JsonConvert.SerializeObject(convertedData, serializerSettings);

                        break;
                    case "Ship":
                        logger.LogInformation("Ships to process for {Nation}: {Count}", nation, data.Count);
                        convertedData = ShipConverter.ConvertShips(data.Cast<WgShip>(), nation, await shipToolDataTask, logger);
                        convertedFileContent = JsonConvert.SerializeObject(convertedData, serializerSettings);

                        break;
                    case "Unit":
                        convertedData = ModuleConverter.ConvertModule(data.Cast<WgModule>());
                        convertedFileContent = JsonConvert.SerializeObject(convertedData, serializerSettings);

                        break;
                    case "Exterior":
                        convertedData = ExteriorConverter.ConvertExterior(data.Cast<WgExterior>(), logger);
                        convertedFileContent = JsonConvert.SerializeObject(convertedData, serializerSettings);

                        break;
                    default:
                        convertedFileContent = null;
                        if (!reportedTypes.Contains(categoryName))
                        {
                            reportedTypes.Add(categoryName);
                            logger.LogWarning("Type not found: {Category}", categoryName);
                        }

                        break;
                }

                if (convertedFileContent is not null)
                {
                    ResultFileContainer resultFileContainer = new(convertedFileContent, categoryName, fileName);
                    resultFiles.Add(resultFileContainer);
                }
            });
        }

        string shipSummaryString = JsonConvert.SerializeObject(ShipConverter.ShipSummaries);
        resultFiles.Add(new(shipSummaryString, "Summary", "Common.json"));

        return new(resultFiles);
    }

    public async Task WriteConvertedData(DataConversionResult convertedData, string outputBasePath)
    {
        foreach (var resultFile in convertedData.Files)
        {
            await resultFile.WriteFileAsync(outputBasePath);
        }
    }

    private async Task<ShiptoolData> LoadShiptoolData()
    {
        logger.LogInformation("Fetching remote json data from shiptool...");
        try
        {
            string content = await client.GetStringAsync(Constants.ShiptoolDataUrl);
            logger.LogInformation("Received remote json data from shiptool");
            return JsonConvert.DeserializeObject<ShiptoolData>(content)!;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while downloading shiptool data");
            return new();
        }
    }
}
