using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace MukhaLab.BrowserConsoleLogger;

// Extension методи для зручності
public static class BrowserConsoleLoggerExtensions
{
    public static ILoggingBuilder AddBrowserConsole(this ILoggingBuilder builder)
    {
        builder.Services.AddSingleton<ILoggerProvider, BrowserConsoleLoggerProvider>();
        return builder;
    }
}
