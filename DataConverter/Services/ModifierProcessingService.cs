using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

    public Dictionary<string, Modifier> ReadModifiersFile()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DataConverter.JsonData.Modifiers.json") ??
                           throw new FileNotFoundException("Unable to locate embedded modifier json.");
        using var reader = new StreamReader(stream);
        var data = reader.ReadToEnd();
        var modifierDictionary = JsonSerializer.Deserialize<List<Modifier>>(data, Constants.ModifierSerializerOptions)!.ToDictionary(x => x.Name, x => x);
        return modifierDictionary;
    }

    public async Task WriteDebugModifierFiles(List<Modifier> modifierList, Dictionary<string, Modifier> startingModifierDictionary, List<string> localizationKeys, string outputPath)
    {
        logger.LogInformation("Starting creation of base modifier file");
        logger.LogInformation("There are {Count} modifiers", modifierList.Count);

        modifierList = modifierList.OrderBy(x => x.Name).ToList();

        modifierList = HandleCommonLocalization(modifierList, localizationKeys);

        // this is used for the initial version of the file. Left for convenience
        // modifierList = HandleDisplayValueOld(modifierList);

        logger.LogInformation("Writing file");

        var modifierJson = JsonSerializer.Serialize(modifierList, Constants.ModifierSerializerOptions);
        await WriteFileAsync(outputPath, "Modifiers.json", modifierJson);

        logger.LogInformation("Gathering modifier with missing or unassigned data");
        var missingTranslationsModifiers = modifierList.Where(x => x.GameLocalizationKey is null && x.AppLocalizationKey is null)
            .ToDictionary(x => x.Name, x => x.Location);
        var missingUnitOrDisplayProcessingModifiers = modifierList.Where(x => x.DisplayedValueProcessingKind == DisplayValueProcessingKind.NotAssigned || x.Unit == Unit.NotAssigned)
            .ToDictionary(x => x.Name, x => x.Location);
        var missingPropertyOrValueProcessingModifiers = modifierList.Where(x => x.ValueProcessingKind == ValueProcessingKind.NotAssigned || (x.ValueProcessingKind != ValueProcessingKind.None && x.AffectedProperties.Count == 0))
            .ToDictionary(x => x.Name, x => x.Location);
        var newModifiers = modifierList.Where(m => !startingModifierDictionary.ContainsKey(m.Name)).ToDictionary(x => x.Name, x => x.Location);
        var removedModifiers = startingModifierDictionary.Where(m => !modifierList.Any(x => x.Name.Equals(m.Key))).ToDictionary(x => x.Value.Name, x => x.Value.Location);
        if (missingTranslationsModifiers.Any() || missingUnitOrDisplayProcessingModifiers.Any() || missingPropertyOrValueProcessingModifiers.Any() || newModifiers.Any() || removedModifiers.Any())
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
        string directory = Path.Join(baseDirectory, "Modifiers");
        Directory.CreateDirectory(directory);
        string path = Path.Join(directory, filename);
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

    // ReSharper disable once UnusedMember.Local
    // This is old display value handling that was in the app. left here for convenience.
    [Obsolete("Do not use this. Modify the json directly for new modifiers.")]
    private static List<Modifier> HandleDisplayValueOld(List<Modifier> modifierList)
    {
        foreach (var modifier in modifierList)
        {
            if (modifier.Unit != Unit.NotAssigned && modifier.DisplayedValueProcessingKind != DisplayValueProcessingKind.NotAssigned)
            {
                continue;
            }

            var modifierName = modifier.Name;

            // Because removing unused things is too hard, right WG?
            if (modifierName.Contains("torpedoDetectionCoefficientByPlane", StringComparison.InvariantCultureIgnoreCase))
            {
                modifier.Unit = Unit.None;
                modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.Empty;
                continue;
            }

            switch (modifierName)
            {
                // defAA modifiers
                case { } str when str.Contains("bubbleDamageMultiplier"):
                    modifier.Unit = Unit.Percent;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.PositivePercentage;
                    break;

                // custom modifier to show hp per heal
                case { } str when str.Contains("hpPerHeal", StringComparison.InvariantCultureIgnoreCase):
                    modifier.Unit = Unit.None;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.ToInt;
                    break;

                // Bonus from Depth Charge upgrade. Needs to be put as first entry because it contains the word "bonus".
                case { } str when str.Contains("dcNumPacksBonus", StringComparison.InvariantCultureIgnoreCase):
                    modifier.Unit = Unit.None;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.ToInt;
                    break;

                case { } str when str.Contains("prioritySectorStrengthBonus", StringComparison.InvariantCultureIgnoreCase):
                    modifier.Unit = Unit.Percent;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.ToInt;
                    break;

                // this is for Vigilance for BBs
                case { } str when str.Contains("uwCoeffBonus", StringComparison.InvariantCultureIgnoreCase) ||
                                  str.Contains("ignorePTZBonus", StringComparison.InvariantCultureIgnoreCase):
                    modifier.Unit = Unit.Percent;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.ToInt;
                    break;

                // This is for IFHE. At the start because of DE sharing similar modifier name
                case { } str when str.Contains("burnChanceFactorHighLevel", StringComparison.InvariantCultureIgnoreCase) ||
                                  str.Contains("burnChanceFactorLowLevel", StringComparison.InvariantCultureIgnoreCase):
                    modifier.Unit = Unit.Percent;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.NegativePercentage;
                    break;

                // this is for HP module
                case { } str when str.Contains("AAMaxHP", StringComparison.InvariantCultureIgnoreCase) ||
                                  str.Contains("GSMaxHP", StringComparison.InvariantCultureIgnoreCase) ||
                                  str.Contains("SGCritRudderTime", StringComparison.InvariantCultureIgnoreCase):
                {
                    modifier.Unit = Unit.Percent;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.VariablePercentage;
                    break;
                }

                // wg doesn't know how math works. -x% drain rate != +x% drain rate
                case { } str when str.Contains("planeForsageDrainRate", StringComparison.InvariantCultureIgnoreCase):
                {
                    modifier.Unit = Unit.Percent;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.DrainPercentage;
                    break;
                }

                // this is for midway leg mod. more accurate numbers
                case { } str when str.Contains("diveBomberMaxSpeedMultiplier", StringComparison.InvariantCultureIgnoreCase) ||
                                  str.Contains("diveBomberMinSpeedMultiplier", StringComparison.InvariantCultureIgnoreCase):
                    modifier.Unit = Unit.Percent;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.DecimalRoundedPercentage;
                    break;

                // this is for aiming time of CV planes
                case { } str when str.Contains("AimingTime", StringComparison.InvariantCultureIgnoreCase):
                    modifier.Unit = Unit.S;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.None;
                    break;

                // This is the anti detonation stuff
                case { } str when str.Contains("PMDetonationProb", StringComparison.InvariantCultureIgnoreCase):
                {
                    modifier.Unit = Unit.Percent;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.IntVariablePercentage;
                    break;
                }

                // This is Demolition Expert. And also flags. Imagine having similar name for a modifier doing the same thing.
                // Also applies to repair party bonus.
                // UPDATE: remember what i said about similar names? Wanna take a guess how they did captain talents?
                case { } str when str.Contains("Bonus", StringComparison.InvariantCultureIgnoreCase) ||
                                  str.Contains("burnChanceFactor", StringComparison.InvariantCultureIgnoreCase) ||
                                  str.Contains("regenerationHPSpeed", StringComparison.InvariantCultureIgnoreCase) ||
                                  str.Contains("regenerationRate", StringComparison.InvariantCultureIgnoreCase):
                {
                    modifier.Unit = Unit.Percent;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.NoneOrPercentage;
                    if (str.Contains("regenerationRate", StringComparison.InvariantCultureIgnoreCase))
                    {
                        modifier.Unit = Unit.PercentPerS;
                    }

                    break;
                }

                // This is Adrenaline Rush
                case { } str when str.Contains("lastChanceReloadCoefficient", StringComparison.InvariantCultureIgnoreCase):
                    modifier.Unit = Unit.Percent;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.ToNegative;
                    break;

                // Something in Last stand. Not sure what make of it tho.
                case { } str when str.Contains("SGCritRudderTime"):
                    modifier.Unit = Unit.None;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.ToPositive;
                    break;

                // Incoming fire alert. Range is in BigWorld Unit
                case { } str when str.Contains("artilleryAlertMinDistance", StringComparison.InvariantCultureIgnoreCase):
                    modifier.Unit = Unit.KM;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.BigWorldToKm;
                    break;

                // Radar and hydro spotting distances
                case { } str when str.Contains("distShip", StringComparison.InvariantCultureIgnoreCase) ||
                                  str.Contains("distTorpedo", StringComparison.InvariantCultureIgnoreCase):
                    modifier.Unit = Unit.KM;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.BigWorldToKmDecimal;
                    break;

                // Speed boost modifier
                case { } str when str.Equals("boostCoeff", StringComparison.InvariantCultureIgnoreCase):
                    modifier.Unit = Unit.Percent;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.RoundedPercentage;
                    break;

                case { } str when str.Contains("fightersNum", StringComparison.InvariantCultureIgnoreCase):
                    modifier.Unit = Unit.None;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.None;
                    break;

                case { } str when str.Contains("hydrophoneWaveRadius", StringComparison.InvariantCultureIgnoreCase):
                    modifier.Unit = Unit.KM;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.MeterToKm;
                    break;

                // Venezia UU
                case { } str when str.Contains("smokeGeneratorAdditionalConsumables", StringComparison.InvariantCultureIgnoreCase):
                    modifier.Unit = Unit.None;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.ToPositive;
                    break;

                // Halland UU
                case { } str when str.Contains("boostCoeffForsage", StringComparison.InvariantCultureIgnoreCase) || str.Contains("regeneratedHPPartCoef", StringComparison.InvariantCultureIgnoreCase):
                    modifier.Unit = Unit.Percent;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.RoundedPercentage;
                    break;

                // this is the modifier
                case { } str when str.Contains("CALLFIGHTERStimeDelayAttack", StringComparison.InvariantCultureIgnoreCase):
                    modifier.Unit = Unit.Percent;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.InverseNegativePercentage;
                    break;

                // this is the actual value
                case { } str when str.Contains("timeDelayAttack", StringComparison.InvariantCultureIgnoreCase):
                    modifier.Unit = Unit.S;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.None;
                    break;
                case { } str when str.Contains("radius"):
                    modifier.Unit = Unit.KM;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.BigWorldToKmDecimal;
                    break;

                case { } str when str.Equals("lifeTime", StringComparison.InvariantCultureIgnoreCase) ||
                                  str.Contains("timeFromHeaven", StringComparison.InvariantCultureIgnoreCase) ||
                                  str.Contains("torpedoReloadTime", StringComparison.InvariantCultureIgnoreCase) ||
                                  str.Equals("hydrophoneUpdateFrequency", StringComparison.InvariantCultureIgnoreCase):
                    modifier.Unit = Unit.S;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.None;
                    break;

                case { } str when Math.Abs(modifier.Value % 1) > (double.Epsilon * 100) ||
                                  str.Contains("WorkTimeCoeff", StringComparison.InvariantCultureIgnoreCase) ||
                                  str.Contains("smokeGeneratorLifeTime"):
                {
                    modifier.Unit = Unit.Percent;
                    modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.DecimalRoundedPercentage;
                    break;
                }
                default:
                    // If Modifier is higher than 1000, we can assume it's in meter, so we convert it to Km for display purposes
                    if (modifier.Value > 1000)
                    {
                        modifier.Unit = Unit.KM;
                        modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.MeterToKm;
                    }
                    else
                    {
                        modifier.Unit = Unit.None;
                        modifier.DisplayedValueProcessingKind = DisplayValueProcessingKind.ToInt;
                    }
                    break;
            }
        }
        return modifierList;
    }
}
