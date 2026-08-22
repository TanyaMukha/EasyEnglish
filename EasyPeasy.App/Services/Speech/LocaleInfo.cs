using Microsoft.Maui.Media;

namespace EasyPeasy.App.Services.Speech;

/// <summary>
/// A voice/locale the app can speak with, normalized from either MAUI's native
/// <see cref="Locale"/> (<see cref="FromLocale"/>) or the browser's Web Speech API on Windows
/// (<see cref="FromWebSpeech"/>).
/// </summary>
/// <param name="IsLocalService"><c>false</c> means the voice needs internet access (an online Web Speech voice); <c>true</c> is always on-device.</param>
public record LocaleInfo(
    string Id,
    string Language,
    string Country,
    string Name,
    string DisplayName,
    bool IsLocalService = true
)
{
    /// <summary>Builds a <see cref="LocaleInfo"/> from a MAUI <c>ITextToSpeech</c> locale — always local (on-device).</summary>
    public static LocaleInfo FromLocale(Locale locale)
    {
        var id = BuildId(locale.Language, locale.Country, locale.Name ?? "");
        var display = string.IsNullOrEmpty(locale.Name)
            ? $"{locale.Language}-{locale.Country}"
            : $"{locale.Name} ({locale.Language}-{locale.Country})";

        return new LocaleInfo(id, locale.Language, locale.Country,
            locale.Name ?? "", display, IsLocalService: true);
    }

    /// <summary>Builds a <see cref="LocaleInfo"/> from a Web Speech API voice (Windows) — may be an online-only voice.</summary>
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

    /// <summary>Composes the stable <c>"{lang}|{country}|{name}"</c> identity key used for <see cref="Id"/>.</summary>
    public static string BuildId(string lang, string country, string name) =>
        $"{lang}|{country}|{name}";

    /// <summary>Overload of <see cref="BuildId(string,string,string)"/> that reads the parts from a MAUI <see cref="Locale"/>.</summary>
    public static string BuildId(Locale locale) =>
        BuildId(locale.Language, locale.Country, locale.Name ?? "");
}
