using Microsoft.Extensions.Logging;
using MukhaLab.BrowserConsoleLogger.Tests.Fixtures;

namespace MukhaLab.BrowserConsoleLogger.Tests;

public class BrowserConsoleServiceTests
{
    [Fact]
    public async Task LogInfoAsync_CallsInvokeVoidAsyncWithConsoleInfo()
    {
        var jsRuntime = new FakeJSRuntime();
        var service = new BrowserConsoleService(jsRuntime);

        await service.LogInfoAsync("hello");

        var call = Assert.Single(jsRuntime.Calls);
        Assert.Equal("console.info", call.Identifier);
        Assert.Contains("hello", call.Args[0]!.ToString());
    }

    [Fact]
    public async Task LogWarningAsync_UsesConsoleWarn()
    {
        var jsRuntime = new FakeJSRuntime();
        var service = new BrowserConsoleService(jsRuntime);

        await service.LogWarningAsync("careful");

        var call = Assert.Single(jsRuntime.Calls);
        Assert.Equal("console.warn", call.Identifier);
    }

    [Fact]
    public async Task LogDebugAsync_UsesConsoleDebug()
    {
        var jsRuntime = new FakeJSRuntime();
        var service = new BrowserConsoleService(jsRuntime);

        await service.LogDebugAsync("details");

        var call = Assert.Single(jsRuntime.Calls);
        Assert.Equal("console.debug", call.Identifier);
    }

    [Fact]
    public async Task LogErrorAsync_WithException_AppendsExceptionText()
    {
        var jsRuntime = new FakeJSRuntime();
        var service = new BrowserConsoleService(jsRuntime);
        var ex = new InvalidOperationException("boom");

        await service.LogErrorAsync("failed", ex);

        var call = Assert.Single(jsRuntime.Calls);
        Assert.Equal("console.error", call.Identifier);
        Assert.Contains("boom", call.Args[0]!.ToString());
    }

    [Fact]
    public async Task LogErrorAsync_WithoutException_OmitsExceptionText()
    {
        var jsRuntime = new FakeJSRuntime();
        var service = new BrowserConsoleService(jsRuntime);

        await service.LogErrorAsync("failed");

        var call = Assert.Single(jsRuntime.Calls);
        Assert.DoesNotContain("Exception", call.Args[0]!.ToString());
    }

    [Fact]
    public async Task LogAsync_WhenJSInteropThrows_DoesNotPropagateException()
    {
        var jsRuntime = new FakeJSRuntime { ThrowOnce = new InvalidOperationException("JS interop not ready") };
        var service = new BrowserConsoleService(jsRuntime);

        var exception = await Record.ExceptionAsync(() => service.LogInfoAsync("hello"));

        Assert.Null(exception);
    }

    [Fact]
    public async Task LogAsync_MapsLogLevelToConsoleMethod()
    {
        var jsRuntime = new FakeJSRuntime();
        var service = new BrowserConsoleService(jsRuntime);

        await service.LogAsync(LogLevel.Critical, "fatal");

        var call = Assert.Single(jsRuntime.Calls);
        Assert.Equal("console.error", call.Identifier);
    }
}
