using Microsoft.Extensions.Logging.Testing;

namespace MukhaLab.LoggerExtensionDelegate.Tests;

public class BeginTimedScopeTests
{
    private static FakeLogger CreateLogger() => new(new FakeLogCollector(), "Test");

    [Fact]
    public void BeginTimedScope_EntryLog_ReportsRealCallerName_NotCtor()
    {
        // Regression: the entry log must name the method that called BeginTimedScope, not
        // TimedScope's own constructor (".ctor").
        var logger = CreateLogger();

        using (logger.BeginTimedScope("ImportWords"))
        {
        }

        var entryRecord = logger.Collector.GetSnapshot().First(r => r.Message.StartsWith("ENTER"));
        Assert.Contains(nameof(BeginTimedScope_EntryLog_ReportsRealCallerName_NotCtor), entryRecord.Message);
        Assert.DoesNotContain(".ctor", entryRecord.Message);
    }

    [Fact]
    public void BeginTimedScope_OnDispose_LogsDurationWithOperationName()
    {
        var logger = CreateLogger();

        using (logger.BeginTimedScope("ImportWords"))
        {
        }

        var durationRecord = logger.Collector.GetSnapshot().First(r => r.Message.Contains("PERFORMANCE"));
        Assert.Contains("ImportWords", durationRecord.Message);
    }
}
