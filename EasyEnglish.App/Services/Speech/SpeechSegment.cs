namespace EasyEnglish.App.Services.Speech;

/// <summary>
/// Один фрагмент/речення для озвучення.
/// Текст між **подвійними зірочками** буде вимовлено мовою InclusionLanguage,
/// решта — PrimaryLanguage.
/// </summary>
public record SpeechSegment(
    string Text,
    SpeechLanguage PrimaryLanguage,
    SpeechLanguage InclusionLanguage
);

