using EasyPeasy.App.Interfaces;

namespace EasyPeasy.App.Services.Speech;

/// <summary>
/// <see cref="ISpeechEngine"/> implementation backed by MAUI's native <see cref="ITextToSpeech"/>
/// API — caches the platform's locale list on first use since it doesn't change at runtime.
/// </summary>
public sealed class MauiSpeechEngine : ISpeechEngine
{
    private readonly ITextToSpeech _tts;
    private IEnumerable<Locale>? _cachedLocales;

    public MauiSpeechEngine(ITextToSpeech tts) => _tts = tts;

    /// <summary>Speaks <paramref name="text"/> using the native locale matching <paramref name="voice"/>'s <see cref="LocaleInfo.Id"/>, or the platform default if no exact match is found.</summary>
    public async Task SpeakAsync(string text, LocaleInfo voice, CancellationToken ct = default)
    {
        _cachedLocales ??= await _tts.GetLocalesAsync();

        var locale = _cachedLocales.FirstOrDefault(l =>
            LocaleInfo.BuildId(l) == voice.Id);

        await _tts.SpeakAsync(text, new SpeechOptions { Locale = locale }, ct);
    }
}
