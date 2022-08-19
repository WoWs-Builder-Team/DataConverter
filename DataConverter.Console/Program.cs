using CommandLine;
using DataConverter.Console.Model;
using DataConverter.Console.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using WowsShipBuilder.GameParamsExtractor.Services;

namespace DataConverter.Console;

public class Program
{
    public static async Task Main(string[] args)
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Debug);
                logging.AddNLog();
            })
            .AddDataConverter()
            .AddTransient<DataConversionHelper>()
            .BuildServiceProvider();

        ParserResult<object>? result = Parser.Default.ParseArguments<ConvertOptions, ExtractOptions>(args);
        result = await result.WithParsedAsync<ConvertOptions>(async options =>
        {
            var dataConversionHelper = serviceProvider.GetRequiredService<DataConversionHelper>();
            await dataConversionHelper.ExtractAndConvertData(options);
        });
        result = result.WithParsed<ExtractOptions>(options =>
        {
            var unpackService = serviceProvider.GetRequiredService<IGameDataUnpackService>();
            Dictionary<object, Dictionary<string, object>> unpackResult = unpackService.ExtractRawGameParamsData(options.GameParamsFile);
            unpackService.WriteUnfilteredFiles(unpackResult, options.OutputDirectory);
        });
        await result.WithNotParsedAsync(async options => { await Task.CompletedTask; });
    }
}
