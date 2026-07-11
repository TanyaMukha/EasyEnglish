using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace MukhaLab.LoggerExtensionDelegate.Tests;

public class ContextLoggingTests
{
    private static FakeLogger CreateLogger() => new(new FakeLogCollector(), "Test");

    [Fact]
    public void FastInfoWithContext_IncludesCallingMemberNameAndMessage()
    {
        var logger = CreateLogger();

        logger.FastInfoWithContext("hello");

        var record = logger.LatestRecord;
        Assert.Equal(LogLevel.Information, record.Level);
        Assert.Contains(nameof(FastInfoWithContext_IncludesCallingMemberNameAndMessage), record.Message);
        Assert.Contains("hello", record.Message);
    }

    [Fact]
    public void FastInfoWithUserContext_IncludesCallingMemberNameUserIdAndMessage()
    {
        var logger = CreateLogger();

        logger.FastInfoWithUserContext(userId: "user-42", message: "profile updated");

        var record = logger.LatestRecord;
        Assert.Equal(LogLevel.Information, record.Level);
        Assert.Contains(nameof(FastInfoWithUserContext_IncludesCallingMemberNameUserIdAndMessage), record.Message);
        Assert.Contains("user-42", record.Message);
        Assert.Contains("profile updated", record.Message);
    }

    [Fact]
    public void FastInfoWithContext_WhenInformationDisabled_DoesNotLog()
    {
        var logger = CreateLogger();
        logger.ControlLevel(LogLevel.Information, false);

        logger.FastInfoWithContext("hello");

        Assert.Equal(0, logger.Collector.Count);
    }

    [Fact]
    public void FastInfoWithUserContext_WhenInformationDisabled_DoesNotLog()
    {
        var logger = CreateLogger();
        logger.ControlLevel(LogLevel.Information, false);

        logger.FastInfoWithUserContext("user-1", "hello");

        Assert.Equal(0, logger.Collector.Count);
    }
}
