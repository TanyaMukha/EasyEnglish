using Microsoft.Extensions.Logging;

namespace MukhaLab.BrowserConsoleLogger;

/// <summary>
/// Direct, on-demand logging to the hosting WebView's JavaScript console. Unlike
/// <see cref="BrowserConsoleLogger"/>, calls are not queued: each call immediately awaits
/// <see cref="Microsoft.JSInterop.IJSRuntime"/>, so it is only safe to use from contexts where JS
/// interop is already available (e.g. Razor component lifecycle methods after first render), not
/// during static prerendering or early application startup.
/// </summary>
/// <remarks>
/// Not registered by any extension method in this library — register it explicitly, e.g.
/// <c>services.AddScoped&lt;IBrowserConsoleService, BrowserConsoleService&gt;();</c>.
/// </remarks>
public interface IBrowserConsoleService
{
    /// <summary>Logs <paramref name="message"/> to the browser console using the console method matching <paramref name="logLevel"/>.</summary>
    Task LogAsync(LogLevel logLevel, string message);

    /// <summary>Logs <paramref name="message"/> via <c>console.info</c>.</summary>
    Task LogInfoAsync(string message);

    /// <summary>Logs <paramref name="message"/> via <c>console.warn</c>.</summary>
    Task LogWarningAsync(string message);

    /// <summary>Logs <paramref name="message"/> (and <paramref name="exception"/>, if provided) via <c>console.error</c>.</summary>
    Task LogErrorAsync(string message, Exception? exception = null);

    /// <summary>Logs <paramref name="message"/> via <c>console.debug</c>.</summary>
    Task LogDebugAsync(string message);
}
