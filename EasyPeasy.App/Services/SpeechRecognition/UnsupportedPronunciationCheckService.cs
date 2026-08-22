using EasyPeasy.App.Interfaces;

namespace EasyPeasy.App.Services.SpeechRecognition;

/// <summary>
/// Fallback used on platforms without a wired-up offline recognizer (currently: everything but
/// Windows). <see cref="IsSupported"/> is <c>false</c> so callers are expected to hide the
/// pronunciation-check UI entirely rather than calling <see cref="CheckAsync"/> — it always throws.
/// </summary>
public sealed class UnsupportedPronunciationCheckService : IPronunciationCheckService
{
    /// <inheritdoc/>
    public bool IsSupported => false;

    /// <inheritdoc/>
    /// <exception cref="PlatformNotSupportedException">Always thrown — this platform has no pronunciation check implementation.</exception>
    public Task<PronunciationCheckResult> CheckAsync(string expectedText, CancellationToken ct = default)
        => throw new PlatformNotSupportedException("Pronunciation check is currently supported on Windows only.");
}
