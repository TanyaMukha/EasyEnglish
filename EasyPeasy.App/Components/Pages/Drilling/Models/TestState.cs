namespace EasyPeasy.App.Components.Pages.Drilling.Models;

/// <summary>
/// Змінний стан одного тест-сесії. Скидається при кожному LoadCurrent().
/// </summary>
public class TestState
{
    // ── Загальні ─────────────────────────────────────────────────────────────

    public bool    IsAnswerSubmitted { get; set; }
    public string? SelectedAnswer    { get; set; }

    /// <summary>
    /// Правильна відповідь — заповнюється MultiTestHost з def.GetCorrectAnswer(vm).
    /// Картки використовують для підсвічування правильного варіанту.
    /// </summary>
    public string? CorrectAnswer { get; set; }

    // ── Single-choice / Manual-input ─────────────────────────────────────────

    public bool IsCorrect { get; set; }

    /// <summary>Варіанти відповідей для single-choice тестів.</summary>
    public IReadOnlyList<string> AnswerOptions { get; set; } = [];

    /// <summary>
    /// true  → питання = Word,        відповідь = Translation  (word-to-translation)
    /// false → питання = Translation,  відповідь = Word         (translation-to-word)
    /// Встановлюється визначенням у PrepareState. За замовчуванням true.
    /// </summary>
    public bool WordIsQuestion { get; set; } = true;

    // ── Know-or-not ───────────────────────────────────────────────────────────

    public bool IsSelfAssessed { get; set; }
    public bool IsKnown        { get; set; }
    public bool IsVerified     { get; set; }

    // ── Reveal (blurred) ──────────────────────────────────────────────────────

    public bool IsRevealed { get; set; }

    // ── Rating ────────────────────────────────────────────────────────────────

    public bool   IsRating       { get; set; }
    public bool   IsRated        { get; set; }
    public double LastRatedValue { get; set; }

    // ─────────────────────────────────────────────────────────────────────────

    public void Reset()
    {
        IsAnswerSubmitted = false;
        SelectedAnswer    = null;
        CorrectAnswer     = null;
        IsCorrect         = false;
        AnswerOptions     = [];
        WordIsQuestion    = true;
        IsSelfAssessed    = false;
        IsKnown           = false;
        IsVerified        = false;
        IsRevealed        = false;
        IsRating          = false;
        IsRated           = false;
        LastRatedValue    = 0;
    }
}
