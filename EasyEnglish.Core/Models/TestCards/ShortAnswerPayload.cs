namespace EasyEnglish.Core.Models;

/// <summary>
/// Payload for <c>TestCardKind.ShortAnswer</c>.
/// Packed/unpacked into the <c>CorrectAnswers</c> field of TestCardEntity.
/// </summary>
public class ShortAnswerPayload
{
    /// <summary>Acceptable spellings/variants of the answer.</summary>
    public string[] AcceptableAnswers { get; set; } = [];
}
