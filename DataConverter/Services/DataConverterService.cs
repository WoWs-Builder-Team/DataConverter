using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using DataConverter.Converters;
using DataConverter.Data;
using DataConverter.JsonData;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.DataStructures.Modifiers;
using WowsShipBuilder.GameParamsExtractor.WGStructure;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Captain;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Projectile;
using WowsShipBuilder.GameParamsExtractor.WGStructure.Ship;

namespace DataConverter.Services;

internal class DataConverterService : IDataConverterService
{
    private readonly ILogger<DataConverterService> logger;

    private readonly HttpClient client;

    private readonly ConcurrentBag<string> reportedTypes = new();

    private readonly ConcurrentBag<Modifier> modifiers = new();

    public DataConverterService(ILogger<DataConverterService> logger, HttpClient client)
    {
        this.logger = logger;
        this.client = client;
    }

    public async Task<DataConversionResult> ConvertRefinedData(Dictionary<string, Dictionary<string, List<WgObject>>> refinedData, bool writeModifierDebugOutput, Dictionary<string, Modifier> modifiersDictionary, Dictionary<long, int> techTreeShipsPositionsDictionary)
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
                switch (categoryName)
                {
                    case "Ability":
                        var consumableData = ConsumableConverter.ConvertConsumable(data.Cast<WgConsumable>(), modifiersDictionary);
                        modifiers.UnionWith(consumableData.SelectMany(consumable => consumable.Value.Modifiers));
                        convertedFileContent = JsonSerializer.Serialize(consumableData, Constants.SerializerOptions);
                        break;
                    case "Aircraft":
                        var aircraftData = AircraftConverter.ConvertAircraft(data.Cast<WgAircraft>());
                        convertedFileContent = JsonSerializer.Serialize(aircraftData, Constants.SerializerOptions);

                        break;
                    case "Crew":
                        string skillsList = CaptainConverter.LoadEmbeddedSkillData();
                        var captainData = CaptainConverter.ConvertCaptain(data.Cast<WgCaptain>(), skillsList, nation.Equals("Common"), modifiersDictionary);
                        var skillModifiers = captainData.SelectMany(captain => captain.Value.Skills.SelectMany(skill => skill.Value.Modifiers));
                        var conditionalSkillModifiers = captainData.SelectMany(captain => captain.Value.Skills.SelectMany(skill => skill.Value.ConditionalModifierGroups.SelectMany(g => g.Modifiers)));
                        var uniqueSkillModifiers = captainData.SelectMany(captain => captain.Value.UniqueSkills.SelectMany(us => us.Value.SkillEffects.SelectMany(se => se.Value.Modifiers)));
                        modifiers.UnionWith(skillModifiers);
                        modifiers.UnionWith(conditionalSkillModifiers);
                        modifiers.UnionWith(uniqueSkillModifiers);
                        convertedFileContent = JsonSerializer.Serialize(captainData, Constants.SerializerOptions);
                        break;
                    case "Modernization":
                        var modernizationData = ModernizationConverter.ConvertModernization(data.Cast<WgModernization>(), modifiersDictionary);
                        var modernizationModifiers = modernizationData.SelectMany(m => m.Value.Modifiers);
                        modifiers.UnionWith(modernizationModifiers);
                        convertedFileContent = JsonSerializer.Serialize(modernizationData, Constants.SerializerOptions);

                        break;
                    case "Projectile":
                        var filteredData = data.OfType<WgProjectile>();
                        var projectileData = ProjectileConverter.ConvertProjectile(filteredData, logger);
                        convertedFileContent = JsonSerializer.Serialize(projectileData, Constants.SerializerOptions);

                        break;
                    case "Ship":
                        logger.LogInformation("Ships to process for {Nation}: {Count}", nation, data.Count);
                        var shipData = ShipConverter.ConvertShips(data.Cast<WgShip>(), nation, await shipToolDataTask, logger, modifiersDictionary, techTreeShipsPositionsDictionary);
                        var specialAbilityModifiers = shipData.Where(s => s.Value.SpecialAbility is not null).SelectMany(s => s.Value.SpecialAbility!.Modifiers);
                        var burstArtilleryModifiers = shipData.SelectMany(s => s.Value.MainBatteryModuleList.Where(mb => mb.Value.BurstModeAbility is not null).SelectMany(mb => mb.Value.BurstModeAbility!.Modifiers));
                        modifiers.UnionWith(specialAbilityModifiers);
                        modifiers.UnionWith(burstArtilleryModifiers);
                        convertedFileContent = JsonSerializer.Serialize(shipData, Constants.SerializerOptions);

                        break;
                    case "Unit":
                        var moduleData = ModuleConverter.ConvertModule(data.Cast<WgModule>());
                        convertedFileContent = JsonSerializer.Serialize(moduleData, Constants.SerializerOptions);

                        break;
                    case "Exterior":
                        if (!nation.Equals("Common", StringComparison.OrdinalIgnoreCase))
                        {
                            convertedFileContent = null;
                            break;
                        }

                        var exteriorData = ExteriorConverter.ConvertExterior(data.Cast<WgExterior>(), logger, modifiersDictionary);
                        var exteriorModifiers = exteriorData.SelectMany(e => e.Value.Modifiers);
                        modifiers.UnionWith(exteriorModifiers);
                        convertedFileContent = JsonSerializer.Serialize(exteriorData, Constants.SerializerOptions);

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

        string shipSummaryString = JsonSerializer.Serialize(ShipConverter.ShipSummaries, Constants.SerializerOptions);
        resultFiles.Add(new(shipSummaryString, "Summary", "Common.json"));

        return new(resultFiles, modifiers.DistinctBy(m => m.Name).ToList());
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
            var result = await client.GetFromJsonAsync<ShiptoolData>(Constants.ShiptoolDataUrl, Constants.SerializerOptions);
            logger.LogInformation("Received remote json data from shiptool");
            return result!;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while downloading shiptool data");
            return new();
        }
    }
}
