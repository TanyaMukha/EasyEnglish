namespace EasyEnglish.Core.Models;

/// <summary>
/// Payload для <c>TestCardKind.ShortAnswer</c>.
/// Пакується/розпаковується у поле <c>CorrectAnswers</c> сутності TestCardEntity.
/// </summary>
public class ShortAnswerPayload
{
    /// <summary>Прийнятні варіанти написання відповіді.</summary>
    public string[] AcceptableAnswers { get; set; } = [];
}
