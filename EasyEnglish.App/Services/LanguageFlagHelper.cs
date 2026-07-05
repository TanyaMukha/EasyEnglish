namespace EasyEnglish.App.Services;

/// <summary>
/// Перетворює код мови у форматі BCP-47 (наприклад, "en-us") на emoji-прапор
/// відповідного регіону. Прапор рендериться кольоровим системним емодзі-шрифтом —
/// окремі SVG-іконки прапорів не потрібні.
/// </summary>
public static class LanguageFlagHelper
{
    private const string DefaultFlag = "🏳️";

    /// <summary>
    /// Повертає emoji-прапор для регіону, вказаного в коді мови.
    /// Наприклад, "en-us" → 🇺🇸, "uk-ua" → 🇺🇦.
    /// Якщо код відсутній або не містить розпізнаваного регіону — повертає нейтральний прапор.
    /// </summary>
    public static string GetFlagEmoji(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
            return DefaultFlag;

        var region = languageCode.Split('-', '_').LastOrDefault();
        if (string.IsNullOrEmpty(region) || region.Length != 2)
            return DefaultFlag;

        region = region.ToUpperInvariant();
        if (region[0] is < 'A' or > 'Z' || region[1] is < 'A' or > 'Z')
            return DefaultFlag;

        // Regional Indicator Symbols: 'A' (U+1F1E6) .. 'Z' (U+1F1FF)
        var first  = char.ConvertFromUtf32(0x1F1E6 + (region[0] - 'A'));
        var second = char.ConvertFromUtf32(0x1F1E6 + (region[1] - 'A'));
        return first + second;
    }
}
