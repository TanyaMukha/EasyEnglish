using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Collections.Concurrent;

namespace MukhaLab.BrowserConsoleLogger;

// Browser Console Logger Provider
public sealed class BrowserConsoleLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, BrowserConsoleLogger> _loggers = new();
    private readonly IServiceProvider _serviceProvider;

    public BrowserConsoleLoggerProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name =>
            new BrowserConsoleLogger(name, _serviceProvider));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}
