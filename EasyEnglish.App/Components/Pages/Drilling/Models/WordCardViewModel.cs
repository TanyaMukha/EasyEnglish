namespace EasyEnglish.App.Components.Pages.Drilling.Models;

/// <summary>
/// Спільний ViewModel для всіх карток типу "слово".
/// TestDefinition відповідає за маппінг будь-якої моделі сюди.
/// Картки знають тільки про цей тип — не про WordTestModel чи IrregularFormModel.
/// </summary>
public record WordCardViewModel(
    string  Word,
    string? Transcription,
    string? Translation,
    IReadOnlyList<ExampleViewModel>? Examples = null,
    byte[]? Pronunciation = null
);

public record ExampleViewModel(string Sentence, string? Translation);
