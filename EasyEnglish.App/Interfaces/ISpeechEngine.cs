using EasyEnglish.App.Services.Speech;

namespace EasyEnglish.App.Interfaces;

public interface ISpeechEngine
{
    Task SpeakAsync(string text, LocaleInfo voice, CancellationToken ct = default);
}
