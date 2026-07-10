using EasyEnglish.App.Interfaces;

namespace EasyEnglish.App.Services.SpeechRecognition;

/// <summary>Fallback used on platforms without a wired-up offline recognizer (currently: everything but Windows).</summary>
public sealed class UnsupportedPronunciationCheckService : IPronunciationCheckService
{
    public bool IsSupported => false;

    public Task<PronunciationCheckResult> CheckAsync(string expectedText, CancellationToken ct = default)
        => throw new PlatformNotSupportedException("Перевірка вимови наразі підтримується лише на Windows.");
}
