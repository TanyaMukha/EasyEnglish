using Microsoft.Extensions.Logging;

namespace MukhaLab.BrowserConsoleLogger;

/// <summary>
/// <see cref="ILogger"/> implementation created per category by <see cref="BrowserConsoleLoggerProvider"/>.
/// Every log entry is written immediately to <see cref="System.Diagnostics.Debug"/> and additionally
/// handed to a shared <see cref="BrowserConsoleLogQueue"/> for delivery to the hosting WebView's
/// JavaScript console (<c>console.log</c>/<c>info</c>/<c>warn</c>/<c>error</c>/<c>debug</c>/<c>trace</c>).
/// </summary>
/// <remarks>
/// <see cref="Log{TState}"/> can be invoked from anywhere in the application — including startup code,
/// background threads, and Blazor prerendering — at points where JS interop is not yet available. Log
/// entries are therefore queued and flushed asynchronously by <see cref="BrowserConsoleLogQueue"/> once
/// an <see cref="Microsoft.JSInterop.IJSRuntime"/> can be resolved, instead of being sent synchronously
/// from this method. All loggers created by the same <see cref="BrowserConsoleLoggerProvider"/> share
/// one <see cref="BrowserConsoleLogQueue"/> instance, owned by that provider — not a process-wide
/// static — so delivery state is scoped to that provider instance rather than the whole process.
/// </remarks>
public sealed class BrowserConsoleLogger : ILogger
{
    private readonly string _categoryName;
    private readonly BrowserConsoleLogQueue _queue;

    /// <summary>Initializes a new logger for the given category.</summary>
    /// <param name="categoryName">Logging category name, prefixed onto every message (e.g. the requesting type's full name).</param>
    /// <param name="queue">Shared delivery pipeline owned by the parent <see cref="BrowserConsoleLoggerProvider"/>.</param>
    /// <remarks>
    /// Internal: instances are only meant to be created by <see cref="BrowserConsoleLoggerProvider"/>
    /// via <see cref="ILoggerProvider.CreateLogger"/>; consumers use it through the <see cref="ILogger"/>
    /// abstraction.
    /// </remarks>
    internal BrowserConsoleLogger(string categoryName, BrowserConsoleLogQueue queue)
    {
        _categoryName = categoryName;
        _queue = queue;
    }

    /// <summary>Logging scopes are not supported; always returns <c>null</c>.</summary>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    /// <summary>
    /// Always <c>true</c> except for <see cref="LogLevel.None"/>. This provider intentionally does not
    /// apply its own minimum level — verbosity is controlled entirely by the standard
    /// <see cref="ILoggingBuilder"/> configuration (<c>SetMinimumLevel</c>, <c>AddFilter</c>) applied by
    /// the host.
    /// </summary>
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    /// <summary>
    /// Formats the log entry, writes it to <see cref="System.Diagnostics.Debug"/> immediately, and
    /// hands it to the shared <see cref="BrowserConsoleLogQueue"/> for asynchronous delivery to the
    /// browser console.
    /// </summary>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        var fullMessage = exception != null
            ? $"[{_categoryName}] {message}\n{exception}"
            : $"[{_categoryName}] {message}";

        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var level = GetLogLevelString(logLevel);

        // Always mirror to the Debug console; this never depends on JS interop being available.
        System.Diagnostics.Debug.WriteLine($"[{timestamp}] [{level}] {fullMessage}");

        _queue.Enqueue(new PendingLogEntry
        {
            LogLevel = logLevel,
            Timestamp = timestamp,
            Message = fullMessage,
            ConsoleMethod = GetConsoleMethod(logLevel)
        });
    }

    /// <summary>Maps a <see cref="LogLevel"/> to the short label used in the Debug-console line prefix.</summary>
    private static string GetLogLevelString(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Critical => "CRIT",
        LogLevel.Error => "ERROR",
        LogLevel.Warning => "WARN",
        LogLevel.Information => "INFO",
        LogLevel.Debug => "DEBUG",
        LogLevel.Trace => "TRACE",
        _ => "LOG"
    };

    /// <summary>Maps a <see cref="LogLevel"/> to the browser console method invoked via JS interop.</summary>
    private static string GetConsoleMethod(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Critical or LogLevel.Error => "console.error",
        LogLevel.Warning => "console.warn",
        LogLevel.Information => "console.info",
        LogLevel.Debug => "console.debug",
        LogLevel.Trace => "console.trace",
        _ => "console.log"
    };
}
