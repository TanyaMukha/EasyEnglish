using EasyEnglish.App.Interfaces;

namespace EasyEnglish.App.Services.Speech;

/// <summary>
/// Scoped сервіс для керування відтворенням з Blazor-компонентів.
/// </summary>
public sealed class SpeechPlayer : IAsyncDisposable
{
    private readonly ISpeechService _service;
    private CancellationTokenSource? _cts;

    public bool IsPlaying => _service.IsSpeaking;

    public SpeechPlayer(ISpeechService service) => _service = service;

    public async Task PlayAsync(
        IEnumerable<SpeechSegment> segments,
        TimeSpan pauseBetween)
    {
        await StopAsync();
        _cts = new CancellationTokenSource();
        try
        {
            await _service.SpeakSegmentsAsync(segments, pauseBetween, _cts.Token);
        }
        catch (OperationCanceledException) { }
    }

    public async Task StopAsync()
    {
        if (_cts is not null)
        {
            await _cts.CancelAsync();
            _cts.Dispose();
            _cts = null;
        }
        await _service.StopAsync();
    }

    public async ValueTask DisposeAsync() => await StopAsync();
}
