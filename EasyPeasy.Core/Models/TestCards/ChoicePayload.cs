namespace EasyPeasy.Core.Models;

/// <summary>
/// Payload for <c>TestCardKind.SingleChoice</c> and <c>TestCardKind.MultipleChoice</c>.
/// Packed/unpacked into the <c>Options</c>/<c>CorrectAnswers</c> fields of TestCardEntity.
/// </summary>
public class ChoicePayload
{
    public string[] Options { get; set; } = [];

    /// <summary>One item for SingleChoice, several for MultipleChoice.</summary>
    public string[] CorrectAnswers { get; set; } = [];
}
