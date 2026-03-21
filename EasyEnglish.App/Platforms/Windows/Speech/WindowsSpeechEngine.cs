#if WINDOWS
using System.Speech.Synthesis;
using Windows.Media.SpeechSynthesis;
using EasyEnglish.App.Interfaces;
using SpeechSynthesizer = System.Speech.Synthesis.SpeechSynthesizer;

namespace EasyEnglish.App.Services.Speech;

public sealed class WindowsSpeechEngine : ISpeechEngine
{
    public Task SpeakAsync(string text, LocaleInfo voice, CancellationToken ct = default)
    {
        return Task.Run(() =>
        {
            using var synth = new SpeechSynthesizer();
            synth.SelectVoice(voice.Name);
            synth.SetOutputToDefaultAudioDevice();

            ct.ThrowIfCancellationRequested();

            var prompt = synth.SpeakAsync(text);

            using var reg = ct.Register(() =>
            {
                try { synth.SpeakAsyncCancel(prompt); } catch { }
            });

            while (!prompt.IsCompleted && !ct.IsCancellationRequested)
                Thread.Sleep(50);

        }, ct);
    }
}
#endif