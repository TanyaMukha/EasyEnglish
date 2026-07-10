using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace MukhaLab.BrowserConsoleLogger;

/// <summary>
/// <see cref="ILoggerProvider"/> that creates and caches one <see cref="BrowserConsoleLogger"/> per
/// logging category. All loggers created by a given provider instance share one
/// <see cref="BrowserConsoleLogQueue"/> owned by that instance. Register it via
/// <see cref="BrowserConsoleLoggerExtensions.AddBrowserConsole"/>.
/// </summary>
/// <remarks>
/// <see cref="Dispose"/> clears both the category-to-logger cache and the shared
/// <see cref="BrowserConsoleLogQueue"/> (pending entries and the cached
/// <see cref="Microsoft.JSInterop.IJSRuntime"/>).
/// </remarks>
public sealed class BrowserConsoleLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, BrowserConsoleLogger> _loggers = new();
    private readonly BrowserConsoleLogQueue _queue;

    /// <summary>Initializes the provider and its shared delivery queue.</summary>
    /// <param name="serviceProvider">Service provider used by the shared queue to lazily resolve <see cref="Microsoft.JSInterop.IJSRuntime"/>.</param>
    public BrowserConsoleLoggerProvider(IServiceProvider serviceProvider)
    {
        _queue = new BrowserConsoleLogQueue(serviceProvider);
    }

    /// <summary>Returns the cached logger for <paramref name="categoryName"/>, creating one on first request.</summary>
    /// <param name="categoryName">Logging category name (typically a fully-qualified type name).</param>
    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name =>
            new BrowserConsoleLogger(name, _queue));
    }

    /// <summary>Clears the category-to-logger cache and the shared delivery queue.</summary>
    public void Dispose()
    {
        _loggers.Clear();
        _queue.Clear();
    }
}
