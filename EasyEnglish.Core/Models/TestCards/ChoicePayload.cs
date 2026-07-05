namespace EasyEnglish.Core.Models;

/// <summary>
/// Payload для <c>TestCardKind.SingleChoice</c> та <c>TestCardKind.MultipleChoice</c>.
/// Пакується/розпаковується у поля <c>Options</c>/<c>CorrectAnswers</c> сутності TestCardEntity.
/// </summary>
public class ChoicePayload
{
    public string[] Options { get; set; } = [];

    /// <summary>Один елемент для SingleChoice, декілька — для MultipleChoice.</summary>
    public string[] CorrectAnswers { get; set; } = [];
}
