using EasyPeasy.Core.Enums;
using EasyPeasy.Core.Models;

namespace EasyPeasy.Core.Content;

/// <summary>
/// Factory methods for each <see cref="TestCardKind"/> — so a course-module class doesn't need to
/// set <c>Kind</c> + the right payload type by hand every time. <c>Id</c>/<c>UnitId</c> are left at
/// <c>0</c>: on import or seeding, EF assigns the FK itself via cascaded insert of the parent
/// <c>Unit</c> (see the comment in <c>ImportCourseZip.razor</c>).
/// </summary>
public static class TestCardBuilder
{
    /// <summary>A free-text card accepted against any of <paramref name="acceptableAnswers"/>.</summary>
    public static TestCardModel ShortAnswer(string text, string[] acceptableAnswers, string? hint = null) => new()
    {
        Kind = TestCardKind.ShortAnswer,
        Question = text,
        Hint = hint,
        ShortAnswer = new ShortAnswerPayload { AcceptableAnswers = acceptableAnswers }
    };

    /// <summary>A card with exactly one correct option out of <paramref name="options"/>.</summary>
    public static TestCardModel SingleChoice(string text, string[] options, string correctAnswer, string? hint = null) => new()
    {
        Kind = TestCardKind.SingleChoice,
        Question = text,
        Hint = hint,
        Choice = new ChoicePayload { Options = options, CorrectAnswers = [correctAnswer] }
    };

    /// <summary>A card where every one of <paramref name="correctAnswers"/> (a subset of <paramref name="options"/>) must be selected.</summary>
    public static TestCardModel MultipleChoice(string text, string[] options, string[] correctAnswers, string? hint = null) => new()
    {
        Kind = TestCardKind.MultipleChoice,
        Question = text,
        Hint = hint,
        Choice = new ChoicePayload { Options = options, CorrectAnswers = correctAnswers }
    };

    /// <summary>
    /// A fill-in-the-blank card. <paramref name="template"/> contains <c>{0}</c>, <c>{1}</c>, ...
    /// placeholders, each matched to an array of acceptable answers at the same index in
    /// <paramref name="correctAnswers"/> (multiple accepted synonyms per blank are allowed).
    /// <paramref name="options"/> is an optional per-blank dropdown-choice array — <c>null</c> or
    /// an empty sub-array for a given <c>{i}</c> means that blank is a free-text input instead.
    /// </summary>
    public static TestCardModel Cloze(string template, string[][] correctAnswers, string[][]? options = null, string? title = null, string? hint = null) => new()
    {
        Title = title,
        Kind = TestCardKind.Cloze,
        FormattedText = template,
        Hint = hint,
        Cloze = new ClozePayload { CorrectAnswers = correctAnswers, Options = options }
    };

    /// <summary>A card pairing each <paramref name="left"/>[i] with <paramref name="right"/>[i] — matching row order defines the correct pairing.</summary>
    public static TestCardModel Matching(string text, string[] left, string[] right, string? hint = null) => new()
    {
        Kind = TestCardKind.Matching,
        Question = text,
        Hint = hint,
        Matching = new MatchingPayload
        {
            Left = left,
            Right = right,
            CorrectRightIndexes = Enumerable.Range(0, left.Length).ToArray()
        }
    };
}

/// <summary>Factory methods for each <see cref="StudyCardKind"/>, mirroring <see cref="TestCardBuilder"/>.</summary>
public static class StudyCardBuilder
{
    /// <summary>A term/definition card — a vocabulary word or grammar-rule name plus its explanation.</summary>
    public static StudyCardModel Term(string term, string definition) => new()
    {
        Kind = StudyCardKind.Term,
        Title = term,
        Body = definition
    };

    /// <summary>A free-form reading card — a title plus a body of prose, optionally with a code block and/or dialogue section.</summary>
    public static StudyCardModel Text(string title, string body, string? codeBlock = null, string? dialogue = null) => new()
    {
        Kind = StudyCardKind.Text,
        Title = title,
        Body = body,
        CodeBlock = codeBlock,
        Dialogue = dialogue
    };

    /// <summary>A card whose text hides one or more spans marked with <c>**double asterisks**</c> in <paramref name="body"/> until revealed.</summary>
    public static StudyCardModel BlurredText(string body, BlurRevealMode revealMode = BlurRevealMode.Independent) => new()
    {
        Kind = StudyCardKind.BlurredText,
        Body = body,
        RevealMode = revealMode
    };
}
