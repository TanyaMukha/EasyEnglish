using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using NSubstitute;

namespace MukhaLab.BrowserConsoleLogger.Tests;

public class BrowserConsoleLoggerProviderTests
{
    private static IServiceProvider CreateServiceProvider()
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IJSRuntime)).Returns((IJSRuntime?)null);
        return serviceProvider;
    }

    [Fact]
    public void CreateLogger_SameCategory_ReturnsCachedInstance()
    {
        var provider = new BrowserConsoleLoggerProvider(CreateServiceProvider());

        var logger1 = provider.CreateLogger("Cat");
        var logger2 = provider.CreateLogger("Cat");

        Assert.Same(logger1, logger2);
    }

    [Fact]
    public void CreateLogger_DifferentCategories_ReturnsDifferentInstances()
    {
        var provider = new BrowserConsoleLoggerProvider(CreateServiceProvider());

        var logger1 = provider.CreateLogger("CatA");
        var logger2 = provider.CreateLogger("CatB");

        Assert.NotSame(logger1, logger2);
    }

    [Fact]
    public void Dispose_ClearsCategoryCache()
    {
        var provider = new BrowserConsoleLoggerProvider(CreateServiceProvider());
        var logger = provider.CreateLogger("Cat");

        provider.Dispose();
        var loggerAfterDispose = provider.CreateLogger("Cat");

        Assert.NotSame(logger, loggerAfterDispose);
    }

    [Fact]
    public void CreateLogger_ReturnsAnILogger()
    {
        var provider = new BrowserConsoleLoggerProvider(CreateServiceProvider());

        var logger = provider.CreateLogger("Cat");

        Assert.IsAssignableFrom<ILogger>(logger);
    }
}
