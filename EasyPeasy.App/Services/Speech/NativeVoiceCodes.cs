namespace EasyPeasy.App.Services.Speech;

/// <summary>
/// The native <c>lang-COUNTRY</c> locale codes to prefer for each <see cref="SpeechLanguage"/>,
/// plus the matching logic built on top of them. Consolidates what used to be three independent
/// copies of the same dictionary and matching lambda in <see cref="MauiSpeechService"/>,
/// <see cref="VoiceAvailabilityService"/>, and <see cref="VoicePickerViewModel"/> — see
/// EasyPeasy.App/Services/README.md Known Issues for the history.
/// </summary>
internal static class NativeVoiceCodes
{
    private static readonly Dictionary<SpeechLanguage, string[]> Codes = new()
    {
        [SpeechLanguage.EnglishBritish] = ["en-GB", "en-AU", "en-IE"],
        [SpeechLanguage.EnglishAmerican] = ["en-US", "en-CA"],
        [SpeechLanguage.Ukrainian] = ["uk-UA"],
    };

    /// <summary>The native <c>lang-COUNTRY</c> codes for <paramref name="language"/>, in priority order.</summary>
    public static string[] For(SpeechLanguage language) => Codes[language];

    /// <summary>The bare language code (e.g. <c>"en"</c>) for <paramref name="language"/> — the language part of its first native code.</summary>
    public static string BareLanguageCode(SpeechLanguage language) => Codes[language][0].Split('-')[0];

    /// <summary>Whether <paramref name="voice"/>'s language (and country, when the code specifies one) matches any of <paramref name="codes"/>.</summary>
    public static bool Matches(LocaleInfo voice, string[] codes) =>
        codes.Any(c =>
        {
            var parts = c.Split('-');
            return string.Equals(voice.Language, parts[0], StringComparison.OrdinalIgnoreCase)
                && (parts.Length < 2 || string.Equals(voice.Country, parts[1],
                    StringComparison.OrdinalIgnoreCase));
        });
}
