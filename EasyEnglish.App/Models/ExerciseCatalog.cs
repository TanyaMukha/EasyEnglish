namespace EasyEnglish.App.Models;

/// <summary>Catalog of exercise types for words/examples/irregular forms — shared by the unit-level
/// and course-level practice setup pages so exercise descriptions aren't duplicated.</summary>
public static class ExerciseCatalog
{
    public static readonly List<ExerciseOption> Words = new()
    {
        new() { Key = "review-words", Title = "Повторення слів", Description = "Переглядайте вивчені слова з перекладом для закріплення", Icon = "bi-book", Color = "blue" },
        new() { Key = "word-to-translation-single-choice", Title = "Переклад для слова", Description = "Оберіть правильний переклад для англійського слова", Icon = "bi-translate", Color = "green" },
        new() { Key = "word-to-translation-know-or-not", Title = "Перевірка перекладу", Description = "Підтвердіть, чи знаєте переклад слова", Icon = "bi-eye", Color = "purple" },
        new() { Key = "translation-to-word-single-choice", Title = "Слово за перекладом", Description = "Знайдіть англійське слово за його перекладом", Icon = "bi-search", Color = "orange" },
        new() { Key = "translation-to-word-know-or-not", Title = "Перевірка слова", Description = "Підтвердіть, чи знаєте слово за перекладом", Icon = "bi-eye-slash", Color = "pink" },
        new() { Key = "translation-to-word-manual-input", Title = "Напишіть слово", Description = "Введіть англійське слово за його перекладом", Icon = "bi-keyboard", Color = "mint" },
        new() { Key = "translation-to-word-pronunciation", Title = "Скажіть слово", Description = "Вимовте англійське слово за перекладом і підказкою", Icon = "bi-mic-fill", Color = "coral" },
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
        new() { Key = "irregular-forms-pronunciation", Title = "Скажіть форми", Description = "Вимовте всі три форми слова одразу за перекладом", Icon = "bi-mic-fill", Color = "sage" },
    };
}
