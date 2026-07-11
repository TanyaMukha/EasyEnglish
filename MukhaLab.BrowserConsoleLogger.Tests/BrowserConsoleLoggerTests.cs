using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MukhaLab.BrowserConsoleLogger.Tests.Fixtures;
using NSubstitute;

namespace MukhaLab.BrowserConsoleLogger.Tests;

public class BrowserConsoleLoggerTests
{
    private static (BrowserConsoleLogger Logger, FakeJSRuntime JsRuntime, BrowserConsoleLogQueue Queue) CreateLogger(string category)
    {
        var jsRuntime = new FakeJSRuntime();
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IJSRuntime)).Returns(jsRuntime);
        var queue = new BrowserConsoleLogQueue(serviceProvider);
        return (new BrowserConsoleLogger(category, queue), jsRuntime, queue);
    }

    [Fact]
    public async Task Log_DeliversMessageWithCategoryPrefix()
    {
        var (logger, jsRuntime, queue) = CreateLogger("MyCategory");

        logger.Log(LogLevel.Information, new EventId(0), "hello", null, (s, _) => s);
        Assert.True(await queue.WaitUntilIdleAsync(TimeSpan.FromSeconds(2)));

        var call = Assert.Single(jsRuntime.Calls);
        Assert.Equal("console.info", call.Identifier);
        Assert.Contains("[MyCategory] hello", call.Args[0]!.ToString());
    }

    [Fact]
    public async Task Log_WithException_AppendsExceptionText()
    {
        var (logger, jsRuntime, queue) = CreateLogger("Cat");
        var ex = new InvalidOperationException("boom");

        logger.Log(LogLevel.Error, new EventId(0), "failed", ex, (s, _) => s);
        Assert.True(await queue.WaitUntilIdleAsync(TimeSpan.FromSeconds(2)));

        var call = Assert.Single(jsRuntime.Calls);
        Assert.Equal("console.error", call.Identifier);
        Assert.Contains("boom", call.Args[0]!.ToString());
    }

    [Theory]
    [InlineData(LogLevel.Critical, "console.error")]
    [InlineData(LogLevel.Error, "console.error")]
    [InlineData(LogLevel.Warning, "console.warn")]
    [InlineData(LogLevel.Information, "console.info")]
    [InlineData(LogLevel.Debug, "console.debug")]
    [InlineData(LogLevel.Trace, "console.trace")]
    public async Task Log_MapsLevelToCorrectConsoleMethod(LogLevel level, string expectedMethod)
    {
        var (logger, jsRuntime, queue) = CreateLogger("Cat");

        logger.Log(level, new EventId(0), "msg", null, (s, _) => s);
        Assert.True(await queue.WaitUntilIdleAsync(TimeSpan.FromSeconds(2)));

        var call = Assert.Single(jsRuntime.Calls);
        Assert.Equal(expectedMethod, call.Identifier);
    }

    [Fact]
    public void IsEnabled_TrueForEveryLevelExceptNone()
    {
        var (logger, _, _) = CreateLogger("Cat");

        Assert.True(logger.IsEnabled(LogLevel.Trace));
        Assert.True(logger.IsEnabled(LogLevel.Critical));
        Assert.False(logger.IsEnabled(LogLevel.None));
    }

    [Fact]
    public void BeginScope_ReturnsNull()
    {
        var (logger, _, _) = CreateLogger("Cat");

        Assert.Null(logger.BeginScope("state"));
    }

    [Fact]
    public async Task Log_WhenLevelIsNone_DoesNotDeliverAnything()
    {
        var (logger, jsRuntime, _) = CreateLogger("Cat");

        logger.Log(LogLevel.None, new EventId(0), "hello", null, (s, _) => s);
        await Task.Delay(300);

        Assert.Empty(jsRuntime.Calls);
    }
}
