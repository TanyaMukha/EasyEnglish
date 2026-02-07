using Microsoft.Extensions.Logging;

namespace MukhaLab.BrowserConsoleLogger;

public interface IBrowserConsoleService
{
    Task LogAsync(LogLevel logLevel, string message);
    Task LogInfoAsync(string message);
    Task LogWarningAsync(string message);
    Task LogErrorAsync(string message, Exception? exception = null);
    Task LogDebugAsync(string message);
}
