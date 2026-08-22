using EasyPeasy.App.Interfaces;


namespace EasyPeasy.App.Services.Speech;

/// <summary>
/// Availability check result for one <see cref="SpeechLanguage"/>: either a resolved voice exists
/// on-device (<see cref="IsAvailable"/> <c>true</c>, <see cref="ResolvedVoice"/> set), or it doesn't
/// and the learner needs platform-specific guidance (<see cref="InstallInstructions"/>) to install
/// a voice pack, optionally jumping straight to system settings (<see cref="CanOpenSettings"/>).
/// </summary>
public record VoiceAvailabilityResult(
    SpeechLanguage Language,
    bool IsAvailable,
    LocaleInfo? ResolvedVoice,
    string? InstallInstructions,
    bool CanOpenSettings
);

/// <summary>
/// Reports, per <see cref="SpeechLanguage"/>, whether a usable voice is installed on the device —
/// used to warn the learner before they hit silent/failed playback rather than after. Voice
/// resolution here mirrors <see cref="MauiSpeechService.ResolveVoiceAsync"/>'s priority order
/// (saved choice, then native-code match, then bare-language match) but stops at "is one
/// available" rather than needing the full online/offline tiering.
/// </summary>
public sealed class VoiceAvailabilityService
{
    private readonly IVoiceProvider _voiceProvider;
    private readonly VoiceSettingsService _settingsService;

    public VoiceAvailabilityService(
        IVoiceProvider voiceProvider,
        VoiceSettingsService settingsService)
    {
        _voiceProvider = voiceProvider;
        _settingsService = settingsService;
    }

    /// <summary>Checks every <see cref="SpeechLanguage"/> and returns one <see cref="VoiceAvailabilityResult"/> for each.</summary>
    public async Task<IReadOnlyList<VoiceAvailabilityResult>> CheckAllAsync()
    {
        var voices = await _voiceProvider.GetAllVoicesAsync();
        var settings = await _settingsService.LoadAsync();

        return Enum.GetValues<SpeechLanguage>()
            .Select(lang => Check(lang, voices, settings))
            .ToList();
    }

    /// <summary>Resolves availability for one language: saved choice first, then a native-code/bare-language match, else "unavailable" with install instructions.</summary>
    private static VoiceAvailabilityResult Check(
        SpeechLanguage lang,
        IReadOnlyList<LocaleInfo> voices,
        VoiceSettings settings)
    {
        var voiceId = settings.GetVoiceId(lang);
        if (voiceId is not null)
        {
            var saved = voices.FirstOrDefault(v => v.Id == voiceId);
            if (saved is not null)
                return new VoiceAvailabilityResult(lang, true, saved, null, false);
        }

        var codes = NativeVoiceCodes.For(lang);
        var langCode = NativeVoiceCodes.BareLanguageCode(lang);

        var found = voices.FirstOrDefault(v => NativeVoiceCodes.Matches(v, codes))
            ?? voices.FirstOrDefault(v =>
                string.Equals(v.Language, langCode, StringComparison.OrdinalIgnoreCase));

        return found is not null
            ? new VoiceAvailabilityResult(lang, true, found, null, false)
            : new VoiceAvailabilityResult(
                lang, false, null,
                BuildInstructions(lang),
                CanOpenSettings());
    }

    /// <summary>Builds a platform-specific, Ukrainian-language, human-readable set of steps for installing a voice pack for <paramref name="lang"/> — shown to the learner in the UI, so left untranslated.</summary>
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

    /// <summary>Whether the app can deep-link directly into system speech settings on this platform (currently Android and WinUI only).</summary>
    private static bool CanOpenSettings() =>
        DeviceInfo.Platform == DevicePlatform.Android ||
        DeviceInfo.Platform == DevicePlatform.WinUI;
}
