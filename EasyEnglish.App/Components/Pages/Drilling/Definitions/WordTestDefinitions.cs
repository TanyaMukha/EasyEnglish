using EasyEnglish.App.Models;
using EasyEnglish.App.Services;
using EasyEnglish.App.Services.SpeechRecognition;
using EasyEnglish.App.Components.Pages.Drilling.Models;
using ReviewWordCard   = EasyEnglish.App.Components.Pages.Drilling.Cards.ReviewWordsCard;
using SingleChoiceCard = EasyEnglish.App.Components.Pages.Drilling.Cards.SingleChoiceWordCard;
using KnowOrNotCard    = EasyEnglish.App.Components.Pages.Drilling.Cards.KnowOrNotWordCard;
using ManualInputCard  = EasyEnglish.App.Components.Pages.Drilling.Cards.ManualInputWordCard;
using StudyWordCard    = EasyEnglish.App.Components.Pages.Drilling.Cards.StudyWordCard;

namespace EasyEnglish.App.Components.Pages.Drilling.Definitions;

// ── Shared helpers ────────────────────────────────────────────────────────────

file static class WordVm
{
    internal static WordCardViewModel From(WordTestModel w) => new(
        Word:          w.Word          ?? "",
        Transcription: w.Transcription,
        Translation:   w.Translation,
        Examples:      w.Examples?.Select(e => new ExampleViewModel(e.Sentence, e.Translation)).ToList(),
        Pronunciation: w.Pronunciation,
        Note:          w.Note
    );

    internal static IReadOnlyList<string> Distractors(
        string                           correct,
        IReadOnlyList<WordCardViewModel> pool,
        Func<WordCardViewModel, string?> selector)
        => pool
            .Where(vm => selector(vm) != correct && !string.IsNullOrEmpty(selector(vm)))
            .Select(vm => selector(vm)!)
            .Distinct()
            .OrderBy(_ => Guid.NewGuid())
            .Take(3)
            .ToList();

    internal static IReadOnlyList<string> ShuffledOptions(string correct, IReadOnlyList<string> distractors)
        => new[] { correct }.Concat(distractors).OrderBy(_ => Guid.NewGuid()).ToList();
}

// ── Review ────────────────────────────────────────────────────────────────────

public sealed class ReviewWordsDef : TestDefinition<WordTestModel>
{
    public override string Key         => "review-words";
    public override string Title       => "Перегляд слів";
    public override string HeaderClass => "pastel-blue";
    public override string IconClass   => "bi-book";

    public override WordCardViewModel  BuildViewModel(WordTestModel item) => WordVm.From(item);
    public override bool               IsBrowse                            => true;
    public override bool               ShowNextButton(TestState s)        => true;
    public override NextItemAction     GetNextAction(TestState s)         => NextItemAction.Remove;
    public override Type               ComponentType                       => typeof(ReviewWordCard);

    public override void    RecordRating(WordTestModel item, double rating) => item.Rate = (float)rating;
    public override double? GetRating(WordTestModel item)                   => item.Rate;
    public override void    OnItemCompleted(WordTestModel item)             => item.RecordReview();
}

// ── Single-choice: Word → Translation ────────────────────────────────────────

public sealed class WordToTranslationSingleChoiceDef : TestDefinition<WordTestModel>
{
    public override string Key         => "word-to-translation-single-choice";
    public override string Title       => "Переклад для слова";
    public override string HeaderClass => "pastel-yellow";
    public override string IconClass   => "bi-question-circle";

    public override WordCardViewModel BuildViewModel(WordTestModel item) => WordVm.From(item);

    public override void PrepareState(
        WordCardViewModel vm, IReadOnlyList<WordTestModel> all,
        Func<WordTestModel, WordCardViewModel> build, TestState state)
    {
        var pool = all.Select(build).ToList();
        state.AnswerOptions = WordVm.ShuffledOptions(
            vm.Translation ?? "",
            WordVm.Distractors(vm.Translation ?? "", pool, v => v.Translation));
    }

    public override string?        GetCorrectAnswer(WordCardViewModel vm) => vm.Translation;
    public override bool           ShowNextButton(TestState s)            => s.IsAnswerSubmitted;
    public override NextItemAction GetNextAction(TestState s)             => s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;
    public override Type           ComponentType                           => typeof(SingleChoiceCard);

    public override void RecordAnswer(WordTestModel item, bool isCorrect) =>
        item.RecordTestAnswer(CardDirection.WordToTranslation, CardType.SingleChoice, isCorrect);
}

// ── Single-choice: Translation → Word ────────────────────────────────────────

public sealed class TranslationToWordSingleChoiceDef : TestDefinition<WordTestModel>
{
    public override string Key          => "translation-to-word-single-choice";
    public override string Title        => "Слово за перекладом";
    public override string HeaderClass  => "pastel-orange";
    public override string IconClass    => "bi-chat-text";
    public override bool   WordIsQuestion => false;

    public override WordCardViewModel BuildViewModel(WordTestModel item) => WordVm.From(item);

