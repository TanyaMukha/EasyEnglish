using Microsoft.Extensions.Logging;

namespace MukhaLab.LoggerExtensionDelegate;

/// <summary>
/// Extensions for measuring and logging the duration of a scoped operation, built on top of
/// <see cref="LogerExtension"/>.
/// </summary>
public static class PerformanceLoggerExtensions
{
    /// <summary>
    /// Starts timing an operation and returns an <see cref="IDisposable"/> that, when disposed,
    /// logs its elapsed duration via <see cref="LogerExtension.FastPerformanceLog"/> (with the
    /// slow-operation "(SLOW)" marker for durations over 1000 ms).
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operationName">Name used to identify the operation in both the entry and duration log lines.</param>
    /// <returns>A disposable scope; dispose it (typically via <c>using</c>) to stop the timer and log the result.</returns>
    /// <example>
    /// <code>
    /// using (logger.BeginTimedScope(nameof(ImportWordsAsync)))
    /// {
    ///     await ImportWordsAsync();
    /// }
    /// // logs "ENTER .ctor(ImportWordsAsync)" on start, "PERFORMANCE: ImportWordsAsync took Nms" on dispose
    /// </code>
    /// </example>
    /// <remarks>
    /// The entry log written by <see cref="TimedScope"/>'s constructor reports <c>.ctor</c> as the
    /// member name, not the name of the method that called <see cref="BeginTimedScope"/> — see the
    /// <see cref="TimedScope"/> remarks for why. The duration log on <see cref="IDisposable.Dispose"/>
    /// is unaffected and correctly reports <paramref name="operationName"/>.
    /// </remarks>
    public static IDisposable BeginTimedScope(this ILogger logger, string operationName)
    {
        return new TimedScope(logger, operationName);
    }

    /// <summary>
    /// <see cref="IDisposable"/> backing <see cref="BeginTimedScope"/>: logs entry on construction
    /// and logs the elapsed time on <see cref="Dispose"/>.
    /// </summary>
    /// <remarks>
    /// The constructor calls <see cref="LogerExtension.FastMethodEntry"/> without an explicit
    /// <c>memberName</c> argument, so <see cref="System.Runtime.CompilerServices.CallerMemberNameAttribute"/>
    /// resolves to the name of the method that contains the call — which is this constructor
    /// (<c>.ctor</c>), not the method that called <see cref="BeginTimedScope"/>. The resulting entry
    /// log therefore reads "ENTER .ctor(&lt;operationName&gt;)" rather than naming the real caller.
    /// This is a known quirk of the current implementation, not a configuration mistake.
    /// </remarks>
    private sealed class TimedScope : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _operationName;
        private readonly System.Diagnostics.Stopwatch _stopwatch;

        public TimedScope(ILogger logger, string operationName)
        {
            _logger = logger;
            _operationName = operationName;
            _stopwatch = System.Diagnostics.Stopwatch.StartNew();
            logger.FastMethodEntry(operationName);
        }

        /// <summary>Stops the stopwatch and logs the elapsed duration for the operation.</summary>
        public void Dispose()
        {
            _stopwatch.Stop();
            _logger.FastPerformanceLog(_operationName, _stopwatch.ElapsedMilliseconds);
        }
    }
}
