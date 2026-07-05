namespace EasyEnglish.Core.Enums;

/// <summary>
/// Вид оцінюваної тестової картки модуля.
/// </summary>
public enum TestCardKind
{
    /// <summary>
    /// Вибір однієї правильної відповіді з декількох варіантів.
    /// </summary>
    SingleChoice = 0,

    /// <summary>
    /// Вибір кількох правильних відповідей з декількох варіантів.
    /// </summary>
    MultipleChoice = 1,

    /// <summary>
    /// Відповідь довільним текстом.
    /// </summary>
    ShortAnswer = 2,

    /// <summary>
    /// Текст із одним чи кількома пропусками (позиційні плейсхолдери {0}, {1}, ... у Text).
    /// </summary>
    Cloze = 3,

    /// <summary>
    /// Встановлення відповідності між двома колонками значень.
    /// </summary>
    Matching = 4
}
