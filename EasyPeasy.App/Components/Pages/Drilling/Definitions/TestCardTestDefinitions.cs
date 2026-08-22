using EasyPeasy.App.Models;
using EasyPeasy.App.Services;
using EasyPeasy.Core.Enums;
using EasyPeasy.App.Components.Pages.Drilling.Models;
using SingleChoiceCard = EasyPeasy.App.Components.Pages.Drilling.Cards.TestCardSingleChoiceCard;
using MultipleChoiceCard = EasyPeasy.App.Components.Pages.Drilling.Cards.TestCardMultipleChoiceCard;
using ShortAnswerCard = EasyPeasy.App.Components.Pages.Drilling.Cards.TestCardShortAnswerCard;
using ClozeCard = EasyPeasy.App.Components.Pages.Drilling.Cards.TestCardClozeCard;
using MatchingCard = EasyPeasy.App.Components.Pages.Drilling.Cards.TestCardMatchingCard;

namespace EasyPeasy.App.Components.Pages.Drilling.Definitions;

// Усі TestCard-визначення — RawItem-картки (BuildViewModel не потрібен), кожна сама
// визначає правильність і надсилає результат через Context.Callbacks.OnCheckAnswer.
// CardDirection/CardType обираються один раз для всієї категорії — тестові картки не
// мають "напрямку" питання, як слова, тож підходить нейтральний найсуворіший варіант.

public sealed class SingleChoiceCardDef : TestDefinition<TestCardTestModel>
{
    public override string Key         => "test-card-single-choice";
    public override string Title       => "Один варіант";
    public override string HeaderClass => "pastel-yellow";
    public override string IconClass   => "bi-check-circle";

    public override bool CanApplyTo(TestCardTestModel item) => item.Kind == TestCardKind.SingleChoice;

    public override bool           ShowNextButton(TestState s) => s.IsAnswerSubmitted;
    public override NextItemAction GetNextAction(TestState s)  => s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;
    public override Type           ComponentType               => typeof(SingleChoiceCard);

    public override void RecordAnswer(TestCardTestModel item, bool isCorrect) =>
        item.RecordTestAnswer(CardDirection.TranslationToWord, CardType.SingleChoice, isCorrect);
}

public sealed class MultipleChoiceCardDef : TestDefinition<TestCardTestModel>
{
    public override string Key         => "test-card-multiple-choice";
    public override string Title       => "Кілька варіантів";
    public override string HeaderClass => "pastel-orange";
    public override string IconClass   => "bi-check2-square";

    public override bool CanApplyTo(TestCardTestModel item) => item.Kind == TestCardKind.MultipleChoice;

    public override bool           ShowNextButton(TestState s) => s.IsAnswerSubmitted;
    public override NextItemAction GetNextAction(TestState s)  => s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;
    public override Type           ComponentType               => typeof(MultipleChoiceCard);

    public override void RecordAnswer(TestCardTestModel item, bool isCorrect) =>
        item.RecordTestAnswer(CardDirection.TranslationToWord, CardType.MultipleChoice, isCorrect);
}

public sealed class ShortAnswerCardDef : TestDefinition<TestCardTestModel>
{
    public override string Key         => "test-card-short-answer";
    public override string Title       => "Коротка відповідь";
    public override string HeaderClass => "pastel-green";
    public override string IconClass   => "bi-keyboard";

    public override bool CanApplyTo(TestCardTestModel item) => item.Kind == TestCardKind.ShortAnswer;

    public override bool           ShowNextButton(TestState s) => s.IsAnswerSubmitted;
    public override NextItemAction GetNextAction(TestState s)  => s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;
    public override Type           ComponentType               => typeof(ShortAnswerCard);

    public override void RecordAnswer(TestCardTestModel item, bool isCorrect) =>
        item.RecordTestAnswer(CardDirection.TranslationToWord, CardType.ManualInput, isCorrect);
}

public sealed class ClozeCardDef : TestDefinition<TestCardTestModel>
{
    public override string Key         => "test-card-cloze";
    public override string Title       => "Заповнити пропуск";
    public override string HeaderClass => "pastel-purple";
    public override string IconClass   => "bi-input-cursor-text";

    public override bool CanApplyTo(TestCardTestModel item) => item.Kind == TestCardKind.Cloze;

    public override bool           ShowNextButton(TestState s) => s.IsAnswerSubmitted;
    public override NextItemAction GetNextAction(TestState s)  => s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;
    public override Type           ComponentType               => typeof(ClozeCard);

    public override void RecordAnswer(TestCardTestModel item, bool isCorrect) =>
        item.RecordTestAnswer(CardDirection.TranslationToWord, CardType.ManualInput, isCorrect);
}

public sealed class MatchingCardDef : TestDefinition<TestCardTestModel>
{
    public override string Key         => "test-card-matching";
    public override string Title       => "Відповідність";
    public override string HeaderClass => "pastel-pink";
    public override string IconClass   => "bi-link-45deg";

    public override bool CanApplyTo(TestCardTestModel item) => item.Kind == TestCardKind.Matching;

    public override bool           ShowNextButton(TestState s) => s.IsAnswerSubmitted;
    public override NextItemAction GetNextAction(TestState s)  => s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;
    public override Type           ComponentType               => typeof(MatchingCard);

    public override void RecordAnswer(TestCardTestModel item, bool isCorrect) =>
        item.RecordTestAnswer(CardDirection.TranslationToWord, CardType.Matching, isCorrect);
}
