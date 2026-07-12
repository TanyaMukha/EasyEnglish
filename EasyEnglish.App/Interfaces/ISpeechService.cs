using EasyEnglish.App.Services.Speech;

namespace EasyEnglish.App.Interfaces;

/// <summary>Text-to-speech playback abstraction used by <see cref="EasyEnglish.App.Services.Speech.SpeechPlayer"/>. See <see cref="EasyEnglish.App.Services.Speech.MauiSpeechService"/> for the default implementation.</summary>
public interface ISpeechService
{
    /// <summary>Whether a <see cref="SpeakSegmentsAsync"/> playback is currently in progress.</summary>
    bool IsSpeaking { get; }

    /// <summary>Speaks plain text in a single language.</summary>
    Task SpeakTextAsync(string text, SpeechLanguage language, CancellationToken ct = default);

    /// <summary>Speaks a single segment (with any language-switch inclusions it contains).</summary>
    Task SpeakSegmentAsync(SpeechSegment segment, CancellationToken ct = default);

    /// <summary>Plays a sequence of segments back-to-back, with <paramref name="pauseBetween"/> silence between each.</summary>
    Task SpeakSegmentsAsync(
        IEnumerable<SpeechSegment> segments,
        TimeSpan pauseBetween,
        CancellationToken ct = default);

    /// <summary>Cancels any in-progress playback.</summary>
    Task StopAsync();
}
