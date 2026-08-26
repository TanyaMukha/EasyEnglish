using System.Text.RegularExpressions;
using EasyPeasy.Core.Content;
using EasyPeasy.Core.Enums;
using EasyPeasy.Core.Models;

namespace EasyPeasy.Core.Tests;

/// <summary>
/// The demo courses are the first thing anyone sees who runs this without a library of their own,
/// so the content is checked the way authored content should be: a cloze card whose placeholders
/// do not match its answers, or a choice card whose correct answer is not among its options, is a
/// broken exercise that compiles perfectly well.
/// </summary>
public class DemoCoursesTests
{
    public static TheoryData<string, CourseModel> Courses()
    {
        var data = new TheoryData<string, CourseModel>();

        foreach (var course in DemoCourses.All())
            data.Add(course.Title, course);

        return data;
    }

    private static IEnumerable<UnitModel> UnitsOf(CourseModel course) => course.Units ?? [];

    private static IEnumerable<TestCardModel> TestCardsOf(CourseModel course) =>
        UnitsOf(course).SelectMany(unit => unit.TestCards ?? []);

    [Fact]
    public void OffersMoreThanOneCourse()
    {
        Assert.True(DemoCourses.All().Count >= 2);
    }

    [Theory]
    [MemberData(nameof(Courses))]
    public void CourseIsDescribedAndHasUnits(string _, CourseModel course)
    {
        Assert.False(string.IsNullOrWhiteSpace(course.Title));
        Assert.False(string.IsNullOrWhiteSpace(course.Description));
        Assert.NotEmpty(UnitsOf(course));

        foreach (var unit in UnitsOf(course))
        {
            Assert.False(string.IsNullOrWhiteSpace(unit.Title));

            var material =
                (unit.Words?.Count ?? 0)
                + (unit.IrregularForms?.Count ?? 0)
                + (unit.StudyCards?.Count ?? 0)
                + (unit.TestCards?.Count ?? 0);

            Assert.True(material > 0, $"Unit '{unit.Title}' has nothing in it");
        }
    }

    [Theory]
    [MemberData(nameof(Courses))]
    public void WordsCarryATranslation(string _, CourseModel course)
    {
        foreach (var word in UnitsOf(course).SelectMany(unit => unit.Words ?? []))
        {
            Assert.False(string.IsNullOrWhiteSpace(word.Word));
            Assert.False(
                string.IsNullOrWhiteSpace(word.Translation),
                $"'{word.Word}' has no translation");

            foreach (var example in word.Examples ?? [])
            {
                Assert.False(string.IsNullOrWhiteSpace(example.Sentence));
                Assert.False(
                    string.IsNullOrWhiteSpace(example.Translation),
                    $"An example for '{word.Word}' is untranslated");
            }
        }
    }

    [Theory]
    [MemberData(nameof(Courses))]
    public void IrregularFormsHaveAtLeastTwoForms(string _, CourseModel course)
    {
        foreach (var form in UnitsOf(course).SelectMany(unit => unit.IrregularForms ?? []))
        {
            Assert.False(string.IsNullOrWhiteSpace(form.FirstForm));
            Assert.False(
                string.IsNullOrWhiteSpace(form.SecondForm),
                $"'{form.FirstForm}' has no second form");
        }
    }

    [Theory]
    [MemberData(nameof(Courses))]
    public void EveryTestCardCarriesThePayloadItsKindNeeds(string _, CourseModel course)
    {
        foreach (var card in TestCardsOf(course))
        {
            switch (card.Kind)
            {
                case TestCardKind.SingleChoice:
                case TestCardKind.MultipleChoice:
                    Assert.NotNull(card.Choice);
                    break;
                case TestCardKind.ShortAnswer:
                    Assert.NotNull(card.ShortAnswer);
                    break;
                case TestCardKind.Cloze:
                    Assert.NotNull(card.Cloze);
                    break;
                case TestCardKind.Matching:
                    Assert.NotNull(card.Matching);
                    break;
                default:
                    Assert.Fail($"Unhandled kind {card.Kind}");
                    break;
            }
        }
    }

