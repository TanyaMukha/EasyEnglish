using EasyEnglish.Core.Interfaces.Storage;

namespace EasyEnglish.App.Services.Speech;

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

    public async Task<VoiceSettings> LoadAsync()
    {
        var british = await _storage.GetAsync<string>(KeyBritish);
        var american = await _storage.GetAsync<string>(KeyAmerican);
        var ukrainian = await _storage.GetAsync<string>(KeyUkrainian);

        return new VoiceSettings(british, american, ukrainian);
    }

    public Task SaveAsync(SpeechLanguage language, string? voiceId)
    {
        var key = Keys[language];
        return voiceId is null
            ? _storage.RemoveAsync(key)
            : _storage.SetAsync(key, voiceId);
    }
}
