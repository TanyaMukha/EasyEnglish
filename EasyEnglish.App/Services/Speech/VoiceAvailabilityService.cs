using EasyEnglish.App.Interfaces;


namespace EasyEnglish.App.Services.Speech;

public record VoiceAvailabilityResult(
    SpeechLanguage Language,
    bool IsAvailable,
    LocaleInfo? ResolvedVoice,
    string? InstallInstructions,
    bool CanOpenSettings
);

public sealed class VoiceAvailabilityService
{
    private readonly IVoiceProvider _voiceProvider;
    private readonly VoiceSettingsService _settingsService;

    private static readonly Dictionary<SpeechLanguage, string[]> NativeCodes = new()
    {
        [SpeechLanguage.EnglishBritish] = ["en-GB", "en-AU", "en-IE"],
        [SpeechLanguage.EnglishAmerican] = ["en-US", "en-CA"],
        [SpeechLanguage.Ukrainian] = ["uk-UA"],
    };

    public VoiceAvailabilityService(
        IVoiceProvider voiceProvider,
        VoiceSettingsService settingsService)
    {
        _voiceProvider = voiceProvider;
        _settingsService = settingsService;
    }

    public async Task<IReadOnlyList<VoiceAvailabilityResult>> CheckAllAsync()
    {
        var voices = await _voiceProvider.GetAllVoicesAsync();
        var settings = await _settingsService.LoadAsync();

        return Enum.GetValues<SpeechLanguage>()
            .Select(lang => Check(lang, voices, settings))
            .ToList();
    }

    private static VoiceAvailabilityResult Check(
        SpeechLanguage lang,
        IReadOnlyList<LocaleInfo> voices,
        VoiceSettings settings)
    {
        // 1. Збережений вибір
        var voiceId = settings.GetVoiceId(lang);
        if (voiceId is not null)
        {
            var saved = voices.FirstOrDefault(v => v.Id == voiceId);
            if (saved is not null)
                return new VoiceAvailabilityResult(lang, true, saved, null, false);
        }

        // 2. Пошук по пріоритетних кодах
        var codes = NativeCodes[lang];
        var langCode = codes[0].Split('-')[0];

        var found = voices.FirstOrDefault(v =>
            codes.Any(c =>
            {
                var parts = c.Split('-');
                return string.Equals(v.Language, parts[0], StringComparison.OrdinalIgnoreCase)
                    && (parts.Length < 2 || string.Equals(v.Country, parts[1],
                        StringComparison.OrdinalIgnoreCase));
            }))
            ?? voices.FirstOrDefault(v =>
                string.Equals(v.Language, langCode, StringComparison.OrdinalIgnoreCase));

        return found is not null
            ? new VoiceAvailabilityResult(lang, true, found, null, false)
            : new VoiceAvailabilityResult(
                lang, false, null,
                BuildInstructions(lang),
                CanOpenSettings());
    }

    private static string BuildInstructions(SpeechLanguage lang)
    {
        var langName = lang switch
        {
            SpeechLanguage.Ukrainian => "Ukrainian / Українська",
            SpeechLanguage.EnglishBritish => "English (United Kingdom)",
            SpeechLanguage.EnglishAmerican => "English (United States)",
            _ => lang.ToString()
        };

        if (DeviceInfo.Platform == DevicePlatform.Android)
            return $"Налаштування → Загальне управління → Мова → " +
                   $"Синтез мовлення → Google TTS → Завантажити «{langName}»";

        if (DeviceInfo.Platform == DevicePlatform.WinUI)
            return $"Параметри → Час і мова → Мовлення → " +
                   $"Керування голосами → Додати → «{langName}»";

        if (DeviceInfo.Platform == DevicePlatform.iOS)
            return $"Налаштування → Доступність → Вимовлений вміст → " +
                   $"Голоси → «{langName}»";

        if (DeviceInfo.Platform == DevicePlatform.MacCatalyst)
            return $"Системні параметри → Доступність → " +
                   $"Мовлення → Системний голос → «{langName}»";

        return $"Встановіть мовний пакет «{langName}» у системних налаштуваннях";
    }

    private static bool CanOpenSettings() =>
        DeviceInfo.Platform == DevicePlatform.Android ||
        DeviceInfo.Platform == DevicePlatform.WinUI;
}
