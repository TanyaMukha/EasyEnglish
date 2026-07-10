using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace MukhaLab.LoggerExtensionDelegate;

/// <summary>
/// Extensions for measuring and logging the duration of a scoped operation, built on top of
/// <see cref="LoggerExtension"/>.
/// </summary>
public static class PerformanceLoggerExtensions
{
    /// <summary>
    /// Starts timing an operation and returns an <see cref="IDisposable"/> that, when disposed,
    /// logs its elapsed duration via <see cref="LoggerExtension.FastPerformanceLog"/> (with the
    /// slow-operation "(SLOW)" marker for durations over 1000 ms).
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operationName">Name used to identify the operation in both the entry and duration log lines.</param>
    /// <param name="memberName">The calling method name (automatically provided); forwarded to the entry log so it names the real caller, not this method's internal wrapper.</param>
    /// <returns>A disposable scope; dispose it (typically via <c>using</c>) to stop the timer and log the result.</returns>
    /// <example>
    /// <code>
    /// using (logger.BeginTimedScope(nameof(ImportWordsAsync)))
    /// {
    ///     await ImportWordsAsync();
    /// }
    /// // logs "ENTER ImportWordsAsync(ImportWordsAsync)" on start, "PERFORMANCE: ImportWordsAsync took Nms" on dispose
    /// </code>
    /// </example>
    public static IDisposable BeginTimedScope(this ILogger logger, string operationName, [CallerMemberName] string memberName = "")
    {
        return new TimedScope(logger, operationName, memberName);
    }

    /// <summary>
    /// <see cref="IDisposable"/> backing <see cref="BeginTimedScope"/>: logs entry on construction
    /// and logs the elapsed time on <see cref="Dispose"/>.
    /// </summary>
    /// <remarks>
    /// The constructor is passed <c>memberName</c> explicitly by <see cref="BeginTimedScope"/> (captured
    /// there via <see cref="CallerMemberNameAttribute"/>) instead of letting
    /// <see cref="LoggerExtension.FastMethodEntry"/> capture its own caller — which would otherwise
    /// resolve to this constructor (<c>.ctor</c>) rather than the code that called
    /// <see cref="BeginTimedScope"/>.
    /// </remarks>
    private sealed class TimedScope : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _operationName;
        private readonly System.Diagnostics.Stopwatch _stopwatch;

        public TimedScope(ILogger logger, string operationName, string memberName)
        {
            _logger = logger;
            _operationName = operationName;
            _stopwatch = System.Diagnostics.Stopwatch.StartNew();
            logger.FastMethodEntry(operationName, memberName);
        }

        /// <summary>Stops the stopwatch and logs the elapsed duration for the operation.</summary>
        public void Dispose()
        {
            _stopwatch.Stop();
            _logger.FastPerformanceLog(_operationName, _stopwatch.ElapsedMilliseconds);
        }
    }
}
