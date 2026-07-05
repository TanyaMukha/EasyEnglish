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

    public static IReadOnlyList<TestDefinition<ExampleTestModel>> ExampleTests { get; } =
    [
        new ReviewExamplesTestDef(),
        new ReviewExamplesBlurredTestDef(),
        new InputExamplesTestDef(),
    ];

    public static IReadOnlyList<TestDefinition<IrregularFormTestModel>> IrregularFormTests { get; } =
    [
        new ReviewIrregularFormsDef(),
        new IrregularWordToTranslationSingleChoiceDef(),
        new IrregularTranslationToWordSingleChoiceDef(),
        new IrregularTranslationToWordManualInputDef(),
        new ReviewIrregularFormsCardDef(),
        new InputIrregularFormsDef(),
    ];

    public static IReadOnlyList<TestDefinition<StudyCardTestModel>> StudyCardTests { get; } =
    [
        new ReviewStudyCardsDef(),
    ];

    public static IReadOnlyList<TestDefinition<TestCardTestModel>> TestCardTests { get; } =
    [
        new SingleChoiceCardDef(),
        new MultipleChoiceCardDef(),
        new ShortAnswerCardDef(),
        new ClozeCardDef(),
        new MatchingCardDef(),
    ];

    public static TestDefinition<WordTestModel>?          GetWordTest(string key)          => WordTests.FirstOrDefault(t => t.Key == key);
    public static TestDefinition<ExampleTestModel>?       GetExampleTest(string key)       => ExampleTests.FirstOrDefault(t => t.Key == key);
    public static TestDefinition<IrregularFormTestModel>? GetIrregularFormTest(string key) => IrregularFormTests.FirstOrDefault(t => t.Key == key);
    public static TestDefinition<StudyCardTestModel>?     GetStudyCardTest(string key)     => StudyCardTests.FirstOrDefault(t => t.Key == key);
    public static TestDefinition<TestCardTestModel>?      GetTestCardTest(string key)      => TestCardTests.FirstOrDefault(t => t.Key == key);
}
