namespace EasyEnglish.Core.Models;

/// <summary>
/// Payload для <c>TestCardKind.Cloze</c>. Text картки — шаблон із позиційними
/// плейсхолдерами "{0}", "{1}", ... Пакується/розпаковується у поля
/// <c>Options</c>/<c>CorrectAnswers</c> сутності TestCardEntity.
/// </summary>
public class ClozePayload
{
    /// <summary>
    /// По одному масиву прийнятних відповідей на кожен "{i}" у Text, індекс = i.
    /// Кілька елементів на позиції i — будь-який з них зараховується як правильний
    /// (актуально і для поля вводу, і для випадаючого списку).
    /// </summary>
    public string[][] CorrectAnswers { get; set; } = [];

    /// <summary>
    /// null або відсутність елемента на позиції i — поле вводу для цього "{i}".
    /// Непорожній масив на позиції i — випадаючий список із цими варіантами.
    /// </summary>
    public string[][]? Options { get; set; }
}
