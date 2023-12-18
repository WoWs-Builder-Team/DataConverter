using System.ComponentModel.DataAnnotations;
using CommandLine;
using WoWsShipBuilder.DataStructures;
using WowsShipBuilder.GameParamsExtractor.Data;

namespace DataConverter.Console.Model;

[Verb("convert", HelpText = "Extract data and convert to ShipBuilder data structures.")]
public class ConvertOptions
{
    [Option('p', "gameparams", HelpText = "The game params file to read from", Required = true)]
    public string GameParamsFile { get; set; } = default!;

    [Option('l', "localization", HelpText = "Input directory that contains localization files. Should follow structure texts/<language>/global.mo", Required = false)]
    public string? LocalizationInputDirectory { get; set; }

    [Option('o', "output", HelpText = "The output directory for the converted files", Required = true)]
    public string OutputDirectory { get; set; } = default!;

    [Option('v', "version", HelpText = "The version name to use for the conversion, for example '0.11.6#1'", Required = true)]
    public string Version { get; set; } = default!;

    [Option('s', "serverType", HelpText = "The server type for the data conversion (live, pts). Overrides the type from the provided version string!", Required = false)]
    public GameVersionType? VersionType { get; set; }

    [Option("writeUnfiltered", Required = false)]
    public bool WriteUnfiltered { get; set; }

    [Option("writeFiltered", Required = false)]
    public bool WriteFiltered { get; set; }

    [Option('d', "debugOutput", HelpText = "Output directory for filtered and unfiltered files as well as debug files", Required = false)]
    public string? DebugOutputDirectory { get; set; }

    [Option('m', "modifierDebug", HelpText = "Write a default modifier json and a list of new modifiers", Required = false)]
    public bool WriteModifierDebugOutput { get; set; }

    public GameParamsExtractionOptions ToExtractionOptions() => new(GameParamsFile, WriteUnfiltered);
}
