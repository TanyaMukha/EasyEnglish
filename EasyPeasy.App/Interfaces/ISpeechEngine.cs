using EasyPeasy.App.Services.Speech;

namespace EasyPeasy.App.Interfaces;

/// <summary>Low-level "speak this text with this exact voice" abstraction over a platform TTS engine. See <see cref="EasyPeasy.App.Services.Speech.MauiSpeechEngine"/> for the MAUI implementation.</summary>
public interface ISpeechEngine
{
    /// <summary>Speaks <paramref name="text"/> using <paramref name="voice"/>.</summary>
    Task SpeakAsync(string text, LocaleInfo voice, CancellationToken ct = default);
}
