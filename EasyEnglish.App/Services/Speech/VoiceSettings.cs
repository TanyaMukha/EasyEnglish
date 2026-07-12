namespace EasyEnglish.App.Services.Speech;

/// <summary>The learner's chosen voice ID (a <see cref="LocaleInfo.Id"/>) for each <see cref="SpeechLanguage"/>, persisted via <c>VoiceSettingsService</c>. A <c>null</c> entry means "use the platform default."</summary>
public record VoiceSettings(
    string? EnglishBritishVoiceId,
    string? EnglishAmericanVoiceId,
    string? UkrainianVoiceId
)
{
    /// <summary>No voice chosen for any language — falls back to platform defaults everywhere.</summary>
    public static VoiceSettings Empty => new(null, null, null);

    /// <summary>Returns the chosen voice ID for <paramref name="language"/>, or <c>null</c> if none is set.</summary>
    public string? GetVoiceId(SpeechLanguage language) => language switch
    {
        SpeechLanguage.EnglishBritish => EnglishBritishVoiceId,
        SpeechLanguage.EnglishAmerican => EnglishAmericanVoiceId,
        SpeechLanguage.Ukrainian => UkrainianVoiceId,
        _ => null
    };

    /// <summary>Returns a copy with <paramref name="language"/>'s voice ID replaced by <paramref name="voiceId"/>. Unrecognized languages are returned unchanged.</summary>
    public VoiceSettings WithVoiceId(SpeechLanguage language, string? voiceId) =>
        language switch
        {
            SpeechLanguage.EnglishBritish => this with { EnglishBritishVoiceId = voiceId },
            SpeechLanguage.EnglishAmerican => this with { EnglishAmericanVoiceId = voiceId },
            SpeechLanguage.Ukrainian => this with { UkrainianVoiceId = voiceId },
            _ => this
        };
}
