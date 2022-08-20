using System.Net;
using System.Net.Http;
using DataConverter.Services;
using Microsoft.Extensions.DependencyInjection;
using WowsShipBuilder.GameParamsExtractor;

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
        serviceCollection.AddTransient<IDataConverterService, DataConverterService>();
        serviceCollection.AddTransient<IVersionCheckService, VersionCheckService>();
        serviceCollection.AddGameParamsExtractor();
        return serviceCollection;
    }
}
