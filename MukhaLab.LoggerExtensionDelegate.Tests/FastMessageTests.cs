using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace MukhaLab.LoggerExtensionDelegate.Tests;

public class FastMessageTests
{
    private static FakeLogger CreateLogger() => new(new FakeLogCollector(), "Test");

    [Fact]
    public void FastInfoMessage_PreFormatted_LogsAtInformation()
    {
        var logger = CreateLogger();

        logger.FastInfoMessage("hello");

        var record = logger.LatestRecord;
        Assert.Equal(LogLevel.Information, record.Level);
        Assert.Equal("hello", record.Message);
    }

    [Fact]
    public void FastInfoMessage_WithFormatArgs_FormatsMessage()
    {
        var logger = CreateLogger();

        logger.FastInfoMessage("Loaded {0} items in {1}ms", 5, 120);

        Assert.Equal("Loaded 5 items in 120ms", logger.LatestRecord.Message);
    }

    [Fact]
    public void FastErrorMessage_PreFormatted_WithException_LogsBoth()
    {
        var logger = CreateLogger();
        var ex = new InvalidOperationException("boom");

        logger.FastErrorMessage("failed", ex);

        var record = logger.LatestRecord;
        Assert.Equal(LogLevel.Error, record.Level);
        Assert.Equal("failed", record.Message);
        Assert.Same(ex, record.Exception);
    }

    [Fact]
    public void FastErrorMessage_ExceptionAndFormatArgs_FormatsAndAttachesException()
    {
        var logger = CreateLogger();
        var ex = new InvalidOperationException("boom");

        logger.FastErrorMessage(ex, "Failed to save item {0}", 42);

        var record = logger.LatestRecord;
        Assert.Equal(LogLevel.Error, record.Level);
        Assert.Equal("Failed to save item 42", record.Message);
        Assert.Same(ex, record.Exception);
    }

    [Fact]
    public void FastWarningMessage_PreFormatted_LogsAtWarning()
    {
        var logger = CreateLogger();

        logger.FastWarningMessage("careful");

        Assert.Equal(LogLevel.Warning, logger.LatestRecord.Level);
    }

    [Fact]
    public void FastWarningMessage_WithFormatArgs_FormatsMessage()
    {
        var logger = CreateLogger();

        logger.FastWarningMessage("Retry {0} of {1}", 2, 3);

        Assert.Equal("Retry 2 of 3", logger.LatestRecord.Message);
    }

    [Fact]
    public void FastDebugMessage_PreFormatted_LogsAtDebug()
    {
        var logger = CreateLogger();

        logger.FastDebugMessage("details");

        Assert.Equal(LogLevel.Debug, logger.LatestRecord.Level);
    }

    [Fact]
    public void FastDebugMessage_WithFormatArgs_FormatsMessage()
    {
        var logger = CreateLogger();

        logger.FastDebugMessage("Cache hit ratio: {0}%", 87);

        Assert.Equal("Cache hit ratio: 87%", logger.LatestRecord.Message);
    }

    [Fact]
    public void FastInfoMessage_FormatArgsOverload_WhenLevelDisabled_DoesNotLog()
    {
        var logger = CreateLogger();
        logger.ControlLevel(LogLevel.Information, false);

        logger.FastInfoMessage("Loaded {0} items", 5);

        Assert.Equal(0, logger.Collector.Count);
    }

    [Fact]
    public void FastWarningMessage_FormatArgsOverload_WhenLevelDisabled_DoesNotLog()
    {
        var logger = CreateLogger();
        logger.ControlLevel(LogLevel.Warning, false);

        logger.FastWarningMessage("Retry {0}", 1);

        Assert.Equal(0, logger.Collector.Count);
    }
}
