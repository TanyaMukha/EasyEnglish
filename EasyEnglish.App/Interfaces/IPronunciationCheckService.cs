using EasyEnglish.App.Services.SpeechRecognition;

namespace EasyEnglish.App.Interfaces;

/// <summary>Checks whether the learner's spoken input matches an expected word/phrase. See <see cref="UnsupportedPronunciationCheckService"/> for the fallback used where no platform implementation exists.</summary>
public interface IPronunciationCheckService
{
    /// <summary>Whether offline pronunciation checking is available on the current platform.</summary>
    bool IsSupported { get; }

    /// <summary>Listens for speech and compares it against <paramref name="expectedText"/>, returning how confident the match is. Callers should check <see cref="IsSupported"/> first.</summary>
    Task<PronunciationCheckResult> CheckAsync(string expectedText, CancellationToken ct = default);
}
