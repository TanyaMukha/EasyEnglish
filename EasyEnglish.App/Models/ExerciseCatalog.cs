namespace EasyEnglish.App.Models;

/// <summary>Catalog of exercise types for words/examples/irregular forms — shared by the unit-level
/// and course-level practice setup pages so exercise descriptions aren't duplicated.
///
/// Titles say what the learner will actually do, and in which direction, because the picker is a
/// plain list of checkboxes: the title is all there is to go on.</summary>
public static class ExerciseCatalog
{
    public static readonly List<ExerciseOption> Words = new()
    {
        new() { Key = "review-words", Title = "Перегляд слів із перекладом", Description = "Гортайте картки зі словом, транскрипцією, перекладом і прикладом", Icon = "bi-book", Color = "blue" },
        new() { Key = "word-to-translation-single-choice", Title = "Обрати переклад із чотирьох варіантів", Description = "Показано англійське слово — оберіть його переклад", Icon = "bi-translate", Color = "green" },
        new() { Key = "translation-to-word-single-choice", Title = "Обрати слово із чотирьох варіантів", Description = "Показано переклад — оберіть англійське слово", Icon = "bi-search", Color = "orange" },
        new() { Key = "translation-to-word-manual-input", Title = "Написати слово за перекладом", Description = "Введіть англійське слово з пам'яті", Icon = "bi-keyboard", Color = "mint" },
        // Needs a microphone and a quiet room — the learner turns it on deliberately, not by default.
        new() { Key = "translation-to-word-pronunciation", Title = "Вимовити слово вголос", Description = "Скажіть слово в мікрофон — застосунок перевірить вимову", Icon = "bi-mic-fill", Color = "coral", RequiresSpeech = true, InDefaultPreset = false },

        // Self-assessment is honest but slow, and the learner grades themselves — off by default.
        new() { Key = "word-to-translation-know-or-not", Title = "Самоперевірка: чи знаю переклад", Description = "Показано слово — зізнайтеся собі, чи знаєте переклад", Icon = "bi-eye", Color = "purple", InDefaultPreset = false },
        new() { Key = "translation-to-word-know-or-not", Title = "Самоперевірка: чи знаю слово", Description = "Показано переклад — зізнайтеся собі, чи знаєте слово", Icon = "bi-eye-slash", Color = "pink", InDefaultPreset = false },
    };

    public static readonly List<ExerciseOption> Examples = new()
    {
        new() { Key = "review-examples", Title = "Перегляд прикладів", Description = "Переглядайте речення з перекладом для закріплення", Icon = "bi-eye", Color = "blue" },
        new() { Key = "review-examples-blurred", Title = "Угадай слово", Description = "Натисніть на розмите слово щоб його відкрити", Icon = "bi-search", Color = "purple" },
        new() { Key = "input-examples", Title = "Введи слово", Description = "Надрукуйте пропущене слово у реченні", Icon = "bi-keyboard", Color = "mint" },
    };

    public static readonly List<ExerciseOption> IrregularForms = new()
    {
        new() { Key = "review-irregular-forms", Title = "Перегляд форм", Description = "Переглядайте всі форми слів для запам'ятовування", Icon = "bi-book", Color = "blue" },
        new() { Key = "irregular-word-to-translation-single-choice", Title = "Переклад форми", Description = "Оберіть правильний переклад для форми слова", Icon = "bi-question-circle", Color = "yellow" },
        new() { Key = "irregular-translation-to-word-single-choice", Title = "Форма за перекладом", Description = "Знайдіть форму слова за його перекладом", Icon = "bi-chat-text", Color = "orange" },
        new() { Key = "irregular-translation-to-word-manual-input", Title = "Напишіть форму", Description = "Введіть форму слова за перекладом", Icon = "bi-keyboard", Color = "mint" },
        new() { Key = "review-irregular-forms-card", Title = "Картка форм", Description = "Переглядайте всі форми слова на одній картці", Icon = "bi-card-list", Color = "blue" },
        new() { Key = "input-irregular-forms", Title = "Введіть форми", Description = "Напишіть другу та третю форму слова", Icon = "bi-keyboard", Color = "green" },
        new() { Key = "irregular-forms-pronunciation", Title = "Скажіть форми", Description = "Вимовте всі три форми слова одразу за перекладом", Icon = "bi-mic-fill", Color = "sage", RequiresSpeech = true, InDefaultPreset = false },
    };

    /// <summary>
    /// The selection a page starts with: everything in the default preset that this device can
    /// actually run. <paramref name="speechAvailable"/> comes from IPronunciationCheckService.
    /// </summary>
    public static HashSet<string> DefaultKeys(IEnumerable<ExerciseOption> options, bool speechAvailable) =>
        options
            .Where(o => o.InDefaultPreset && (speechAvailable || !o.RequiresSpeech))
            .Select(o => o.Key)
            .ToHashSet();
}
