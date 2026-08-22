namespace EasyPeasy.App.Services.Speech;

using EasyPeasy.App.Interfaces;
using Microsoft.Maui.Media;

/// <summary>
/// Default <see cref="ISpeechService"/> implementation: parses a segment's text into
/// language-tagged chunks via <see cref="TextChunkParser"/>, resolves a voice per chunk through
/// <see cref="ResolveVoiceAsync"/>, and hands each chunk to the injected <see cref="ISpeechEngine"/>
/// in order. A single <see cref="CancellationTokenSource"/> backs <see cref="StopAsync"/>, letting a
/// caller interrupt playback mid-segment; it's replaced (not reused) after every stop.
/// </summary>
public sealed class MauiSpeechService : ISpeechService
{
    private readonly ISpeechEngine _engine;
    private readonly IVoiceProvider _voiceProvider;
    private readonly VoiceSettingsService _settingsService;

    private IReadOnlyList<LocaleInfo>? _cachedVoices;
    private CancellationTokenSource _cts = new();

    /// <inheritdoc/>
    public bool IsSpeaking { get; private set; }

    public MauiSpeechService(
        ISpeechEngine engine,
        IVoiceProvider voiceProvider,
        VoiceSettingsService settingsService)
    {
        _engine = engine;
        _voiceProvider = voiceProvider;
        _settingsService = settingsService;
    }

    /// <inheritdoc/>
    public async Task SpeakTextAsync(string text, SpeechLanguage language, CancellationToken ct = default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token);

        linked.Token.ThrowIfCancellationRequested();

        var voice = await ResolveVoiceAsync(language);
        if (voice is not null)
            await _engine.SpeakAsync(text, voice, linked.Token);
    }
    
    /// <inheritdoc/>
    public async Task SpeakSegmentAsync(SpeechSegment segment, CancellationToken ct = default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, _cts.Token);

        var chunks = TextChunkParser.Parse(
            segment.Text, segment.PrimaryLanguage, segment.InclusionLanguage);

        foreach (var chunk in chunks)
        {
            linked.Token.ThrowIfCancellationRequested();

            var voice = await ResolveVoiceAsync(chunk.Language);
            if (voice is not null)
                await _engine.SpeakAsync(chunk.Text, voice, linked.Token);
        }
    }

    /// <inheritdoc/>
    public async Task SpeakSegmentsAsync(
        IEnumerable<SpeechSegment> segments,
        TimeSpan pauseBetween,
        CancellationToken ct = default)
    {
        IsSpeaking = true;
        try
        {
            var list = segments.ToList();
            for (var i = 0; i < list.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                _cts.Token.ThrowIfCancellationRequested();

                await SpeakSegmentAsync(list[i], ct);

                if (i < list.Count - 1 && pauseBetween > TimeSpan.Zero)
                    await Task.Delay(pauseBetween, ct);
            }
        }
        finally
        {
            IsSpeaking = false;
        }
    }

    /// <inheritdoc/>
    public Task StopAsync()
    {
        IsSpeaking = false;
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Picks the voice to use for <paramref name="language"/>, in priority order: (1) the
    /// learner's saved choice for this language, if it still exists in the voice list; (2) the
    /// first offline voice matching one of <see cref="NativeVoiceCodes"/>'s full <c>lang-COUNTRY</c>
    /// codes; (3) any offline voice matching just the bare language; (4) any online voice matching
    /// a full code; (5) any voice matching just the bare language. Returns <c>null</c> only if no
    /// voice on the device matches the language at all.
    /// </summary>
    private async Task<LocaleInfo?> ResolveVoiceAsync(SpeechLanguage language)
    {
        _cachedVoices ??= await _voiceProvider.GetAllVoicesAsync();

        var settings = await _settingsService.LoadAsync();
        var voiceId = settings.GetVoiceId(language);

        if (voiceId is not null)
        {
            var saved = _cachedVoices.FirstOrDefault(v => v.Id == voiceId);
            if (saved is not null) return saved;
        }

        var codes = NativeVoiceCodes.For(language);
        var langCode = NativeVoiceCodes.BareLanguageCode(language);

        return _cachedVoices.FirstOrDefault(v =>
                v.IsLocalService && NativeVoiceCodes.Matches(v, codes))
            ?? _cachedVoices.FirstOrDefault(v =>
                v.IsLocalService &&
                string.Equals(v.Language, langCode, StringComparison.OrdinalIgnoreCase))
            ?? _cachedVoices.FirstOrDefault(v =>
                NativeVoiceCodes.Matches(v, codes))
            ?? _cachedVoices.FirstOrDefault(v =>
                string.Equals(v.Language, langCode, StringComparison.OrdinalIgnoreCase));
    }
}
