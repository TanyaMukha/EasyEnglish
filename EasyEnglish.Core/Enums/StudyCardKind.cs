namespace EasyEnglish.Core.Enums;

/// <summary>
/// The kind of non-graded study card in a unit.
/// </summary>
public enum StudyCardKind
{
    /// <summary>A term and its definition.</summary>
    Term = 0,

    /// <summary>A short text, possibly with a code block or a dialogue.</summary>
    Text = 1,

    /// <summary>Text with blurred segments that reveal on click.</summary>
    BlurredText = 2
}
