namespace EasyEnglish.App.Services.Speech;

using EasyEnglish.App.Interfaces;
using Microsoft.Maui.Media;

public sealed class MauiSpeechService : ISpeechService
{
    private readonly ISpeechEngine _engine;
    private readonly IVoiceProvider _voiceProvider;
    private readonly VoiceSettingsService _settingsService;

    private IReadOnlyList<LocaleInfo>? _cachedVoices;
    private CancellationTokenSource _cts = new();

    public bool IsSpeaking { get; private set; }

    private static readonly Dictionary<SpeechLanguage, string[]> NativeCodes = new()
    {
        [SpeechLanguage.EnglishBritish] = ["en-GB", "en-AU", "en-IE"],
        [SpeechLanguage.EnglishAmerican] = ["en-US", "en-CA"],
        [SpeechLanguage.Ukrainian] = ["uk-UA"],
    };

    public MauiSpeechService(
        ISpeechEngine engine,
        IVoiceProvider voiceProvider,
        VoiceSettingsService settingsService)
    {
        _engine = engine;
        _voiceProvider = voiceProvider;
        _settingsService = settingsService;
    }

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

    public Task StopAsync()
    {
        IsSpeaking = false;
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        return Task.CompletedTask;
    }

    private async Task<LocaleInfo?> ResolveVoiceAsync(SpeechLanguage language)
    {
        _cachedVoices ??= await _voiceProvider.GetAllVoicesAsync();

        var settings = await _settingsService.LoadAsync();
        var voiceId = settings.GetVoiceId(language);

        // 1. Збережений вибір користувача
        if (voiceId is not null)
        {
            var saved = _cachedVoices.FirstOrDefault(v => v.Id == voiceId);
            if (saved is not null) return saved;
        }

        var codes = NativeCodes[language];
        var langCode = codes[0].Split('-')[0];

        // 2. Перший офлайн голос для правильної країни
        // 3. Будь-який офлайн для цієї мови
        // 4. Онлайн голос для правильної країни
        // 5. Будь-який голос для цієї мови
        return _cachedVoices.FirstOrDefault(v =>
                v.IsLocalService && MatchesCodes(v, codes))
            ?? _cachedVoices.FirstOrDefault(v =>
                v.IsLocalService &&
                string.Equals(v.Language, langCode, StringComparison.OrdinalIgnoreCase))
            ?? _cachedVoices.FirstOrDefault(v =>
                MatchesCodes(v, codes))
            ?? _cachedVoices.FirstOrDefault(v =>
                string.Equals(v.Language, langCode, StringComparison.OrdinalIgnoreCase));
    }

    private static bool MatchesCodes(LocaleInfo voice, string[] codes) =>
        codes.Any(c =>
        {
            var parts = c.Split('-');
            return string.Equals(voice.Language, parts[0], StringComparison.OrdinalIgnoreCase)
                && (parts.Length < 2 || string.Equals(voice.Country, parts[1],
                    StringComparison.OrdinalIgnoreCase));
        });
}
