#if WINDOWS
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using EasyEnglish.App.Interfaces;

namespace EasyEnglish.App.Services.SpeechRecognition;

public sealed class WindowsPronunciationCheckService : IPronunciationCheckService
{
    public bool IsSupported => true;

    public async Task<PronunciationCheckResult> CheckAsync(string expectedText, CancellationToken ct = default)
    {
        using var recognizer = new SpeechRecognizer(new Language("en-US"));
        recognizer.Constraints.Add(new SpeechRecognitionListConstraint(new[] { expectedText }));

        var compilation = await recognizer.CompileConstraintsAsync();
        if (compilation.Status != SpeechRecognitionResultStatus.Success)
            return new PronunciationCheckResult(false, string.Empty, PronunciationConfidence.Rejected);

        ct.ThrowIfCancellationRequested();

        using var registration = ct.Register(() => _ = recognizer.StopRecognitionAsync());

        var result = await recognizer.RecognizeAsync();

        var confidence = result.Confidence switch
        {
            SpeechRecognitionConfidence.High => PronunciationConfidence.High,
            SpeechRecognitionConfidence.Medium => PronunciationConfidence.Medium,
            SpeechRecognitionConfidence.Low => PronunciationConfidence.Low,
            _ => PronunciationConfidence.Rejected,
        };

        var recognized = result.Status == SpeechRecognitionResultStatus.Success
            && confidence != PronunciationConfidence.Rejected;

        return new PronunciationCheckResult(recognized, recognized ? (result.Text ?? string.Empty) : string.Empty, confidence);
    }
}
#endif
