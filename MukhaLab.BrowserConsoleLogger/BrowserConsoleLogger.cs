using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Collections.Concurrent;

namespace MukhaLab.BrowserConsoleLogger;

// Browser Console Logger
public sealed class BrowserConsoleLogger : ILogger
{
    private readonly string _categoryName;
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentQueue<PendingLogEntry> _pendingLogs = new();
    private static IJSRuntime? _cachedJSRuntime;
    private static bool _isProcessingQueue;

    public BrowserConsoleLogger(string categoryName, IServiceProvider serviceProvider)
    {
        _categoryName = categoryName;
        _serviceProvider = serviceProvider;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        var fullMessage = exception != null
            ? $"[{_categoryName}] {message}\n{exception}"
            : $"[{_categoryName}] {message}";

        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var level = GetLogLevelString(logLevel);

        // Завжди логуємо в Debug консоль
        System.Diagnostics.Debug.WriteLine($"[{timestamp}] [{level}] {fullMessage}");

        // Додаємо в чергу для браузерної консолі
        var pendingLog = new PendingLogEntry
        {
            LogLevel = logLevel,
            Timestamp = timestamp,
            Message = fullMessage,
            ConsoleMethod = GetConsoleMethod(logLevel)
        };

        _pendingLogs.Enqueue(pendingLog);

        // Намагаємося обробити чергу
        _ = Task.Run(async () => await ProcessLogQueue());
    }

    private async Task ProcessLogQueue()
    {
        if (_isProcessingQueue) return;

        try
        {
            _isProcessingQueue = true;

            // Спробуємо отримати JSRuntime
            if (_cachedJSRuntime == null)
            {
                _cachedJSRuntime = _serviceProvider.GetService<IJSRuntime>();
                if (_cachedJSRuntime == null) return;
            }

            // Обробляємо всі логи з черги
            var logsToProcess = new List<PendingLogEntry>();
            while (_pendingLogs.TryDequeue(out var log) && logsToProcess.Count < 50) // Обмежуємо кількість за раз
            {
                logsToProcess.Add(log);
            }

            if (logsToProcess.Count == 0) return;

            // Намагаємося відправити всі логи в браузер
            foreach (var log in logsToProcess)
            {
                try
                {
                    var browserMessage = $"[{log.Timestamp}] {log.Message}";
                    await _cachedJSRuntime.InvokeVoidAsync(log.ConsoleMethod, browserMessage);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("WebView context"))
                {
                    // Повертаємо логи назад в чергу для пізнішої обробки
                    _pendingLogs.Enqueue(log);
                    break; // Припиняємо обробку цієї партії
                }
                catch (JSDisconnectedException)
                {
                    // JavaScript відключений, повертаємо логи назад
                    _pendingLogs.Enqueue(log);
                    _cachedJSRuntime = null; // Сбрасываем кеш
                    break;
                }
                catch (Exception)
                {
                    // Інші помилки - просто пропускаємо цей лог
                    continue;
                }
            }
        }
        finally
        {
            _isProcessingQueue = false;
        }
    }

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

    private static string GetConsoleMethod(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Critical or LogLevel.Error => "console.error",
        LogLevel.Warning => "console.warn",
        LogLevel.Information => "console.info",
        LogLevel.Debug => "console.debug",
        LogLevel.Trace => "console.trace",
        _ => "console.log"
    };

    private record PendingLogEntry
    {
        public LogLevel LogLevel { get; init; }
        public string Timestamp { get; init; } = "";
        public string Message { get; init; } = "";
        public string ConsoleMethod { get; init; } = "";
    }
}
