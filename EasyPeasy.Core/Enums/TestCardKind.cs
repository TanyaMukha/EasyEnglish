namespace EasyPeasy.Core.Enums;

/// <summary>
/// The kind of graded test card in a unit. Determines the shape of the packed
/// <c>Options</c>/<c>CorrectAnswers</c> JSON — see <see cref="Entities.TestCardEntity"/>.
/// </summary>
public enum TestCardKind
{
    /// <summary>Choosing one correct answer out of several options.</summary>
    SingleChoice = 0,

    /// <summary>Choosing several correct answers out of several options.</summary>
    MultipleChoice = 1,

    /// <summary>A free-text answer.</summary>
    ShortAnswer = 2,

    /// <summary>Text with one or more blanks (positional placeholders {0}, {1}, ... in the text).</summary>
    Cloze = 3,

    /// <summary>Matching items between two columns of values.</summary>
    Matching = 4
}
