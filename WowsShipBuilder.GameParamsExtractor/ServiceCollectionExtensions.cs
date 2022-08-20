using Microsoft.Extensions.DependencyInjection;
using WowsShipBuilder.GameParamsExtractor.Services;

namespace WowsShipBuilder.GameParamsExtractor;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameParamsExtractor(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IGameDataUnpackService, GameDataUnpackService>();
        serviceCollection.AddTransient<ILocalizationExtractor, LocalizationExtractor>();
        return serviceCollection;
    }
}
