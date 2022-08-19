using CommandLine;

namespace DataConverter.Console.Model;

[Verb("extract", HelpText = "Extracts raw data from the game parameters and writes it to json files")]
public class ExtractOptions
{
    [Option('p', "gameparams", HelpText = "The game params file to read from", Required = true)]
    public string GameParamsFile { get; set; } = default!;

    [Option('o', "output", HelpText = "The output directory for the extracted files", Required = true)]
    public string OutputDirectory { get; set; } = default!;
}
