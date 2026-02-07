using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace MukhaLab.BrowserConsoleLogger;

public class BrowserConsoleService : IBrowserConsoleService
{
    private readonly IJSRuntime _jsRuntime;

    public BrowserConsoleService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

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
            // Fallback в Debug консоль
            System.Diagnostics.Debug.WriteLine($"Browser console failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{GetLogLevelString(logLevel)}] {message}");
        }
    }

    public async Task LogInfoAsync(string message) => await LogAsync(LogLevel.Information, message);
    public async Task LogWarningAsync(string message) => await LogAsync(LogLevel.Warning, message);
    public async Task LogErrorAsync(string message, Exception? exception = null)
    {
        var fullMessage = exception != null ? $"{message}\n{exception}" : message;
        await LogAsync(LogLevel.Error, fullMessage);
    }
    public async Task LogDebugAsync(string message) => await LogAsync(LogLevel.Debug, message);

    private static string GetConsoleMethod(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Critical or LogLevel.Error => "console.error",
        LogLevel.Warning => "console.warn",
        LogLevel.Information => "console.info",
        LogLevel.Debug => "console.debug",
        LogLevel.Trace => "console.trace",
        _ => "console.log"
    };

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
