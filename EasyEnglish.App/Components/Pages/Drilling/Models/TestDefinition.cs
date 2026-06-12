namespace EasyEnglish.App.Components.Pages.Drilling.Models;

/// <summary>
/// Описує один тип тесту: метадані, lifecycle-логіку та маппінг моделі в ViewModel.
/// TItem — модель даних (WordTestModel, IrregularFormModel, ExampleModel, …).
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
    /// Перетворює модель у WordCardViewModel для карток типу "слово".
    /// Визначення для прикладів залишають базову реалізацію — вони передають
    /// сирий item через TestCardContext.RawItem і не використовують WordCardViewModel.
    /// </summary>
    public virtual WordCardViewModel BuildViewModel(TItem item) =>
        new(Word: "", Transcription: null, Translation: null);

    // ── Filtering ─────────────────────────────────────────────────────────────

    public virtual bool CanApplyTo(TItem item) => true;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Викликається після State.Reset(). Override для генерації варіантів тощо.
    /// Для визначень що не використовують BuildViewModel (приклади) — просто ігнорується.
    /// </summary>
    public virtual void PrepareState(
        WordCardViewModel              viewModel,
        IReadOnlyList<TItem>           allItems,
        Func<TItem, WordCardViewModel> allBuildVm,
        TestState                      state) { }

    // ── Result recording ──────────────────────────────────────────────────────

    /// <summary>
    /// Записує фінальний результат відповіді в статистику item-а
    /// (наприклад, item.Tests[direction][type]). За замовчуванням — no-op.
    /// Викликається MultiTestHost при кожному OnCheckAnswer.
    /// </summary>
    public virtual void RecordAnswer(TItem item, bool isCorrect) { }

    /// <summary>
    /// Записує ручну оцінку складності (review-картки) в item.
    /// За замовчуванням — no-op. Викликається MultiTestHost при OnRate.
    /// </summary>
    public virtual void RecordRating(TItem item, double rating) { }

    /// <summary>
    /// Викликається коли item завершено (Next з NextItemAction.Remove).
    /// Review-визначення оновлюють тут дату перегляду. За замовчуванням — no-op.
    /// </summary>
    public virtual void OnItemCompleted(TItem item) { }

    // ── Correctness ───────────────────────────────────────────────────────────

    /// <summary>
    /// Правильна відповідь для порівняння (word/translation-style тести).
    /// Повернути null якщо картка сама визначає правильність через "✓"/"✗".
    /// </summary>
    public virtual string? GetCorrectAnswer(WordCardViewModel viewModel) => null;

    // ── UI decisions ──────────────────────────────────────────────────────────

    /// <summary>
    /// true  → питання = Word,        відповідь = Translation
    /// false → питання = Translation,  відповідь = Word
    /// </summary>
    public virtual bool WordIsQuestion => true;

    public abstract bool           ShowNextButton(TestState state);
    public abstract NextItemAction GetNextAction(TestState state);

    // ── Rendering ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Тип Blazor-компонента для рендеру.
    /// Компонент повинен мати [Parameter] TestCardContext Context.
    /// </summary>
    public abstract Type ComponentType { get; }
}
