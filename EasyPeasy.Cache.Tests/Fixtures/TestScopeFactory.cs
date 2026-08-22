using Microsoft.Extensions.DependencyInjection;

namespace EasyPeasy.Cache.Tests.Fixtures;

/// <summary>Builds a real <see cref="IServiceScopeFactory"/> backed by a throwaway container with a single registered instance — used to test the cache services' scope-per-fetch resolution against real DI behavior instead of hand-mocking the scope chain.</summary>
public static class TestScopeFactory
{
    public static IServiceScopeFactory ForInstance<TService>(TService instance) where TService : class
    {
        var services = new ServiceCollection();
        services.AddSingleton(instance);
        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IServiceScopeFactory>();
    }
}
