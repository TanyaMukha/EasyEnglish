using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MukhaLab.BrowserConsoleLogger.Tests;

public class BrowserConsoleLoggerExtensionsTests
{
    [Fact]
    public void AddBrowserConsole_RegistersProviderAsSingleton()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder => builder.AddBrowserConsole());

        var descriptor = services.Single(d =>
            d.ServiceType == typeof(ILoggerProvider) && d.ImplementationType == typeof(BrowserConsoleLoggerProvider));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddBrowserConsole_ResolvesToBrowserConsoleLoggerProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddBrowserConsole());

        using var provider = services.BuildServiceProvider();
        var loggerProviders = provider.GetServices<ILoggerProvider>();

        Assert.Contains(loggerProviders, p => p is BrowserConsoleLoggerProvider);
    }

    [Fact]
    public void AddBrowserConsoleService_RegistersServiceAsScoped()
    {
        var services = new ServiceCollection();

        services.AddBrowserConsoleService();

        var descriptor = services.Single(d => d.ServiceType == typeof(IBrowserConsoleService));
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(BrowserConsoleService), descriptor.ImplementationType);
    }
}
