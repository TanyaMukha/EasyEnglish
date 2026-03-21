using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using EasyEnglish.App.Interfaces;

namespace EasyEnglish.App.Services.Speech;

public record LanguageVoiceGroup(
    SpeechLanguage Language,
    string Label,
    ObservableCollection<LocaleInfo> Voices,
    string[] NativeCodes
);

public sealed partial class VoicePickerViewModel : ObservableObject
{
    private readonly IVoiceProvider _voiceProvider;
    private readonly VoiceSettingsService _settingsService;
    private readonly ISpeechEngine _engine;

    private readonly Dictionary<SpeechLanguage, LocaleInfo?> _selected = new();

    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private string? _errorMessage;

    public ObservableCollection<LanguageVoiceGroup> Groups { get; } = [];

    private static readonly Dictionary<SpeechLanguage, string[]> NativeCodes = new()
    {
        [SpeechLanguage.EnglishBritish] = ["en-GB", "en-AU", "en-IE"],
        [SpeechLanguage.EnglishAmerican] = ["en-US", "en-CA"],
        [SpeechLanguage.Ukrainian] = ["uk-UA"],
    };

    public VoicePickerViewModel(
        IVoiceProvider voiceProvider,
        VoiceSettingsService settingsService,
        ISpeechEngine engine)
    {
        _voiceProvider = voiceProvider;
        _settingsService = settingsService;
        _engine = engine;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        Groups.Clear();

        try
        {
            var allVoices = await _voiceProvider.GetAllVoicesAsync();
            var settings = await _settingsService.LoadAsync();

            foreach (var lang in Enum.GetValues<SpeechLanguage>())
                Groups.Add(BuildGroup(lang, allVoices, settings));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Помилка завантаження голосів: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void InvalidateCache() => _voiceProvider.InvalidateCache();

    private LanguageVoiceGroup BuildGroup(
        SpeechLanguage lang,
        IReadOnlyList<LocaleInfo> allVoices,
        VoiceSettings settings)
    {
        var codes = NativeCodes[lang];
        var langCode = codes[0].Split('-')[0];
        var savedVoiceId = settings.GetVoiceId(lang);

        var voices = allVoices
            .Where(v => string.Equals(v.Language, langCode,
                StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(v => v.IsLocalService)
            .ThenBy(v =>
            {
                var code = $"{v.Language}-{v.Country}";
                var idx = Array.FindIndex(codes,
                    c => c.Equals(code, StringComparison.OrdinalIgnoreCase));
                return idx >= 0 ? idx : 999;
            })
            .ThenBy(v => v.DisplayName)
            .ToList();

        // Визначаємо поточний вибір
        LocaleInfo? current = null;

        if (savedVoiceId is not null)
            current = voices.FirstOrDefault(v => v.Id == savedVoiceId);

        current ??= voices.FirstOrDefault(v =>
            codes.Any(c =>
            {
                var parts = c.Split('-');
                return string.Equals(v.Language, parts[0], StringComparison.OrdinalIgnoreCase)
                    && (parts.Length < 2 || string.Equals(v.Country, parts[1],
                        StringComparison.OrdinalIgnoreCase));
            }));

        current ??= voices.FirstOrDefault();
        _selected[lang] = current;

        var label = lang switch
        {
            SpeechLanguage.EnglishBritish => "English — British (en-GB)",
            SpeechLanguage.EnglishAmerican => "English — American (en-US)",
            SpeechLanguage.Ukrainian => "Українська (uk-UA)",
            _ => lang.ToString()
        };

        return new LanguageVoiceGroup(lang, label,
            new ObservableCollection<LocaleInfo>(voices), codes);
    }

    public LocaleInfo? GetSelected(SpeechLanguage lang) =>
        _selected.TryGetValue(lang, out var v) ? v : null;

    public void SetSelected(SpeechLanguage lang, LocaleInfo? voice)
    {
        _selected[lang] = voice;
        _ = _settingsService.SaveAsync(lang, voice?.Id);
    }

    public async Task PreviewAsync(LocaleInfo voice)
    {
        var text = voice.Language switch
        {
            "uk" => "Привіт! Це тестове озвучення.",
            "en" when voice.Country == "GB" => "Hello! This is a British voice test.",
            _ => "Hello! This is an American voice test."
        };

        await _engine.SpeakAsync(text, voice);
    }
}