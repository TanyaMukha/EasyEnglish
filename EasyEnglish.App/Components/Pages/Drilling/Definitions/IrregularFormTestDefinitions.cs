using EasyEnglish.Core.Models;
using EasyEnglish.App.Models;
using EasyEnglish.App.Services;
using EasyEnglish.App.Services.SpeechRecognition;
using EasyEnglish.App.Components.Pages.Drilling.Models;
using ReviewWordCard = EasyEnglish.App.Components.Pages.Drilling.Cards.ReviewWordsCard;
using SingleChoiceCard = EasyEnglish.App.Components.Pages.Drilling.Cards.SingleChoiceWordCard;
using ManualInputCard = EasyEnglish.App.Components.Pages.Drilling.Cards.ManualInputWordCard;
using ReviewIrregularFormsCard = EasyEnglish.App.Components.Pages.Drilling.Cards.ReviewIrregularFormsCard;
using InputIrregularFormsCard = EasyEnglish.App.Components.Pages.Drilling.Cards.InputIrregularFormsCard;
using StudyIrregularFormsCard = EasyEnglish.App.Components.Pages.Drilling.Cards.StudyIrregularFormsCard;

namespace EasyEnglish.App.Components.Pages.Drilling.Definitions;

// ── Shared mapper ─────────────────────────────────────────────────────────────

