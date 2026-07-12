namespace EasyEnglish.App.Services.Speech;

/// <summary>
/// A single chunk/sentence to speak. Text wrapped in <c>**double asterisks**</c> is voiced in
/// <see cref="InclusionLanguage"/>; everything else in <see cref="PrimaryLanguage"/>. See
/// <see cref="TextChunkParser"/> for how the marker is parsed out of raw text.
/// </summary>
public record SpeechSegment(
    string Text,
    SpeechLanguage PrimaryLanguage,
    SpeechLanguage InclusionLanguage
);

