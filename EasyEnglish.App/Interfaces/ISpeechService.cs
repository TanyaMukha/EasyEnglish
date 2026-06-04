using EasyEnglish.App.Services.Speech;

namespace EasyEnglish.App.Interfaces;

public interface ISpeechService
{
    bool IsSpeaking { get; }

    Task SpeakTextAsync(string text, SpeechLanguage language, CancellationToken ct = default);

    /// <summary>Озвучити один сегмент (з можливими включеннями іншою мовою).</summary>
    Task SpeakSegmentAsync(SpeechSegment segment, CancellationToken ct = default);

    /// <summary>
    /// Безперервно програти список сегментів із вказаною паузою між ними.
    /// </summary>
    Task SpeakSegmentsAsync(
        IEnumerable<SpeechSegment> segments,
        TimeSpan pauseBetween,
        CancellationToken ct = default);

    Task StopAsync();
}
