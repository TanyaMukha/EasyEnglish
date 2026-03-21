using EasyEnglish.Core.Models;
using EasyEnglish.App.Services;
using EasyEnglish.App.Components.Pages.Drilling.Models;
// Картки прикладів — повні назви класів Blazor-компонентів:
using ReviewExamplesCard        = EasyEnglish.App.Components.Pages.Drilling.Cards.ReviewExamplesCard;
using ReviewExamplesBlurredCard = EasyEnglish.App.Components.Pages.Drilling.Cards.ReviewExamplesBlurredCard;
using InputExamplesCard         = EasyEnglish.App.Components.Pages.Drilling.Cards.InputExamplesCard;

namespace EasyEnglish.App.Components.Pages.Drilling.Definitions;

// ── Review ────────────────────────────────────────────────────────────────────

public sealed class ReviewExamplesTestDef : TestDefinition<ExampleModel>
{
    public override string Key         => "review-examples";
    public override string Title       => "Перегляд прикладів";
    public override string HeaderClass => "pastel-blue";
    public override string IconClass   => "bi-eye";

    // BuildViewModel не потрібен — ReviewExamplesCard використовує Context.GetRawItem<ExampleModel>()
    public override bool           ShowNextButton(TestState s) => true;
    public override NextItemAction GetNextAction(TestState s)  => NextItemAction.Remove;
    public override Type           ComponentType               => typeof(ReviewExamplesCard);
}

// ── Blurred ───────────────────────────────────────────────────────────────────

public sealed class ReviewExamplesBlurredTestDef : TestDefinition<ExampleModel>
{
    public override string Key         => "review-examples-blurred";
    public override string Title       => "Угадай слово";
    public override string HeaderClass => "pastel-purple";
    public override string IconClass   => "bi-search";

    public override bool CanApplyTo(ExampleModel item) =>
        ExampleMarkdownService.HasHiddenText(item.Sentence, ExampleMarkdownService.HiddenTextMarker.Bold);

    public override bool           ShowNextButton(TestState s) => s.IsRevealed;
    public override NextItemAction GetNextAction(TestState s)  => NextItemAction.Remove;
    public override Type           ComponentType               => typeof(ReviewExamplesBlurredCard);
}

// ── Input ─────────────────────────────────────────────────────────────────────

public sealed class InputExamplesTestDef : TestDefinition<ExampleModel>
{
    public override string Key         => "input-examples";
    public override string Title       => "Введи слово";
    public override string HeaderClass => "pastel-green";
    public override string IconClass   => "bi-keyboard";

    public override bool CanApplyTo(ExampleModel item) =>
        ExampleMarkdownService.HasHiddenText(item.Sentence, ExampleMarkdownService.HiddenTextMarker.Bold);

    public override bool           ShowNextButton(TestState s) => s.IsAnswerSubmitted;
    public override NextItemAction GetNextAction(TestState s)  =>
        s.IsCorrect ? NextItemAction.Remove : NextItemAction.Requeue;
    public override Type           ComponentType               => typeof(InputExamplesCard);
}
