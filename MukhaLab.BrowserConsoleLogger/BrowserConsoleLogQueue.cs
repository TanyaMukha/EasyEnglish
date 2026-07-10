using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Collections.Concurrent;
using System.Threading;

namespace MukhaLab.BrowserConsoleLogger;

/// <summary>
/// Delivery pipeline shared by every <see cref="BrowserConsoleLogger"/> created by the same
/// <see cref="BrowserConsoleLoggerProvider"/> instance: one pending-log queue and one cached
/// <see cref="IJSRuntime"/>, flushed to the browser console in the background.
/// </summary>
/// <remarks>
/// Owned as an instance field of <see cref="BrowserConsoleLoggerProvider"/> rather than a
/// <c>static</c> field on the logger type, so state does not leak across independent provider
/// instances (e.g. between test runs, or between an app's providers if more than one is ever
/// created). <see cref="ILoggerProvider"/> registrations are still host-level singletons in
/// ASP.NET Core, so a single Blazor Server host still has exactly one provider — and therefore one
/// queue — shared by every circuit; full per-user isolation would additionally require registering
/// a provider per circuit, which is outside the scope of this class.
/// </remarks>
internal sealed class BrowserConsoleLogQueue
{
    /// <summary>Maximum number of entries retained while delivery is not keeping up. Oldest entries are dropped once exceeded, so recent logs are prioritized.</summary>
    private const int MaxQueueSize = 1000;

    /// <summary>Maximum number of entries flushed to the browser console per background attempt.</summary>
    private const int MaxBatchSize = 50;

    private readonly ConcurrentQueue<PendingLogEntry> _pendingLogs = new();
    private readonly IServiceProvider _serviceProvider;
    private IJSRuntime? _cachedJSRuntime;
    private int _isProcessingQueue;

    /// <summary>Initializes the queue.</summary>
    /// <param name="serviceProvider">Service provider used to lazily resolve <see cref="IJSRuntime"/> when flushing.</param>
    public BrowserConsoleLogQueue(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Queues <paramref name="entry"/> for delivery and schedules a background flush attempt. If the
    /// queue is at capacity (<see cref="MaxQueueSize"/>), the oldest entries are dropped to make room.
    /// </summary>
    public void Enqueue(PendingLogEntry entry)
    {
        _pendingLogs.Enqueue(entry);

        while (_pendingLogs.Count > MaxQueueSize && _pendingLogs.TryDequeue(out _))
        {
        }

        // Fire-and-forget: try to drain the queue without blocking the caller.
        _ = Task.Run(ProcessQueueAsync);
    }

    /// <summary>Discards all queued entries and forgets the cached <see cref="IJSRuntime"/>.</summary>
    public void Clear()
    {
        while (_pendingLogs.TryDequeue(out _))
        {
        }

        _cachedJSRuntime = null;
    }

    /// <summary>
    /// Drains up to <see cref="MaxBatchSize"/> queued entries and forwards each to the browser console
    /// via <see cref="IJSRuntime.InvokeVoidAsync(string, object[])"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="InvalidOperationException"/> whose message mentions "WebView context" and
    /// <see cref="JSDisconnectedException"/> are treated as transient/JS-not-ready conditions: the
    /// entry is re-queued for a later attempt and the current batch is abandoned. A
    /// <see cref="JSDisconnectedException"/> additionally clears the cached <see cref="IJSRuntime"/> so
    /// the next flush attempt re-resolves it from DI. Any other exception is <b>not</b> silently
    /// dropped: the entry did not reach the browser console, but it is written to
    /// <see cref="System.Diagnostics.Debug"/> so it remains observable, then abandoned (not retried).
    /// </remarks>
    private async Task ProcessQueueAsync()
    {
        // Interlocked-based re-entrancy guard: at most one flush runs at a time for this queue.
        if (Interlocked.CompareExchange(ref _isProcessingQueue, 1, 0) != 0)
            return;

        try
        {
            // Resolve and cache the IJSRuntime once JS interop becomes available.
            if (_cachedJSRuntime == null)
            {
                _cachedJSRuntime = _serviceProvider.GetService<IJSRuntime>();
                if (_cachedJSRuntime == null) return;
            }

            // Dequeue a bounded batch so a single flush can't run unbounded.
            var logsToProcess = new List<PendingLogEntry>();
            while (_pendingLogs.TryDequeue(out var log) && logsToProcess.Count < MaxBatchSize)
            {
                logsToProcess.Add(log);
            }

            if (logsToProcess.Count == 0) return;

            foreach (var log in logsToProcess)
            {
                try
                {
                    var browserMessage = $"[{log.Timestamp}] {log.Message}";
                    await _cachedJSRuntime.InvokeVoidAsync(log.ConsoleMethod, browserMessage);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("WebView context"))
                {
                    // JS interop not ready yet: re-queue and stop processing this batch.
                    _pendingLogs.Enqueue(log);
                    break;
                }
                catch (JSDisconnectedException)
                {
                    // The circuit/WebView went away: re-queue and force re-resolution of IJSRuntime.
                    _pendingLogs.Enqueue(log);
                    _cachedJSRuntime = null;
                    break;
                }
                catch (Exception ex)
                {
                    // Don't silently drop the entry: it won't reach the browser console, but it
                    // stays observable via the Debug output instead of vanishing without a trace.
                    System.Diagnostics.Debug.WriteLine(
                        $"BrowserConsoleLogger: failed to deliver a log entry to the browser console ({ex.GetType().Name}: {ex.Message}).");
                    System.Diagnostics.Debug.WriteLine($"[{log.Timestamp}] {log.Message}");
                }
            }
        }
        finally
        {
            Interlocked.Exchange(ref _isProcessingQueue, 0);
        }
    }
}

/// <summary>A single formatted log entry waiting to be delivered to the browser console.</summary>
internal sealed record PendingLogEntry
{
    /// <summary>Original log level, retained for diagnostics (not used for routing beyond <see cref="ConsoleMethod"/>, which is precomputed).</summary>
    public LogLevel LogLevel { get; init; }

    /// <summary>Local time the entry was logged, formatted as <c>HH:mm:ss.fff</c>.</summary>
    public string Timestamp { get; init; } = "";

    /// <summary>Fully formatted message, including the <c>[category]</c> prefix and any exception text.</summary>
    public string Message { get; init; } = "";

    /// <summary>Browser console method to invoke (e.g. <c>"console.error"</c>), precomputed by the caller.</summary>
    public string ConsoleMethod { get; init; } = "";
}
