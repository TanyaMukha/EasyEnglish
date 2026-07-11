using Microsoft.JSInterop;

namespace MukhaLab.BrowserConsoleLogger.Tests.Fixtures;

/// <summary>
/// Hand-rolled <see cref="IJSRuntime"/> test double. <c>InvokeVoidAsync</c> (what the library
/// actually calls) is a JSInterop extension method that internally invokes
/// <c>InvokeAsync&lt;TValue&gt;</c> with an internal, inaccessible <c>TValue</c> — a generic
/// mocking framework can't configure a specific closed-generic instantiation it can't name. A
/// hand-written fake sidesteps this entirely since its <see cref="InvokeAsync{TValue}(string, object?[])"/>
/// implementation works for any <c>TValue</c> the caller happens to use.
/// </summary>
public class FakeJSRuntime : IJSRuntime
{
    public List<(string Identifier, object?[] Args)> Calls { get; } = new();

    /// <summary>When set, the next call throws this exception once, then clears itself.</summary>
    public Exception? ThrowOnce { get; set; }

    /// <summary>When set, called for every invocation to decide whether (and what) to throw.</summary>
    public Func<string, object?[], Exception?>? ThrowSelector { get; set; }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        var argsArray = args ?? Array.Empty<object?>();
        Calls.Add((identifier, argsArray));

        var toThrow = ThrowSelector?.Invoke(identifier, argsArray);
        if (toThrow == null && ThrowOnce != null)
        {
            toThrow = ThrowOnce;
            ThrowOnce = null;
        }

        if (toThrow != null)
            return new ValueTask<TValue>(Task.FromException<TValue>(toThrow));

        return ValueTask.FromResult(default(TValue)!);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        => InvokeAsync<TValue>(identifier, args);
}
