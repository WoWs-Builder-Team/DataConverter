using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DataConverter.Data;
using Microsoft.Extensions.Logging;
using WoWsShipBuilder.DataStructures.Modifiers;

namespace DataConverter.Services;

public class ModifierProcessingService : IModifierProcessingService
{
    private const string DefaultPrefix = "PARAMS_MODIFIER_";
    private const string ModernizationSuffix = "_MODERNIZATION";

    private readonly ILogger<ModifierProcessingService> logger;

    public ModifierProcessingService(ILogger<ModifierProcessingService> logger)
    {
        this.logger = logger;
    }

    public Dictionary<string, Modifier> ReadModifiersFile(string modifiersFilePath)
    {
        var data = File.ReadAllText(modifiersFilePath);
        var modifierDictionary = JsonSerializer.Deserialize<List<Modifier>>(data, Constants.ModifierSerializerOptions)!.ToDictionary(x => x.Name, x => x);
        return modifierDictionary;
    }

    public async Task WriteDebugModifierFiles(List<Modifier> modifierList, Dictionary<string, Modifier> startingModifierDictionary, List<string> localizationKeys, string outputPath)
    {
        logger.LogInformation("Starting creation of base modifier file");
        logger.LogInformation("There are {Count} modifiers", modifierList.Count);

        modifierList = modifierList.OrderBy(x => x.Name).ToList();

        modifierList = HandleCommonLocalization(modifierList, localizationKeys);

        logger.LogInformation("Writing file");

        var modifierJson = JsonSerializer.Serialize(modifierList, Constants.ModifierSerializerOptions);
        await WriteFileAsync(outputPath, "Modifiers.json", modifierJson);

        logger.LogInformation("Gathering modifier with missing or unassigned data");
        var missingTranslationsModifiers = modifierList.Where(x => x.GameLocalizationKey is null && x.AppLocalizationKey is null)
            .ToDictionary(x => x.Name, x => x.Location);
        var missingUnitOrDisplayProcessingModifiers = modifierList.Where(x => x.DisplayValueProcessingKind == DisplayValueProcessingKind.NotAssigned || x.Unit == Unit.NotAssigned)
            .ToDictionary(x => x.Name, x => x.Location);
        var missingPropertyOrValueProcessingModifiers = modifierList.Where(x => x.ValueProcessingKind == ValueProcessingKind.NotAssigned || (x.ValueProcessingKind != ValueProcessingKind.None && x.AffectedProperties.Count == 0))
            .ToDictionary(x => x.Name, x => x.Location);
        var newModifiers = modifierList.Where(m => !startingModifierDictionary.ContainsKey(m.Name)).ToDictionary(x => x.Name, x => x.Location);
        var removedModifiers = startingModifierDictionary.Where(m => !modifierList.Exists(x => x.Name.Equals(m.Key))).ToDictionary(x => x.Value.Name, x => x.Value.Location);
        if (missingTranslationsModifiers.Count != 0 || missingUnitOrDisplayProcessingModifiers.Count != 0 || missingPropertyOrValueProcessingModifiers.Count != 0 || newModifiers.Count != 0 || removedModifiers.Count != 0)
        {
            logger.LogWarning("There are {Count} modifiers without translation", missingTranslationsModifiers.Count);
            logger.LogWarning("There are {Count} modifiers without unit or display value processing kind", missingUnitOrDisplayProcessingModifiers.Count);
            logger.LogWarning("There are {Count} modifiers without property or value processing kind", missingPropertyOrValueProcessingModifiers.Count);
            logger.LogWarning("There are {Count} new modifiers ", newModifiers.Count);
            logger.LogWarning("There are {Count} removed modifiers", removedModifiers.Count);
            Dictionary<string, Dictionary<string, string>> missingDataDictionary = new Dictionary<string, Dictionary<string, string>>()
            {
                { "Translations", missingTranslationsModifiers },
                { "UnitOrDisplay", missingUnitOrDisplayProcessingModifiers },
                { "PropertyHandling", missingPropertyOrValueProcessingModifiers },
                { "NewModifiers", newModifiers },
                { "RemovedModifers", removedModifiers },
            };
            var missingDataJson = JsonSerializer.Serialize(missingDataDictionary, Constants.ModifierSerializerOptions);
            await WriteFileAsync(outputPath, "MissingDataModifiers.json", missingDataJson);
        }
        else
        {
            logger.LogInformation("No changes to modifiers");
        }
    }

