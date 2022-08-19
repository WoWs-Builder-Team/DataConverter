using System.Net;
using System.Net.Http;
using DataConverter.Services;
using Microsoft.Extensions.DependencyInjection;
using WowsShipBuilder.GameParamsExtractor.Services;

namespace DataConverter;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataConverter(this IServiceCollection serviceCollection, HttpClient? httpClient = null)
    {
        httpClient ??= new(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
        });
        serviceCollection.AddSingleton(httpClient);
        serviceCollection.AddSingleton<IGameDataUnpackService, GameDataUnpackService>();
        serviceCollection.AddTransient<IDataConverterService, DataConverterService>();
        serviceCollection.AddTransient<IVersionCheckService, VersionCheckService>();
        serviceCollection.AddTransient<ILocalizationExtractor, LocalizationExtractor>();
        return serviceCollection;
    }
}
