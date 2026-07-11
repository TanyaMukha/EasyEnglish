using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace MukhaLab.LoggerExtensionDelegate.Tests;

public class PerformanceLoggingTests
{
    private static FakeLogger CreateLogger() => new(new FakeLogCollector(), "Test");

    [Fact]
    public void FastPerformanceLog_UnderThreshold_LogsAtInformation()
    {
        var logger = CreateLogger();

        logger.FastPerformanceLog("Op", 500);

        var record = logger.LatestRecord;
        Assert.Equal(LogLevel.Information, record.Level);
        Assert.Contains("Op", record.Message);
        Assert.DoesNotContain("SLOW", record.Message);
    }

    [Fact]
    public void FastPerformanceLog_OverThreshold_LogsAtWarningWithSlowMarker()
    {
        var logger = CreateLogger();

        logger.FastPerformanceLog("Op", 1500);

        var record = logger.LatestRecord;
        Assert.Equal(LogLevel.Warning, record.Level);
        Assert.Contains("(SLOW)", record.Message);
    }

    [Fact]
    public void FastPerformanceLog_SlowOperation_StillLogsWhenOnlyWarningEnabled()
    {
        // Regression: the slow-operation branch must check IsEnabled(Warning), not
        // IsEnabled(Information) — a category that disables Information but keeps Warning enabled
        // must still receive slow-operation warnings.
        var logger = CreateLogger();
        logger.ControlLevel(LogLevel.Information, false);
        logger.ControlLevel(LogLevel.Warning, true);

        logger.FastPerformanceLog("Op", 1500);

        Assert.Equal(1, logger.Collector.Count);
        Assert.Equal(LogLevel.Warning, logger.LatestRecord.Level);
    }

    [Fact]
    public void FastPerformanceLog_NormalOperation_SuppressedWhenInformationDisabled()
    {
        var logger = CreateLogger();
        logger.ControlLevel(LogLevel.Information, false);

        logger.FastPerformanceLog("Op", 500);

        Assert.Equal(0, logger.Collector.Count);
    }

    [Fact]
    public void FastPerformanceLog_NullLogger_DoesNotThrow()
    {
        ILogger? logger = null;

        var exception = Record.Exception(() => logger.FastPerformanceLog("Op", 500));

        Assert.Null(exception);
    }
}