    [Theory]
    [MemberData(nameof(Courses))]
    public void ChoiceAnswersAreAmongTheOptions(string _, CourseModel course)
    {
        var cards = TestCardsOf(course).Where(card => card.Choice is not null);

        foreach (var card in cards)
        {
            var choice = card.Choice!;

            Assert.True(choice.Options.Length >= 2, $"'{card.Question}' offers fewer than two options");
            Assert.NotEmpty(choice.CorrectAnswers);

            foreach (var answer in choice.CorrectAnswers)
            {
                Assert.True(
                    choice.Options.Contains(answer),
                    $"'{answer}' is not among the options of '{card.Question}'");
            }

            if (card.Kind == TestCardKind.SingleChoice)
                Assert.Single(choice.CorrectAnswers);
        }
    }

    [Theory]
    [MemberData(nameof(Courses))]
    public void ClozeBlanksMatchTheirAnswers(string _, CourseModel course)
    {
        var cards = TestCardsOf(course).Where(card => card.Cloze is not null);

        foreach (var card in cards)
        {
            var template = card.FormattedText ?? string.Empty;

            var blanks = Regex
                .Matches(template, @"\{(\d+)\}")
                .Select(match => int.Parse(match.Groups[1].Value))
                .Distinct()
                .OrderBy(index => index)
                .ToArray();

            Assert.True(blanks.Length > 0, $"Cloze card '{card.Title}' has no placeholders");
            Assert.Equal(Enumerable.Range(0, blanks.Length), blanks);

            var answers = card.Cloze!.CorrectAnswers;
            Assert.Equal(blanks.Length, answers.Length);

            foreach (var perBlank in answers)
                Assert.NotEmpty(perBlank);

            // An options array, when given, must cover every blank — a shorter one would leave a
            // blank with no dropdown and no way to tell that from a deliberate free-text answer
            if (card.Cloze.Options is { } options)
                Assert.Equal(blanks.Length, options.Length);
        }
    }

    [Theory]
    [MemberData(nameof(Courses))]
    public void MatchingColumnsAreTheSameLength(string _, CourseModel course)
    {
        var cards = TestCardsOf(course).Where(card => card.Matching is not null);

        foreach (var card in cards)
        {
            var matching = card.Matching!;

            Assert.NotEmpty(matching.Left);
            Assert.Equal(matching.Left.Length, matching.Right.Length);
            Assert.Equal(matching.Left.Length, matching.CorrectRightIndexes.Length);
        }
    }

    [Theory]
    [MemberData(nameof(Courses))]
    public void ShortAnswersAcceptSomething(string _, CourseModel course)
    {
        var cards = TestCardsOf(course).Where(card => card.ShortAnswer is not null);

        foreach (var card in cards)
        {
            Assert.NotEmpty(card.ShortAnswer!.AcceptableAnswers);

            foreach (var answer in card.ShortAnswer.AcceptableAnswers)
                Assert.False(string.IsNullOrWhiteSpace(answer));
        }
    }

    [Fact]
    public void BetweenThemTheCoursesShowEveryCardKind()
    {
        var all = DemoCourses.All();

        var testKinds = all.SelectMany(TestCardsOf).Select(card => card.Kind).Distinct();
        Assert.Equal(
            Enum.GetValues<TestCardKind>().OrderBy(kind => kind),
            testKinds.OrderBy(kind => kind));

        var studyKinds = all
            .SelectMany(UnitsOf)
            .SelectMany(unit => unit.StudyCards ?? [])
            .Select(card => card.Kind)
            .Distinct();
        Assert.Equal(
            Enum.GetValues<StudyCardKind>().OrderBy(kind => kind),
            studyKinds.OrderBy(kind => kind));
    }
}
