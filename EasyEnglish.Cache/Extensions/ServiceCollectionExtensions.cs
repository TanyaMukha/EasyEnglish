using EasyEnglish.Core.Interfaces.Cache;
using EasyEnglish.Cache.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EasyEnglish.Cache.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEasyEnglishCacheServices(this IServiceCollection services)
    {
        services.AddSingleton<ICurrentUnitCacheService, CurrentUnitCacheService>();
        services.AddSingleton<IWordCacheService, WordCacheService>();

        return services;
    }
}