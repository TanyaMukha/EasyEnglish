using EasyEnglish.App.Interfaces;

namespace EasyEnglish.App.Services.Speech;

public sealed class MauiSpeechEngine : ISpeechEngine
{
    private readonly ITextToSpeech _tts;
    private IEnumerable<Locale>? _cachedLocales;
    private CancellationTokenSource _cts = new();

    public MauiSpeechEngine(ITextToSpeech tts) => _tts = tts;

    public async Task SpeakAsync(string text, LocaleInfo voice, CancellationToken ct = default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token);

        _cachedLocales ??= await _tts.GetLocalesAsync();

        var locale = _cachedLocales.FirstOrDefault(l =>
            LocaleInfo.BuildId(l) == voice.Id);

        await _tts.SpeakAsync(text, new SpeechOptions { Locale = locale }, linked.Token);
    }
}
