using EasyEnglish.Core.Interfaces.Storage;

namespace EasyEnglish.App.Services.Speech;

/// <summary>Persists the learner's chosen voice ID per <see cref="SpeechLanguage"/> via <see cref="IStorageService"/>, as three independent keys (not one serialized <see cref="VoiceSettings"/> blob).</summary>
public sealed class VoiceSettingsService
{
    private const string KeyBritish = "voice_en_gb";
    private const string KeyAmerican = "voice_en_us";
    private const string KeyUkrainian = "voice_uk_ua";

    private static readonly Dictionary<SpeechLanguage, string> Keys = new()
    {
        [SpeechLanguage.EnglishBritish] = KeyBritish,
        [SpeechLanguage.EnglishAmerican] = KeyAmerican,
        [SpeechLanguage.Ukrainian] = KeyUkrainian,
    };

    private readonly IStorageService _storage;

    public VoiceSettingsService(IStorageService storage) => _storage = storage;

    /// <summary>Loads the saved voice ID for every language into one <see cref="VoiceSettings"/> (missing entries come back <c>null</c>).</summary>
    public async Task<VoiceSettings> LoadAsync()
    {
        var british = await _storage.GetAsync<string>(KeyBritish);
        var american = await _storage.GetAsync<string>(KeyAmerican);
        var ukrainian = await _storage.GetAsync<string>(KeyUkrainian);

        return new VoiceSettings(british, american, ukrainian);
    }

    /// <summary>Saves <paramref name="voiceId"/> as the chosen voice for <paramref name="language"/>, or clears the saved choice (reverting to platform default) when <paramref name="voiceId"/> is <c>null</c>.</summary>
    public Task SaveAsync(SpeechLanguage language, string? voiceId)
    {
        var key = Keys[language];
        return voiceId is null
            ? _storage.RemoveAsync(key)
            : _storage.SetAsync(key, voiceId);
    }
}
