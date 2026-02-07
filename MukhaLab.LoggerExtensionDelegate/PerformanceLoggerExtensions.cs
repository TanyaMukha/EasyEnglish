using Microsoft.Extensions.Logging;

namespace MukhaLab.LoggerExtensionDelegate;

// Розширення для роботи з performance
public static class PerformanceLoggerExtensions
{
    public static IDisposable BeginTimedScope(this ILogger logger, string operationName)
    {
        return new TimedScope(logger, operationName);
    }

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

        public void Dispose()
        {
            _stopwatch.Stop();
            _logger.FastPerformanceLog(_operationName, _stopwatch.ElapsedMilliseconds);
        }
    }
}
