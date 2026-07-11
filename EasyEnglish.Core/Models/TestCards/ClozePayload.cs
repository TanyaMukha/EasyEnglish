namespace EasyEnglish.Core.Models;

/// <summary>
/// Payload for <c>TestCardKind.Cloze</c>. The card's text is a template with positional
/// placeholders "{0}", "{1}", ... Packed/unpacked into the <c>Options</c>/<c>CorrectAnswers</c>
/// fields of TestCardEntity.
/// </summary>
public class ClozePayload
{
    /// <summary>
    /// One array of acceptable answers per "{i}" in the text, indexed by i. Multiple items at
    /// position i means any of them counts as correct (applies to both a free-text input and a
    /// dropdown).
    /// </summary>
    public string[][] CorrectAnswers { get; set; } = [];

    /// <summary>
    /// <c>null</c> or a missing element at position i means a free-text input for that "{i}".
    /// A non-empty array at position i means a dropdown with those choices.
    /// </summary>
    public string[][]? Options { get; set; }
}
