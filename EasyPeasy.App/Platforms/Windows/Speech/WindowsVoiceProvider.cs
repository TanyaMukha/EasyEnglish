#if WINDOWS
using System.Speech.Synthesis;
using Windows.Media.SpeechSynthesis;
using EasyPeasy.App.Interfaces;
using SpeechSynthesizer = System.Speech.Synthesis.SpeechSynthesizer;

namespace EasyPeasy.App.Services.Speech;

public sealed class WindowsVoiceProvider : IVoiceProvider
{
    private IReadOnlyList<LocaleInfo>? _cache;

    public Task<IReadOnlyList<LocaleInfo>> GetAllVoicesAsync()
    {
        if (_cache is not null)
            return Task.FromResult(_cache);

        using var synth = new SpeechSynthesizer();

        _cache = synth.GetInstalledVoices()
            .Where(v => v.Enabled)
            .Select(v =>
            {
                var info = v.VoiceInfo;
                var parts = info.Culture.Name.Split('-');
                var lang = parts[0].ToLowerInvariant();
                var country = parts.Length > 1 ? parts[1].ToUpperInvariant() : "";
                var id = LocaleInfo.BuildId(lang, country, info.Name);

                return new LocaleInfo(
                    Id: id,
                    Language: lang,
                    Country: country,
                    Name: info.Name,
                    DisplayName: $"{info.Name} ({info.Culture.Name})",
                    IsLocalService: true   // SAPI — завжди локальний
                );
            })
            .OrderBy(v => v.DisplayName)
            .ToList();

        return Task.FromResult(_cache);
    }

    public void InvalidateCache() => _cache = null;
}
#endif