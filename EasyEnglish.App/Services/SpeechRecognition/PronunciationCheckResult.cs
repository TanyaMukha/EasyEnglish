namespace EasyEnglish.App.Services.SpeechRecognition;

/// <summary>How closely the recognized speech matched the expected word/phrase.</summary>
public enum PronunciationConfidence
{
    /// <summary>No usable match — treat as incorrect.</summary>
    Rejected,
    /// <summary>A weak match.</summary>
    Low,
    /// <summary>A reasonable match.</summary>
    Medium,
    /// <summary>A strong, confident match.</summary>
    High
}

/// <summary>Result of one <c>IPronunciationCheckService.CheckAsync</c> call.</summary>
/// <param name="Recognized">Whether any speech was recognized at all (independent of whether it matched).</param>
/// <param name="RecognizedText">The raw text the recognizer heard, for display/debugging.</param>
/// <param name="Confidence">How closely <paramref name="RecognizedText"/> matched the expected text.</param>
public record PronunciationCheckResult(bool Recognized, string RecognizedText, PronunciationConfidence Confidence);
