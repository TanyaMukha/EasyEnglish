using EasyPeasy.App.Interfaces;

namespace EasyPeasy.App.Services.Speech;

/// <summary><see cref="IVoiceProvider"/> implementation listing every voice MAUI's native <see cref="ITextToSpeech"/> reports for the device, sorted by display name and cached until <see cref="InvalidateCache"/> is called.</summary>
public sealed class MauiVoiceProvider : IVoiceProvider
{
    private readonly ITextToSpeech _tts;
    private IReadOnlyList<LocaleInfo>? _cache;

    public MauiVoiceProvider(ITextToSpeech tts) => _tts = tts;

    /// <summary>Returns every native voice available on this device, sorted by display name. Cached after the first call.</summary>
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

    /// <summary>Clears the cached voice list so the next <see cref="GetAllVoicesAsync"/> call re-queries the platform.</summary>
    public void InvalidateCache() => _cache = null;
}
