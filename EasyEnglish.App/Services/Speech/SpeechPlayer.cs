using EasyEnglish.App.Interfaces;

namespace EasyEnglish.App.Services.Speech;

/// <summary>
/// Scoped wrapper around <see cref="ISpeechService"/> for Blazor components to drive playback
/// from: each <c>Play*Async</c> call cancels/replaces whatever was previously playing (so a
/// component never needs to track cancellation itself), and disposal stops playback.
/// </summary>
public sealed class SpeechPlayer : IAsyncDisposable
{
    private readonly ISpeechService _service;
    private CancellationTokenSource? _cts;

    /// <summary>Whether the underlying <see cref="ISpeechService"/> is currently speaking.</summary>
    public bool IsPlaying => _service.IsSpeaking;

    public SpeechPlayer(ISpeechService service) => _service = service;

    /// <summary>Plays a sequence of segments back-to-back with <paramref name="pauseBetween"/> silence in between, canceling any current playback first.</summary>
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

    /// <summary>Plays a single segment, canceling any current playback first.</summary>
    public async Task PlayAsync(SpeechSegment segment)
    {
        await StopAsync();
        _cts = new CancellationTokenSource();
        try
        {
            await _service.SpeakSegmentAsync(segment, _cts.Token);
        }
        catch (OperationCanceledException) { }
    }

    /// <summary>Plays plain text in a single language, canceling any current playback first.</summary>
    public async Task PlayTextAsync(string text, SpeechLanguage language)
    {
        await StopAsync();
        _cts = new CancellationTokenSource();
        try
        {
            await _service.SpeakTextAsync(text, language, _cts.Token);
        }
        catch (OperationCanceledException) { }
    }

    /// <summary>Cancels any in-flight playback and stops the underlying speech service.</summary>
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
