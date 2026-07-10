namespace EasyEnglish.App.Services;

/// <summary>
/// Перетворює код мови у форматі BCP-47 (наприклад, "en-us") на шлях до SVG-іконки прапора
/// відповідного регіону. Іконки — з набору flag-icons (MIT, github.com/lipis/flag-icons),
/// самохостяться в wwwroot/flags/. Emoji-прапори (комбінації Regional Indicator Symbols)
/// на Windows часто не рендеряться як прапор, а показуються двома літерами — тому іконки.
/// </summary>
public static class LanguageFlagHelper
{
    private const string DefaultFlagPath = "flags/xx.svg";

    /// <summary>
    /// Повертає шлях (відносно wwwroot) до SVG-іконки прапора для регіону, вказаного в коді мови.
    /// Наприклад, "en-us" → "flags/us.svg", "uk-ua" → "flags/ua.svg".
    /// Якщо код відсутній або не містить розпізнаваного регіону — повертає нейтральний прапор.
    /// </summary>
    public static string GetFlagIconPath(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            return DefaultFlagPath;

        var region = languageCode.Split('-', '_').LastOrDefault();
        if (string.IsNullOrEmpty(region) || region.Length != 2)
            return DefaultFlagPath;

        region = region.ToLowerInvariant();
        if (region[0] is < 'a' or > 'z' || region[1] is < 'a' or > 'z')
            return DefaultFlagPath;

        return $"flags/{region}.svg";
    }
}
