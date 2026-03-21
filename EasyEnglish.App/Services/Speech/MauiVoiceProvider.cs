using EasyEnglish.App.Interfaces;

namespace EasyEnglish.App.Services.Speech;

public sealed class MauiVoiceProvider : IVoiceProvider
{
    private readonly ITextToSpeech _tts;
    private IReadOnlyList<LocaleInfo>? _cache;

    public MauiVoiceProvider(ITextToSpeech tts) => _tts = tts;

    public async Task<IReadOnlyList<LocaleInfo>> GetAllVoicesAsync()
    {
        if (_cache is not null) return _cache;

        var locales = await _tts.GetLocalesAsync();
        _cache = locales
            .Select(LocaleInfo.FromLocale)
            .OrderBy(v => v.DisplayName)
            .ToList();

        return _cache;
    }

    public void InvalidateCache() => _cache = null;
}
