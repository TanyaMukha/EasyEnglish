using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using EasyEnglish.App.Interfaces;

namespace EasyEnglish.App.Services.Speech;

/// <summary>All voices available for one <see cref="SpeechLanguage"/> in the voice-picker UI, with a display label and the currently-relevant native codes for ordering (see <see cref="VoicePickerViewModel.BuildGroup"/>).</summary>
public record LanguageVoiceGroup(
    SpeechLanguage Language,
    string Label,
    ObservableCollection<LocaleInfo> Voices,
    string[] NativeCodes
);

/// <summary>
/// Backs the voice-selection settings screen: loads every available voice grouped by
/// <see cref="SpeechLanguage"/>, tracks the learner's in-progress selection per language (not
/// persisted until <see cref="SetSelected"/> is called), and can preview a voice by speaking a
/// short sample sentence.
/// </summary>
public sealed partial class VoicePickerViewModel : ObservableObject
{
    private readonly IVoiceProvider _voiceProvider;
    private readonly VoiceSettingsService _settingsService;
    private readonly ISpeechEngine _engine;

    private readonly Dictionary<SpeechLanguage, LocaleInfo?> _selected = new();

    [ObservableProperty] private bool _isLoading = true;
    [ObservableProperty] private string? _errorMessage;

    /// <summary>One group per <see cref="SpeechLanguage"/>, populated by <see cref="LoadAsync"/>.</summary>
    public ObservableCollection<LanguageVoiceGroup> Groups { get; } = [];

    public VoicePickerViewModel(
        IVoiceProvider voiceProvider,
        VoiceSettingsService settingsService,
        ISpeechEngine engine)
    {
        _voiceProvider = voiceProvider;
        _settingsService = settingsService;
        _engine = engine;
    }

    /// <summary>Loads every available voice and rebuilds <see cref="Groups"/>. Sets <see cref="ErrorMessage"/> (Ukrainian, user-facing) instead of throwing if the underlying provider fails.</summary>
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

    /// <summary>Clears the underlying voice-provider cache; the next <see cref="LoadAsync"/> call re-queries the platform.</summary>
    public void InvalidateCache() => _voiceProvider.InvalidateCache();

    /// <summary>
    /// Builds the voice list for one language: filters to voices matching the bare language code,
    /// ordered offline-first then by priority within <see cref="NativeVoiceCodes"/> then by display
    /// name. Resolves the group's initial selection the same way — saved choice, then a native-code
    /// match, then simply the first voice in the ordered list — and records it in <c>_selected</c>.
    /// </summary>
    private LanguageVoiceGroup BuildGroup(
        SpeechLanguage lang,
        IReadOnlyList<LocaleInfo> allVoices,
        VoiceSettings settings)
    {
        var codes = NativeVoiceCodes.For(lang);
        var langCode = NativeVoiceCodes.BareLanguageCode(lang);
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

        LocaleInfo? current = null;

        if (savedVoiceId is not null)
            current = voices.FirstOrDefault(v => v.Id == savedVoiceId);

        current ??= voices.FirstOrDefault(v => NativeVoiceCodes.Matches(v, codes));

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

    /// <summary>Returns the currently selected voice for <paramref name="lang"/>, or <c>null</c> if none is selected/loaded yet.</summary>
    public LocaleInfo? GetSelected(SpeechLanguage lang) =>
        _selected.TryGetValue(lang, out var v) ? v : null;

    /// <summary>Updates the in-memory selection for <paramref name="lang"/> and fires off a (fire-and-forget) persist via <see cref="VoiceSettingsService.SaveAsync"/>.</summary>
    public void SetSelected(SpeechLanguage lang, LocaleInfo? voice)
    {
        _selected[lang] = voice;
        _ = _settingsService.SaveAsync(lang, voice?.Id);
    }

    /// <summary>Speaks a short sample sentence (chosen by <paramref name="voice"/>'s language/country) so the learner can hear the voice before selecting it.</summary>
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