file static class IrregularVm
{
    // FirstForm acts as "Word" — every word card works unchanged
    internal static WordCardViewModel From(IrregularFormModel f) => new(
        Word:          f.FirstForm             ?? "",
        Transcription: f.FirstFormTranscription,
        Translation:   f.FirstFormTranslation,
        Pronunciation: f.FirstFormPronunciation
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

public sealed class ReviewIrregularFormsDef : TestDefinition<IrregularFormTestModel>
{
    public override string Key         => "review-irregular-forms";
    public override string Title       => "Перегляд форм";
    public override string HeaderClass => "pastel-blue";
    public override string IconClass   => "bi-book";

    public override WordCardViewModel  BuildViewModel(IrregularFormTestModel item) => IrregularVm.From(item);
    public override bool               IsBrowse                                     => true;
    public override bool               ShowNextButton(TestState s)                  => true;
    public override NextItemAction     GetNextAction(TestState s)                   => NextItemAction.Remove;
    public override Type               ComponentType                                 => typeof(ReviewWordCard);

    public override void    RecordRating(IrregularFormTestModel item, double rating) => item.Rate = (float)rating;
    public override double? GetRating(IrregularFormTestModel item)                   => item.Rate;
    public override void    OnItemCompleted(IrregularFormTestModel item)             => item.RecordReview();
}

// ── Single-choice: Word → Translation ────────────────────────────────────────

public sealed class IrregularWordToTranslationSingleChoiceDef : TestDefinition<IrregularFormTestModel>
{
    public override string Key         => "irregular-word-to-translation-single-choice";
    public override string Title       => "Переклад форми";
    public override string HeaderClass => "pastel-yellow";
    public override string IconClass   => "bi-question-circle";

    public override bool CanApplyTo(IrregularFormTestModel item) =>
        !string.IsNullOrEmpty(item.FirstFormTranslation);

    public override WordCardViewModel BuildViewModel(IrregularFormTestModel item) => IrregularVm.From(item);

    public override void PrepareState(
        WordCardViewModel vm, IReadOnlyList<IrregularFormTestModel> all,
        Func<IrregularFormTestModel, WordCardViewModel> build, TestState state)
    {
        var pool = all.Select(build).ToList();
        state.AnswerOptions = IrregularVm.ShuffledOptions(
            vm.Translation ?? "",
            IrregularVm.Distractors(vm.Translation ?? "", pool, v => v.Translation));
    }

    public override string?        GetCorrectAnswer(WordCardViewModel vm) => vm.Translation;
    public override bool           ShowNextButton(TestState s)            => s.IsAnswerSubmitted;
    public override NextItemAction GetNextAction(TestState s)             => s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;
    public override Type           ComponentType                           => typeof(SingleChoiceCard);

    public override void RecordAnswer(IrregularFormTestModel item, bool isCorrect) =>
        item.RecordTestAnswer(CardDirection.WordToTranslation, CardType.SingleChoice, isCorrect);
}

// ── Single-choice: Translation → Word ────────────────────────────────────────

public sealed class IrregularTranslationToWordSingleChoiceDef : TestDefinition<IrregularFormTestModel>
{
    public override string Key          => "irregular-translation-to-word-single-choice";
    public override string Title        => "Форма за перекладом";
    public override string HeaderClass  => "pastel-orange";
    public override string IconClass    => "bi-chat-text";
    public override bool   WordIsQuestion => false;

    public override bool CanApplyTo(IrregularFormTestModel item) =>
        !string.IsNullOrEmpty(item.FirstFormTranslation);

    public override WordCardViewModel BuildViewModel(IrregularFormTestModel item) => IrregularVm.From(item);

    public override void PrepareState(
        WordCardViewModel vm, IReadOnlyList<IrregularFormTestModel> all,
        Func<IrregularFormTestModel, WordCardViewModel> build, TestState state)
    {
        var pool = all.Select(build).ToList();
        state.AnswerOptions = IrregularVm.ShuffledOptions(
            vm.Word,
            IrregularVm.Distractors(vm.Word, pool, v => v.Word));
    }

    public override string?        GetCorrectAnswer(WordCardViewModel vm) => vm.Word;
    public override bool           ShowNextButton(TestState s)            => s.IsAnswerSubmitted;
    public override NextItemAction GetNextAction(TestState s)             => s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;
    public override Type           ComponentType                           => typeof(SingleChoiceCard);

    public override void RecordAnswer(IrregularFormTestModel item, bool isCorrect) =>
        item.RecordTestAnswer(CardDirection.TranslationToWord, CardType.SingleChoice, isCorrect);
}

// ── Manual input: Translation → Word ─────────────────────────────────────────

public sealed class IrregularTranslationToWordManualInputDef : TestDefinition<IrregularFormTestModel>
{
    public override string Key          => "irregular-translation-to-word-manual-input";
    public override string Title        => "Напишіть форму";
    public override string HeaderClass  => "pastel-green";
    public override string IconClass    => "bi-keyboard";
    public override bool   WordIsQuestion => false;

    public override bool CanApplyTo(IrregularFormTestModel item) =>
        !string.IsNullOrEmpty(item.FirstFormTranslation);

    public override WordCardViewModel  BuildViewModel(IrregularFormTestModel item) => IrregularVm.From(item);

    // The raw form, markers and all: AnswerMatcher needs them to know what is optional.
    public override string?            GetCorrectAnswer(WordCardViewModel vm)       => vm.Word;
    public override bool               ShowNextButton(TestState s)                  => s.IsAnswerSubmitted;
    public override NextItemAction     GetNextAction(TestState s)                   => s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;
    public override Type               ComponentType                                 => typeof(ManualInputCard);

    public override void RecordAnswer(IrregularFormTestModel item, bool isCorrect) =>
        item.RecordTestAnswer(CardDirection.TranslationToWord, CardType.ManualInput, isCorrect);
}

public sealed class ReviewIrregularFormsCardDef : TestDefinition<IrregularFormTestModel>
{
    public override string Key => "review-irregular-forms-card";
    public override string Title => "Картка форм";
    public override string HeaderClass => "pastel-blue";
    public override string IconClass => "bi-card-list";

    // RawItem card — BuildViewModel is not needed
    public override bool IsBrowse => true;
    public override bool ShowNextButton(TestState s) => true;
    public override NextItemAction GetNextAction(TestState s) => NextItemAction.Remove;
    public override Type ComponentType => typeof(ReviewIrregularFormsCard);

    public override void    RecordRating(IrregularFormTestModel item, double rating) => item.Rate = (float)rating;
    public override double? GetRating(IrregularFormTestModel item)                   => item.Rate;
    public override string  GetLabel(IrregularFormTestModel item)                    => item.FirstForm ?? "";
    public override void    OnItemCompleted(IrregularFormTestModel item)             => item.RecordReview();
}

public sealed class InputIrregularFormsDef : TestDefinition<IrregularFormTestModel>
{
    public override string Key => "input-irregular-forms";
    public override string Title => "Введіть форми";
    public override string HeaderClass => "pastel-green";
    public override string IconClass => "bi-keyboard";

    // RawItem card — BuildViewModel is not needed
    public override bool ShowNextButton(TestState s) => s.IsAnswerSubmitted;
    public override NextItemAction GetNextAction(TestState s) =>
        s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;
    public override Type ComponentType => typeof(InputIrregularFormsCard);

    // Question is the first form (Word), the answer is typing the forms by hand
    public override void RecordAnswer(IrregularFormTestModel item, bool isCorrect) =>
        item.RecordTestAnswer(CardDirection.WordToTranslation, CardType.ManualInput, isCorrect);
}

public sealed class IrregularFormsPronunciationDef : TestDefinition<IrregularFormTestModel>
{
    public override string Key          => "irregular-forms-pronunciation";
    public override string Title        => "Скажіть форми";
    public override string HeaderClass  => "pastel-sage";
    public override string IconClass    => "bi-mic-fill";
    public override bool   WordIsQuestion => false;

    public override bool CanApplyTo(IrregularFormTestModel item) =>
        !string.IsNullOrEmpty(item.FirstFormTranslation);

    // RawItem card — BuildViewModel is not needed
    // The learner decides when to move on — Next is available right away
    public override bool           ShowNextButton(TestState s) => true;
    public override NextItemAction GetNextAction(TestState s)  => s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;
    public override Type           ComponentType                => typeof(StudyIrregularFormsCard);

    public override void RecordAnswer(IrregularFormTestModel item, bool isCorrect) =>
        item.RecordTestAnswer(CardDirection.TranslationToWord, CardType.Pronunciation, isCorrect);
}