using System.Net;
using System.Net.Http;
using DataConverter.Services;
using Microsoft.Extensions.DependencyInjection;
using WowsShipBuilder.GameParamsExtractor;

namespace DataConverter;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the services used by DataConverter and GameParamsExtractor with the provided <see cref="IServiceCollection"/>.
    /// <br/>
    /// <b>It is not necessary to use <see cref="WowsShipBuilder.GameParamsExtractor.ServiceCollectionExtensions.AddGameParamsExtractor">IServiceCollection.AddGameParamsExtractor</see>
    /// after this method as it is already included.</b>
    /// </summary>
    /// <param name="serviceCollection">The service collection to register the services with.</param>
    /// <param name="httpClient">An optional <see cref="HttpClient"/> instance. If none is provided, this method will register a new instance.</param>
    /// <returns>The configured service collection.</returns>
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