    private static async Task WriteFileAsync(string baseDirectory, string filename, string content)
    {
        string directory = Path.Combine(baseDirectory, "Modifiers");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, filename);
        await File.WriteAllTextAsync(path, content);
    }

    private List<Modifier> HandleCommonLocalization(List<Modifier> modifierList, List<string> localizationKeys)
    {
        logger.LogInformation("Iterating over all modifiers to find a translation");
        foreach (var modifier in modifierList)
        {
            // if the modifier already has a localization set, then ignore it.
            if (modifier.GameLocalizationKey is not null || modifier.AppLocalizationKey is not null)
            {
                continue;
            }

            var name = modifier.Name.ToUpperInvariant();
            string? description = null;
            if (localizationKeys.Contains(DefaultPrefix + name))
            {
                description = DefaultPrefix + name;
            }
            if (localizationKeys.Contains(DefaultPrefix + name + ModernizationSuffix))
            {
                description = DefaultPrefix + name + ModernizationSuffix;
            }

            modifier.GameLocalizationKey = description;
        }

        logger.LogInformation("Finished assigning translations");
        return modifierList;
    }

    // ReSharper disable once UnusedMember.Local
    // This is old localization handling that was in the app. left here for convenience.
    [Obsolete("Do not use this. Modify the json directly for new modifiers, or use the HandleCommonLocalization method.")]
    private List<Modifier> HandleLocalizationOld(List<Modifier> modifierList, List<string> localizationKeys)
    {
        logger.LogInformation("Iterating over all modifiers to find a translation. Using old method");
        foreach (var modifier in modifierList)
        {
            // if the modifier already has a localization set, then ignore it.
            if (modifier.GameLocalizationKey is not null || modifier.AppLocalizationKey is not null)
            {
                continue;
            }

            var modifierTranslationName = modifier.Name;

            if (modifierTranslationName.Contains("regenerationHPSpeedUnits", StringComparison.InvariantCultureIgnoreCase))
            {
                modifier.GameLocalizationKey = string.Empty;
            }

            // There is one translation per class, but all values are equal, so we can just choose a random one. I like DDs.
            if (modifierTranslationName.ToUpperInvariant().Equals("VISIBILITYDISTCOEFF", StringComparison.InvariantCultureIgnoreCase) ||
                modifierTranslationName.ToUpperInvariant().Equals("AABubbleDamage", StringComparison.InvariantCultureIgnoreCase) ||
                modifierTranslationName.ToUpperInvariant().Equals("AAAuraDamage", StringComparison.InvariantCultureIgnoreCase) ||
                modifierTranslationName.ToUpperInvariant().Equals("GMROTATIONSPEED", StringComparison.InvariantCultureIgnoreCase) ||
                modifierTranslationName.ToUpperInvariant().Equals("dcAlphaDamageMultiplier", StringComparison.InvariantCultureIgnoreCase) ||
                modifierTranslationName.ToUpperInvariant().Equals("ConsumableReloadTime", StringComparison.InvariantCultureIgnoreCase))
            {
                modifierTranslationName = $"{modifierTranslationName}_DESTROYER";
            }

            if (modifierTranslationName.Equals("talentMaxDistGM", StringComparison.InvariantCultureIgnoreCase))
            {
                modifierTranslationName = "GMMAXDIST";
            }

            if (modifierTranslationName.Equals("talentConsumablesWorkTime", StringComparison.InvariantCultureIgnoreCase))
            {
                modifierTranslationName = "ConsumablesWorkTime";
            }

            if (modifierTranslationName.Equals("timeDelayAttack", StringComparison.InvariantCultureIgnoreCase))
            {
                modifierTranslationName = $"CALLFIGHTERS{modifierTranslationName}";
            }

            modifierTranslationName = $"{DefaultPrefix}{modifierTranslationName}";
            string? finalTranslationKey = null;

            bool found = localizationKeys.Contains(modifierTranslationName.ToUpperInvariant());
            if (found)
            {
                finalTranslationKey = modifierTranslationName.ToUpperInvariant();
            }

            // We need this to deal with the consumable mod of slot 5
            string? moduleFallback = null;

            if (modifierTranslationName.Contains("ReloadCoeff", StringComparison.InvariantCultureIgnoreCase) ||
                modifierTranslationName.Contains("WorkTimeCoeff", StringComparison.InvariantCultureIgnoreCase) ||
                modifierTranslationName.Contains("AAEXTRABUBBLES", StringComparison.InvariantCultureIgnoreCase) ||
                modifierTranslationName.Contains("callFightersAdditionalConsumables", StringComparison.InvariantCultureIgnoreCase))
            {
                moduleFallback = $"{modifierTranslationName.ToUpperInvariant()}_SKILL";
                found = localizationKeys.Contains($"{modifierTranslationName.ToUpperInvariant()}_SKILL");
                if (found)
                {
                    finalTranslationKey = $"{modifierTranslationName.ToUpperInvariant()}_SKILL";
                }
            }

            if (!found)
            {
                found = localizationKeys.Contains($"{modifierTranslationName.ToUpperInvariant()}_MODERNIZATION");
                if (found)
                {
                    finalTranslationKey = $"{modifierTranslationName.ToUpperInvariant()}_MODERNIZATION";
                }
            }

            if (!found)
            {
                finalTranslationKey = !string.IsNullOrEmpty(moduleFallback) ? moduleFallback.ToUpperInvariant() : null;
            }

            if (modifierTranslationName.Contains("artilleryAlertMinDistance", StringComparison.InvariantCultureIgnoreCase))
            {
                modifier.AppLocalizationKey = "IncomingFireAlertDesc";
            }

            if (modifierTranslationName.Contains("timeFromHeaven", StringComparison.InvariantCultureIgnoreCase))
            {
                finalTranslationKey = "PARAMS_MODIFIER_CALLFIGHTERSAPPEARDELAY";
            }

            if (modifierTranslationName.Contains("SHIPSPEEDCOEFF", StringComparison.InvariantCultureIgnoreCase))
            {
                finalTranslationKey = "PARAMS_MODIFIER_SHIPSPEEDCOEFFFORRIBBONS";
            }

            if (modifierTranslationName.Contains("burnProbabilityBonus", StringComparison.InvariantCultureIgnoreCase))
            {
                finalTranslationKey = "PARAMS_MODIFIER_MAINGAUGEBURNPROBABILITYFORCAPTURE";
            }

            if (modifierTranslationName.Contains("hpPerHeal", StringComparison.InvariantCultureIgnoreCase))
            {
                modifier.AppLocalizationKey = "Consumable_HpPerHeal";
            }

            modifier.GameLocalizationKey = finalTranslationKey;
        }

        logger.LogInformation("Finished assigning translations");
        return modifierList;
    }
}
