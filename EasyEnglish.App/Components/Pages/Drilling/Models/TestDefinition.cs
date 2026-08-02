namespace EasyEnglish.App.Components.Pages.Drilling.Models;

/// <summary>
/// Describes one kind of test: metadata, lifecycle logic and the model-to-ViewModel mapping.
/// TItem is the data model (WordTestModel, IrregularFormModel, ExampleModel, …).
/// </summary>
public abstract class TestDefinition<TItem>
{
    // ── Identity & UI metadata ────────────────────────────────────────────────

    public abstract string Key         { get; }
    public abstract string Title       { get; }
    public abstract string HeaderClass { get; }
    public abstract string IconClass   { get; }

    // ── ViewModel ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Maps the model to a WordCardViewModel for word-shaped cards.
    /// Example definitions keep the base implementation — they pass the raw item
    /// through TestCardContext.RawItem and never touch WordCardViewModel.
    /// </summary>
    public virtual WordCardViewModel BuildViewModel(TItem item) =>
        new(Word: "", Transcription: null, Translation: null);

    // ── Filtering ─────────────────────────────────────────────────────────────

    public virtual bool CanApplyTo(TItem item) => true;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Called right after State.Reset(). Override to generate answer options and the like.
    /// Definitions that do not use BuildViewModel (examples) simply ignore it.
    /// </summary>
    public virtual void PrepareState(
        WordCardViewModel              viewModel,
        IReadOnlyList<TItem>           allItems,
        Func<TItem, WordCardViewModel> allBuildVm,
        TestState                      state) { }

    // ── Result recording ──────────────────────────────────────────────────────

    /// <summary>
    /// Writes the final answer result into the item statistics
    /// (for example item.Tests[direction][type]). No-op by default.
    /// Called by the engine on every OnCheckAnswer.
    /// </summary>
    public virtual void RecordAnswer(TItem item, bool isCorrect) { }

    /// <summary>
    /// Writes a manual difficulty rating (review cards) into the item.
    /// No-op by default. Called by the engine on OnRate.
    /// </summary>
    public virtual void RecordRating(TItem item, double rating) { }

    /// <summary>
    /// Short label for the browse-phase card list. Defaults to the view model's word;
    /// definitions whose cards are not word-shaped override it.
    /// </summary>
    public virtual string GetLabel(TItem item) => BuildViewModel(item).Word;

    /// <summary>
    /// Rating the item carries right now. MultiTestHost snapshots it before the first manual
    /// rating so that clearing the choice (clicking the same level again) can restore it.
    /// Returns null for definitions that do not rate anything.
    /// </summary>
    public virtual double? GetRating(TItem item) => null;

    /// <summary>
    /// Called when an item is completed (Next with NextItemAction.Remove).
    /// Review definitions update the review date here. No-op by default.
    /// </summary>
    public virtual void OnItemCompleted(TItem item) { }

    // ── Correctness ───────────────────────────────────────────────────────────

    /// <summary>
    /// The correct answer to compare against (word/translation-style tests).
    /// Return null when the card decides correctness itself.
    /// </summary>
    public virtual string? GetCorrectAnswer(WordCardViewModel viewModel) => null;

    // ── UI decisions ──────────────────────────────────────────────────────────

    /// <summary>
    /// true  → question = Word,        answer = Translation
    /// false → question = Translation,  answer = Word
    /// </summary>
    public virtual bool WordIsQuestion => true;

    public abstract bool           ShowNextButton(TestState state);
    public abstract NextItemAction GetNextAction(TestState state);

    /// <summary>
    /// true → browsing mode: the learner flips through these cards back and forth without
    /// answering anything, and leaves the phase explicitly (the "to tests" / "finish" buttons).
    /// Such items never enter the answer queue and are never removed from it.
    /// </summary>
    public virtual bool IsBrowse => false;

    // ── Rendering ─────────────────────────────────────────────────────────────

    /// <summary>
    /// The Blazor component type used for rendering.
    /// The component must expose [Parameter] TestCardContext Context.
    /// </summary>
    public abstract Type ComponentType { get; }
}