    public override void PrepareState(
        WordCardViewModel vm, IReadOnlyList<WordTestModel> all,
        Func<WordTestModel, WordCardViewModel> build, TestState state)
    {
        var pool = all.Select(build).ToList();
        state.AnswerOptions = WordVm.ShuffledOptions(
            vm.Word,
            WordVm.Distractors(vm.Word, pool, v => v.Word));
    }

    public override string?        GetCorrectAnswer(WordCardViewModel vm) => vm.Word;
    public override bool           ShowNextButton(TestState s)            => s.IsAnswerSubmitted;
    public override NextItemAction GetNextAction(TestState s)             => s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;
    public override Type           ComponentType                           => typeof(SingleChoiceCard);

    public override void RecordAnswer(WordTestModel item, bool isCorrect) =>
        item.RecordTestAnswer(CardDirection.TranslationToWord, CardType.SingleChoice, isCorrect);
}

// ── Know-or-not: Word → Translation ──────────────────────────────────────────

public sealed class WordToTranslationKnowOrNotDef : TestDefinition<WordTestModel>
{
    public override string Key         => "word-to-translation-know-or-not";
    public override string Title       => "Перевірка перекладу";
    public override string HeaderClass => "pastel-purple";
    public override string IconClass   => "bi-eye";

    public override WordCardViewModel  BuildViewModel(WordTestModel item) => WordVm.From(item);
    // The card calls OnCheckAnswer itself — show Next once the result arrives
    public override bool               ShowNextButton(TestState s)        => s.IsAnswerSubmitted;
    public override NextItemAction     GetNextAction(TestState s)         => s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;
    public override Type               ComponentType                       => typeof(KnowOrNotCard);

    public override void RecordAnswer(WordTestModel item, bool isCorrect) =>
        item.RecordTestAnswer(CardDirection.WordToTranslation, CardType.KnowOrNot, isCorrect);
}

// ── Know-or-not: Translation → Word ──────────────────────────────────────────

public sealed class TranslationToWordKnowOrNotDef : TestDefinition<WordTestModel>
{
    public override string Key          => "translation-to-word-know-or-not";
    public override string Title        => "Перевірка слова";
    public override string HeaderClass  => "pastel-pink";
    public override string IconClass    => "bi-eye-slash";
    public override bool   WordIsQuestion => false;

    public override WordCardViewModel  BuildViewModel(WordTestModel item) => WordVm.From(item);
    // The card calls OnCheckAnswer itself — show Next once the result arrives
    public override bool               ShowNextButton(TestState s)        => s.IsAnswerSubmitted;
    public override NextItemAction     GetNextAction(TestState s)         => s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;
    public override Type               ComponentType                       => typeof(KnowOrNotCard);

    public override void RecordAnswer(WordTestModel item, bool isCorrect) =>
        item.RecordTestAnswer(CardDirection.TranslationToWord, CardType.KnowOrNot, isCorrect);
}

// ── Manual input: Translation → Word ─────────────────────────────────────────

public sealed class TranslationToWordManualInputDef : TestDefinition<WordTestModel>
{
    public override string Key          => "translation-to-word-manual-input";
    public override string Title        => "Напишіть слово";
    public override string HeaderClass  => "pastel-green";
    public override string IconClass    => "bi-keyboard";
    public override bool   WordIsQuestion => false;

    public override WordCardViewModel  BuildViewModel(WordTestModel item)      => WordVm.From(item);
    public override string?            GetCorrectAnswer(WordCardViewModel vm)  => PronunciationTextNormalizer.PrepareExpectedText(vm.Word);
    public override bool               ShowNextButton(TestState s)             => s.IsAnswerSubmitted;
    public override NextItemAction     GetNextAction(TestState s)              => s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;
    public override Type               ComponentType                            => typeof(ManualInputCard);

    public override void RecordAnswer(WordTestModel item, bool isCorrect) =>
        item.RecordTestAnswer(CardDirection.TranslationToWord, CardType.ManualInput, isCorrect);
}

// ── Pronunciation: Translation → Word (spoken) ───────────────────────────────

public sealed class TranslationToWordPronunciationDef : TestDefinition<WordTestModel>
{
    public override string Key          => "translation-to-word-pronunciation";
    public override string Title        => "Скажіть слово";
    public override string HeaderClass  => "pastel-coral";
    public override string IconClass    => "bi-mic-fill";
    public override bool   WordIsQuestion => false;

    public override WordCardViewModel  BuildViewModel(WordTestModel item) => WordVm.From(item);
    // The learner decides when to move on — Next is available right away
    public override bool               ShowNextButton(TestState s)        => true;
    public override NextItemAction     GetNextAction(TestState s)         => s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;
    public override Type               ComponentType                       => typeof(StudyWordCard);

    public override void RecordAnswer(WordTestModel item, bool isCorrect) =>
        item.RecordTestAnswer(CardDirection.TranslationToWord, CardType.Pronunciation, isCorrect);
}
