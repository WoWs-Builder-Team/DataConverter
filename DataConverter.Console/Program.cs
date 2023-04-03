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
            .AddTransient<DataDownloader>()
            .BuildServiceProvider();

        ParserResult<object>? result = Parser.Default.ParseArguments<ConvertOptions, ExtractOptions>(args);
        result = await result.WithParsedAsync<ConvertOptions>(async options =>
        {
            if (options.DataUrl is not null)
            {
                _ = options.DataDownloadTarget ?? throw new ArgumentNullException(nameof(options), "The data download target must be specified");
                await serviceProvider.GetRequiredService<DataDownloader>().DownloadGameData(options.DataUrl, options.DataDownloadTarget);
                options.GameParamsFile = Path.Join(options.DataDownloadTarget, "GameParams.data");
                options.LocalizationInputDirectory = Path.Join(options.DataDownloadTarget, "texts");
            }

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
