using EasyPeasy.Core.Models;
using EasyPeasy.App.Models;
using EasyPeasy.App.Services;
using EasyPeasy.App.Components.Pages.Drilling.Models;
// Example cards — full Blazor component class names:
using ReviewExamplesCard        = EasyPeasy.App.Components.Pages.Drilling.Cards.ReviewExamplesCard;
using ReviewExamplesBlurredCard = EasyPeasy.App.Components.Pages.Drilling.Cards.ReviewExamplesBlurredCard;
using InputExamplesCard         = EasyPeasy.App.Components.Pages.Drilling.Cards.InputExamplesCard;

namespace EasyPeasy.App.Components.Pages.Drilling.Definitions;

// ── Review ────────────────────────────────────────────────────────────────────

public sealed class ReviewExamplesTestDef : TestDefinition<ExampleTestModel>
{
    public override string Key         => "review-examples";
    public override string Title       => "Перегляд прикладів";
    public override string HeaderClass => "pastel-blue";
    public override string IconClass   => "bi-eye";

    // BuildViewModel is not needed — ReviewExamplesCard uses Context.GetRawItem<ExampleModel>()
    public override bool           IsBrowse                    => true;
    public override bool           ShowNextButton(TestState s) => true;
    public override NextItemAction GetNextAction(TestState s)  => NextItemAction.Remove;
    public override Type           ComponentType               => typeof(ReviewExamplesCard);

    // An example rating is applied to the word the example belongs to
    public override void RecordRating(ExampleTestModel item, double rating)
    {
        if (item.TestWord is not null)
            item.TestWord.Rate = (float)rating;
    }

    public override double? GetRating(ExampleTestModel item) => item.TestWord?.Rate;

    public override string GetLabel(ExampleTestModel item) =>
        ExampleMarkdownService.StripMarkdown(item.Sentence ?? "");

    public override void OnItemCompleted(ExampleTestModel item) => item.TestWord?.RecordReview();
}

// ── Blurred ───────────────────────────────────────────────────────────────────

public sealed class ReviewExamplesBlurredTestDef : TestDefinition<ExampleTestModel>
{
    public override string Key         => "review-examples-blurred";
    public override string Title       => "Угадай слово";
    public override string HeaderClass => "pastel-purple";
    public override string IconClass   => "bi-search";

    public override bool CanApplyTo(ExampleTestModel item) =>
        ExampleMarkdownService.HasHiddenText(item.Sentence, ExampleMarkdownService.HiddenTextMarker.Bold);

    public override bool           ShowNextButton(TestState s) => s.IsRevealed;
    public override NextItemAction GetNextAction(TestState s)  => NextItemAction.Remove;
    public override Type           ComponentType               => typeof(ReviewExamplesBlurredCard);
}

// ── Input ─────────────────────────────────────────────────────────────────────

public sealed class InputExamplesTestDef : TestDefinition<ExampleTestModel>
{
    public override string Key         => "input-examples";
    public override string Title       => "Введи слово";
    public override string HeaderClass => "pastel-green";
    public override string IconClass   => "bi-keyboard";

    public override bool CanApplyTo(ExampleTestModel item) =>
        ExampleMarkdownService.HasHiddenText(item.Sentence, ExampleMarkdownService.HiddenTextMarker.Bold);

    public override bool           ShowNextButton(TestState s) => s.IsAnswerSubmitted;
    public override NextItemAction GetNextAction(TestState s)  =>
        s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;
    public override Type           ComponentType               => typeof(InputExamplesCard);

    // Typing the hidden word of a sentence counts as manual input for that word
    public override void RecordAnswer(ExampleTestModel item, bool isCorrect) =>
        item.TestWord?.RecordTestAnswer(CardDirection.TranslationToWord, CardType.ManualInput, isCorrect);
}
