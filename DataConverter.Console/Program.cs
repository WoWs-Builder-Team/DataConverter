using CommandLine;
using DataConverter.Console.Model;
using DataConverter.Console.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

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
            .AddTransient<DataUnpackHelper>()
            .BuildServiceProvider();

        ParserResult<object>? result = Parser.Default.ParseArguments<ConvertOptions, ExtractOptions>(args);
        result = await result.WithParsedAsync<ConvertOptions>(async options =>
        {
            var dataConversionHelper = serviceProvider.GetRequiredService<DataConversionHelper>();
            await dataConversionHelper.ExtractAndConvertData(options);
        });

        await result.WithParsedAsync<ExtractOptions>(async options =>
        {
            var unpackHelper = serviceProvider.GetRequiredService<DataUnpackHelper>();
            await unpackHelper.UnpackData(options);
        });
    }
}
