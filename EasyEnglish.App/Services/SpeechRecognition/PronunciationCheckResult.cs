namespace EasyEnglish.App.Services.SpeechRecognition;

public enum PronunciationConfidence
{
    Rejected,
    Low,
    Medium,
    High
}

public record PronunciationCheckResult(bool Recognized, string RecognizedText, PronunciationConfidence Confidence);
