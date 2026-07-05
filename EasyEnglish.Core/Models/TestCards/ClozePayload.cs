namespace EasyEnglish.Core.Models;

/// <summary>
/// Payload для <c>TestCardKind.Cloze</c>. Text картки — шаблон із позиційними
/// плейсхолдерами "{0}", "{1}", ... Пакується/розпаковується у поля
/// <c>Options</c>/<c>CorrectAnswers</c> сутності TestCardEntity.
/// </summary>
public class ClozePayload
{
    /// <summary>По одній відповіді на кожен "{i}" у Text, індекс = i.</summary>
    public string[] CorrectAnswers { get; set; } = [];

    /// <summary>
    /// null або відсутність елемента на позиції i — поле вводу для цього "{i}".
    /// Непорожній масив на позиції i — випадаючий список із цими варіантами.
    /// </summary>
    public string[][]? Options { get; set; }
}
