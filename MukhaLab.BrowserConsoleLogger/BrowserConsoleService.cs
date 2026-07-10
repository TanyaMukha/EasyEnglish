using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace MukhaLab.BrowserConsoleLogger;

/// <inheritdoc cref="IBrowserConsoleService"/>
public class BrowserConsoleService : IBrowserConsoleService
{
    private readonly IJSRuntime _jsRuntime;

    /// <summary>Initializes the service with the <see cref="IJSRuntime"/> for the current scope.</summary>
    public BrowserConsoleService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Sends <paramref name="message"/> to the browser console via JS interop, prefixed with a local
    /// timestamp. If the JS interop call throws for any reason (e.g. interop not ready, circuit
    /// disconnected), the failure and the original message are both written to
    /// <see cref="System.Diagnostics.Debug"/> instead — unlike <see cref="BrowserConsoleLogger"/>, no
    /// entry is silently dropped or retried.
    /// </summary>
    public async Task LogAsync(LogLevel logLevel, string message)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var consoleMethod = GetConsoleMethod(logLevel);
            var browserMessage = $"[{timestamp}] {message}";

            await _jsRuntime.InvokeVoidAsync(consoleMethod, browserMessage);
        }
        catch (Exception ex)
        {
            // JS interop unavailable or failed: fall back to the Debug console so the entry isn't lost.
            System.Diagnostics.Debug.WriteLine($"Browser console failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{GetLogLevelString(logLevel)}] {message}");
        }
    }

    /// <inheritdoc/>
    public async Task LogInfoAsync(string message) => await LogAsync(LogLevel.Information, message);

    /// <inheritdoc/>
    public async Task LogWarningAsync(string message) => await LogAsync(LogLevel.Warning, message);

    /// <inheritdoc/>
    public async Task LogErrorAsync(string message, Exception? exception = null)
    {
        var fullMessage = exception != null ? $"{message}\n{exception}" : message;
        await LogAsync(LogLevel.Error, fullMessage);
    }

    /// <inheritdoc/>
    public async Task LogDebugAsync(string message) => await LogAsync(LogLevel.Debug, message);

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

    /// <summary>Maps a <see cref="LogLevel"/> to the short label used in the Debug-console fallback line.</summary>
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
}
