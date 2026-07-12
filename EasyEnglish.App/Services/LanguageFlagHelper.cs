namespace EasyEnglish.App.Services;

/// <summary>
/// Maps a BCP-47 language code (e.g. "en-us") to the wwwroot-relative path of the matching
/// region's SVG flag icon. Icons are self-hosted under <c>wwwroot/flags/</c>, sourced from the
/// flag-icons set (MIT, github.com/lipis/flag-icons) — emoji flags (Regional Indicator Symbol
/// pairs) frequently fail to render as an actual flag glyph on Windows, showing two letters
/// instead, hence real icon files.
/// </summary>
public static class LanguageFlagHelper
{
    private const string DefaultFlagPath = "flags/xx.svg";

    /// <summary>
    /// Returns the wwwroot-relative path to the region's flag icon, e.g. "en-us" → "flags/us.svg",
    /// "uk-ua" → "flags/ua.svg". Falls back to the neutral flag when <paramref name="languageCode"/>
    /// is missing or has no recognizable 2-letter region.
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
