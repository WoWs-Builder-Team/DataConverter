using CommandLine;
using DataConverter.Console.Model;
using DataConverter.Console.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Conditions;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace DataConverter.Console;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Debug);
                logging.AddNLog(CreateLoggingConfiguration());
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

    private static LoggingConfiguration CreateLoggingConfiguration()
    {
        var config = new LoggingConfiguration();

        var coloredConsole = new ColoredConsoleTarget("console")
        {
            Layout = "${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}",
            Header = "----------WoWs Builder Team DataConverter----------",
            UseDefaultRowHighlightingRules = false,
            RowHighlightingRules =
            {
                new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Debug"), ConsoleOutputColor.DarkGray, ConsoleOutputColor.NoChange),
                new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Info"), ConsoleOutputColor.Gray, ConsoleOutputColor.NoChange),
                new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Warn"), ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange),
                new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Error"), ConsoleOutputColor.Red, ConsoleOutputColor.NoChange),
                new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Fatal"), ConsoleOutputColor.Red, ConsoleOutputColor.White),
            },
        };

        config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, coloredConsole);

        return config;
    }
}
