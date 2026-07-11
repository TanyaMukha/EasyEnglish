using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace MukhaLab.LoggerExtensionDelegate.Tests;

public class MethodEntryExitTests
{
    private static FakeLogger CreateLogger() => new(new FakeLogCollector(), "Test");

    [Fact]
    public void FastMethodEntry_NoParameters_LogsEnterWithCallerName()
    {
        var logger = CreateLogger();

        logger.FastMethodEntry();

        var record = logger.LatestRecord;
        Assert.Equal(LogLevel.Debug, record.Level);
        Assert.Equal($"ENTER {nameof(FastMethodEntry_NoParameters_LogsEnterWithCallerName)}()", record.Message);
    }

    [Fact]
    public void FastMethodEntry_WithParameters_IncludesThem()
    {
        var logger = CreateLogger();

        logger.FastMethodEntry(new { id = 5 });

        Assert.Contains("id = 5", logger.LatestRecord.Message);
    }

    [Fact]
    public void FastMethodExit_NoResult_LogsExitWithCallerName()
    {
        var logger = CreateLogger();

        logger.FastMethodExit();

        Assert.Equal($"EXIT {nameof(FastMethodExit_NoResult_LogsExitWithCallerName)}", logger.LatestRecord.Message);
    }

    [Fact]
    public void FastMethodExit_WithResult_IncludesIt()
    {
        var logger = CreateLogger();

        logger.FastMethodExit(42);

        Assert.Contains("-> 42", logger.LatestRecord.Message);
    }

    [Fact]
    public void FastMethodEntry_WhenDebugDisabled_DoesNotLog()
    {
        var logger = CreateLogger();
        logger.ControlLevel(LogLevel.Debug, false);

        logger.FastMethodEntry();

        Assert.Equal(0, logger.Collector.Count);
    }

    [Fact]
    public void FastMethodExit_WhenDebugDisabled_DoesNotLog()
    {
        var logger = CreateLogger();
        logger.ControlLevel(LogLevel.Debug, false);

        logger.FastMethodExit();

        Assert.Equal(0, logger.Collector.Count);
    }
}
