using System.Diagnostics;
using GameParamsExtractor.WGStructure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WowsShipBuilder.GameParamsExtractor.Data;

namespace WowsShipBuilder.GameParamsExtractor.Services;

public class GameDataUnpackService : IGameDataUnpackService
{
    private readonly ILogger<GameDataUnpackService> logger;

    public GameDataUnpackService(ILogger<GameDataUnpackService> logger)
    {
        this.logger = logger;
    }

    public GameParamsExtractionResult ExtractAndRefineGameParams(GameParamsExtractionOptions options)
    {
        var sw = Stopwatch.StartNew();
        var rawGameParams = ExtractRawGameParamsData(options.GameParamsFilePath);
        Dictionary<string, Dictionary<string, List<WGObject>>> refinedGameParams = GameParamsUtility.FilterAndConvertGameParams(rawGameParams, logger);

        sw.Stop();
        logger.LogInformation("Time passed for extracting and refining gameparams: {}", sw.Elapsed);
        return new(refinedGameParams, options.ReturnUnfilteredFiles ? rawGameParams : null);
    }

    public Dictionary<object, Dictionary<string, object>> ExtractRawGameParamsData(string gameParamsFilePath)
    {
        logger.LogInformation("Starting unpickling.");
        var sw = Stopwatch.StartNew();
        Dictionary<object, Dictionary<string, object>> result = GameParamsUtility.UnpickleGameParams(gameParamsFilePath);
        logger.LogInformation("Finished unpickling. Time passed: {time}", sw.Elapsed);
        return result;
    }

    public void WriteUnfilteredFiles(Dictionary<object, Dictionary<string, object>> rawGameParams, string outputBasePath)
    {
        const string filePrefix = "unfiltered_";
        logger.LogInformation("Writing files for unfiltered data in directory {} with prefix {}.", outputBasePath, filePrefix);
        var serializer = new JsonSerializer
        {
            Formatting = Formatting.Indented,
        };

        foreach ((object category, Dictionary<string, object> nations) in rawGameParams)
        {
            string directory = Path.Join(outputBasePath, category.ToString());
            Directory.CreateDirectory(directory);
            foreach ((string nation, object data) in nations)
            {
                string nationFilePath = Path.Join(directory, $"{filePrefix}{nation}.json");
                using var file = File.CreateText(nationFilePath);
                serializer.Serialize(file, data);
            }
        }

        logger.LogInformation("Finished writing unfiltered data.");
    }

    public void WriteFilteredFiles(Dictionary<string, Dictionary<string, List<WGObject>>> refinedGameParams, string outputBasePath)
    {
        const string filePrefix = "filtered_";
        logger.LogInformation("Writing files for filtered data in directory {} with prefix {}.", outputBasePath, filePrefix);
        var serializer = new JsonSerializer
        {
            Formatting = Formatting.Indented,
        };

        foreach ((string category, Dictionary<string, List<WGObject>> nations) in refinedGameParams)
        {
            string directory = Path.Join(outputBasePath, category);
            Directory.CreateDirectory(directory);
            foreach ((string nation, List<WGObject> data) in nations)
            {
                string nationFilePath = Path.Join(directory, $"{filePrefix}{nation}.json");
                using StreamWriter file = File.CreateText(nationFilePath);
                serializer.Serialize(file, data);
            }
        }

        logger.LogInformation("Finished writing filtered data.");
    }
}
