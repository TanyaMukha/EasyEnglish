namespace EasyEnglish.App.Services.Speech;

public record VoiceSettings(
    string? EnglishBritishVoiceId,
    string? EnglishAmericanVoiceId,
    string? UkrainianVoiceId
)
{
    public static VoiceSettings Empty => new(null, null, null);

    public string? GetVoiceId(SpeechLanguage language) => language switch
    {
        SpeechLanguage.EnglishBritish => EnglishBritishVoiceId,
        SpeechLanguage.EnglishAmerican => EnglishAmericanVoiceId,
        SpeechLanguage.Ukrainian => UkrainianVoiceId,
        _ => null
    };

    public VoiceSettings WithVoiceId(SpeechLanguage language, string? voiceId) =>
        language switch
        {
            SpeechLanguage.EnglishBritish => this with { EnglishBritishVoiceId = voiceId },
            SpeechLanguage.EnglishAmerican => this with { EnglishAmericanVoiceId = voiceId },
            SpeechLanguage.Ukrainian => this with { UkrainianVoiceId = voiceId },
            _ => this
        };
}
