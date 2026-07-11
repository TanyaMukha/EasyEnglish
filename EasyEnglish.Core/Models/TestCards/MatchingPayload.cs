namespace EasyEnglish.Core.Models;

/// <summary>
/// Payload for <c>TestCardKind.Matching</c>. Left is packed together with Right into the
/// <c>Options</c> field of TestCardEntity; CorrectRightIndexes goes into <c>CorrectAnswers</c>.
/// </summary>
public class MatchingPayload
{
    public string[] Left { get; set; } = [];

    public string[] Right { get; set; } = [];

    /// <summary>CorrectRightIndexes[i] = the index in Right that correctly matches Left[i].</summary>
    public int[] CorrectRightIndexes { get; set; } = [];
}
