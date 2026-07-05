namespace EasyEnglish.Core.Models;

/// <summary>
/// Payload для <c>TestCardKind.Matching</c>. Left пакується разом із Right
/// у поле <c>Options</c> сутності TestCardEntity, CorrectRightIndexes — у <c>CorrectAnswers</c>.
/// </summary>
public class MatchingPayload
{
    public string[] Left { get; set; } = [];

    public string[] Right { get; set; } = [];

    /// <summary>CorrectRightIndexes[i] = індекс у Right, що правильно пасує до Left[i].</summary>
    public int[] CorrectRightIndexes { get; set; } = [];
}
