using EasyEnglish.Core.Interfaces.Cache;
using EasyEnglish.Cache.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EasyEnglish.Cache.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="ICurrentUnitCacheService"/> and <see cref="IWordCacheService"/> as
    /// singletons. Requires an <c>IStorageService</c> registration to already be present (not
    /// registered here). Both cache services resolve the scoped <c>IUnitService</c>/<c>IWordService</c>
    /// (registered scoped in <c>EasyEnglish.Business</c>) via the container-provided
    /// <c>IServiceScopeFactory</c>, in a short-lived scope per fetch — not as a captive dependency.
    /// </summary>
    public static IServiceCollection AddEasyEnglishCacheServices(this IServiceCollection services)
    {
        services.AddSingleton<ICurrentUnitCacheService, CurrentUnitCacheService>();
        services.AddSingleton<IWordCacheService, WordCacheService>();

        return services;
    }
}