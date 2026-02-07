namespace EasyEnglish.Core.Enums;

/// <summary>
/// Пріоритет вибору слів
/// </summary>
public enum WordPriority
{
    /// <summary>
    /// Випадкові слова
    /// </summary>
    Random = 0,

    /// <summary>
    /// Складні слова (з найбільшою кількістю помилок)
    /// </summary>
    Difficult = 1,

    /// <summary>
    /// Нові слова (ще не вивчені)
    /// </summary>
    New = 2,

    /// <summary>
    /// Слова для повторення
    /// </summary>
    Review = 3,

    /// <summary>
    /// Найстаріші слова (давно не вивчались)
    /// </summary>
    Old = 4
}
