using Microsoft.JSInterop;
using MukhaLab.BrowserConsoleLogger.Tests.Fixtures;
using NSubstitute;
using NSubstitute.Core;

namespace MukhaLab.BrowserConsoleLogger.Tests;

public class BrowserConsoleLogQueueTests
{
    private static IServiceProvider CreateServiceProvider(IJSRuntime? jsRuntime) =>
        CreateServiceProvider(_ => jsRuntime);

    private static IServiceProvider CreateServiceProvider(Func<CallInfo, IJSRuntime?> resolve)
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IJSRuntime)).Returns(resolve);
        return serviceProvider;
    }

    [Fact]
    public void Enqueue_MoreThanMaxCapacity_DropsOldestEntries()
    {
        // Regression: the pending queue is capped at 1000 entries. No JSRuntime is ever resolved
        // here, so nothing gets flushed away — this isolates the cap-enforcement behavior itself.
        var queue = new BrowserConsoleLogQueue(CreateServiceProvider((IJSRuntime?)null));

        for (var i = 0; i < 1500; i++)
            queue.Enqueue(new PendingLogEntry { Timestamp = "t", Message = $"msg-{i}", ConsoleMethod = "console.log" });

        Assert.True(queue.PendingCount <= 1000, $"Expected PendingCount <= 1000, was {queue.PendingCount}");
    }

    [Fact]
    public async Task Enqueue_DeliversEntryWithFormattedTimestampAndConsoleMethod()
    {
        var jsRuntime = new FakeJSRuntime();
        var queue = new BrowserConsoleLogQueue(CreateServiceProvider(jsRuntime));

        queue.Enqueue(new PendingLogEntry { Timestamp = "12:00:00.000", Message = "[Cat] hello", ConsoleMethod = "console.error" });

        Assert.True(await queue.WaitUntilIdleAsync(TimeSpan.FromSeconds(2)));

        var call = Assert.Single(jsRuntime.Calls);
        Assert.Equal("console.error", call.Identifier);
        Assert.Equal("[12:00:00.000] [Cat] hello", call.Args[0]);
    }

    [Fact]
    public async Task Enqueue_WebViewContextException_ReQueuesEntry_DeliveredOnNextFlushAttempt()
    {
        // Regression: "WebView context" InvalidOperationException is treated as transient — the
        // entry must be re-queued (not dropped), and delivered once a later flush succeeds.
        var jsRuntime = new FakeJSRuntime
        {
            ThrowOnce = new InvalidOperationException("Cannot invoke ... WebView context is not available ...")
        };
        var queue = new BrowserConsoleLogQueue(CreateServiceProvider(jsRuntime));

        queue.Enqueue(new PendingLogEntry { Timestamp = "t1", Message = "first", ConsoleMethod = "console.log" });

        // Give the failing attempt a moment to run; there is no automatic retry loop, so it stays
        // queued until another Enqueue call triggers the next flush attempt.
        await Task.Delay(150);
        Assert.Equal(1, queue.PendingCount);
        Assert.Single(jsRuntime.Calls);

        queue.Enqueue(new PendingLogEntry { Timestamp = "t2", Message = "second", ConsoleMethod = "console.log" });
        Assert.True(await queue.WaitUntilIdleAsync(TimeSpan.FromSeconds(2)));

        // 3 total invocations: the initial failed attempt on "first", its successful retry, and
        // "second" delivered alongside it. Both entries ended up delivered — nothing was lost.
        Assert.Equal(3, jsRuntime.Calls.Count);
        Assert.Contains(jsRuntime.Calls, c => c.Args[0]!.ToString()!.Contains("first"));
        Assert.Contains(jsRuntime.Calls, c => c.Args[0]!.ToString()!.Contains("second"));
    }

    [Fact]
    public async Task Enqueue_JSDisconnectedException_ReQueuesEntryAndClearsCachedRuntime()
    {
        var failingRuntime = new FakeJSRuntime { ThrowOnce = new JSDisconnectedException("disconnected") };
        var workingRuntime = new FakeJSRuntime();
        var resolutionCount = 0;

        var queue = new BrowserConsoleLogQueue(CreateServiceProvider(_ =>
        {
            resolutionCount++;
            return resolutionCount == 1 ? failingRuntime : workingRuntime;
        }));

        queue.Enqueue(new PendingLogEntry { Timestamp = "t1", Message = "first", ConsoleMethod = "console.log" });
        await Task.Delay(150);

        queue.Enqueue(new PendingLogEntry { Timestamp = "t2", Message = "second", ConsoleMethod = "console.log" });
        Assert.True(await queue.WaitUntilIdleAsync(TimeSpan.FromSeconds(2)));

        // The cached (failing) runtime was discarded after JSDisconnectedException, forcing
        // re-resolution from DI — which is what let the second attempt succeed on workingRuntime.
        Assert.True(resolutionCount >= 2);
        Assert.Single(failingRuntime.Calls);
        Assert.Equal(2, workingRuntime.Calls.Count);
    }

    [Fact]
    public async Task Enqueue_UnknownException_DropsOnlyThatEntry_RestOfBatchStillDelivered()
    {
        // Regression: an unrecognized exception must not silently swallow the rest of the batch —
        // only the offending entry is dropped (not retried); other entries in the same batch still
        // get delivered.
        var jsRuntime = new FakeJSRuntime
        {
            ThrowSelector = (_, args) => args[0]!.ToString()!.Contains("bad-entry")
                ? new InvalidOperationException("boom")
                : null
        };
        var queue = new BrowserConsoleLogQueue(CreateServiceProvider(jsRuntime));

        queue.Enqueue(new PendingLogEntry { Timestamp = "t1", Message = "bad-entry", ConsoleMethod = "console.log" });
        queue.Enqueue(new PendingLogEntry { Timestamp = "t2", Message = "good-entry", ConsoleMethod = "console.log" });

        Assert.True(await queue.WaitUntilIdleAsync(TimeSpan.FromSeconds(2)));

        // Reached idle (queue empty) even though one entry failed: it was dropped, not stuck retrying.
        Assert.Equal(0, queue.PendingCount);
        Assert.Contains(jsRuntime.Calls, c => c.Args[0]!.ToString()!.Contains("good-entry"));
    }

    [Fact]
    public async Task Enqueue_WhenJSRuntimeUnavailable_LeavesEntryQueued()
    {
        var queue = new BrowserConsoleLogQueue(CreateServiceProvider((IJSRuntime?)null));

        queue.Enqueue(new PendingLogEntry { Timestamp = "t", Message = "m", ConsoleMethod = "console.log" });
        await Task.Delay(100);

        Assert.Equal(1, queue.PendingCount);
    }

    [Fact]
    public async Task Clear_DiscardsPendingEntriesAndCachedRuntime()
    {
        var jsRuntime = new FakeJSRuntime();
        var resolutionCount = 0;
        var queue = new BrowserConsoleLogQueue(CreateServiceProvider(_ => { resolutionCount++; return jsRuntime; }));

        queue.Enqueue(new PendingLogEntry { Timestamp = "t1", Message = "first", ConsoleMethod = "console.log" });
        Assert.True(await queue.WaitUntilIdleAsync(TimeSpan.FromSeconds(2)));
        var resolutionsBeforeClear = resolutionCount;

        queue.Clear();
        queue.Enqueue(new PendingLogEntry { Timestamp = "t2", Message = "second", ConsoleMethod = "console.log" });
        Assert.True(await queue.WaitUntilIdleAsync(TimeSpan.FromSeconds(2)));

        // Clear() forgets the cached runtime, so the next flush re-resolves it from DI.
        Assert.True(resolutionCount > resolutionsBeforeClear);
    }
}
