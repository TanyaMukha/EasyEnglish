using Microsoft.Maui.Media;

namespace EasyEnglish.App.Services.Speech;

public record LocaleInfo(
    string Id,
    string Language,
    string Country,
    string Name,
    string DisplayName,
    bool IsLocalService = true   // false = потребує інтернету (Web Speech онлайн голоси)
)
{
    /// <summary>З MAUI ITextToSpeech — завжди локальний</summary>
    public static LocaleInfo FromLocale(Locale locale)
    {
        var id = BuildId(locale.Language, locale.Country, locale.Name ?? "");
        var display = string.IsNullOrEmpty(locale.Name)
            ? $"{locale.Language}-{locale.Country}"
            : $"{locale.Name} ({locale.Language}-{locale.Country})";

        return new LocaleInfo(id, locale.Language, locale.Country,
            locale.Name ?? "", display, IsLocalService: true);
    }

    /// <summary>З Web Speech API (Windows) — може бути онлайн</summary>
    public static LocaleInfo FromWebSpeech(string name, string lang, bool localService)
    {
        var parts = lang.Split('-');
        var language = parts[0].ToLowerInvariant();
        var country = parts.Length > 1 ? parts[1].ToUpperInvariant() : "";
        var id = BuildId(language, country, name);
        var display = localService
            ? $"{name} ({lang})"
            : $"{name} ({lang}) 🌐";

        return new LocaleInfo(id, language, country, name, display,
            IsLocalService: localService);
    }

    public static string BuildId(string lang, string country, string name) =>
        $"{lang}|{country}|{name}";

    public static string BuildId(Locale locale) =>
        BuildId(locale.Language, locale.Country, locale.Name ?? "");
}
