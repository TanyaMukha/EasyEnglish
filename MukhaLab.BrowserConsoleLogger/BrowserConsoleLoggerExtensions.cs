using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace MukhaLab.BrowserConsoleLogger;

/// <summary>
/// DI registration helpers for <see cref="MukhaLab.BrowserConsoleLogger"/>.
/// </summary>
public static class BrowserConsoleLoggerExtensions
{
    /// <summary>
    /// Registers <see cref="BrowserConsoleLoggerProvider"/> as a singleton <see cref="ILoggerProvider"/>,
    /// so every <see cref="ILogger{TCategoryName}"/> resolved afterwards also mirrors its output to the
    /// hosting WebView's JavaScript console. Verbosity is controlled by the usual
    /// <see cref="ILoggingBuilder"/> configuration (<c>SetMinimumLevel</c>, <c>AddFilter</c>), not by
    /// this method.
    /// </summary>
    /// <param name="builder">The logging builder to register the provider on.</param>
    /// <returns><paramref name="builder"/>, for chaining.</returns>
    /// <example>
    /// <code>
    /// #if DEBUG
    /// builder.Logging.AddDebug();
    /// builder.Logging.AddBrowserConsole();
    /// builder.Logging.SetMinimumLevel(LogLevel.Debug);
    /// #endif
    /// </code>
    /// </example>
    public static ILoggingBuilder AddBrowserConsole(this ILoggingBuilder builder)
    {
        builder.Services.AddSingleton<ILoggerProvider, BrowserConsoleLoggerProvider>();
        return builder;
    }

    /// <summary>
    /// Registers <see cref="BrowserConsoleService"/> as the scoped implementation of
    /// <see cref="IBrowserConsoleService"/>, for direct on-demand console logging from Razor
    /// components (see the library README). Mirrors <see cref="AddBrowserConsole"/> for the
    /// <see cref="ILoggerProvider"/> path.
    /// </summary>
    /// <param name="services">The service collection to register the service on.</param>
    /// <returns><paramref name="services"/>, for chaining.</returns>
    public static IServiceCollection AddBrowserConsoleService(this IServiceCollection services)
    {
        services.AddScoped<IBrowserConsoleService, BrowserConsoleService>();
        return services;
    }
}
