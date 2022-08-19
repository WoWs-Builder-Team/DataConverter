using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataConverter.Converters;
using DataConverter.Data;
using GameParamsExtractor.WGStructure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WowsShipBuilder.GameParamsExtractor.WGStructure;

namespace DataConverter.Services;

internal class DataConverterService : IDataConverterService
{
    private readonly ILogger<DataConverterService> logger;

    private readonly JsonSerializerSettings serializerSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new ShouldSerializeContractResolver(),
    };

    private readonly ConcurrentBag<string> reportedTypes = new();

    public DataConverterService(ILogger<DataConverterService> logger)
    {
        this.logger = logger;
    }

    public DataConversionResult ConvertRefinedData(Dictionary<string, Dictionary<string, List<WGObject>>> refinedData)
    {
        var resultFiles = new List<ResultFileContainer>();
        var counter = 0;
        foreach ((string categoryName, var nationDictionary) in refinedData)
        {
            Parallel.ForEach(nationDictionary, nationDataPair =>
            {
                (string? nation, List<WGObject>? data) = nationDataPair;

                logger.LogInformation("Converting category: {category} - nation: {nation}", categoryName, nation);
                counter++;
                if (counter % 10 == 0)
                {
                    logger.LogInformation("Processed {counter} dictionaries.", counter);
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
                        convertedData = CaptainConverter.ConvertCaptain(data.Cast<WGCaptain>(), skillsList, nation.Equals("Common"));
                        convertedFileContent = JsonConvert.SerializeObject(convertedData, serializerSettings);

                        break;
                    case "Modernization":
                        convertedData = ModernizationConverter.ConvertModernization(data.Cast<WGModernization>());
                        convertedFileContent = JsonConvert.SerializeObject(convertedData, serializerSettings);

                        break;
                    case "Projectile":
                        var filteredData = data.OfType<WGProjectile>();
                        convertedData = ProjectileConverter.ConvertProjectile(filteredData);
                        convertedFileContent = JsonConvert.SerializeObject(convertedData, serializerSettings);

                        break;
                    case "Ship":
                        Console.WriteLine($"Ships to process for {nation}: {data.Count}");
                        convertedData = ShipConverter.ConvertShips(data.Cast<WgShip>(), nation);
                        convertedFileContent = JsonConvert.SerializeObject(convertedData, serializerSettings);

                        break;
                    case "Unit":
                        convertedData = ModuleConverter.ConvertModule(data.Cast<WGModule>());
                        convertedFileContent = JsonConvert.SerializeObject(convertedData, serializerSettings);

                        break;
                    case "Exterior":
                        convertedData = ExteriorConverter.ConvertExterior(data.Cast<WgExterior>());
                        convertedFileContent = JsonConvert.SerializeObject(convertedData, serializerSettings);

                        break;
                    default:
                        convertedFileContent = null;
                        if (!reportedTypes.Contains(categoryName))
                        {
                            reportedTypes.Add(categoryName);
                            logger.LogWarning("Type not found: {category}", categoryName);
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
}
