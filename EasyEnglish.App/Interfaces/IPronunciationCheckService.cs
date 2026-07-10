using EasyEnglish.App.Services.SpeechRecognition;

namespace EasyEnglish.App.Interfaces;

public interface IPronunciationCheckService
{
    /// <summary>Whether offline pronunciation checking is available on the current platform.</summary>
    bool IsSupported { get; }

    Task<PronunciationCheckResult> CheckAsync(string expectedText, CancellationToken ct = default);
}
