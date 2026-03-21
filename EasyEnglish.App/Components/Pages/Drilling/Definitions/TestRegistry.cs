using EasyEnglish.App.Models;
using EasyEnglish.Core.Models;
using EasyEnglish.App.Components.Pages.Drilling.Models;

namespace EasyEnglish.App.Components.Pages.Drilling.Definitions;

/// <summary>
/// Центральний реєстр усіх тестів.
/// Щоб додати новий тест — тільки сюди + новий TestDefinition клас.
/// </summary>
public static class TestRegistry
{
    public static IReadOnlyList<TestDefinition<WordTestModel>> WordTests { get; } =
    [
        new ReviewWordsDef(),
        new WordToTranslationSingleChoiceDef(),
        new WordToTranslationKnowOrNotDef(),
        new TranslationToWordSingleChoiceDef(),
        new TranslationToWordKnowOrNotDef(),
        new TranslationToWordManualInputDef(),
    ];

    public static IReadOnlyList<TestDefinition<ExampleModel>> ExampleTests { get; } =
    [
        new ReviewExamplesTestDef(),
        new ReviewExamplesBlurredTestDef(),
        new InputExamplesTestDef(),
    ];

    public static IReadOnlyList<TestDefinition<IrregularFormModel>> IrregularFormTests { get; } =
    [
        new ReviewIrregularFormsDef(),
        new IrregularWordToTranslationSingleChoiceDef(),
        new IrregularTranslationToWordSingleChoiceDef(),
        new IrregularTranslationToWordManualInputDef(),
        new ReviewIrregularFormsCardDef(),
        new InputIrregularFormsDef(),
    ];

    public static TestDefinition<WordTestModel>?      GetWordTest(string key)          => WordTests.FirstOrDefault(t => t.Key == key);
    public static TestDefinition<ExampleModel>?       GetExampleTest(string key)       => ExampleTests.FirstOrDefault(t => t.Key == key);
    public static TestDefinition<IrregularFormModel>? GetIrregularFormTest(string key) => IrregularFormTests.FirstOrDefault(t => t.Key == key);
}
