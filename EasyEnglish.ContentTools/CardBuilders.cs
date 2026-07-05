using EasyEnglish.Core.Enums;
using EasyEnglish.Core.Models;

namespace EasyEnglish.ContentTools;

/// <summary>
/// Фабрики для кожного виду TestCard — щоб не писати вручну Kind + payload щоразу.
/// Id/UnitId лишаються 0: при імпорті архіву EF сам виставляє FK через
/// каскадну вставку батьківського Unit (див. коментар в ImportCourseZip.razor).
/// </summary>
public static class TestCardBuilder
{
    public static TestCardModel ShortAnswer(string text, params string[] acceptableAnswers) => new()
    {
        Kind = TestCardKind.ShortAnswer,
        Text = text,
        ShortAnswer = new ShortAnswerPayload { AcceptableAnswers = acceptableAnswers }
    };

    public static TestCardModel SingleChoice(string text, string[] options, string correctAnswer) => new()
    {
        Kind = TestCardKind.SingleChoice,
        Text = text,
        Choice = new ChoicePayload { Options = options, CorrectAnswers = [correctAnswer] }
    };

    public static TestCardModel MultipleChoice(string text, string[] options, string[] correctAnswers) => new()
    {
        Kind = TestCardKind.MultipleChoice,
        Text = text,
        Choice = new ChoicePayload { Options = options, CorrectAnswers = correctAnswers }
    };

    /// <summary>
    /// template містить {0},{1}... — по одній відповіді на кожен. options — опційний
    /// масив варіантів на кожен пропуск (null або порожній підмасив = поле вводу для цього {i}).
    /// </summary>
    public static TestCardModel Cloze(string template, string[] correctAnswers, string[][]? options = null, string? title = null) => new()
    {
        Title = title,
        Kind = TestCardKind.Cloze,
        Text = template,
        Cloze = new ClozePayload { CorrectAnswers = correctAnswers, Options = options }
    };

    /// <summary>Left[i] завжди пасує до Right[i] — порядок рядків і є відповідністю.</summary>
    public static TestCardModel Matching(string text, string[] left, string[] right) => new()
    {
        Kind = TestCardKind.Matching,
        Text = text,
        Matching = new MatchingPayload
        {
            Left = left,
            Right = right,
            CorrectRightIndexes = Enumerable.Range(0, left.Length).ToArray()
        }
    };
}

public static class StudyCardBuilder
{
    public static StudyCardModel Term(string term, string definition) => new()
    {
        Kind = StudyCardKind.Term,
        Title = term,
        Body = definition
    };

    public static StudyCardModel Text(string title, string body, string? codeBlock = null, string? dialogue = null) => new()
    {
        Kind = StudyCardKind.Text,
        Title = title,
        Body = body,
        CodeBlock = codeBlock,
        Dialogue = dialogue
    };

    /// <summary>body позначає розмиті слова подвійними зірочками: **word**.</summary>
    public static StudyCardModel BlurredText(string body, BlurRevealMode revealMode = BlurRevealMode.Independent) => new()
    {
        Kind = StudyCardKind.BlurredText,
        Body = body,
        RevealMode = revealMode
    };
}
